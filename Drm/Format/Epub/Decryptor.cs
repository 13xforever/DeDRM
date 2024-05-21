using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Ionic.Zlib;

namespace Drm.Format.Epub;

internal static class Decryptor
{
	public static byte[] Decrypt(byte[]data, Cipher cipher, byte[] key)
		=> cipher switch
		{
			Cipher.Aes128CbcWithGzip => DecryptAes128CbcWithGzip(data, key),
			Cipher.Aes128Ecb => DecryptAes128Ecb(data, key),
			_ => throw new NotSupportedException()
		};

	private static byte[] DecryptAes128CbcWithGzip(byte[] data, byte[] key)
	{
		using var cipher = Aes.Create();
		cipher.Mode = CipherMode.CBC;
		var gzippedFile = cipher.CreateDecryptor(key, null).TransformFinalBlock(data, 0, data.Length)[16..];
		using var inStream = new MemoryStream(gzippedFile);
		using var zipStream = new DeflateStream(inStream, CompressionMode.Decompress);
		using var outStream = new MemoryStream();
		zipStream.CopyTo(outStream);
		return outStream.ToArray();
	}

	internal static byte[] DecryptAes128Ecb(byte[] data, byte[] key, PaddingMode paddingMode = PaddingMode.PKCS7)
	{
		using var cipher = Aes.Create();
		cipher.Mode = CipherMode.ECB;
		cipher.Padding = paddingMode;
		return cipher.CreateDecryptor(key, null).TransformFinalBlock(data, 0, data.Length);
	}
}