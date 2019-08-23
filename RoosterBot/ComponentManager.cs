﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;

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
			List<string> assemblyPaths = await ReadComponentsFileAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "components.txt"));
			List<Assembly> assemblies = LoadAssemblies(assemblyPaths);
			Type[] types = FindComponentClasses(assemblies);

			m_Components = new Dictionary<Type, ComponentBase>(types.Length);
			m_ComponentsByModule = new ConcurrentDictionary<ModuleInfo, ComponentBase>();

			// Start components
			ConstructComponents(types);
			Services = await AddComponentServicesAsync(serviceCollection);
			await AddComponentModulesAsync(Services);
		}

		private async Task<List<string>> ReadComponentsFileAsync(string path) {
			// TODO move components.txt into DataPath rather than git
			List<string> assemblyPaths = new List<string>();
			using (StreamReader fs = File.OpenText(path)) {
				while (!fs.EndOfStream) {
					assemblyPaths.Add(await fs.ReadLineAsync());
				}
			}
			return assemblyPaths;
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
					throw new ComponentException("Component " + type.Name + " threw an exception during construction.", ex, type);
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
					servicesLoading[i] = component.AddServices(serviceCollection, Path.Combine(Program.DataPath, "Config", type.Name));
				} catch (Exception ex) {
					throw new ComponentException("Component " + type.Name + " threw an exception during AddServices.", ex, type);
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

					modulesLoading[moduleIndex] = componentKVP.Value.AddModules(services, commands, help, registerModule);
				} catch (Exception ex) {
					throw new ComponentException("Component " + componentKVP.Key.Name + " threw an exception during AddModules.", ex, componentKVP.Key);
				}
				moduleIndex++;
			}

			await Task.WhenAll(modulesLoading);
		}

		internal async Task ShutdownComponentsAsync() {
			foreach (KeyValuePair<Type, ComponentBase> componentKVP in m_Components) {
				await componentKVP.Value.OnShutdown();
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


	/// <summary>
	/// Thrown by ComponentManager when a component throws an exception. Always has a message and an inner exception.
	/// </summary>
	[Serializable]
	public class ComponentException : Exception {
		public Type CausingComponent { get; }

		public ComponentException(string message, Exception inner, Type causingComponent) : base(message, inner) {
			CausingComponent = causingComponent;
		}

		protected ComponentException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
	}
}
