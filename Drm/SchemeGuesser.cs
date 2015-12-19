using Drm.Format.Epub;

namespace Drm
{
	public static class SchemeGuesser
	{
		public static PrivateKeyScheme Guess(string filePath, BookFormat format)
		{
			switch (format)
			{
				case BookFormat.EPub:
					return Epub.GuessScheme(filePath);
				default:
					return PrivateKeyScheme.Unknown;
			}
		}
	}
}