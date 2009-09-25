using System;
using System.IO;

namespace Drm.Utils
{
	public static class StreamUtils
	{
		public static void CopyTo(this Stream src, Stream dest)
		{
			int size = (src.CanSeek) ? Math.Min((int)(src.Length - src.Position), 0x2000) : 0x2000;
			var buffer = new byte[size];
			int n;
			do
			{
				n = src.Read(buffer, 0, buffer.Length);
				dest.Write(buffer, 0, n);
			} while (n != 0);
		}

		public static void CopyTo(this MemoryStream src, Stream dest)
		{
			dest.Write(src.GetBuffer(), (int)src.Position, (int)(src.Length - src.Position));
		}

		public static void CopyTo(this Stream src, MemoryStream dest)
		{
			if (src.CanSeek)
			{
				var pos = (int)dest.Position;
				int length = (int)(src.Length - src.Position) + pos;
				dest.SetLength(length);
				while (pos < length) pos += src.Read(dest.GetBuffer(), pos, length - pos);
			}
			else
				src.CopyTo((Stream)dest);
		}
	}
}