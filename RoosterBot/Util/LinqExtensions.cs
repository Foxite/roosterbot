using System.Collections.Generic;

namespace System.Linq {
	/// <summary>
	/// Adds several extensions methods to <see cref="IEnumerable{T}"/>.
	/// </summary>
	public static class LinqExtensions {
		/// <summary>
		/// Yields all items in the source enumeration that are not <see langword="null"/>. This function offers null safety when using C# 8.0 nullable reference types.
		/// </summary>
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class {
			foreach (T? item in enumerable) {
				if (!(item is null)) {
					yield return item;
				}
			}
		}

		/// <summary>
		/// Yields all items in the source enumeration that have a value. This function offers safety when using <see cref="Nullable{T}"/>
		/// </summary>
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : struct {
			foreach (T? item in enumerable) {
				if (item.HasValue) {
					yield return item.Value;
				}
			}
		}

		public static TSource MaxBy<TSource, TSelect>(this IEnumerable<TSource> enumerable, Func<TSource, TSelect> selector) where TSelect : IComparable<TSelect> {
			using var enumerator = enumerable.GetEnumerator();
			if (!enumerator.MoveNext()) {
				throw new InvalidOperationException("Sequence is empty.");
			}
			TSource max = enumerator.Current;
			while (enumerator.MoveNext()) {
				if (selector(enumerator.Current).CompareTo(selector(enumerator.Current)) > 0) {
					max = enumerator.Current;
				}
			}
			return max;
		}

		public static TSource MinBy<TSource, TSelect>(this IEnumerable<TSource> enumerable, Func<TSource, TSelect> selector) where TSelect : IComparable<TSelect> {
			using var enumerator = enumerable.GetEnumerator();
			if (!enumerator.MoveNext()) {
				throw new InvalidOperationException("Sequence is empty.");
			}
			TSource min = enumerator.Current;
			while (enumerator.MoveNext()) {
				if (selector(enumerator.Current).CompareTo(selector(enumerator.Current)) < 0) {
					min = enumerator.Current;
				}
			}
			return min;
		}
	}
}
