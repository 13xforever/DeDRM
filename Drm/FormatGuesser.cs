using System.IO;

namespace Drm.Format
{
	public static class FormatGuesser
	{
		public static BookFormat Guess(string filePath)
		{
			var ext = (Path.GetExtension(filePath) ?? "").ToLower();
			if (ext == ".pdb")
				return BookFormat.EReader;
			if (ext == ".epub" || ext == ".kepub")
				return BookFormat.EPub;
			return BookFormat.Unknown;
		}
	}
}