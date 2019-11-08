using System;
using System.Collections.Generic;
using System.Globalization;

namespace RoosterBot {
	public class CultureNameService {
		/// <summary>
		/// Effectively a table, this stores the name of one language (the first, identified by code) in another (the second, also code).
		/// </summary>
		private Dictionary<(string, string), string> m_Table;

		public CultureNameService() {
			m_Table = new Dictionary<(string, string), string>();
		}

		public string GetLocalizedName(string nameOf, string inLanguage) {
			m_Table.TryGetValue((nameOf, inLanguage), out string? ret);
			return ret ?? throw new ArgumentException($"The name of {nameOf} in {inLanguage} is not known.");
		}

		public bool AddLocalizedName(string nameOf, string inLanguage, string name) {
			(string, string) key = (nameOf, inLanguage);
			if (m_Table.ContainsKey(key)) {
				return false;
			} else {
				m_Table[key] = name;
				return true;
			}
		}

		public string GetLocalizedName(CultureInfo nameOf, CultureInfo inLanguage) => GetLocalizedName(nameOf.Name, inLanguage.Name);
		public bool AddLocalizedName(CultureInfo nameOf, CultureInfo inLanguage, string name) => AddLocalizedName(nameOf.Name, inLanguage.Name, name);
	}
}
