﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoosterBot {
	public sealed class HelpService {
		private List<HelpSection> m_Sections;
		private ResourceService m_Resources;

		internal HelpService(ResourceService resources) {
			m_Sections = new List<HelpSection>();
			m_Resources = resources;
		}

		public void AddHelpSection(ComponentBase component, string name, string text, params CultureInfo[] restrict) {
			m_Sections.Add(new HelpSection(component, name, text, restrict));
		}

		public string GetHelpSection(CultureInfo culture, string name) {
			HelpSection helpSection = GetSection(culture, name);
			if (helpSection != null) {
				return m_Resources.ResolveString(culture, helpSection.Component, GetSection(culture, name).HelpText);
			} else {
				throw new InvalidOperationException($"No help section is known as {name} in {culture.Name}");
			}
		}

		public bool HelpSectionExists(CultureInfo culture, string name) {
			HelpSection section = GetSection(culture, name);
			if (section != null) {
				return section.Cultures.Contains(culture);
			} else {
				return false;
			}
		}

		public string[] GetSectionNames(CultureInfo culture) {
			return m_Sections
				.Where(section => section.Cultures.Count == 0 ||  section.Cultures.Contains(culture))
				.Select(section => m_Resources.ResolveString(culture, section.Component, section.Name))
				.ToArray();
		}

		private HelpSection GetSection(CultureInfo culture, string name) {
			return m_Sections.First(thisSection =>
				(thisSection.Cultures.Count == 0 || thisSection.Cultures.Contains(culture)) &&
				m_Resources.ResolveString(culture, thisSection.Component, thisSection.Name) == name);
		}

		private class HelpSection {
			public ComponentBase Component { get; }
			public string Name { get; }
			public string HelpText { get; }
			public IReadOnlyCollection<CultureInfo> Cultures { get; }

			internal HelpSection(ComponentBase component, string Name, string helpText, IReadOnlyCollection<CultureInfo> cultures) {
				Component = component;
				this.Name = Name;
				HelpText = helpText;
				Cultures = cultures;
			}
		}
	}
}
