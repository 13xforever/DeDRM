using System;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Drm.Adept
{
	public static class KeyRetriever
	{
		public static void Retrieve()
		{
			ulong systemDriveSerial = GetSystemDriveSerialNumber();
			string cpuVendor = GetCpuVendor();
			byte[] signature = MakeSignature();
			string username = Environment.UserName;
			byte[] entropy = MakeEntropy(systemDriveSerial, cpuVendor, signature, username);
			RegistryKey deviceData = Registry.CurrentUser.OpenSubKey(DeviceKey);
			if (deviceData == null) throw new InvalidOperationException("Adobe Digital Editions isn't activated");
			var device = (byte[])deviceData.GetValue("key"); //\x01\x00\x00\x00\xd0\x8c\x9...01UD,\x90\xbf*1\xbd\xd1{\xe4
			byte[] decryptedKey = CryptUnprotectData(device, entropy);
		}

		private static byte[] CryptUnprotectData(byte[] device, byte[] entropy)
		{
			throw new NotImplementedException();
		}

#if DEBUG
		public
#else
		private
#endif
			static byte[] MakeEntropy(ulong serial, string vendor, byte[] signature, string user)
		{
			//4 bytes
			var result = new byte[32];
			BitConverter.GetBytes((uint)serial).Reverse().ToArray().CopyTo(result, 0);
			//12 bytes
			Encoding.ASCII.GetBytes(vendor).CopyTo(result, 4);
			//3 bytes
			signature.CopyTo(result, 16);
			//13 bytes
			Encoding.Unicode.GetBytes(user).Take(26).Where((value, idx) => idx % 2 == 0).ToArray().CopyTo(result, 19);
			return result;
		}

		private static byte[] MakeSignature()
		{
			string sig;
			using (var cpu = new ManagementObject(@"Win32_Processor.DeviceID=""CPU0"""))
			{
				cpu.Get();
				sig = cpu["ProcessorId"].ToString();
			}
			var result = new byte[3];
			for (byte i = 0; i < 3; i++) result[i] = Convert.ToByte(sig.Substring(10 + i * 2, 2), 16);
			return result;
		}

		private static string GetCpuVendor()
		{
			using (var cpu = new ManagementObject(@"Win32_Processor.DeviceID=""CPU0"""))
			{
				cpu.Get();
				return cpu["Manufacturer"].ToString();
			}
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
		private const string PrivateLicenseKey = "Software\\Adobe\\Adept\\Activation\\{0}\\{1}";
	}
}