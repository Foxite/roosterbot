using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// Allows you to look up the name of one language in another language.
	/// </summary>
	public sealed class CultureNameService {
		/// <summary>
		/// Effectively a table, this stores the name of one language (the first, identified by code) in another (the second, also code).
		/// </summary>
		/// <example>
		/// m_Table[("en-US", "es-ES")] == "inglés" // The name of the English language in Spanish
		/// m_Table[("nl-NL", "en-US")] == "Dutch" // The name of Dutch in English
		/// </example>
		private readonly Dictionary<(string, string), string> m_Table;

		internal CultureNameService() {
			m_Table = new Dictionary<(string, string), string>();
		}

		/// <summary>
		/// Get the name of one language in another language.
		/// </summary>
		/// <example>
		/// <code>GetLocalizedName("en-US", "en-ES") == "inglés"; // The name of the English language in Spanish</code>
		/// <code>GetLocalizedName("nl-NL", "en-US") == "Dutch"; // The name of Dutch in English</code>
		/// </example>
		public string GetLocalizedName(string nameOf, string inLanguage) {
			m_Table.TryGetValue((nameOf, inLanguage), out string? ret);
			return ret ?? throw new ArgumentException($"The name of {nameOf} in {inLanguage} is not known.");
		}

		/// <summary>
		/// Register the name of one language in another.
		/// </summary>
		public bool AddLocalizedName(string nameOf, string inLanguage, string name) {
			(string, string) key = (nameOf, inLanguage);
			if (m_Table.ContainsKey(key)) {
				return false;
			} else {
				m_Table[key] = name;
				return true;
			}
		}

		/// <summary>
		/// Returns the code of a CultureInfo, which is named similar <paramref name="input"/> in <paramref name="inputLanguage"/>.
		/// </summary>
		/// <param name="inputLanguage"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public string? Search(CultureInfo inputLanguage, string input) {
			input = input.ToLower();
			var results = m_Table.Where(kvp => kvp.Key.Item2 == inputLanguage.Name && kvp.Value.ToLower() == input);
			if (results.Any()) {
				return results.First().Key.Item1;
			} else {
				return null;
			}
		}

		/// <summary>
		/// Get the name of one language in another language.
		/// </summary>
		/// <seealso cref="GetLocalizedName(string, string)"/>
		public string GetLocalizedName(CultureInfo nameOf, CultureInfo inLanguage) => GetLocalizedName(nameOf.Name, inLanguage.Name);

		/// <summary>
		/// Register the name of one language in another.
		/// </summary>
		/// <seealso cref="AddLocalizedName(string, string, string)"/>.
		public bool AddLocalizedName(CultureInfo nameOf, CultureInfo inLanguage, string name) => AddLocalizedName(nameOf.Name, inLanguage.Name, name);
	}
}
