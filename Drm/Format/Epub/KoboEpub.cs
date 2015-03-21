using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
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

		protected override bool IsEncrypted(ZipFile zipFile, string originalFilePath)
		{
			var bookId = GetBookId(originalFilePath);
			using (var cmd = new SQLiteCommand("select IsEncrypted from content where ContentID='" + bookId + "'", connection))
			using (var reader = cmd.ExecuteReader())
			{
				if (!reader.Read())
					throw new InvalidOperationException("Couldn't identify book record in local Kobo database.");
				return reader.GetBoolean(0);
			}
		}


		protected override Dictionary<string, Tuple<Cipher, byte[]>> GetSessionKeys(ZipFile zipFile, string originalFilePath)
		{
			var bookId = GetBookId(originalFilePath);
			var tmp = new Dictionary<string, Tuple<Cipher, byte[]>>();
			using (var cmd = new SQLiteCommand("select * from content_keys where volumeId='" + bookId + "'", connection))
			using (var reader = cmd.ExecuteReader())
				while (reader.Read())
				{
					var elementId = reader["elementId"];
					var elementKey = Convert.FromBase64String(reader["elementKey"] as string);
					tmp[elementId as string] = Tuple.Create(Cipher.Aes128Ecb, elementKey);
				}

			foreach (var masterKey in MasterKeys)
			{
				var result = new Dictionary<string, Tuple<Cipher, byte[]>>();
				foreach (var key in tmp.Keys)
					result[key] = Tuple.Create(tmp[key].Item1, Decryptor.Decrypt(tmp[key].Item2, Cipher.Aes128Ecb, masterKey));
				if (IsValidDecryptionKey(zipFile, result))
					return result;
			}

			throw new InvalidOperationException("Couldn't find valid book decryption key.");
		}

		public override string GetFileName(string originalFilePath)
		{
			var bookId = GetBookId(originalFilePath);
			using (var cmd = new SQLiteCommand("select Title from content where ContentID='" + bookId + "'", connection))
			using (var reader = cmd.ExecuteReader())
			{
				if (!reader.Read())
					throw new InvalidOperationException("Couldn't identify book record in local Kobo database.");
				return reader[0] as string + ".epub";
			}
		}

		private Guid GetBookId(string originalFilePath)
		{
			var filename = Path.GetFileNameWithoutExtension(originalFilePath);
			Guid bookId;
			if (Guid.TryParse(filename, out bookId))
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
						return Guid.Parse(reader[0] as string);
				}
			}
			throw new InvalidOperationException("Couldn't identify book record in local Kobo database.");
		}

		private SQLiteConnection connection;
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