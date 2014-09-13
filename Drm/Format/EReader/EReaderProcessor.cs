using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Drm.Utils;
using Ionic.Crc;
using Ionic.Zlib;

namespace Drm.Format.EReader
{
	public class EReaderProcessor
	{
		private EReaderProcessor(Pdb pdb, string name, string ccNumber)
		{
			if (!(pdb.Filetype == "PNRd" && pdb.Creator == "PPrs")) throw new FormatException("Invalid eReader file.");
			pdbReader = pdb;
			var eReaderPdb = new EReaderPdb(pdbReader);
			if (!eReaderPdb.HaveDrm)
				throw new InvalidOperationException("File doesn't have DRM or have unknown DRM version: {0}!");
			if (!(eReaderPdb.CompressionMethod == EReaderCompression.Drm1 || eReaderPdb.CompressionMethod == EReaderCompression.Drm2))
				throw new InvalidOperationException(string.Format("Unsupported compression method or DRM version: {0}!", eReaderPdb.CompressionMethod));

			data = pdbReader.GetSection(1);
			ICryptoTransform desEngine = GetDesEngine(data.Copy(0, 8));
			byte[] decryptedData = desEngine.TransformFinalBlock(data.Copy(-8), 0, 8);
			int cookieShuf = decryptedData[0] << 24 | decryptedData[1] << 16 | decryptedData[2] << 8 | decryptedData[3];
			int cookieSize = decryptedData[4] << 24 | decryptedData[5] << 16 | decryptedData[6] << 8 | decryptedData[7];
			if (cookieShuf < 0x03 || cookieShuf > 0x14 || cookieSize < 0xf0 || cookieSize > 0x200) throw new InvalidOperationException("Unsupportd eReader format");
			byte[] input = desEngine.TransformFinalBlock(data.Copy(-cookieSize), 0, cookieSize);
			byte[] r = UnshuffData(input.SubRange(0, -8), cookieShuf);
			//using (var stream = new FileStream(pdb.Filename+".eReaderSection1Dump", FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) stream.Write(r, 0, r.Length);
			byte[] userKeyPart1 = Encoding.ASCII.GetBytes(FixUserName(name));
			byte[] userKeyPart2 = Encoding.ASCII.GetBytes(ccNumber.ToCharArray().Copy(-8));
			long userKey;
			using (var stream1 = new MemoryStream(userKeyPart1))
			using (var stream2 = new MemoryStream(userKeyPart2))
			{
				userKey = new CRC32().GetCrc32(stream1) & 0xffffffff;
				userKey = userKey << 32 | new CRC32().GetCrc32(stream2) & 0xffffffff;
			}
			var drmSubVersion = (ushort)(r[0] << 8 | r[1]);
			numTextPages = (ushort)(r[2] << 8 | r[3]) - 1;
			flags = r[4] << 24 | r[5] << 16 | r[6] << 8 | r[7];
			firstImagePage = (ushort)(r[24] << 8 | r[25]);
			numImagePages = (ushort)(r[26] << 8 | r[27]);
			if ((flags & ReqdFlags) != ReqdFlags) throw new InvalidOperationException(string.Format("Unsupported flags combination: {0:x8}", flags));
			byte[] userKeyArray = BitConverter.GetBytes(userKey);
			if (BitConverter.IsLittleEndian) userKeyArray = userKeyArray.Reverse();
			desEngine = GetDesEngine(userKeyArray);
			byte[] encryptedKey = new byte[0],
			       encryptedKeySha = new byte[0];
			if (eReaderPdb.CompressionMethod == EReaderCompression.Drm1)
			{
				if (drmSubVersion != 13) throw new InvalidOperationException(string.Format("Unknown eReader DRM subversion ID: {0}", drmSubVersion));
				encryptedKey = r.Copy(44, 8);
				encryptedKeySha = r.Copy(52, 20);
			}
			else if (eReaderPdb.CompressionMethod == EReaderCompression.Drm2)
			{
				encryptedKey = r.Copy(172, 8);
				encryptedKeySha = r.Copy(56, 20);
			}
			contentKey = desEngine.TransformFinalBlock(encryptedKey, 0, encryptedKey.Length);
			byte[] checkHash = SHA1.Create().ComputeHash(contentKey);
			if (!encryptedKeySha.IsEqualTo(checkHash))
			{
				var s = new StringBuilder();
				for (var x = 0; x < (r.Length - 8); x += 2)
				{
					for (var y = 0; y < (x - 20); y += 2) if (TestKeyDecryption(desEngine, r, x, y)) s.AppendFormat("keyOffset={0}, hashOffset={1}\n", x, y);
					for (var y = x + 8; y < (r.Length - 20); y += 2) if (TestKeyDecryption(desEngine, r, x, y)) s.AppendFormat("keyOffset={0}, hashOffset={1}\n", x, y);
				}
				if (s.Length > 0) throw new InvalidDataException("Key and/or KeyHash offset mismatch. Possible values:\n\n" + s);
				throw new ArgumentException("Incorrect Name of Credit Card number.");
			}
			contentDecryptor = GetDesEngine(contentKey);
		}

		private static bool TestKeyDecryption(ICryptoTransform desEngine, byte[] r, int keyOffset, int hashOffset)
		{
			var testKey = desEngine.TransformFinalBlock(r.Copy(keyOffset, 8), 0, 8);
			var testHash = r.Copy(hashOffset, 20);
			return SHA1.Create().ComputeHash(testKey).IsEqualTo(testHash);
		}

		public static void Strip(string ebookPath, string name, string ccNumber)
		{
			Strip(ebookPath, null, name, ccNumber);
		}
		public static void Strip(string ebookPath, string outputDir, string name, string ccNumber)
		{
			if (string.IsNullOrEmpty(outputDir)) outputDir = Path.GetDirectoryName(ebookPath);
			var ebook = new Pdb(ebookPath);

			var processor = new EReaderProcessor(ebook, name, ccNumber);
			outputDir = Path.Combine(outputDir, ebook.Filename);
			if (!Directory.Exists(outputDir)) Directory.CreateDirectory(outputDir);
			string path;
			for (int i = 0; i < processor.numImagePages; i++)
			{
				EReaderImageInfo img = processor.GetImage(i);
				path = Path.Combine(outputDir, img.filename);
				using (FileStream stream = File.Create(path)) stream.Write(img.content, 0, img.content.Length);
			}
			string pml = processor.GetText();
			path = Path.Combine(outputDir, ebook.Filename + ".pml");
			using (StreamWriter stream = File.CreateText(path)) stream.Write(pml);
		}

		private EReaderImageInfo GetImage(int imageNumber)
		{
			byte[] sect = pdbReader.GetSection(firstImagePage + imageNumber);
			string name = Encoding.ASCII.GetString(sect.Skip(4).TakeWhile(b => b > 0).ToArray());
			byte[] content = sect.Copy(62);
			return new EReaderImageInfo(name, content);
		}

		private string GetText()
		{
			var r = new StringBuilder(numTextPages);
			for (int i = 1; i <= numTextPages; i++)
			{
				byte[] encryptedSection = pdbReader.GetSection(i);
				byte[] decryptedSection = contentDecryptor.TransformFinalBlock(encryptedSection, 0, encryptedSection.Length);
				byte[] decompressedSection;
				using (var inStream = new MemoryStream(decryptedSection))
				using (var zipStream = new ZlibStream(inStream, CompressionMode.Decompress))
				using (var outStream = new MemoryStream())
				{
					zipStream.CopyTo(outStream);
					decompressedSection = outStream.ToArray();
				}
				string text = Encoding.GetEncoding(1252).GetString(decompressedSection);
				r.Append(text);
			}
			return r.ToString();
		}

		private static ICryptoTransform GetDesEngine(byte[] key)
		{
			byte[] decryptionKey = FixBytes(key);
			DES des = DES.Create();
			des.Mode = CipherMode.ECB;
			des.Padding = PaddingMode.None;
			des.BlockSize = 8 * 8;
			des.KeySize = 8 * 8;
			return des.CreateDecryptor(decryptionKey, new byte[8]);
		}

		private static byte[] FixBytes(byte[] key)
		{
			for (int i = 0; i < key.Length; i++)
				key[i] = (byte)(key[i] ^ ((key[i] ^ (key[i] << 1) ^ (key[i] << 2) ^ (key[i] << 3) ^ (key[i] << 4) ^ (key[i] << 5) ^ (key[i] << 6) ^ (key[i] << 7) ^ 0x80) & 0x80));
			return key;
		}

		private static byte[] UnshuffData(byte[] data, int shuf)
		{
			var r = new byte[data.Length];
			long j = 0;
			for (int i = 0; i < data.Length; i++)
			{
				j = (j + shuf) % data.Length;
				r[j] = data[i];
			}
			return r;
		}

		private static string FixUserName(string name)
		{
			var r = new StringBuilder();
			foreach (var c in name.ToLower()) if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9')) r.Append(c);
			return r.ToString();
		}

		private readonly Pdb pdbReader;
		private readonly byte[] data;
		private readonly int numTextPages;
		private readonly int numImagePages;
		private readonly int firstImagePage;
		private readonly int flags;
		private readonly byte[] contentKey;
		private readonly ICryptoTransform contentDecryptor;
		private const int ReqdFlags = (1 << 7) | (1 << 9) | (1 << 10);
	}
}