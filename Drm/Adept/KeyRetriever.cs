using System;
using System.Management;
using Microsoft.Win32;

namespace Drm.Adept
{
	public class KeyRetriever
	{
		public void Retrieve()
		{
			string systemDriveSerial = GetSystemDriveSerialNumber();
			string cpuVendor = GetCpuVendor();
			string signature = MakeSignature();
			string username = Environment.UserName;
			byte[] entropy = MakeEntropy(systemDriveSerial, cpuVendor, signature, username);
			RegistryKey deviceData = Registry.CurrentUser.OpenSubKey(DeviceKey);
			if (deviceData == null) throw new InvalidOperationException("Adobe Digital Editions isn't activated");
			var device = (byte[])deviceData.GetValue("key");
			var decryptedKey = CryptUnprotectData(device, entropy);
		}

		private static byte[] CryptUnprotectData(byte[] device, byte[] entropy)
		{
			throw new NotImplementedException();
		}

		private static byte[] MakeEntropy(string serial, string vendor, string signature, string user)
		{
			//entropy = pack('>I12s3s13s', serial, vendor, signature, user)
			throw new NotImplementedException();
		}

		private static string MakeSignature()
		{
			//CPUID1_INSNS = create_string_buffer("\x53\x31\xc0\x40\x0f\xa2\x5b\xc3")
			//signature = pack('>I', cpuid1())[1:]
			throw new NotImplementedException();
		}

		private static string GetCpuVendor()
		{
			throw new NotImplementedException();
		}

		private static string GetSystemDriveSerialNumber()
		{
			char systemDriveLetter = Environment.SystemDirectory[0];
			var dsk = new ManagementObject(@"win32_logicaldisk.deviceid=""" + systemDriveLetter + @":""");
			dsk.Get();
			return dsk["VolumeSerialNumber"].ToString();
		}

		private const string DeviceKey = "Software\\Adobe\\Adept\\Device";
		private const string PrivateLicenseKey = "Software\\Adobe\\Adept\\Activation\\{0}\\{1}";
	}
}