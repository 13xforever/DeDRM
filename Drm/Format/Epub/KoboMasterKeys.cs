using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Drm.Utils;

namespace Drm.Format.Epub
{
	public static class KoboMasterKeys
	{
		private const string DeviceIdPrefix = "NoCanLook";

		public static List<byte[]> Retrieve(SQLiteConnection connection)
		{
			var userIds = new List<string>();

			using (var cmd = new SQLiteCommand("select UserID from user", connection))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
					userIds.Add(reader["UserID"] as string);

			using (var sha256 = new SHA256Managed())
			{
				var macs = NetworkInterface.GetAllNetworkInterfaces()
					.Select(iface => iface.GetPhysicalAddress().ToMacString())
					.Distinct()
					.ToList();
				var deviceIds = macs
					.Select(mac => DeviceIdPrefix + mac)
					.Select(secret => Encoding.ASCII.GetBytes(secret))
					.Select(sha256.ComputeHash)
					.Select(hash => hash.ToHexString())
					.ToList();

				var result = (from deviceId in deviceIds
					from userId in userIds
					select Encoding.ASCII.GetBytes(deviceId + userId) into bytes
					select sha256.ComputeHash(bytes) into hash
					select hash.Copy(hash.Length-16)
				).ToList();

				//var strResult = result.Select(b => b.ToHexString()).Distinct().ToList();
				return result;
			}
		}
	}
}