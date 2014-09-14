using System;
using System.Collections.Generic;
using Ionic.Zip;

namespace Drm.Format.Epub
{
	public class KoboEpub : Epub
	{
		public KoboEpub()
		{
			
		}

		protected override Dictionary<string, Tuple<Cipher, byte[]>> GetSessionKeys(ZipFile zipFile)
		{
			
		}
	}
}