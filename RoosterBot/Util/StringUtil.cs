using System;

namespace RoosterBot {
	/// <summary>
	/// A static class containing several functions to manipulate strings or string arrays.
	/// </summary>
	public static class StringUtil {
		/// <summary>
		/// This trim all instances of <paramref name="trimString"/> from <paramref name="target"/> from the end of <paramref name="target"/>. It will not touch any instances of
		/// <paramref name="trimString"/> that do not occur at the end of <paramref name="target"/>.
		/// </summary>
		/// <seealso cref="TrimEnd(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
		public static ReadOnlySpan<char> TrimStart(this ReadOnlySpan<char> target, ReadOnlySpan<char> trimString) {
			if (trimString.IsEmpty) return target;

			ReadOnlySpan<char> result = target;
			while (result.StartsWith(trimString)) {
				result = result.Slice(trimString.Length);
			}

			return result;
		}

		/// <summary>
		/// This trim all instances of <paramref name="trimString"/> from <paramref name="target"/> from the end of <paramref name="target"/>. It will not touch any instances of
		/// <paramref name="trimString"/> that do not occur at the end of <paramref name="target"/>.
		/// </summary>
		/// <seealso cref="TrimStart(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
		public static ReadOnlySpan<char> TrimEnd(this ReadOnlySpan<char> target, ReadOnlySpan<char> trimString) {
			if (trimString.IsEmpty) return target;

			ReadOnlySpan<char> result = target;
			while (result.EndsWith(trimString)) {
				result = result.Slice(0, result.Length - trimString.Length);
			}

			return result;
		}
	}
}
