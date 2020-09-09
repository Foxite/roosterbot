using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace RoosterBot {
	/// <summary>
	/// Allows you to look up the name of one language in another language.
	/// </summary>
	public sealed class CultureNameService {
		private const string KeyPrefix = "CultureName_";
		private readonly ResourceService m_Resources;

		internal CultureNameService(ResourceService resources) {
			m_Resources = resources;
		}

		/// <summary>
		/// Returns a CultureInfo, which is named similar to <paramref name="input"/> in <paramref name="culture"/>.
		/// </summary>
		/// <example>
		/// Search(CultureInfo.GetCultureInfo("es-ES", "inglés").Name == "en-US"
		/// </example>
		public CultureInfo? Search(CultureInfo? culture, string input) {
			input = input.ToLower();
			IEnumerable<KeyValuePair<string, string>> enumerable = m_Resources.GetAvailableKeys(Assembly.GetExecutingAssembly(), culture);
			string? resultCode = enumerable
				.Where(kvp => culture is null || kvp.Key.StartsWith(KeyPrefix))
				.FirstOrDefault(kvp => kvp.Value.ToLower() == input)
				.Key?.Substring(KeyPrefix.Length);
			return resultCode != null ? CultureInfo.GetCultureInfo(resultCode) : null;
		}

		/// <summary>
		/// Get the name of one language in another language.
		/// </summary>
		public string GetLocalizedName(CultureInfo nameOf, CultureInfo inLanguage) {
			try {
				return m_Resources.GetString(inLanguage, KeyPrefix + nameOf.Name);
			} catch (MissingResourceException e) {
				throw new ArgumentException($"The name of {nameOf.Name} in {inLanguage.Name} is not known.", e);
			}
		}
	}
}
