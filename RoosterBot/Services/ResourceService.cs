using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;

namespace RoosterBot {
	/// <summary>
	/// Allows you to localize your UI strings.
	/// </summary>
	public sealed class ResourceService {
		private readonly Dictionary<Assembly, ResourceManager> m_ResourceManagers;

		internal ResourceService() {
			m_ResourceManagers = new Dictionary<Assembly, ResourceManager>();
		}

		/// <summary>
		/// Register the resources for your component. Call this first in your AddModules function.
		/// </summary>
		/// <param name="baseName">The name of your component, followed by ".Resources".</param>
		public void RegisterResources(string baseName) {
			var assembly = Assembly.GetCallingAssembly();
			m_ResourceManagers[assembly] = new ResourceManager(baseName, assembly);
		}

		/// <summary>
		/// Get a string by a given key and for a given culture.
		/// </summary>
		public string GetString(CultureInfo culture, string name) {
			var assembly = Assembly.GetCallingAssembly();
			try {
				return m_ResourceManagers[assembly].GetString(name, culture)!;
			} catch (MissingManifestResourceException e) {
				throw new MissingResourceException($"Resource named {name} in culture {culture.Name} was not found in assembly {assembly.FullName}", e);
			}
		}

		/// <summary>
		/// Resolve a string. When the input starts with a # symbol, it is seen as a string resource key and the string except the # is passed into <see cref="GetString(CultureInfo, string)"/>.
		/// Otherwise, it is returned as-is.
		/// If you want your string to start with a # but don't want it to be resolved, then you can escape it with a \. If your string needs to start with \# then you can use \\# and so on.
		/// </summary>
		public string ResolveString(CultureInfo culture, Component? component, string str) {
			if (str.StartsWith("#")) {
				Assembly? assembly;
				if (component == null) {
					assembly = Assembly.GetExecutingAssembly();
				} else {
					assembly = component.GetType().Assembly;
				}
				return GetString(assembly, culture, str.Substring(1));
			} else if (str.StartsWith('\\')) {
				return str.Substring(1); // If a string is not meant to be resolved but needs to start with a # then it can be escaped with \.
			} else {
				return str; // If the string needs to start with a \ then you can use \\ and so on.
			}
		}

		internal string GetString(Assembly assembly, CultureInfo culture, string name) {
			try {
				return m_ResourceManagers[assembly].GetString(name, culture)!;
			} catch (MissingManifestResourceException e) {
				throw new MissingResourceException($"Resource named {name} in culture {culture.Name} was not found in assembly {assembly.FullName}", e);
			}
		}

		internal IEnumerable<KeyValuePair<string, string>> GetAvailableKeys(Assembly assembly, CultureInfo? culture) {
			ResourceManager resourceManager = m_ResourceManagers[assembly];
			IEnumerable<ResourceSet> resourceSets;

			if (culture is null) {
				resourceSets = GetAvailableCultures(assembly).Select(culture => resourceManager.GetResourceSet(culture, false, true)).WhereNotNull();
			} else {
				ResourceSet? resourceSet = resourceManager.GetResourceSet(culture, false, true);
				resourceSets = resourceSet is null ? Enumerable.Empty<ResourceSet>() : new[] { resourceSet };
			}
			return resourceSets != null
				? resourceSets.Cast<DictionaryEntry>().Select(Util.ToGeneric<string, string>)
				: Enumerable.Empty<KeyValuePair<string, string>>();
		}

		internal static IReadOnlyCollection<CultureInfo> GetAvailableCultures(Component component) => GetAvailableCultures(component.GetType().Assembly);

		internal static IReadOnlyCollection<CultureInfo> GetAvailableCultures(Assembly componentAssembly) {
			// https://stackoverflow.com/a/3227549
			string programLocation = componentAssembly.Location;
			var resourceFileName = Path.GetFileNameWithoutExtension(programLocation) + ".resources.dll";
		    var rootDir = new DirectoryInfo(Path.GetDirectoryName(programLocation));
			return new List<CultureInfo>(
				from c in CultureInfo.GetCultures(CultureTypes.AllCultures)
				join d in rootDir.EnumerateDirectories() on c.IetfLanguageTag equals d.Name
				where d.EnumerateFiles(resourceFileName).Any()
				select c
			);
		}
	}

	/// <summary>
	/// Thrown when a resource cannot be found.
	/// </summary>
	[Serializable]
	public class MissingResourceException : Exception {
		/// <inheritdoc/>
		public MissingResourceException() { }
		/// <inheritdoc/>
		public MissingResourceException(string message) : base(message) { }
		/// <inheritdoc/>
		public MissingResourceException(string message, Exception inner) : base(message, inner) { }
		/// <inheritdoc/>
		protected MissingResourceException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
