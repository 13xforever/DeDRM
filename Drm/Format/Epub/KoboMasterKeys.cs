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
		private static readonly string[] Salts = { "QJhwzAtXL", "XzUhGYdFp", "NoCanLook" }; //lol, new salt isn't really being used

		public static List<byte[]> Retrieve(SQLiteConnection connection)
		{
			var userIds = new List<string>();
			using (var cmd = new SQLiteCommand("select UserID from user", connection))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
					userIds.Add(reader["UserID"] as string);
			using (var sha256 = new SHA256Managed())
			{
				var combinedSalt = Salts[1] + Salts[2];
				var combinedHash = sha256.ComputeHash(Encoding.ASCII.GetBytes(combinedSalt)).ToHexString();
				var realSalt = combinedHash.Substring(11, 9);
				var mac = NetworkInterface.GetAllNetworkInterfaces()
					.Where(iface => iface.NetworkInterfaceType != NetworkInterfaceType.Loopback)
					.Select(iface => iface.GetPhysicalAddress().ToMacString())
					.Distinct()
					.First(addr => !string.IsNullOrEmpty(addr));
				var secret = realSalt + mac;
				var deviceId = sha256.ComputeHash(Encoding.ASCII.GetBytes(secret)).ToHexString();

				var result = (
					from userId in userIds
					select Encoding.UTF8.GetBytes((deviceId + userId).Trim()) into key
					select sha256.ComputeHash(key) into hash
					select hash.Copy(hash.Length - 16)
				).ToList();

				//var strResult = result.Select(b => b.ToHexString()).Distinct().ToList();
				return result;
			}
		}
	}
}