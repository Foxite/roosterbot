using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
		protected virtual Task AddServicesAsync(IServiceCollection services, string configPath) => Task.CompletedTask;
		protected virtual Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) => Task.CompletedTask;

		internal DependencyResult CheckDependenciesInternal(IEnumerable<Component> components) => CheckDependencies(components);
		internal Task AddServicesInternalAsync(IServiceCollection services, string configPath) => AddServicesAsync(services, configPath);
		internal Task AddModulesInternalAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) => AddModulesAsync(services, commandService, help);
		
		#region IDisposable Support
		protected virtual void Dispose(bool disposing) { }

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
