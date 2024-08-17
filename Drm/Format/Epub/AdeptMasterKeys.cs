using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using Aes = System.Security.Cryptography.Aes;

namespace Drm.Format.Epub;

public static partial class AdeptMasterKeys
{
	public static List<byte[]> Retrieve()
	{
		var systemDriveSerial = GetSystemDriveSerialNumber();
		var username = Environment.UserName;
		var entropy = new byte[32];
		FillEntropy(entropy, systemDriveSerial, username);
		using var deviceKey = Registry.CurrentUser.OpenSubKey(DeviceKey);
		if (deviceKey is null)
			throw new InvalidOperationException("Adobe Digital Editions isn't activated");

		var deviceKeyData = (byte[]?)deviceKey.GetValue("key");
		if (deviceKeyData is null)
			throw new InvalidOperationException("Adobe Digital Editions isn't activated");
		
		var decryptedKey = ProtectedData.Unprotect(deviceKeyData, entropy, DataProtectionScope.CurrentUser);
		var privateLicenses = GetPrivateLicenses();
		return DecryptData(decryptedKey, privateLicenses);
	}

	private static List<byte[]> DecryptData(byte[] key, List<byte[]> data)
	{
		var result = new List<byte[]>(data.Count);
		using var cipher = Aes.Create();
		cipher.Mode = CipherMode.CBC;
		foreach (var privateKey in data)
		{
			var decryptedPrivateKey = cipher.CreateDecryptor(key, null).TransformFinalBlock(privateKey, 0, privateKey.Length);
			result.Add(decryptedPrivateKey[26..]);
		}
		return result;
	}

	private static List<byte[]> GetPrivateLicenses()
	{
		using var activationKey = Registry.CurrentUser.OpenSubKey(ActivationKey);
		if (activationKey is null)
			throw new InvalidOperationException("ADE activation is corrupted or absent.");
			
		var result = new List<byte[]>();
		foreach (var subKeyName in activationKey.GetSubKeyNames())
		{
			using var subKey = activationKey.OpenSubKey(subKeyName);
			if (subKey?.GetValue("")?.ToString() is not "credentials")
				continue;

			string? pkcs12Store = null;
			byte[]? privateKey = null;
			foreach (var keyName in subKey.GetSubKeyNames())
			{
				using var key = subKey.OpenSubKey(keyName);
				if (key is null)
					continue;

				if (key.GetValue("")?.ToString() is "privateLicenseKey"
				    && key.GetValue("value") is string plkValue)
					privateKey = Convert.FromBase64String(plkValue);
				else if (key.GetValue("")?.ToString() is "pkcs12"
				         && key.GetValue("value") is string pkcs12Value)
					pkcs12Store = pkcs12Value;
			}
			if (pkcs12Store is { Length: >0 } && privateKey is {Length: >0})
				result.Add(privateKey);
		}
		if (result.Count is 0)
			throw new InvalidOperationException("Couldn't find private license key!");

		return result;
	}

	private static void FillEntropy(Span<byte> result, ulong serial, ReadOnlySpan<char> user)
	{
		//4 bytes
		var serialReg = Vector64.Create(serial).AsByte();
		(result[0], result[1], result[2], result[3]) = (serialReg[3], serialReg[2], serialReg[1], serialReg[0]);
		//12 bytes
		var vendorSlice = result[4..16];
		Encoding.ASCII.TryGetBytes(CpuInfo.Vendor, vendorSlice, out _);
		//3 bytes
		CpuInfo.FamilyModelStepping.CopyTo(result[16..19]);
		//13 bytes
		if (user.Length > 13)
			user = user[..13];
		Span<byte> tmp = stackalloc byte[13 * sizeof(char)];
		Encoding.Unicode.TryGetBytes(user, tmp, out var nameByteCount);
		var nameDest = result[19..32];
		for (var i = 0; i < nameByteCount; i += 2)
			nameDest[i / 2] = tmp[i];
	}

	private static ulong GetSystemDriveSerialNumber()
	{
		var systemDriveLetter = Environment.SystemDirectory[..1];
		using var dsk = new ManagementObject($@"Win32_LogicalDisk.DeviceID=""{systemDriveLetter}""");
		dsk.Get();
		var numberAsHexString = dsk["VolumeSerialNumber"].ToString();
		if (!HexStringPattern().IsMatch(numberAsHexString))
			throw new ArgumentException($"Serial number for volume {systemDriveLetter} isn't a valid hexadecimal string ({numberAsHexString})");

		return Convert.ToUInt64(numberAsHexString, 16);
	}

	[SkipLocalsInit]
	private static class CpuInfo
	{
		public static readonly string Vendor;
		public static readonly byte[] FamilyModelStepping = new byte[3];

		static CpuInfo()
		{
			var r = X86Base.CpuId(0, 0);
			var reg = Vector128.Create(r.Ebx, r.Edx, r.Ecx, 0);
			Span<byte> vendorId = stackalloc byte[4 * sizeof(int)];
			reg.AsByte().CopyTo(vendorId);
			Vendor = Encoding.ASCII.GetString(vendorId[..(r.Eax - 1)]);

			var processorId = X86Base.CpuId(1, 0).Eax;
			for (byte i = 0; i < 3; i++)
			{
				FamilyModelStepping[2 - i] = (byte)processorId;
				processorId >>= 8;
			}
		}
	}

	[GeneratedRegex(@"\A\b[0-9a-fA-F]+\b\Z")]
	private static partial Regex HexStringPattern();
	private const string DeviceKey = @"Software\Adobe\Adept\Device";
	private const string ActivationKey = @"Software\Adobe\Adept\Activation";
}