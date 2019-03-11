using System.Collections.Generic;
using System.Linq;

namespace RoosterBot.Services {
	public class HelpService {
		private Dictionary<string, string> m_ComponentHelpTexts;

		public HelpService() {
			m_ComponentHelpTexts = new Dictionary<string, string>();
		}

		public void AddHelpSection(string name, string text) {
			m_ComponentHelpTexts.Add(name, text);
		}

		public string GetHelpSection(string name) {
			return m_ComponentHelpTexts[name];
		}

		public bool HelpSectionExists(string name) {
			return m_ComponentHelpTexts.ContainsKey(name);
		}

		public string[] GetSectionNames() {
			return m_ComponentHelpTexts.Keys.ToArray();
		}
	}
}
