using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public abstract class Component : IDisposable {
		public abstract Version ComponentVersion { get; }
		public virtual IEnumerable<string> Tags { get; } = Enumerable.Empty<string>();

		// TODO (feature) Just use all available resource files instead of declaring which ones are supported
		// This allows people to translate components without the original dev having to explicitly support it, by simply adding the sattelite assembly to the folder
		public virtual IReadOnlyCollection<CultureInfo> SupportedCultures => Array.Empty<CultureInfo>();

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

		public virtual DependencyResult CheckDependencies(IEnumerable<Component> components) => new DependencyResult();
		public virtual Task AddServicesAsync(IServiceCollection services, string configPath) => Task.CompletedTask;
		public virtual Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) => Task.CompletedTask;

		#region IDisposable Support
		protected virtual void Dispose(bool disposing) { }

		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
