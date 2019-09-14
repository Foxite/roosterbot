using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	public class HelpService {
		private Dictionary<string, (ComponentBase, string)> m_ComponentHelpTexts;

		internal HelpService() {
			m_ComponentHelpTexts = new Dictionary<string, (ComponentBase, string)>();
		}

		public void AddHelpSection(ComponentBase component, string name, string text) {
			m_ComponentHelpTexts.Add(name, (component, text));
		}

		public (ComponentBase, string) GetHelpSection(string name) {
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
