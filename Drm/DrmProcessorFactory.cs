using System;
using Drm.Format.Epub;

namespace Drm;

public static class DrmProcessorFactory
{
	public static IDrmProcessor Get(BookFormat format, PrivateKeyScheme scheme)
	{
		if (scheme is PrivateKeyScheme.None)
			return PassThrough.Value;

		return format switch
		{
			BookFormat.EPub => scheme switch
			{
				PrivateKeyScheme.Adept => AdeptEPub.Value,
				PrivateKeyScheme.Kobo or PrivateKeyScheme.KoboNone => KoboEPub.Value,
				_ => throw new NotSupportedException("Unsupported combination of book format and DRM scheme.")
			},
			BookFormat.EReader => throw (scheme switch
			{
				_ => new NotSupportedException("Unsupported combination of book format and DRM scheme.")
			}),
			_ => throw new NotSupportedException("Unsupported book format.")
		};
	}

	private static readonly Lazy<IDrmProcessor> PassThrough = new(() => new PassThrough());
	private static readonly Lazy<IDrmProcessor> AdeptEPub = new(() => new AdeptEpub());
	private static readonly Lazy<IDrmProcessor> KoboEPub = new(() => new KoboEpub());
}