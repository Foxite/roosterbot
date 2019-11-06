using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;

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
			return m_ResourceManagers[Assembly.GetCallingAssembly()].GetString(name, culture)
				?? throw new MissingResourceException("Resource named " + name + " in culture " + culture.Name + " was not found in your assembly.");
		}

		public string GetString(Assembly assembly, CultureInfo culture, string name) {
			return m_ResourceManagers[assembly].GetString(name, culture)
				?? throw new MissingResourceException("Resource named " + name + " in culture " + culture.Name + " was not found in your assembly.");
		}

		public string ResolveString(CultureInfo culture, ComponentBase? component, string str) {
			if (str.StartsWith("#")) {
				Assembly? assembly;
				if (component == null) {
					assembly = Assembly.GetExecutingAssembly();
				} else {
					assembly = component.GetType().Assembly;
				}
				return GetString(assembly, culture, str.Substring(1))
					?? throw new MissingResourceException("Resource named " + str.Substring(1) + " in culture " + culture.Name + " was not found in the given assembly.");
			} else if (str.StartsWith('\\')) {
				return str.Substring(1); // If a string is not meant to be resolved but needs to start with a # then it can be escaped with \.
			} else {
				return str; // If the string needs to start with \# then you can use \\# and so on.
			}
		}
	}


	[Serializable]
	public class MissingResourceException : Exception {
		public MissingResourceException() { }
		public MissingResourceException(string message) : base(message) { }
		public MissingResourceException(string message, Exception inner) : base(message, inner) { }
		protected MissingResourceException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
