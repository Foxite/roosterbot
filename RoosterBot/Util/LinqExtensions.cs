using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	public static class LinqExtensions {
		public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> enumerable) where T : class {
			foreach (T? item in enumerable) {
				if (!(item is null)) {
					yield return item;
				}
			}
		}
		
		/// <summary>
		/// Returns the params list as an IEnumerable.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="items"></param>
		/// <returns></returns>
		public static IEnumerable<T> Pack<T>(params T[] items) {
			for (int i = 0; i < items.Length; i++) {
				yield return items[i];
			}
		}

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
	}
}
