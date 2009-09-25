using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Drm.Utils
{
	public static class EnumerableUtils
	{

		/// <summary>
		/// Checks whether a collection is the same as another collection
		/// </summary>
		/// <param name="value">The current instance object</param>
		/// <param name="compareList">The collection to compare with</param>
		/// <param name="comparer">The comparer object to use to compare each item in the collection.  If null uses EqualityComparer(T).Default</param>
		/// <returns>True if the two collections contain all the same items in the same order</returns>
		public static bool IsEqualTo<TSource>(this IEnumerable<TSource> value, IEnumerable<TSource> compareList, IEqualityComparer<TSource> comparer)
		{
			if (value == compareList)
				return true;
			else if (value == null || compareList == null)
				return false;
			else
			{
				if (comparer == null)
					comparer = EqualityComparer<TSource>.Default;

				IEnumerator<TSource> enumerator1 = value.GetEnumerator();
				IEnumerator<TSource> enumerator2 = compareList.GetEnumerator();

				bool enum1HasValue = enumerator1.MoveNext();
				bool enum2HasValue = enumerator2.MoveNext();

				try
				{
					while (enum1HasValue && enum2HasValue)
					{
						if (!comparer.Equals(enumerator1.Current, enumerator2.Current))
							return false;

						enum1HasValue = enumerator1.MoveNext();
						enum2HasValue = enumerator2.MoveNext();
					}

					return !(enum1HasValue || enum2HasValue);
				}
				finally
				{
					if (enumerator1 != null) enumerator1.Dispose();
					if (enumerator2 != null) enumerator2.Dispose();
				}
			}
		}

		/// <summary>
		/// Checks whether a collection is the same as another collection
		/// </summary>
		/// <param name="value">The current instance object</param>
		/// <param name="compareList">The collection to compare with</param>
		/// <returns>True if the two collections contain all the same items in the same order</returns>
		public static bool IsEqualTo<TSource>(this IEnumerable<TSource> value, IEnumerable<TSource> compareList)
		{
			return IsEqualTo(value, compareList, null);
		}

		/// <summary>
		/// Checks whether a collection is the same as another collection
		/// </summary>
		/// <param name="value">The current instance object</param>
		/// <param name="compareList">The collection to compare with</param>
		/// <returns>True if the two collections contain all the same items in the same order</returns>
		public static bool IsEqualTo(this IEnumerable value, IEnumerable compareList)
		{
			return IsEqualTo(value.OfType<object>(), compareList.OfType<object>());
		}
	}
}