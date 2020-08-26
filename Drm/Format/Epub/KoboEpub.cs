using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Security.Cryptography;
using Ionic.Zip;

namespace Drm.Format.Epub
{
	public class KoboEpub : Epub, IDisposable
	{
		public KoboEpub()
		{
			var dataSource = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @"Kobo\Kobo Desktop Edition\Kobo.sqlite");
			connection = new SQLiteConnection("Data Source=" + dataSource);
			connection.Open();
			MasterKeys = KoboMasterKeys.Retrieve(connection);
		}

		protected override Dictionary<string, (Cipher cipher, byte[] data)> GetSessionKeys(ZipFile zipFile, string originalFilePath)
		{
			var bookId = GetBookId(originalFilePath);
			var encryptedSessionKeys = new Dictionary<string, byte[]>();
			using (var cmd = new SQLiteCommand("select * from content_keys where volumeId='" + bookId + "'", connection))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var elementId = (string)reader["elementId"];
					var elementKey = Convert.FromBase64String((string)reader["elementKey"]);
					encryptedSessionKeys[elementId] = elementKey;
				}
			foreach (var masterKey in MasterKeys)
			{
				var sessionKeys = new Dictionary<string, (Cipher cipher, byte[] data)>();
				foreach (var key in encryptedSessionKeys.Keys)
					sessionKeys[key] = (Cipher.Aes128Ecb, Decryptor.DecryptAes128Ecb(encryptedSessionKeys[key], masterKey, PaddingMode.None));
				if (IsValidDecryptionKey(zipFile, sessionKeys))
					return sessionKeys;
			}

			throw new InvalidOperationException("Couldn't find valid book decryption key.");
		}

		public override string GetFileName(string originalFilePath)
		{
			var bookId = GetBookId(originalFilePath);
			using (var cmd = new SQLiteCommand("select Title, Subtitle from content where ContentID='" + bookId + "'", connection))
			using (var reader = cmd.ExecuteReader())
			{
				if (!reader.Read())
					throw new InvalidOperationException("Couldn't identify book record in local Kobo database.");

				var title = (string)reader[0];
				var subtitle = (string)reader[1];
				if (!string.IsNullOrEmpty(subtitle))
					title = $"{title} - {subtitle}";
				return title + ".epub";
			}
		}

		private Guid GetBookId(string originalFilePath)
		{
			var filename = Path.GetFileNameWithoutExtension(originalFilePath);
			if (Guid.TryParse(filename, out var bookId))
			{
				using (var cmd = new SQLiteCommand("select count(ContentID) from content where ContentID='" + bookId + "'", connection))
				{
					var rows = cmd.ExecuteScalar() as long?;
					if (rows > 0)
						return bookId;
				}
			}
			else
			{
				var filesize = new FileInfo(originalFilePath).Length;
				using (var cmd = new SQLiteCommand("select ContentID, Title from content where ___FileSize=" + filesize, connection))
				using (var reader = cmd.ExecuteReader())
				{
					if (reader.Read())
						return Guid.Parse((string)reader[0]);
				}
			}
			throw new InvalidOperationException("Couldn't identify book record in local Kobo database.");
		}

		private readonly SQLiteConnection connection;
		private readonly List<byte[]> MasterKeys;

		public void Dispose()
		{
			if (connection == null)
				return;

			connection.Close();
			connection.Dispose();
		}
	}
}