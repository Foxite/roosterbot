using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// Provide informational texts to users.
	/// </summary>
	public sealed class HelpService {
		private readonly List<HelpSection> m_Sections;
		private readonly ResourceService m_Resources;

		internal HelpService(ResourceService resources) {
			m_Sections = new List<HelpSection>();
			m_Resources = resources;
		}

		/// <summary>
		/// Add a text section to the help service.
		/// </summary>
		public void AddHelpSection(Component component, string name, string text) {
			m_Sections.Add(new HelpSection(component, name, text));
		}

		/// <summary>
		/// Get a help section.
		/// </summary>
		public string GetHelpSection(CultureInfo culture, string name) {
			HelpSection helpSection = GetSection(culture, name);
			if (helpSection != null) {
				return m_Resources.ResolveString(culture, helpSection.Component, GetSection(culture, name).HelpText);
			} else {
				throw new InvalidOperationException($"No help section is known as {name} in {culture.Name}");
			}
		}

		/// <summary>
		/// Check if a help section exists.
		/// </summary>
		public bool HelpSectionExists(CultureInfo culture, string name) {
			HelpSection section = GetSection(culture, name);
			if (section != null) {
				return !section.Cultures.Any() || section.Cultures.Contains(culture);
			} else {
				return false;
			}
		}

		/// <summary>
		/// Get the names of all help sections for a given language.
		/// </summary>
		/// <param name="culture"></param>
		/// <returns></returns>
		public string[] GetSectionNames(CultureInfo culture) {
			return m_Sections
				.Where(section => section.Cultures.Count == 0 ||  section.Cultures.Contains(culture))
				.Select(section => m_Resources.ResolveString(culture, section.Component, section.Name))
				.ToArray();
		}

		private HelpSection GetSection(CultureInfo culture, string name) {
			return m_Sections.FirstOrDefault(thisSection =>
				(thisSection.Cultures.Count == 0 || thisSection.Cultures.Contains(culture)) &&
				m_Resources.ResolveString(culture, thisSection.Component, thisSection.Name) == name);
		}

		private class HelpSection {
			public Component Component { get; }
			public string Name { get; }
			public string HelpText { get; }
			public IReadOnlyCollection<CultureInfo> Cultures { get; }

			internal HelpSection(Component component, string Name, string helpText) {
				Component = component;
				this.Name = Name;
				HelpText = helpText;

				Cultures = ResourceService.GetAvailableCultures(component);
			}

		}
	}
}
