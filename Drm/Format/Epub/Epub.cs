using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Drm.Utils;
using Ionic.Zip;
using Ionic.Zlib;

namespace Drm.Format.Epub
{
	public abstract class Epub: IDrmProcessor
	{
		public static event Action<string> OnParseIssue;

		public byte[] Strip(byte[] bookData, string originalFilePath)
		{
			using (var bookStream = new MemoryStream(bookData))
			using (var zip = ZipFile.Read(bookStream, new ReadOptions {Encoding = Encoding.UTF8}))
			{
				if (!IsEncrypted(zip, originalFilePath))
					return bookData;

				var sessionKeys = GetSessionKeys(zip, originalFilePath);
				return Strip(zip, sessionKeys);
			}
		}

		public virtual string GetFileName(string originalFilePath) { return Path.GetFileName(originalFilePath); }

		public byte[] Strip(ZipFile zip, Dictionary<string, Tuple<Cipher, byte[]>> sessionKeys)
		{
			IEnumerable<ZipEntry> entriesToDecrypt = zip.Entries.Where(e => !META_NAMES.Contains(e.FileName));
			using (var output = new ZipFile(Encoding.UTF8))
			{
				output.UseZip64WhenSaving = Zip64Option.Never;
				output.CompressionLevel = CompressionLevel.None;
				using (var s = new MemoryStream())
				{
					zip["mimetype"].Extract(s);
					output.AddEntry("mimetype", s.ToArray());
				}
				output.CompressionLevel = CompressionLevel.BestCompression; //some files, like jpgs and mp3s will be stored anyway
				foreach (var file in entriesToDecrypt)
				{
					byte[] data;
					using (var s = new MemoryStream())
					{
						file.Extract(s);
						data = s.ToArray();
					}
					if (sessionKeys.ContainsKey(file.FileName))
					{
						data = Decryptor.Decrypt(data, sessionKeys[file.FileName].Item1, sessionKeys[file.FileName].Item2);
					}
					output.AddEntry(file.FileName, data);
				}
				using (var result = new MemoryStream())
				{
					output.Save(result);
					return result.ToArray();
				}
			}
		}

		internal static PrivateKeyScheme GuessScheme(string filePath)
		{
			try
			{
				using (var zip = new ZipFile(filePath))
				{
					var rights = zip.Entries.FirstOrDefault(e => e.FileName.EndsWith("rights.xml"));
					if (rights == null)
					{
						Guid guid;
						if (Guid.TryParse(Path.GetFileNameWithoutExtension(filePath), out guid))
							return PrivateKeyScheme.KoboNone;
						return PrivateKeyScheme.None;
					}

					using (var stream = new MemoryStream())
					{
						rights.Extract(stream);
						stream.Seek(0, SeekOrigin.Begin);
						var xml = XDocument.Load(stream);
						if (xml.Root.Name == "kdrm")
							return PrivateKeyScheme.Kobo;
						if (xml.Root.Name.Namespace == "http://ns.adobe.com/adept")
							return PrivateKeyScheme.Adept;
						return PrivateKeyScheme.Unknown;
					}
				}
			}
			catch
			{
				return PrivateKeyScheme.Unknown;
			}
		}

		protected bool IsValidDecryptionKey(ZipFile zip, Dictionary<string, Tuple<Cipher, byte[]>> encryptedEntries)
		{
			return IsValidDecryptionKey(zip, encryptedEntries, JpgExt, new byte[] {0xff, 0xd8, 0xff}) ||
					IsValidDecryptionKey(zip, encryptedEntries, PngExt, new byte[] {0x89, 0x50, 0x4e, 0x47}) ||
					IsValidDecryptionKey(zip, encryptedEntries, HtmExt, "<html");
		}

		private bool IsValidDecryptionKey(ZipFile zip, Dictionary<string, Tuple<Cipher, byte[]>> encryptedEntries, string[] extensions, byte[] signature)
		{
			var file = encryptedEntries.Keys.FirstOrDefault(e => extensions.Contains(Path.GetExtension(e).ToUpper()));
			if (file == null) return false;

			using (var stream = new MemoryStream())
			{
				zip[file].Extract(stream);
				var content = Decryptor.Decrypt(stream.ToArray(), encryptedEntries[file].Item1, encryptedEntries[file].Item2);
				return content.StartsWith(signature);
			}
		}

		private bool IsValidDecryptionKey(ZipFile zip, Dictionary<string, Tuple<Cipher, byte[]>> encryptedEntries, string[] extensions, string substr)
		{
			var file = encryptedEntries.Keys.FirstOrDefault(e => extensions.Contains(Path.GetExtension(e).ToUpper()));
			if (file == null) return false;

			using (var stream = new MemoryStream())
			{
				zip[file].Extract(stream);
				var content = Decryptor.Decrypt(stream.ToArray(), encryptedEntries[file].Item1, encryptedEntries[file].Item2);
				var text = Encoding.ASCII.GetString(content).ToUpper();
				return text.Contains(substr.ToUpper());
			}
		}

		protected abstract Dictionary<string, Tuple<Cipher, byte[]>> GetSessionKeys(ZipFile zipFile, string originalFilePath);

		protected virtual bool IsEncrypted(ZipFile zipFile, string originalFilePath)
		{
			return (zipFile["META-INF/rights.xml"] ?? zipFile["rights.xml"]) != null;
		}

		private static readonly HashSet<string> META_NAMES = new HashSet<string> {"mimetype", "rights.xml", "META-INF/rights.xml", "META-INF/encryption.xml" };
		private static readonly string[] JpgExt = {".JPG", ".JPEG"};
		private static readonly string[] PngExt = {".PNG"};
		private static readonly string[] HtmExt = {".HTML", ".HTM", ".XHTML"};
	}
}