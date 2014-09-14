using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.XPath;
using Ionic.Zip;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Drm.Format.Epub
{
	public class AdeptEpub : Epub
	{
		private List<byte[]> MasterKeys;

		public AdeptEpub() { MasterKeys = Adept.Retrieve(); }

		protected override Dictionary<string, Tuple<Cipher, byte[]>> GetSessionKeys(ZipFile zipFile)
		{
			XPathNavigator navigator;
			using (var s = new MemoryStream())
			{
				zipFile["META-INF/rights.xml"].Extract(s);
				s.Seek(0, SeekOrigin.Begin);
				navigator = new XPathDocument(s).CreateNavigator();
			}
			var nsm = new XmlNamespaceManager(navigator.NameTable);
			nsm.AddNamespace("a", "http://ns.adobe.com/adept");
			nsm.AddNamespace("e", "http://www.w3.org/2001/04/xmlenc#");
			XPathNavigator node = navigator.SelectSingleNode("//a:encryptedKey[1]", nsm);
			if (node == null)
				throw new InvalidOperationException("Can't find session key.");

			string base64Key = node.Value;
			byte[] contentKey = Convert.FromBase64String(base64Key);
			byte[] bookkey = null;
			foreach (var masterKey in MasterKeys)
			{
				var rsa = GetRsaEngine(masterKey);
				rsa.ProcessBlock(contentKey, 0, contentKey.Length);
				//Padded as per RSAES-PKCS1-v1_5
				if (bookkey[bookkey.Length - 17] != 0x00)
					bookkey = null;
			}
			if (bookkey == null)
				throw new InvalidOperationException("Problem decrypting session key");

			bookkey = bookkey.Skip(bookkey.Length - 16).ToArray();
			using (var s = new MemoryStream())
			{
				zipFile["META-INF/encryption.xml"].Extract(s);
				s.Seek(0, SeekOrigin.Begin);
				navigator = new XPathDocument(s).CreateNavigator();
			}
			XPathNodeIterator contentLinks = navigator.Select("//e:EncryptedData", nsm);
			var result = new Dictionary<string, Tuple<Cipher, byte[]>>(contentLinks.Count);
			foreach (XPathNavigator link in contentLinks)
			{
				string em = link.SelectSingleNode("./e:EncryptionMethod/@Algorithm", nsm).Value;
				string path = link.SelectSingleNode("./e:CipherData/e:CipherReference/@URI", nsm).Value;
				var cipher = GetCipher(em);
				if (cipher == Cipher.Unknown)
					throw new InvalidOperationException("This ebook uses unsupported encryption method: " + em);

				result[path] = Tuple.Create(cipher, bookkey);
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

		private Cipher GetCipher(string ns)
		{
			if (ns == "http://www.w3.org/2001/04/xmlenc#aes128-cbc")
				return Cipher.Aes128Cbc;
			else
				return Cipher.Unknown;
		}

	}
}