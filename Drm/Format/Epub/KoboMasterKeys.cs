using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Drm.Utils;

namespace Drm.Format.Epub
{
	public static class KoboMasterKeys
	{
		private static readonly string[] Salts = { "NoCanLook", "XzUhGYdFp" };

		public static List<byte[]> Retrieve(SQLiteConnection connection)
		{
			var userIds = new List<string>();

			using (var cmd = new SQLiteCommand("select UserID from user", connection))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
					userIds.Add(reader["UserID"] as string);

			using (var sha256 = new SHA256Managed())
			{
				List<string> deviceIds;
				var pwsdid = GetPwsdidFromCookies();
				if (!string.IsNullOrEmpty(pwsdid))
					deviceIds = new List<string> {pwsdid};
				else
				{
					var macs = NetworkInterface.GetAllNetworkInterfaces()
						.Where(iface => iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
						.Select(iface => iface.GetPhysicalAddress().ToMacString())
						.Distinct()
						.Where(addr => !string.IsNullOrEmpty(addr))
						.ToList();
					deviceIds = (
						from mac in macs
						from salt in Salts
						let secret = salt + mac
						let trimmedSecret = Encoding.UTF8.GetBytes(secret.Trim())
						let hash = sha256.ComputeHash(trimmedSecret)
						select hash.ToHexString()
					).ToList();
				}
				var result = (from deviceId in deviceIds
					from userId in userIds
					select Encoding.UTF8.GetBytes((deviceId + userId).Trim()) into key
					select sha256.ComputeHash(key) into hash
					select hash.Copy(hash.Length - 16)
				).ToList();

				//var result = masterHashes.Select(hash => hash.Copy(hash.Length - 16)).ToList();
				//var strResult = result.Select(b => b.ToHexString()).Distinct().ToList();
				return result;
			}
		}

		private static string GetPwsdidFromCookies()
		{
			var cookies = (string[])Microsoft.Win32.Registry.GetValue(@"HKEY_CURRENT_USER\Software\Kobo\Kobo Desktop Edition\Browser", "cookies", null);
			if (cookies == null || cookies.Length == 0)
				return null;

			var pwsdid = cookies.FirstOrDefault(cookie => cookie.Contains("pwsdid"));
			if (string.IsNullOrEmpty(pwsdid))
				return null;
				
			var start = pwsdid.IndexOf('=');
			var end = pwsdid.IndexOf(';', start);
			var result = pwsdid.Substring(start + 1, end - start - 1);
			return result;
		}
	}
}