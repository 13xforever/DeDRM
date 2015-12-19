using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Drm.Utils
{
	public static class PathUtils
	{
		public static string ReplaceInvalidChars(this string filename)
		{
			var result = new StringBuilder(filename.Length);
			var invalidChars = new HashSet<char>(Path.GetInvalidFileNameChars());
			foreach (var c in filename)
				result.Append(invalidChars.Contains(c) ? '_' : c);
			return result.ToString();
		}
	}
}