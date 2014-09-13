using System;

namespace Drm
{
	public interface IDrmProcessor
	{
		byte[] Strip(byte[] bookData);
	}

	public static class DrmProcessor
	{
		public static IDrmProcessor Get(BookFormat format, PrivateKeyScheme scheme)
		{
			throw new NotImplementedException();
		}
	}
}