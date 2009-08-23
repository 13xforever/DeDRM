using System;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Drm.Adept
{
	public static class KeyRetriever
	{
		public static byte[] Retrieve()
		{
			ulong systemDriveSerial = GetSystemDriveSerialNumber();
			var cpuInfo = GetCpuInfo();
			string username = Environment.UserName;
			byte[] entropy = MakeEntropy(systemDriveSerial, cpuInfo, username);
			byte[] deviceKeyData;
			using (RegistryKey deviceKey = Registry.CurrentUser.OpenSubKey(DeviceKey))
			{
				if (deviceKey == null) throw new InvalidOperationException("Adobe Digital Editions isn't activated");
				deviceKeyData = (byte[])deviceKey.GetValue("key");
			}
			byte[] decryptedKey = ProtectedData.Unprotect(deviceKeyData, entropy, DataProtectionScope.CurrentUser);
			byte[] privateLicense = GetPrivateLicense();
			return DecryptData(decryptedKey, privateLicense);
		}

		private static byte[] DecryptData(byte[] key, byte[] data)
		{
			byte[] result;
			using (var cipher = new AesManaged {Mode = CipherMode.CBC, Key = key})
				result = cipher.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
			return result.Skip(26).ToArray();
		}

		private static byte[] GetPrivateLicense()
		{
			using (RegistryKey activationKey = Registry.CurrentUser.OpenSubKey(ActivationKey))
			{
				if (activationKey == null) throw new InvalidOperationException("ADE activation is corrupted or absent.");
				foreach (var subKeyName in activationKey.GetSubKeyNames())
					using (var subKey = activationKey.OpenSubKey(subKeyName))
						foreach (var keyName in subKey.GetSubKeyNames())
							using (var key = subKey.OpenSubKey(keyName))
								if (key.GetValue("").ToString() == "privateLicenseKey")
									return Convert.FromBase64String((string)key.GetValue("value"));
			}
			throw new InvalidOperationException("Couldn't find private license key!");
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
		private const string DeviceKey = "Software\\Adobe\\Adept\\Device";
		private const string ActivationKey = "Software\\Adobe\\Adept\\Activation";

		private class CpuInfo
		{

			public string vendor;
			public readonly byte[] familyModelStepping = new byte[3];
		}
	}
}