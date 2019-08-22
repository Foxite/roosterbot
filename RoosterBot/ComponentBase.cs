using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Modules;
using RoosterBot.Services;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract string VersionString { get; }
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

		public abstract Task AddServices(IServiceCollection services, string configPath);
		public abstract Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help, Func<Type, Task> addModuleFunction);
		public virtual Task OnShutdown() { return Task.CompletedTask; }

		/// <summary>
		/// Returns a string resource for this component.
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public string GetStringResource(string propertyName) {
			if (ResourcesType == null) {
				Logger.Error("ComponentBase", $"String resource requested for component {Name} but no ResourcesType was set for its component");
				return "ERROR 1";
			} else {
				PropertyInfo propertyInfo = ResourcesType.GetProperty(propertyName);
				if (propertyInfo == null) {
					Logger.Error("ComponentBase", $"No resource named {propertyName} is defined by {Name}");
					return "ERROR 2";
				} else {
					try {
						return (string) propertyInfo.GetValue(null);
					} catch (InvalidCastException) {
						Logger.Error("ComponentBase", $"Requested resource {propertyName} defined by {Name} is not a string");
						return "ERROR 3";
					}
				}
			}
		}
	}
}
