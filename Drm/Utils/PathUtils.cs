using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Drm.Utils;

public static class PathUtils
{
	private static readonly HashSet<char> InvalidChars = [..Path.GetInvalidFileNameChars()];
	
	public static string ReplaceInvalidChars(this string filename)
		=> string.Create(filename.Length, filename,
			(span, fname) =>
			{
				for (var i = 0; i < fname.Length; i++)
				{
					var c = fname[i];
					span[i] = InvalidChars.Contains(c) ? '_' : c;
				}	
			}
		);
}