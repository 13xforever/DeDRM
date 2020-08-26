using System;
using System.Linq;
using System.Text;

namespace Drm.Utils
{
	public static class ArrayUtils
	{
		public static T[] Copy<T>(this T[] source, long from, long length)
		{
			if (from < 0)
				throw new ArgumentOutOfRangeException(nameof(@from));

			if ((from + length) > source.LongLength)
				throw new ArgumentOutOfRangeException(nameof(length));

			var result = new T[length];
			Array.Copy(source, from, result, 0, length);
			return result;
		}

		public static T[] Copy<T>(this T[] source, long from)
		{
			if (from < 0)
				from = source.LongLength + from;

			if (from < 0 || from > source.LongLength)
				throw new ArgumentOutOfRangeException(nameof(@from));

			var length = source.LongLength - from;
			var result = new T[length];
			Array.Copy(source, from, result, 0, length);
			return result;
		}

		/// <summary>
		/// Copies right-opened subrange of array to a new array and returns it as a result
		/// </summary>
		/// <typeparam name="T">Type of array</typeparam>
		/// <param name="source">Source array</param>
		/// <param name="from">Start index in <paramref name="source"/>, which will be copied to resulting array</param>
		/// <param name="to">Opened end index in <paramref name="source"/>, which will <b>not</b> be copied to resulting array</param>
		/// <returns>New array, containing subrange of <paramref name="source"/>, including element at <paramref name="from"/>, but excluding element <paramref name="to"/></returns>
		public static T[] SubRange<T>(this T[] source, long from, long to)
		{
			if (from < 0)
				from = source.LongLength + from;
			if (to < 0)
				to = source.LongLength + to;

			if (from < 0)
				throw new ArgumentOutOfRangeException(nameof(@from));

			if (to > source.LongLength)
				throw new ArgumentOutOfRangeException(nameof(to));

			if (from > to)
				throw new ArgumentException("Left border cannot be higher than Right border.", nameof(@from));

			var length = to - from;
			var result = new T[length];
			Array.Copy(source, from, result, 0, length);
			return result;
		}

		public static T[] Reverse<T>(this T[] source)
		{
			var result = new T[source.LongLength];
			for (var i = 0; i < source.LongLength; i++)
				result[i] = source[source.LongLength - i - 1];
			return result;
		}

		public static string ToHexString(this byte[] source)
		{
			if (source == null)
				return null;

			if (source.Length == 0)
				return "";

			var result = new StringBuilder(source.Length);
			foreach (var b in source)
				result.Append(b.ToString("x2"));
			return result.ToString();
		}

		public static bool StartsWith(this byte[] source, byte[] pattern)
		{
			if (source == null || pattern == null)
				return false;

			if (pattern.Length == 0)
				return false;

			if (source.Length < pattern.Length)
				return false;

			return source.Copy(0, pattern.Length).SequenceEqual(pattern);
		}
	}
}