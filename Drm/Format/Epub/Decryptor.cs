using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Ionic.Zlib;

namespace Drm.Format.Epub
{
	internal static class Decryptor
	{
		public static byte[] Decrypt(byte[]data, Cipher cipher, byte[] key)
		{
			switch (cipher)
			{
				case Cipher.Aes128CbcWithGzip:
					return DecryptAes128CbcWithGzip(data, key);
				case Cipher.Aes128Ecb:
					return DecryptAes128Ecb(data, key);
				default:
					throw new NotSupportedException();
			}
		}

		private static byte[] DecryptAes128CbcWithGzip(byte[] data, byte[] key)
		{
			byte[] gzippedFile;
			using (var cipher = new AesManaged { Mode = CipherMode.CBC, Key = key })
				gzippedFile = cipher.CreateDecryptor().TransformFinalBlock(data, 0, data.Length).Skip(16).ToArray();
			using (var inStream = new MemoryStream(gzippedFile))
			using (var zipStream = new DeflateStream(inStream, CompressionMode.Decompress))
			using (var outStream = new MemoryStream())
			{
				zipStream.CopyTo(outStream);
				return outStream.ToArray();
			}
		}

		private static byte[] DecryptAes128Ecb(byte[] data, byte[] key)
		{
			using (var cipher = new AesManaged {Mode = CipherMode.ECB, Key = key, Padding = PaddingMode.None})
				return cipher.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
		}
	}
}