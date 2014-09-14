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
			var result = new List<byte[]>();
			var userIds = new List<string>();
			var deviceIds = new List<string>();

			using (var cmd = new SQLiteCommand("select UserID from user", connection))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
					userIds.Add(reader["UserID"] as string);

			using (var sha256 = new SHA256Managed())
			{
				deviceIds.AddRange(NetworkInterface.GetAllNetworkInterfaces()
					.Select(iface => iface.GetPhysicalAddress().ToString().ToUpper())
					.Distinct()
					.Select(mac => DeviceIdPrefix + mac)
					.Select(secret => Encoding.ASCII.GetBytes(secret))
					.Select(bytes => sha256.TransformFinalBlock(bytes, 0, bytes.Length))
					.Select(hash => hash.ToHexString())
				);

				result.AddRange(from deviceId in deviceIds
					from userId in userIds
					select Encoding.ASCII.GetBytes(deviceId + userId) into bytes
					select sha256.TransformFinalBlock(bytes, 0, bytes.Length) into hash
					select hash.Copy(hash.Length-16)
				);
			}
			return result;
		}
	}
}