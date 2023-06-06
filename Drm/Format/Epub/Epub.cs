using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Drm.Utils;
using Ionic.Zip;
using Ionic.Zlib;

namespace Drm.Format.Epub;

public abstract class Epub: IDrmProcessor
{
	public static event Action<string> OnParseIssue;

	public byte[] Strip(byte[] bookData, string originalFilePath)
	{
		using var bookStream = new MemoryStream(bookData);
		using var zip = ZipFile.Read(bookStream, new() {Encoding = Encoding.UTF8});
		if (!IsEncrypted(zip, originalFilePath))
			return bookData;

		var sessionKeys = GetSessionKeys(zip, originalFilePath);
		return Strip(zip, sessionKeys);
	}

	public virtual string GetFileName(string originalFilePath) => Path.GetFileName(originalFilePath);

	private byte[] Strip(ZipFile zip, Dictionary<string, (Cipher cipher, byte[] data)> sessionKeys)
	{
		var entriesToDecrypt = zip.Entries.Where(e => !META_NAMES.Contains(e.FileName));
		using var output = new ZipFile(Encoding.UTF8);
		output.UseZip64WhenSaving = Zip64Option.Never;
		output.CompressionLevel = CompressionLevel.None;
		using (var s = new MemoryStream())
		{
			zip["mimetype"].Extract(s);
			output.AddEntry("mimetype", s.ToArray());
		}
		foreach (var file in entriesToDecrypt)
		{
			byte[] data;
			using (var s = new MemoryStream())
			{
				file.Extract(s);
				data = s.ToArray();
			}
			if (sessionKeys.ContainsKey(file.FileName))
				data = Decryptor.Decrypt(data, sessionKeys[file.FileName].Item1, sessionKeys[file.FileName].Item2);
			var ext = Path.GetExtension(file.FileName);
			output.CompressionLevel = UncompressibleExts.Contains(ext) ? CompressionLevel.None : CompressionLevel.BestCompression;
			output.AddEntry(file.FileName, data);
		}
		using (var result = new MemoryStream())
		{
			output.Save(result);
			return result.ToArray();
		}
	}

	internal static PrivateKeyScheme GuessScheme(string filePath)
	{
		try
		{
			using var zip = new ZipFile(filePath);
			var rights = zip.Entries.FirstOrDefault(e => e.FileName.EndsWith("rights.xml"));
			if (rights is null)
			{
				if (Guid.TryParse(Path.GetFileNameWithoutExtension(filePath), out _))
					return PrivateKeyScheme.KoboNone;
				return PrivateKeyScheme.None;
			}

			using var stream = new MemoryStream();
			rights.Extract(stream);
			stream.Seek(0, SeekOrigin.Begin);
			var xml = XDocument.Load(stream);
			if (xml.Root.Name.LocalName is "kdrm")
				return PrivateKeyScheme.Kobo;
			
			if (xml.Root.Name.Namespace == "http://ns.adobe.com/adept")
				return PrivateKeyScheme.Adept;
			return PrivateKeyScheme.Unknown;
		}
		catch
		{
			return PrivateKeyScheme.Unknown;
		}
	}

	protected bool IsValidDecryptionKey(ZipFile zip, Dictionary<string, (Cipher cipher, byte[] data)> encryptedEntries)
	{
		return IsValidDecryptionKey(zip, encryptedEntries, JpgExt, new byte[] {0xff, 0xd8, 0xff}) ||
		       IsValidDecryptionKey(zip, encryptedEntries, PngExt, new byte[] {0x89, 0x50, 0x4e, 0x47}) ||
		       IsValidDecryptionKey(zip, encryptedEntries, HtmExt, "<html");
	}

	private bool IsValidDecryptionKey(ZipFile zip, Dictionary<string, (Cipher cipher, byte[] data)> encryptedEntries, string[] extensions, byte[] signature)
	{
		var file = encryptedEntries.Keys.FirstOrDefault(e => extensions.Contains(Path.GetExtension(e).ToUpper()));
		if (file is null)
			return false;

		using var stream = new MemoryStream();
		try
		{
			zip[file].Extract(stream);
			var content = Decryptor.Decrypt(stream.ToArray(), encryptedEntries[file].cipher, encryptedEntries[file].data);
			return content.StartsWith(signature);
		}
		catch
		{
			return false;
		}
	}

	private bool IsValidDecryptionKey(ZipFile zip, Dictionary<string, (Cipher cipher, byte[] data)> encryptedEntries, string[] extensions, string substr)
	{
		var file = encryptedEntries.Keys.FirstOrDefault(e => extensions.Contains(Path.GetExtension(e).ToUpper()));
		if (file is null)
			return false;

		using var stream = new MemoryStream();
		try
		{
			zip[file].Extract(stream);
			var content = Decryptor.Decrypt(stream.ToArray(), encryptedEntries[file].Item1, encryptedEntries[file].Item2);
			var text = Encoding.ASCII.GetString(content).ToUpper();
			return text.Contains(substr.ToUpper());
		}
		catch
		{
			return false;
		}
	}

	protected abstract Dictionary<string, (Cipher cipher, byte[] data)> GetSessionKeys(ZipFile zipFile, string originalFilePath);

	protected virtual bool IsEncrypted(ZipFile zipFile, string originalFilePath)
		=> (zipFile["META-INF/rights.xml"] ?? zipFile["rights.xml"]) is not null;

	private static readonly HashSet<string> META_NAMES = new() {"mimetype", "rights.xml", "META-INF/rights.xml", "META-INF/encryption.xml" };
	private static readonly string[] JpgExt = {".JPG", ".JPEG"};
	private static readonly string[] PngExt = {".PNG"};
	private static readonly string[] HtmExt = {".HTML", ".HTM", ".XHTML"};
	private static readonly HashSet<string> UncompressibleExts = new(StringComparer.InvariantCultureIgnoreCase)
	{
		".jpg",
		".jpeg",
		".png",
	};
}