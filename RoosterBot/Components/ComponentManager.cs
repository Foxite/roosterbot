using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace RoosterBot {
	public sealed class ComponentManager {
		private Dictionary<Type, ComponentBase> m_Components;
		private ConcurrentDictionary<ModuleInfo, ComponentBase> m_ComponentsByModule;

		public IServiceProvider Services { get; private set; }

		private ComponentManager() { }

		internal static async Task<ComponentManager> CreateAsync(IServiceCollection serviceCollection) {
			ComponentManager cm = new ComponentManager();
			await cm.SetupComponents(serviceCollection);
			return cm;
		}

		/// <summary>
		/// Runs the full initialization process for components.
		/// </summary>
		private async Task SetupComponents(IServiceCollection serviceCollection) {
			Logger.Info("ComponentManager", "ComponentManager starting");

			// Load assemblies and find classes deriving from ComponentBase
			IEnumerable<string> assemblyPaths = ReadComponentsFile();
			List<Assembly> assemblies = LoadAssemblies(assemblyPaths);
			Type[] types = FindComponentClasses(assemblies);

			m_Components = new Dictionary<Type, ComponentBase>(types.Length);
			m_ComponentsByModule = new ConcurrentDictionary<ModuleInfo, ComponentBase>();

			// Start components
			ConstructComponents(types);
			Services = await AddComponentServicesAsync(serviceCollection);
			await AddComponentModulesAsync(Services);
		}

		private IEnumerable<string> ReadComponentsFile() {
			List<string> assemblyPaths = new List<string>();
			string filePath = Path.Combine(Program.DataPath, "Config", "Components.json");

			if (!File.Exists(filePath)) {
				throw new FileNotFoundException("Components.json was not found in the DataPath.");
			}

			JObject json = null;
			try {
				json = JObject.Parse(File.ReadAllText(filePath));
			} catch (JsonReaderException e) {
				throw new FormatException("Components.json contains invalid JSON.", e);
			}

			try {
				return json["components"].ToObject<JArray>().Select(jt => jt.ToObject<string>() + ".dll");
			} catch (Exception e) {
				throw new FormatException("Components.json contains invalid data.", e);
			}
		}

		private List<Assembly> LoadAssemblies(IEnumerable<string> assemblyPaths) {
			List<Assembly> assemblies = new List<Assembly>();
			foreach (string file in assemblyPaths) {
				string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, file);
				if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".dll") {
					Logger.Debug("ComponentManager", "Loading assembly " + file);
					
					assemblies.Add(AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(path)));
				} else {
					Logger.Error("ComponentManager", "Component " + file + " does not exist or it is not a DLL file");
				}
			}
			return assemblies;
		}

		private Type[] FindComponentClasses(IEnumerable<Assembly> assemblies) {
			// Look for children of ComponentBase in the loaded assemblies
			Type[] componentTypes = (from domainAssembly in assemblies
									 from assemblyType in domainAssembly.GetExportedTypes()
									 where assemblyType.IsSubclassOf(typeof(ComponentBase))
									 select assemblyType).ToArray();

			return componentTypes;
		}

		private void ConstructComponents(IEnumerable<Type> componentTypes) {
			foreach (Type type in componentTypes) {
				Logger.Debug("ComponentManager", "Constructing component " + type.Name);
				try {
					m_Components[type] = Activator.CreateInstance(type) as ComponentBase;
				} catch (Exception ex) {
					throw new ComponentConstructionException("Component " + type.Name + " threw an exception during construction.", ex, type);
				}
			}
		}

		private async Task<IServiceProvider> AddComponentServicesAsync(IServiceCollection serviceCollection) {
			Task[] servicesLoading = new Task[m_Components.Count];

			int i = 0;
			foreach (KeyValuePair<Type, ComponentBase> kvp in m_Components) {
				Type type = kvp.Key;
				ComponentBase component = kvp.Value;

				Logger.Info("ComponentManager", "Adding services from " + type.Name);
				
				try {
					servicesLoading[i] = component.AddServicesAsync(serviceCollection, Path.Combine(Program.DataPath, "Config", type.Name));
				} catch (Exception ex) {
					throw new ComponentServiceException("Component " + type.Name + " threw an exception during AddServices.", ex, type);
				}
				i++;
			}
			await Task.WhenAll(servicesLoading);

			return serviceCollection.BuildServiceProvider();
		}

		private async Task AddComponentModulesAsync(IServiceProvider services) {
			EditedCommandService commands = services.GetService<EditedCommandService>();
			HelpService help = services.GetService<HelpService>();
			Task[] modulesLoading = new Task[m_Components.Count];

			int moduleIndex = 0;
			foreach (KeyValuePair<Type, ComponentBase> componentKVP in m_Components) {
				Logger.Info("ComponentManager", "Adding modules from " + componentKVP.Key.Name);
				try {
					void registerModule(ModuleInfo[] modules) {
						foreach (ModuleInfo module in modules) {
							m_ComponentsByModule[module] = componentKVP.Value;
						}
					}

					modulesLoading[moduleIndex] = componentKVP.Value.AddModulesAsync(services, commands, help, registerModule);
				} catch (Exception ex) {
					throw new ComponentModuleException("Component " + componentKVP.Key.Name + " threw an exception during AddModules.", ex, componentKVP.Key);
				}
				moduleIndex++;
			}

			await Task.WhenAll(modulesLoading);
		}

		internal async Task ShutdownComponentsAsync() {
			foreach (KeyValuePair<Type, ComponentBase> componentKVP in m_Components) {
				await componentKVP.Value.ShutdownAsync();
			}
		}

		public IEnumerable<ComponentBase> GetComponents() {
			return m_Components.Values;
		}

		public ComponentBase GetComponentForModule(ModuleInfo module) {
			if (m_ComponentsByModule.TryGetValue(module, out ComponentBase result)) {
				return result;
			} else {
				throw new ArgumentException($"Module of type {module.Name} is not registered");
			}
		}
	}


}
