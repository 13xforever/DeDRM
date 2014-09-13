using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.XPath;
using Drm.Utils;
using Ionic.Zip;
using Ionic.Zlib;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Drm.Adept
{
	public static class Epub
	{
		public static ProcessResult Strip(string ebookPath, string output)
		{
			return Strip(KeyRetriever.Retrieve(), ebookPath, output);
		}

		public static event Action<string> OnParseIssue;

		public static ProcessResult Strip(List<byte[]> keys, string ebookPath, string outputPath)
		{
			for (var i = 0; i < keys.Count; i++)
			{
				try
				{
					if (Strip(keys[i], ebookPath, outputPath)) return ProcessResult.Success;
				}
				catch(InvalidOperationException)
				{
					Console.WriteLine("Decryption failed for {0} with key #{1} out of {2}.", ebookPath, i + 1, keys.Count);
				}
			}
			return ProcessResult.Fail;
		}

		public static bool Strip(byte[] key, string ebookPath, string outputPath)
		{
			bool result = false;
			RsaEngine rsa = GetRsaEngine(key);
			using (var zip = new ZipFile(ebookPath, Encoding.UTF8))
			{
				IEnumerable<ZipEntry> metaNames = zip.Entries.Where(e => META_NAMES.Contains(e.FileName));
				if (metaNames.Count() != META_NAMES.Count) throw new ArgumentException("Not an ADEPT ePub.", "ebookPath");
				IEnumerable<ZipEntry> entriesToDecrypt = zip.Entries.Except(metaNames);

				XPathNavigator navigator;
				using (var s = new MemoryStream())
				{
					zip["META-INF/rights.xml"].Extract(s);
					s.Seek(0, SeekOrigin.Begin);
					navigator = new XPathDocument(s).CreateNavigator();
				}
				var nsm = new XmlNamespaceManager(navigator.NameTable);
				nsm.AddNamespace("a", "http://ns.adobe.com/adept");
				nsm.AddNamespace("e", "http://www.w3.org/2001/04/xmlenc#");
				XPathNavigator node = navigator.SelectSingleNode("//a:encryptedKey[1]", nsm);
				if (node == null) throw new InvalidOperationException("Can't find session key.");
				string base64Key = node.Value;
				byte[] contentKey = Convert.FromBase64String(base64Key);
				byte[] bookkey = rsa.ProcessBlock(contentKey, 0, contentKey.Length);
				//Padded as per RSAES-PKCS1-v1_5
				if (bookkey[bookkey.Length - 17] != 0x00) throw new InvalidOperationException("Problem decrypting session key");
				bookkey = bookkey.Skip(bookkey.Length - 16).ToArray();

				using (var s = new MemoryStream())
				{
					zip["META-INF/encryption.xml"].Extract(s);
					s.Seek(0, SeekOrigin.Begin);
					navigator = new XPathDocument(s).CreateNavigator();
				}
				XPathNodeIterator contentLinks = navigator.Select("//e:EncryptedData", nsm);
				var encryptedEntryies = new Dictionary<string, string>(contentLinks.Count);
				foreach (XPathNavigator link in contentLinks)
				{
					string em = link.SelectSingleNode("./e:EncryptionMethod/@Algorithm", nsm).Value;
					string path = link.SelectSingleNode("./e:CipherData/e:CipherReference/@URI", nsm).Value;
					encryptedEntryies[path] = em;
				}
				string[] unknownAlgos = encryptedEntryies.Values.Where(ns => ns != "http://www.w3.org/2001/04/xmlenc#aes128-cbc").Distinct().ToArray();
				if (unknownAlgos.Length > 0)
					throw new InvalidOperationException("This ebook uses unsupported encryption method(s): " + string.Join(", ", unknownAlgos));

				using (var cipher = new AesManaged {Mode = CipherMode.CBC, Key = bookkey})
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
						if (encryptedEntryies.ContainsKey(file.FileName))
						{
							byte[] gzippedFile = cipher.CreateDecryptor().TransformFinalBlock(data, 0, data.Length).Skip(16).ToArray();
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
					output.Save(outputPath);
					result = true;
				}
			}
			return result;
		}

		private static RsaEngine GetRsaEngine(byte[] key)
		{
			//Org.BouncyCastle.Security.PrivateKeyFactory.CreateKey()
			var rsaPrivateKey = (Asn1Sequence)new Asn1InputStream(key).ReadObject();

			//http://tools.ietf.org/html/rfc3447#page-60
			//version = rsaPrivateKey[0]
			BigInteger n = ((DerInteger)rsaPrivateKey[1]).Value;
			BigInteger e = ((DerInteger)rsaPrivateKey[2]).Value;
			BigInteger d = ((DerInteger)rsaPrivateKey[3]).Value;
			BigInteger p = ((DerInteger)rsaPrivateKey[4]).Value;
			BigInteger q = ((DerInteger)rsaPrivateKey[5]).Value;
			BigInteger dP = ((DerInteger)rsaPrivateKey[6]).Value;
			BigInteger dQ = ((DerInteger)rsaPrivateKey[7]).Value;
			BigInteger qInv = ((DerInteger)rsaPrivateKey[8]).Value;
			var rsa = new RsaEngine();
			rsa.Init(false, new RsaPrivateCrtKeyParameters(n, e, d, p, q, dP, dQ, qInv));
			return rsa;
		}

		private static readonly HashSet<string> META_NAMES = new HashSet<string> {"mimetype", "META-INF/rights.xml", "META-INF/encryption.xml"};
	}
}