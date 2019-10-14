using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Discord.Commands;

namespace RoosterBot {
	public sealed class ResourceService {
		private Dictionary<Assembly, ResourceManager> m_ResourceManagers;

		internal ResourceService() {
			m_ResourceManagers = new Dictionary<Assembly, ResourceManager>();
		}

		public void RegisterResources(string baseName) {
			Assembly assembly = Assembly.GetCallingAssembly();
			m_ResourceManagers[assembly] = new ResourceManager(baseName, assembly);
		}
		
		public string GetString(CultureInfo culture, string name) {
			return m_ResourceManagers[Assembly.GetCallingAssembly()].GetString(name, culture);
		}

		public string GetString(Assembly assembly, CultureInfo culture, string name) {
			return m_ResourceManagers[assembly].GetString(name, culture);
		}

		public string ResolveString(CultureInfo culture, ComponentBase component, string str) {
			if (str.StartsWith("#")) {
				Assembly assembly = null;
				if (component == null) {
					assembly = Assembly.GetExecutingAssembly();
				} else {
					assembly = component.GetType().Assembly;
				}
				return GetString(assembly, culture, str.Substring(1));
			} else {
				return str;
			}
		}
	}
}
