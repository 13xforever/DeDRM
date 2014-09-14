using System;
using Drm.Format.Epub;

namespace Drm
{
	public static class DrmProcessorFactory
	{
		public static IDrmProcessor Get(BookFormat format, PrivateKeyScheme scheme)
		{
			if (scheme == PrivateKeyScheme.None)
				return PassThrough.Value;

			switch (format)
			{
				case BookFormat.EPub:
					switch (scheme)
					{
						case PrivateKeyScheme.Adept:
							return AdeptEPub.Value;
						case PrivateKeyScheme.Kobo:
							return KoboEPub.Value;
						default:
							throw new NotSupportedException("Unsupported combination of book format and DRM scheme.");
					}
				case BookFormat.EReader:
					switch (scheme)
					{
						default:
							throw new NotSupportedException("Unsupported combination of book format and DRM scheme.");
					}
				default:
					throw new NotSupportedException("Unsupported book format.");
			}
		}

		private static readonly Lazy<IDrmProcessor> PassThrough = new Lazy<IDrmProcessor>(() => new PassThrough());
		private static readonly Lazy<IDrmProcessor> AdeptEPub = new Lazy<IDrmProcessor>(() => new AdeptEpub());
		private static readonly Lazy<IDrmProcessor> KoboEPub = new Lazy<IDrmProcessor>(() => new KoboEpub());
	}
}