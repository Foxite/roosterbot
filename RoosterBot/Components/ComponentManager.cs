using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Module = Qmmands.Module;

namespace RoosterBot {
	/// <summary>
	/// The singleton class that takes care of setting up and shutting down components.
	/// </summary>
	public sealed class ComponentManager {
		private readonly List<Component> m_Components;
		private readonly ConcurrentDictionary<Assembly, Component> m_ComponentsByAssembly;

		internal ComponentManager() {
			m_Components = new List<Component>();
			m_ComponentsByAssembly = new ConcurrentDictionary<Assembly, Component>();
		}

		/// <summary>
		/// Runs the full initialization process for components.
		/// </summary>
		internal IServiceProvider SetupComponents(IServiceCollection serviceCollection) {
			Logger.Info("ComponentManager", "ComponentManager starting");

			// Load assemblies and find classes deriving from ComponentBase
			IEnumerable<string> componentNames = ReadComponentsFile();
			EnsureNoDuplicates(componentNames);
			List<Assembly> assemblies = LoadAssemblies(componentNames);
			Type[] types = FindComponentClasses(assemblies);
			EnsureNoMultiComponentAssemblies(types);

			// Start components
			ConstructComponents(types);
			CheckDependencies(m_Components);
			IServiceProvider services = AddComponentServices(serviceCollection);
			AddComponentModules(services);
			Logger.Info("ComponentManager", "Components ready");
			ConnectPlatforms(services);
			return services;
		}

		private IEnumerable<string> ReadComponentsFile() {
			try {
				return Util.LoadJsonConfigFromTemplate(Path.Combine(Program.DataPath, "Config", "Components.json"), new { Components = new List<string>() }).Components;
			} catch (JsonReaderException e) {
				throw new FormatException("Components.json contains invalid data.", e);
			}
		}
		
		private void EnsureNoDuplicates(IEnumerable<string> componentNames) {
			IEnumerable<string> duplicates = componentNames.Duplicates();
			if (duplicates.Any()) {
				throw new InvalidOperationException("One or more components was listed more than once in Components.json:\n" + string.Join("\n", duplicates));
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
					+ string.Join('\n', assembliesWithMultipleComponents.Select(grp => grp.Key.FullName + "\n" + string.Join("\n", grp.Select(type => "- " + type.FullName)))));
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
				DependencyResult dependencyResult = component.CheckDependenciesInternal(components);
				if (!dependencyResult.OK) {
					throw new ComponentDependencyException($"{component.Name} cannot satisfy dependencies:\n{dependencyResult.ErrorMessage}", component.GetType());
				}
			}
		}

		private IServiceProvider AddComponentServices(IServiceCollection serviceCollection) {
			foreach (Component component in m_Components) {
				Logger.Debug("ComponentManager", "Adding services from " + component.Name);
				
				try {
					component.AddServicesInternal(serviceCollection, Path.Combine(Program.DataPath, "Config", component.Name));
				} catch (Exception ex) {
					throw new ComponentServiceException("Component " + component.Name + " threw an exception during AddServices.", component.GetType(), ex);
				}
			}
			return serviceCollection.BuildServiceProvider();
		}

		private void AddComponentModules(IServiceProvider services) {
			RoosterCommandService commands = services.GetRequiredService<RoosterCommandService>();

			foreach (Component component in m_Components) {
				Logger.Debug("ComponentManager", "Adding modules from " + component.Name);
				try {
					component.AddModulesInternal(services, commands);
				} catch (Exception ex) {
					throw new ComponentModuleException("Component " + component.Name + " threw an exception during AddModules.", component.GetType(), ex);
				}
			}
		}

		private void ConnectPlatforms(IServiceProvider services) {
			foreach (var component in m_Components.OfType<PlatformComponent>()) {
				component.ConnectInternal(services);
			}
		}

		internal void ShutdownComponents() {
			foreach (var component in m_Components.OfType<PlatformComponent>()) {
				component.DisconnectInternal();
			}

			foreach (Component component in m_Components) {
				component.Dispose();
			}
		}

		/// <summary>
		/// Get a read-only list of all installed Components.
		/// </summary>
		/// <returns></returns>
		public IReadOnlyList<Component> GetComponents() {
			return m_Components.AsReadOnly();
		}

		/// <summary>
		/// Get the <see cref="Component"/> that owns a module. For this to work, a module <b>must</b> be defined in an assembly that also defines a component.
		/// </summary>
		/// <param name="module"></param>
		/// <returns></returns>
		public Component GetComponentForModule(Module module) {
			if (module.Type == null) {
				throw new ArgumentException($"Module named {module.Name} was built manually. This is not supported since RoosterBot 2.2.");
			} else if (m_ComponentsByAssembly.TryGetValue(module.Type.Assembly, out Component? result)) {
				return result;
			} else {
				throw new ArgumentException($"Module of type {module.Name} is not registered");
			}
		}

		internal Component? GetComponentFromAssembly(Assembly assembly) {
			if (m_ComponentsByAssembly.TryGetValue(assembly, out Component? result)) {
				return result;
			} else if (assembly.Equals(Assembly.GetExecutingAssembly())) {
				return null;
			} else {
				throw new ArgumentException($"Assembly {assembly.FullName} does not have a ComponentBase");
			}
		}

		/// <summary>
		/// Gets the installed <see cref="PlatformComponent"/> that has a <see cref="PlatformComponent.PlatformName"/> equal to <paramref name="name"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">When no appropriate <see cref="PlatformComponent"/> is installed.</exception>
		public PlatformComponent? GetPlatform(string name) {
			return m_Components.OfType<PlatformComponent>().SingleOrDefault(platform => platform.PlatformName == name);
		}
	}
}
