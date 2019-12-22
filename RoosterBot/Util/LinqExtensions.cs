using System;
using System.Collections.Generic;
using System.Linq;

namespace System.Linq {
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
		/// Returns all <see cref="LinkedListNode{T}"/>s in a <see cref="LinkedList{T}"/>, as opposed to all <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="list"></param>
		/// <returns></returns>
		public static IEnumerable<LinkedListNode<T>> GetNodes<T>(this LinkedList<T> list) {
			if (list.Count > 0) {
				LinkedListNode<T>? node = list.First;
				while (node != null) {
					yield return node;
					node = node.Next;
				}
			}
		}

		/// <summary>
		/// Adds all items that match a predicate into a separate IEnumerable<T>, and returns all items that did not pass the predicate.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="source"></param>
		/// <param name="moveInto"></param>
		/// <param name="predicate"></param>
		/// <returns></returns>
		public static IEnumerable<T> Divide<T>(this IEnumerable<T> source, out IEnumerable<T> moveInto, Func<T, bool> predicate) {
			var outA = new List<T>();
			var outB = new List<T>();

			foreach (T item in source) {
				List<T> outInto = predicate(item) ? outB : outA;
				outInto.Add(item);
			}
			moveInto = outB;
			return outA;
		}

		/// <summary>
		/// Does effectively the same as enumerable.ToArray().CopyTo(...), but does not convert the enumerable to an array.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="enumerable"></param>
		/// <param name="array"></param>
		/// <param name="index"></param>
		/// <param name="amount"></param>
		public static void CopyTo<T>(this IEnumerable<T> enumerable, T[] array, int index) {
			int i = index;
			foreach (T item in enumerable) {
				array[i] = item;
				i++;
			}
		}

		/// <summary>
		/// Enumerates all duplicate items in an enumeration.
		/// If a duplicate occurs more than twice in an enumeration it will be yielded one time less than the amount of times it occurs in the source enumeration.
		/// For example, a duplicate occuring 4 times will be yielded 3 times.
		/// You can combine this with .Distinct() to avoid this.
		/// </summary>
		/// <param name="equalityComparer">Uses <see cref="EqualityComparer{T}.Default"/> if null.</param>
		public static IEnumerable<T> Duplicates<T>(this IEnumerable<T> enumerable, IEqualityComparer<T>? equalityComparer = null) {
			var d = new HashSet<T>(equalityComparer ?? EqualityComparer<T>.Default);
			foreach (var t in enumerable) {
				if (!d.Add(t)) {
					yield return t;
				}
			}
		}

		/// <summary>
		/// Enumerates all duplicate items in an enumerable.
		/// If a duplicate occurs more than twice in an enumeration it will be yielded one time less than the amount of times it occurs in the source enumeration.
		/// For example, a duplicate occuring 4 times will be yielded 3 times.
		/// You can combine this with .Distinct() to avoid this.
		/// </summary>
		/// <param name="equalityComparer">Uses <see cref="EqualityComparer{T}.Default"/> if null.</param>
		/// <param name="selector">Determine equality based on this selector.</param>
		public static IEnumerable<TSource> Duplicates<TSource, TValue>(this IEnumerable<TSource> enumerable, Func<TSource, TValue> selector, IEqualityComparer<TValue>? equalityComparer = null) {
			var d = new HashSet<TValue>(equalityComparer ?? EqualityComparer<TValue>.Default);
			foreach (var t in enumerable) {
				if (!d.Add(selector(t))) {
					yield return t;
				}
			}
		}
	}
}
