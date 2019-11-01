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
		private List<ComponentBase> m_Components;
		private ConcurrentDictionary<ModuleInfo, ComponentBase> m_ComponentsByModule;
		private ConcurrentDictionary<Assembly, ComponentBase> m_ComponentsByAssembly;

		public IServiceProvider Services { get; private set; }

		internal ComponentManager() { }

		/// <summary>
		/// Runs the full initialization process for components.
		/// </summary>
		internal async Task SetupComponents(IServiceCollection serviceCollection) {
			Logger.Info("ComponentManager", "ComponentManager starting");

			// Load assemblies and find classes deriving from ComponentBase
			IEnumerable<string> assemblyPaths = ReadComponentsFile();
			List<Assembly> assemblies = LoadAssemblies(assemblyPaths);
			Type[] types = FindComponentClasses(assemblies);

			m_Components = new List<ComponentBase>(types.Length);
			m_ComponentsByModule = new ConcurrentDictionary<ModuleInfo, ComponentBase>();
			m_ComponentsByAssembly = new ConcurrentDictionary<Assembly, ComponentBase>();

			// Start components
			ConstructComponents(types);
			CheckDependencies(m_Components);
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

				List<string> folders = path.Split('\\').ToList();
				for (int i = 0; i < folders.Count; i++) {
					if (folders[i] == "..") {
						folders.RemoveAt(i);
						folders.RemoveAt(i - 1);
						i--;
					}
				}
				path = Path.Combine(folders.ToArray());

				if (File.Exists(path) && Path.GetExtension(path).ToLower() == ".dll") {
					Logger.Debug("ComponentManager", "Loading assembly " + file);

					//AssemblyName assemblyRef = AssemblyName.GetAssemblyName(path);
					//Assembly assembly = AppDomain.CurrentDomain.Load(assemblyRef);
					Assembly assembly = Assembly.LoadFrom(path);
					assemblies.Add(assembly);
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
					ComponentBase component = Activator.CreateInstance(type) as ComponentBase;
					m_Components.Add(component);
					m_ComponentsByAssembly[type.Assembly] = component;
				} catch (Exception ex) {
					throw new ComponentConstructionException("Component " + type.Name + " threw an exception during construction.", type, ex);
				}
			}
		}

		private void CheckDependencies(IEnumerable<ComponentBase> components) {
			foreach (ComponentBase component in components) {
				DependencyResult dependencyResult = component.CheckDependencies(components);
				if (!dependencyResult.OK) {
					throw new ComponentDependencyException($"{component.Name} cannot satisfy dependencies:\n{dependencyResult.ErrorMessage}", component.GetType());
				}
			}
		}

		private async Task<IServiceProvider> AddComponentServicesAsync(IServiceCollection serviceCollection) {
			Task[] servicesLoading = new Task[m_Components.Count];

			int i = 0;
			foreach (ComponentBase component in m_Components) {
				Logger.Debug("ComponentManager", "Adding services from " + component.Name);
				
				try {
					servicesLoading[i] = component.AddServicesAsync(serviceCollection, Path.Combine(Program.DataPath, "Config", component.Name));
				} catch (Exception ex) {
					throw new ComponentServiceException("Component " + component.Name + " threw an exception during AddServices.", component.GetType(), ex);
				}
				i++;
			}
			await Task.WhenAll(servicesLoading);

			return serviceCollection.BuildServiceProvider();
		}

		private async Task AddComponentModulesAsync(IServiceProvider services) {
			RoosterCommandService commands = services.GetService<RoosterCommandService>();
			HelpService help = services.GetService<HelpService>();
			Task[] modulesLoading = new Task[m_Components.Count];

			int moduleIndex = 0;
			foreach (ComponentBase component in m_Components) {
				Logger.Debug("ComponentManager", "Adding modules from " + component.Name);
				try {
					void registerModule(ModuleInfo[] modules) {
						foreach (ModuleInfo module in modules) {
							m_ComponentsByModule[module] = component;
						}
					}

					modulesLoading[moduleIndex] = component.AddModulesAsync(services, commands, help, registerModule);
				} catch (Exception ex) {
					throw new ComponentModuleException("Component " + component.Name + " threw an exception during AddModules.", component.GetType(), ex);
				}
				moduleIndex++;
			}

			await Task.WhenAll(modulesLoading);
		}

		internal void ShutdownComponents() {
			foreach (ComponentBase component in m_Components) {
				component.Dispose();
			}
		}

		public IReadOnlyList<ComponentBase> GetComponents() {
			return m_Components.AsReadOnly();
		}

		public ComponentBase GetComponentForModule(ModuleInfo module) {
			if (m_ComponentsByModule.TryGetValue(module, out ComponentBase result)) {
				return result;
			} else {
				throw new ArgumentException($"Module of type {module.Name} is not registered");
			}
		}

		internal ComponentBase GetComponentFromAssembly(Assembly assembly) {
			if (m_ComponentsByAssembly.TryGetValue(assembly, out ComponentBase result)) {
				return result;
			} else {
				throw new ArgumentException($"Assembly {assembly.FullName} does not have a ComponentBase");
			}
		}
	}
}
