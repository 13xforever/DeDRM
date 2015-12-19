using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Drm.Format.EReader
{
	internal class EReaderImageInfo
	{
		public EReaderImageInfo(string filename, byte[] content)
		{
			this.filename = SanitizeFilename(filename);
			this.content = content;
		}

		public readonly string filename;
		public readonly byte[] content;

		private static string SanitizeFilename(string name)
		{
			var r = new StringBuilder(name.Length);
			foreach (var c in name.ToLower()) if (!invalidChars.Contains(c)) r.Append(c);
			return r.ToString();
		}

		private static readonly HashSet<char> invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
	}
}