using System.Collections.Generic;
using RoosterBot;

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

		/// <summary>
		/// Creates a <see cref="IBidirectionalEnumerator{T}"/> over a <see cref="IReadOnlyList{T}"/>.
		/// </summary>
		public static BidirectionalListEnumerator<T> GetBidirectionalEnumerator<T>(this IReadOnlyList<T> list, int position = 0) {
			return new BidirectionalListEnumerator<T>(list, position);
		}
	}
}
