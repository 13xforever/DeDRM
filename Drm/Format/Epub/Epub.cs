using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zip;
using Ionic.Zlib;

namespace Drm.Format.Epub
{
	public abstract class Epub: IDrmProcessor
	{
		public static event Action<string> OnParseIssue;

		public byte[] Strip(byte[] bookData)
		{
			using (var bookStream = new MemoryStream(bookData))
			using (var zip = ZipFile.Read(bookStream, new ReadOptions {Encoding = Encoding.UTF8}))
			{
				var sessionKeys = GetSessionKeys(zip);
				return Strip(zip, sessionKeys);
			}
		}

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
						byte[] gzippedFile;
						using (var cipher = new AesManaged {Mode = CipherMode.CBC, Key = sessionKeys[file.FileName].Item2})
							gzippedFile = cipher.CreateDecryptor().TransformFinalBlock(data, 0, data.Length).Skip(16).ToArray();
						using (var inStream = new MemoryStream(gzippedFile))
						using (var zipStream = new DeflateStream(inStream, CompressionMode.Decompress))
						using (var outStream = new MemoryStream())
						{
							zipStream.CopyTo(outStream);
							if (outStream.Length == 0 && OnParseIssue != null)
								OnParseIssue(string.Format("Warning! Decompression failed for '{0}'", file.FileName));
							data = outStream.ToArray();
						}
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

		protected abstract Dictionary<string, Tuple<Cipher, byte[]>> GetSessionKeys(ZipFile zipFile);

		private static readonly HashSet<string> META_NAMES = new HashSet<string> {"mimetype", "rights.xml", "META-INF/rights.xml", "META-INF/encryption.xml" };
	}
}