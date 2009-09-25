using System;

namespace Drm.Utils
{
	public static class ArrayUtils
	{
		public static T[] Copy<T>(this T[] source, long from, long length)
		{
			if (from < 0) throw new ArgumentOutOfRangeException("from");
			if ((from + length) > source.LongLength) throw new ArgumentOutOfRangeException("length");

			var result = new T[length];
			Array.Copy(source, from, result, 0, length);
			return result;
		}

		public static T[] Copy<T>(this T[] source, long from)
		{
			if (from < 0) from = source.LongLength + from;

			if (from < 0 || from > source.LongLength) throw new ArgumentOutOfRangeException("from");

			long length = source.LongLength - from;
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
			if (from < 0) from = source.LongLength + from;
			if (to < 0) to = source.LongLength + to;

			if (from < 0) throw new ArgumentOutOfRangeException("from");
			if (to > source.LongLength) throw new ArgumentOutOfRangeException("to");
			if (from > to) throw new ArgumentException("Left border cannot be higher than Right border.", "from");

			long length = to - from;
			var result = new T[length];
			Array.Copy(source, from, result, 0, length);
			return result;
		}

		public static T[] Reverse<T>(this T[] source)
		{
			var result = new T[source.LongLength];
			for (int i = 0; i < source.LongLength; i++) result[i] = source[source.LongLength - i - 1];
			return result;
		}
	}
}