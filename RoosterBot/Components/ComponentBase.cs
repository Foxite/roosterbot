using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract Version ComponentVersion { get; }
		public virtual string[] Tags { get; } = new string[] { };

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

		public virtual DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) => new DependencyResult() {
			OK = true
		};
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
}
