using System.Collections.Generic;

namespace System.Linq {
	public static class LinqExtensions {
		/// <summary>
		/// This will enumerate <paramref name="source"/> and determine if the count is equal to <paramref name="count"/>.
		/// It will stop as soon as it finds an item beyond the target count, which makes it faster than <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		/// <remarks>
		/// If <paramref name="source"/> implements <see cref="ICollection{T}"/>, then <see cref="ICollection{T}.Count"/> will be used and <paramref name="source"/>
		/// will not be enumerated.
		/// </remarks>
		public static bool CountEquals<T>(this IEnumerable<T> source, int count) {
			if (source is ICollection<T> ico) {
				return ico.Count == count;
			}

			int i = 0;
			foreach (T _ in source) {
				i++;
				if (i > count) {
					// There were more than {count}
					return false;
				}
			}
			if (i == count) {
				// There were exactly {count}
				return true;
			} else {
				// There were less then {count}
				return false;
			}
		}
		
		/// <summary>
		/// This will enumerate <paramref name="source"/> and determine if the count is greater than or equal to <paramref name="count"/>.
		/// It will stop as soon as it finds an item beyond the target count, which makes it faster than <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		/// <remarks>
		/// If <paramref name="source"/> implements <see cref="ICollection{T}"/>, then <see cref="ICollection{T}.Count"/> will be used and <paramref name="source"/>
		/// will not be enumerated.
		/// </remarks>
		public static bool CountIsGreaterThanOrEquals<T>(this IEnumerable<T> source, int count) {
			if (source is ICollection<T> ico) {
				return ico.Count >= count;
			}

			int i = 0;
			foreach (T _ in source) {
				i++;
				if (i >= count) {
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// This will enumerate <paramref name="source"/> and determine if the count is greater than <paramref name="count"/>.
		/// It will stop as soon as it finds an item beyond the target count, which makes it faster than <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		/// <remarks>
		/// If <paramref name="source"/> implements <see cref="ICollection{T}"/>, then <see cref="ICollection{T}.Count"/> will be used and <paramref name="source"/>
		/// will not be enumerated.
		/// </remarks>
		public static bool CountIsGreaterThan<T>(this IEnumerable<T> source, int count) {
			if (source is ICollection<T> ico) {
				return ico.Count > count;
			}

			int i = 0;
			foreach (T _ in source) {
				i++;
				if (i > count) {
					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// This will enumerate <paramref name="source"/> and determine if the count is less than or equal to <paramref name="count"/>.
		/// It will stop as soon as it finds an item beyond the target count, which makes it faster than <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		/// <remarks>
		/// If <paramref name="source"/> implements <see cref="ICollection{T}"/>, then <see cref="ICollection{T}.Count"/> will be used and <paramref name="source"/>
		/// will not be enumerated.
		/// </remarks>
		public static bool CountIsLessThanOrEquals<T>(this IEnumerable<T> source, int count) {
			if (source is ICollection<T> ico) {
				return ico.Count <= count;
			}

			int i = 0;
			foreach (T _ in source) {
				i++;
				if (i > count) {
					return false;
				}
			}
			return true;
		}
		
		/// <summary>
		/// This will enumerate <paramref name="source"/> and determine if the count is less than <paramref name="count"/>.
		/// It will stop as soon as it finds an item beyond the target count, which makes it faster than <see cref="Enumerable.Count{TSource}(IEnumerable{TSource})"/>
		/// </summary>
		/// <remarks>
		/// If <paramref name="source"/> implements <see cref="ICollection{T}"/>, then <see cref="ICollection{T}.Count"/> will be used and <paramref name="source"/>
		/// will not be enumerated.
		/// </remarks>
		public static bool CountIsLessThan<T>(this IEnumerable<T> source, int count) {
			if (source is ICollection<T> ico) {
				return ico.Count < count;
			}

			int i = 0;
			foreach (T _ in source) {
				i++;
				if (i >= count) {
					return false;
				}
			}
			return true;
		}

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
		/// Returns all <see cref="LinkedListNode{T}"/>s in a <see cref="LinkedList{T}"/>, in reversed order, as opposed to all <typeparamref name="T"/>.
		/// </summary>
		/// <remarks>
		/// This is much faster than <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> as that function will enumerate the entire source and store the results,
		/// and then yielding that in reverse. This function yields <see cref="LinkedList{T}.Last"/> and then yields that node's <see cref="LinkedListNode{T}.Previous"/>
		/// until there are no more items.
		/// </remarks>
		public static IEnumerable<LinkedListNode<T>> GetNodesBackwards<T>(this LinkedList<T> list) {
			if (list.Count > 0) {
				LinkedListNode<T>? node = list.Last;
				while (node != null) {
					yield return node;
					node = node.Previous;
				}
			}
		}

		/// <summary>
		/// Returns all <see cref="LinkedListNode{T}"/>s in a <see cref="LinkedList{T}"/>, in reversed order, as opposed to all <typeparamref name="T"/>.
		/// </summary>
		/// <remarks>
		/// This is much faster than <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> as that function will enumerate the entire source and store the results,
		/// and then yielding that in reverse. This function yields <see cref="LinkedList{T}.Last"/> and then yields the previous node's <see cref="LinkedListNode{T}.Previous"/>
		/// until there are no more items.
		/// 
		/// This function is not called Reverse to avoid naming conflicts with the aforementioned function.
		/// </remarks>
		public static IEnumerable<LinkedListNode<T>> Backwards<T>(this LinkedList<T> list) {
			if (list.Count > 0) {
				LinkedListNode<T>? node = list.Last;
				while (node != null) {
					yield return node;
					node = node.Previous;
				}
			}
		}

		/// <summary>
		/// Enumerates all items in an <see cref="IList{T}"/> in reverse order.
		/// </summary>
		/// <remarks>
		/// This is much faster than <see cref="Enumerable.Reverse{TSource}(IEnumerable{TSource})"/> as that function will enumerate the entire source and store the results,
		/// and then yielding that in reverse. This function utilizes a reverse for loop over the IList<T>.
		/// 
		/// This function is not called Reverse to avoid naming conflicts with the aforementioned function.
		/// </remarks>
		public static IEnumerable<T> Backwards<T>(this IList<T> list) {
			for (int i = list.Count - 1; i >= 0; i--) {
				yield return list[i];
			}
		}

		/// <summary>
		/// Adds all items that match a predicate into a separate IEnumerable<T>, and returns all items that did not pass the predicate.
		/// </summary>
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
