using Drm.Format.Epub;

namespace Drm;

public static class SchemeGuesser
{
	public static PrivateKeyScheme Guess(string filePath, BookFormat format)
		=> format switch
		{
			BookFormat.EPub => Epub.GuessScheme(filePath),
			_ => PrivateKeyScheme.Unknown
		};
}