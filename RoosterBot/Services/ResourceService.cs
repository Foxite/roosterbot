using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using Discord.Commands;

namespace RoosterBot {
	public sealed class ResourceService {
		private Dictionary<Assembly, ResourceManager> m_ResourceManagers;
		private GuildCultureService m_GCS;

		internal ResourceService(GuildCultureService gcs) {
			m_ResourceManagers = new Dictionary<Assembly, ResourceManager>();
			m_GCS = gcs;
		}

		public void RegisterResources(string baseName) {
			Assembly assembly = Assembly.GetCallingAssembly();
			m_ResourceManagers[assembly] = new ResourceManager(baseName, assembly);
		}
		
		public string GetString(CultureInfo culture, string name) {
			return m_ResourceManagers[Assembly.GetCallingAssembly()].GetString(name, culture);
		}

		public string GetString(ICommandContext context, string name) {
			return m_ResourceManagers[Assembly.GetCallingAssembly()].GetString(name, m_GCS.GetCultureForGuild(context.Guild));
		}

		public string GetString(Assembly assembly, CultureInfo culture, string name) {
			return m_ResourceManagers[assembly].GetString(name, culture);
		}

		public string ResolveString(ICommandContext context, ComponentBase component, string str) {
			return ResolveString(m_GCS.GetCultureForGuild(context.Guild), component, str);
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
