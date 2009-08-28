using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.XPath;
using Ionic.Zip;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;

namespace Drm.Adept
{
	public static class Epub
	{
		public static void Strip(byte[] key, string ebookPath, string output)
		{
			RsaEngine rsa = GetRsaEngine(key);
			using (var zip = new ZipFile(ebookPath))
			{
				var metaNames = zip.Entries.Where(e => META_NAMES.Contains(e.FileName));
				if (metaNames.Count() != META_NAMES.Count)
					throw new ArgumentException("Not an ADEPT ePub.", "ebookPath");
				var entriesToDecrypt = zip.Entries.Except(metaNames);

				XPathNavigator navigator;
				using (var s = new MemoryStream())
				{
					ZipEntry rightsEntry = zip.Entries.Where(ze => ze.FileName == "META-INF/rights.xml").First();
					rightsEntry.Extract(s);
					s.Seek(0, SeekOrigin.Begin);
					navigator = new XPathDocument(s).CreateNavigator();
				}
				var nsm = new XmlNamespaceManager(navigator.NameTable);
				nsm.AddNamespace("a", NSMAP["adept"]);
				var node = navigator.SelectSingleNode("//a:encryptedKey[1]", nsm);
				if (node == null) throw new InvalidOperationException("Can't find ebook encryption key.");
				string base64Key = node.Value;
				var contentKey = Convert.FromBase64String(base64Key);
				contentKey = rsa.ProcessBlock(contentKey, 0, contentKey.Length); //\x02j\x92\xd1r`\xf0\t\xfd\x...hY\xba\xa0\xfc\x82\xd8q\xcf<
			}
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
		private static readonly Dictionary<string, string> NSMAP = new Dictionary<string, string>
			{
				{"adept", "http://ns.adobe.com/adept"},
				{"enc", "http://www.w3.org/2001/04/xmlenc#"}
			};
	}
}