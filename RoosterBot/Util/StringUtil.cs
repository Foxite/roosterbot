using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
				result = result[trimString.Length..];
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

		private static readonly IDictionary<char, char> UpsideDownVariants =
			@"abcdefghijklmnopqrtuvwy!?'.,/\()<>{}[]123456789".Zip(
			@"68𝘓95ߤ↋↊⇂[]{}<>()\/ʻ.╻¿¡ʎʍʌnʇɹbdouɯʅʞɾᴉɥƃⅎǝpɔqɐ".Reverse()
		).ToDictionary(tuple => tuple.First, tuple => tuple.Second);

		private static readonly IDictionary<char, char> UpsideDownVariants2 =
			@"68𝘓95ߤ↋↊⇂[]{}<>()\/ʻ.╻¿¡ʎʍʌnʇɹbdouɯʅʞɾᴉɥƃⅎǝpɔqɐ".Reverse().Zip(
			@"abcdefghijklmnopqrtuvwy!?'.,/\()<>{}[]123456789"
		).ToDictionary(tuple => tuple.First, tuple => tuple.Second);

		/// <summary>
		/// Makes a string upside-down.
		/// </summary>
		public static string UpsideDown(this string str) {
			var ret = new StringBuilder(str.Length);

			foreach (char c in str.ToLowerInvariant().Reverse()) {
				if (UpsideDownVariants.TryGetValue(c, out char upsideDown)) {
					ret.Append(upsideDown);
				} else if (UpsideDownVariants2.TryGetValue(c, out upsideDown)) {
					ret.Append(upsideDown);
				} else {
					ret.Append(c);
				}
			}

			string v = ret.ToString();
			return v;
		}
	}
}
