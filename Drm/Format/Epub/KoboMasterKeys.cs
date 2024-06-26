using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Drm.Utils;

namespace Drm.Format.Epub;

public static class KoboMasterKeys
{
	private static readonly string[] Salts = ["QJhwzAtXL", "XzUhGYdFp", "NoCanLook"]; //lol, new salt isn't really being used

	public static List<byte[]> Retrieve(SQLiteConnection connection)
	{
		var combinedSalt = Salts[1] + Salts[2]; //XzUhGYdFpNoCanLook
		var combinedHash = SHA256.HashData(Encoding.ASCII.GetBytes(combinedSalt)).ToHexString(); //8bd007187bc88b3a2e1371b6f5f4fa0719f8b45104841b382b18e671f8ba2057
		var realSalt = combinedHash[11..20]; //88b3a2e13
		var macs = NetworkInterface.GetAllNetworkInterfaces()
			.Where(iface => iface.NetworkInterfaceType is not NetworkInterfaceType.Loopback)
			.Select(iface => iface.GetPhysicalAddress().ToMacString())
			.Distinct()
			.Where(addr => addr is { Length: > 0 })
			.ToList();
		var secrets = macs.Select(mac => realSalt + mac).ToList(); //88b3a2e13FF:FF:FF:FF:FF:FF //should select first active ethernet adapter
		var deviceIds = secrets.Select(secret => SHA256.HashData(Encoding.ASCII.GetBytes(secret)).ToHexString()).ToList();

		var userIds = new List<string>();
		using (var cmd = new SQLiteCommand("select UserID from user", connection))
		using (var reader = cmd.ExecuteReader())
			while (reader.Read())
				userIds.Add((string)reader["UserID"]);

		var result = (
			from userId in userIds
			from deviceId in deviceIds
			select Encoding.UTF8.GetBytes((deviceId + userId).Trim()) into key
			select SHA256.HashData(key) into hash
			select hash[^16..]
		).ToList();

		//var strResult = result.Select(b => b.ToHexString()).Distinct().ToList();
		return result;
	}
}