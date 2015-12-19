using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Drm.Format.Epub
{
	internal class AuthData
	{
		public byte[] privateKey;
		public string pkcs12Store;
	}

	public static class AdeptMasterKeys
	{
		public static List<byte[]> Retrieve()
		{
			ulong systemDriveSerial = GetSystemDriveSerialNumber();
			CpuInfo cpuInfo = GetCpuInfo();
			string username = Environment.UserName;
			byte[] entropy = MakeEntropy(systemDriveSerial, cpuInfo, username);
			byte[] deviceKeyData;
			using (RegistryKey deviceKey = Registry.CurrentUser.OpenSubKey(DeviceKey))
			{
				if (deviceKey == null) throw new InvalidOperationException("Adobe Digital Editions isn't activated");
				deviceKeyData = (byte[])deviceKey.GetValue("key");
			}
			byte[] decryptedKey = ProtectedData.Unprotect(deviceKeyData, entropy, DataProtectionScope.CurrentUser);
			var privateLicenses = GetPrivateLicenses();
			return DecryptData(decryptedKey, privateLicenses);
		}

		private static List<byte[]> DecryptData(byte[] key, List<byte[]> data)
		{
			var result = new List<byte[]>(data.Count);
			foreach (var privateKey in data)
			{
				byte[] decryptedPrivateKey;
				using (var cipher = new AesManaged {Mode = CipherMode.CBC, Key = key})
					decryptedPrivateKey = cipher.CreateDecryptor().TransformFinalBlock(privateKey, 0, privateKey.Length);
				result.Add(decryptedPrivateKey.Skip(26).ToArray());
			}
			return result;
		}

		private static List<byte[]> GetPrivateLicenses()
		{
			var result = new List<byte[]>();
			using (RegistryKey activationKey = Registry.CurrentUser.OpenSubKey(ActivationKey))
			{
				if (activationKey == null) throw new InvalidOperationException("ADE activation is corrupted or absent.");
				foreach (var subKeyName in activationKey.GetSubKeyNames())
					using (RegistryKey subKey = activationKey.OpenSubKey(subKeyName))
					if (subKey.GetValue("").ToString() == "credentials")
					{
						var authData = new AuthData();
						foreach (var keyName in subKey.GetSubKeyNames())
							using (RegistryKey key = subKey.OpenSubKey(keyName))
							if (key.GetValue("").ToString() == "privateLicenseKey")
								authData.privateKey = Convert.FromBase64String((string)key.GetValue("value"));
							else if (key.GetValue("").ToString() == "pkcs12")
								authData.pkcs12Store = (string)key.GetValue("value");
						if (!string.IsNullOrEmpty(authData.pkcs12Store)) result.Add(authData.privateKey);
					}
			}
			if (result.Count == 0) throw new InvalidOperationException("Couldn't find private license key!");
			return result;
		}

		private static byte[] MakeEntropy(ulong serial, CpuInfo cpuInfo, string user)
		{
			//4 bytes
			var result = new byte[32];
			BitConverter.GetBytes((uint)serial).Reverse().ToArray().CopyTo(result, 0);
			//12 bytes
			Encoding.ASCII.GetBytes(cpuInfo.vendor).CopyTo(result, 4);
			//3 bytes
			cpuInfo.familyModelStepping.CopyTo(result, 16);
			//13 bytes
			Encoding.Unicode.GetBytes(user).Take(26).Where((value, idx) => idx % 2 == 0).ToArray().CopyTo(result, 19);
			return result;
		}

		private static CpuInfo GetCpuInfo()
		{
			var result = new CpuInfo();
			string sig;
			using (var cpu = new ManagementObject(@"Win32_Processor.DeviceID=""CPU0"""))
			{
				cpu.Get();
				result.vendor = cpu["Manufacturer"].ToString();
				sig = cpu["ProcessorId"].ToString();
			}
			for (byte i = 0; i < 3; i++) result.familyModelStepping[i] = Convert.ToByte(sig.Substring(10 + i * 2, 2), 16);
			return result;
		}

		private static ulong GetSystemDriveSerialNumber()
		{
			char systemDriveLetter = Environment.SystemDirectory[0];
			string numberAsHexString;
			using (var dsk = new ManagementObject(@"Win32_LogicalDisk.DeviceID=""" + systemDriveLetter + @":"""))
			{
				dsk.Get();
				numberAsHexString = dsk["VolumeSerialNumber"].ToString();
			}
			if (!HexString.IsMatch(numberAsHexString))
				throw new ArgumentException(string.Format("Serial number for volume {0} isn't a valid hexadecimal string ({1})", systemDriveLetter, numberAsHexString));
			return Convert.ToUInt64(numberAsHexString, 16);
		}

		private static readonly Regex HexString = new Regex(@"\A\b[0-9a-fA-F]+\b\Z");

		private class CpuInfo
		{
			public string vendor;
			public readonly byte[] familyModelStepping = new byte[3];
		}

		private const string DeviceKey = "Software\\Adobe\\Adept\\Device";
		private const string ActivationKey = "Software\\Adobe\\Adept\\Activation";
	}
}