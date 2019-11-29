using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Module = Qmmands.Module;

namespace RoosterBot {
	public sealed class ComponentManager {
		private readonly List<Component> m_Components;
		private readonly ConcurrentDictionary<Assembly, Component> m_ComponentsByAssembly;

#nullable disable
		// This is a bit similar to the problem explained in Program.cs, namely this property is set in an async "sequel" to the constructor.
		// There's only a single instance of this class in the entire program and it needs to await stuff in its constructor, so we make an essentially empty constructor
		//  and do all the things in an async method that's supposed to be called right after construction.
		// There wasn't a problem with this approach until we switched to .NET Core 3.0 and enabled nullable reference types. The compiler doesn't know about SetupComponents.
		// Again simple solution is to ignore or disable the warnings.
		// That said I'd really love to get rid of this property, or even make this entire class internal, as it doesn't feel like something any component should be using.
		// But the few (currently 4) uses outside of the main project aren't easily worked around. Removing the need for this property in all those places are each todo items on their own.
		// Since the entire command system is likely to be thrown over its head once we switch to Qmmands for 3.0, it may be better to not fix those issues for now, as the staging branch won't
		//  be released until 3.0 is done.
		public IServiceProvider Services { get; private set; }
#nullable restore

		internal ComponentManager() {
			m_Components = new List<Component>();
			m_ComponentsByAssembly = new ConcurrentDictionary<Assembly, Component>();
		}

		/// <summary>
		/// Runs the full initialization process for components.
		/// </summary>
		internal async Task SetupComponents(IServiceCollection serviceCollection) {
			Logger.Info("ComponentManager", "ComponentManager starting");

			// Load assemblies and find classes deriving from ComponentBase
			IEnumerable<string> componentNames = ReadComponentsFile();
			List<Assembly> assemblies = LoadAssemblies(componentNames);
			Type[] types = FindComponentClasses(assemblies);
			EnsureNoMultiComponentAssemblies(types);

			// Start components
			ConstructComponents(types);
			CheckDependencies(m_Components);
			Services = await AddComponentServicesAsync(serviceCollection);
			await AddComponentModulesAsync(Services);
			Logger.Info("ComponentManager", "Components ready");
		}

		private IEnumerable<string> ReadComponentsFile() {
			List<string> componentNames = new List<string>();
			string filePath = Path.Combine(Program.DataPath, "Config", "Components.json");

			if (!File.Exists(filePath)) {
				throw new FileNotFoundException("Components.json was not found in the DataPath.");
			}

			JObject json;
			try {
				json = JObject.Parse(File.ReadAllText(filePath));
			} catch (JsonReaderException e) {
				throw new FormatException("Components.json contains invalid JSON.", e);
			}

			try {
				return json["components"].ToObject<JArray>().Select(jt => jt.ToObject<string>());
			} catch (Exception e) {
				throw new FormatException("Components.json contains invalid data.", e);
			}
		}

		private List<Assembly> LoadAssemblies(IEnumerable<string> componentNames) {
			var assemblies = new List<Assembly>();
			foreach (string componentName in componentNames) {
				string path = Path.Combine(AppContext.BaseDirectory, "Components", componentName, componentName + ".dll");

				if (File.Exists(path)) {
					Logger.Debug("ComponentManager", "Loading assembly " + componentName);

					var assembly = Assembly.LoadFrom(path);
					assemblies.Add(assembly);
				} else {
					Logger.Error("ComponentManager", "Component " + componentName + " could not be found");
				}
			}
			return assemblies;
		}

		private Type[] FindComponentClasses(IEnumerable<Assembly> assemblies) {
			// Look for children of ComponentBase in the loaded assemblies
			Type[] componentTypes = (from domainAssembly in assemblies
									 from assemblyType in domainAssembly.GetExportedTypes()
									 where assemblyType.IsSubclassOf(typeof(Component))
									 select assemblyType).ToArray();

			return componentTypes;
		}

		private void EnsureNoMultiComponentAssemblies(IEnumerable<Type> componentTypes) {
			var assembliesWithMultipleComponents = componentTypes.GroupBy(component => component.Assembly).Where(group => group.Count() > 1);
			if (assembliesWithMultipleComponents.Any()) {
				throw new InvalidOperationException(
					$"One or more assemblies contain more than one {nameof(Component)} class. An assembly can have at most one component. The offending assemblies are:\n"
					+ string.Join('\n', assembliesWithMultipleComponents));
			}
		}

		private void ConstructComponents(IEnumerable<Type> componentTypes) {
			foreach (Type type in componentTypes) {
				Logger.Debug("ComponentManager", "Constructing component " + type.Name);
				try {
					Component component = (Activator.CreateInstance(type) as Component)!; // Can technically be null but should never happen.
					m_Components.Add(component);
					m_ComponentsByAssembly[type.Assembly] = component;
				} catch (Exception ex) {
					throw new ComponentConstructionException("Component " + type.Name + " threw an exception during construction.", type, ex);
				}
			}
		}

		private void CheckDependencies(IEnumerable<Component> components) {
			foreach (Component component in components) {
				DependencyResult dependencyResult = component.CheckDependencies(components);
				if (!dependencyResult.OK) {
					throw new ComponentDependencyException($"{component.Name} cannot satisfy dependencies:\n{dependencyResult.ErrorMessage}", component.GetType());
				}
			}
		}

		private async Task<IServiceProvider> AddComponentServicesAsync(IServiceCollection serviceCollection) {
			Task[] servicesLoading = new Task[m_Components.Count];

			int i = 0;
			foreach (Component component in m_Components) {
				Logger.Debug("ComponentManager", "Adding services from " + component.Name);
				
				try {
#if DEBUG
					await
#else
					servicesLoading[i] = 
#endif               
						component.AddServicesAsync(serviceCollection, Path.Combine(Program.DataPath, "Config", component.Name));
				} catch (Exception ex) {
					throw new ComponentServiceException("Component " + component.Name + " threw an exception during AddServices.", component.GetType(), ex);
				}
				i++;
			}
#if !DEBUG
			await Task.WhenAll(servicesLoading);
#endif

			return serviceCollection.BuildServiceProvider();
		}

		private async Task AddComponentModulesAsync(IServiceProvider services) {
			RoosterCommandService commands = services.GetService<RoosterCommandService>();
			HelpService help = services.GetService<HelpService>();
			Task[] modulesLoading = new Task[m_Components.Count];

			int moduleIndex = 0;
			foreach (Component component in m_Components) {
				Logger.Debug("ComponentManager", "Adding modules from " + component.Name);
				try {
#if DEBUG
					await
#else
					modulesLoading[moduleIndex] = 
#endif
						component.AddModulesAsync(services, commands, help);
				} catch (Exception ex) {
					throw new ComponentModuleException("Component " + component.Name + " threw an exception during AddModules.", component.GetType(), ex);
				}
				moduleIndex++;
			}

#if !DEBUG
			await Task.WhenAll(modulesLoading);
#endif
		}

		internal void ShutdownComponents() {
			foreach (Component component in m_Components) {
				component.Dispose();
			}
		}

		public IReadOnlyList<Component> GetComponents() {
			return m_Components.AsReadOnly();
		}

		public Component GetComponentForModule(Module module) {
			if (module.Type == null) {
				throw new ArgumentException($"Module named {module.Name} was built manually. This is not supported since RoosterBot 2.2.");
			} else if (m_ComponentsByAssembly.TryGetValue(module.Type.Assembly, out Component? result)) {
				return result;
			} else {
				throw new ArgumentException($"Module of type {module.Name} is not registered");
			}
		}

		internal Component GetComponentFromAssembly(Assembly assembly) {
			if (m_ComponentsByAssembly.TryGetValue(assembly, out Component? result)) {
				return result;
			} else {
				throw new ArgumentException($"Assembly {assembly.FullName} does not have a ComponentBase");
			}
		}
	}
}
