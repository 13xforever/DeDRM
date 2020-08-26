using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;

namespace Drm
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

			try
			{
				using (var zip = new ZipFile(filePath, Encoding.UTF8))
				{
					var mime = zip.Entries.FirstOrDefault(e => e.FileName == "mimetype");
					if (mime != null)
						using (var stream = new MemoryStream())
						{
							mime.Extract(stream);
							var mimeStr = Encoding.UTF8.GetString(stream.ToArray());
							if (mimeStr.StartsWith("application/epub+zip"))
								return BookFormat.EPub;
						}
				}
			}
			catch {}
			return BookFormat.Unknown;
		}
	}
}