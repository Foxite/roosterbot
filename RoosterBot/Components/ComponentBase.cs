using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract Version ComponentVersion { get; }

		// TODO Components should be able to define dependencies in the following ways:
		// Another component must be newer, older, or between than a Version
		// Another component must match a VersionPredicate
		// There must be one (or more) components that has a "tag" (TODO tag system) (for example Schedule requires at least 1 schedule providing component, Schedule.GLU is such a component)
		// Another component (matching any of these dependencies) must load before or after this component
		// The proposed way to do this is to keep the DependencyChecker idea and provide helper methods to test for common dependencies
		public virtual IEnumerable<DependencyChecker> Dependencies { get; }
		public string Name {
			get {
				string longName = GetType().Name;
				int suffixIndex = longName.IndexOf("Component");
				if (suffixIndex != -1) {
					return longName.Substring(0, suffixIndex);
				} else {
					return longName;
				}
			}
		}

		protected Type ResourcesType { get; set; }

		public ComponentBase() { }

		public virtual Task AddServicesAsync(IServiceCollection services, string configPath) => Task.CompletedTask;
		public virtual Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) => Task.CompletedTask;
		public virtual Task ShutdownAsync() => Task.CompletedTask;

		/// <summary>
		/// Returns a string resource for this component.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public string GetStringResource(string propertyName) {
			if (ResourcesType != null) {
				PropertyInfo propertyInfo = ResourcesType.GetProperty(propertyName);
				if (propertyInfo != null) {
					object value = propertyInfo.GetValue(null);
					if (value is string strValue) {
						return strValue;
					} else {
						throw new ArgumentException($"Requested resource {propertyName} defined by {Name} is not a string");
					}
				} else {
					throw new ArgumentException($"No resource named {propertyName} is defined by {Name}");
				}
			} else {
				throw new ArgumentException($"String resource requested for component {Name} but no ResourcesType was set for its component");
			}
		}
	}

	public delegate DependencyResult DependencyChecker(ComponentBase component);

	public class DependencyResult {
		public bool OK { get; }
		public string ErrorMessage { get; }

		public DependencyResult(bool ok, string errorMessage) {
			OK = ok;
			ErrorMessage = errorMessage;
		}
	}
}
