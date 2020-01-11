using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public abstract class Component : IDisposable {
		public abstract Version ComponentVersion { get; }
		public virtual IEnumerable<string> Tags { get; } = Enumerable.Empty<string>();

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

		protected virtual DependencyResult CheckDependencies(IEnumerable<Component> components) => new DependencyResult();
		protected virtual void AddServices(IServiceCollection services, string configPath) { }
		protected virtual void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) { }

		internal DependencyResult CheckDependenciesInternal(IEnumerable<Component> components) => CheckDependencies(components);
		internal void AddServicesInternal(IServiceCollection services, string configPath) => AddServices(services, configPath);
		internal void AddModulesInternal(IServiceProvider services, RoosterCommandService commandService, HelpService help) => AddModules(services, commandService, help);
		
		#region IDisposable Support
		protected virtual void Dispose(bool disposing) { }

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
