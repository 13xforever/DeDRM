using System.Collections.Generic;
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
		}

		private static RsaEngine GetRsaEngine(byte[] key)
		{
			var reader = new Asn1InputStream(key);
			var o = (Asn1Sequence)reader.ReadObject();
			BigInteger n = ((DerInteger)o[1]).Value;
			BigInteger e = ((DerInteger)o[2]).Value;
			BigInteger d = ((DerInteger)o[3]).Value;
			BigInteger p = ((DerInteger)o[4]).Value;
			BigInteger q = ((DerInteger)o[5]).Value;
			BigInteger dP = ((DerInteger)o[6]).Value;
			BigInteger dQ = ((DerInteger)o[7]).Value;
			BigInteger qInv = ((DerInteger)o[8]).Value;
			var rsa = new RsaEngine();
			rsa.Init(false, new RsaPrivateCrtKeyParameters(n, e, d, p, q, dP, dQ, qInv));
			return rsa;
		}

		private static readonly List<string> META_NAMES = new List<string> {"mimetype", "META-INF/rights.xml", "META-INF/encryption.xml"};
		private static readonly Dictionary<string, string> NSMAP = new Dictionary<string, string>
			{
				{"adept", "http://ns.adobe.com/adept"},
				{"enc", "http://www.w3.org/2001/04/xmlenc#"}
			};
	}
}