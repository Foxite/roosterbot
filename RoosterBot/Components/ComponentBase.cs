using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	// TODO using IServiceProvider directly is apparently considered an anti-pattern, and should be done in framework using DI
	// https://blog.ploeh.dk/2010/02/03/ServiceLocatorisanAnti-Pattern/
	public abstract class ComponentBase {
		public abstract Version ComponentVersion { get; }
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
		public abstract Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction);
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
					object value = propertyInfo.GetValue(null);
					if (value is string strValue) {
						return strValue;
					} else {
						Logger.Error("ComponentBase", $"Requested resource {propertyName} defined by {Name} is not a string");
						return "ERROR 3";
					}
				}
			}
		}
	}
}
