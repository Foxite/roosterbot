using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	/// <summary>
	/// The base class for all feature packages for RoosterBot.
	/// </summary>
	public abstract class Component : IDisposable {
		/// <summary>
		/// The version of this Component.
		/// </summary>
		public abstract Version ComponentVersion { get; }

		/// <summary>
		/// The tag list for this Component. Other components may require that at least one component with a tag is installed (for example, to provide an implementation for
		/// an abstract service defined by that component); this is how you indicate your tags.
		/// </summary>
		public virtual IEnumerable<string> Tags { get; } = Enumerable.Empty<string>();

		/// <summary>
		/// The name for this component. It is equal to the class name 
		/// </summary>
		public string Name {
			get {
				string longName = GetType().Name;
				const string ComponentString = "Component";
				if (longName.EndsWith(ComponentString)) {
					return longName.Substring(0, longName.Length - ComponentString.Length);
				} else {
					return longName;
				}
			}
		}

		/// <summary>
		/// Check your dependencies. Use <see cref="DependencyResult.Build(IEnumerable{Component})"/> for this.
		/// </summary>
		protected virtual DependencyResult CheckDependencies(IEnumerable<Component> components) => new DependencyResult();

		/// <summary>
		/// Add your services to the global <see cref="IServiceProvider"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/> that will be used to create the global <see cref="IServiceProvider"/>.</param>
		/// <param name="configPath">The path to your config folder.</param>
		protected virtual void AddServices(IServiceCollection services, string configPath) { }

		/// <summary>
		/// Add your modules to the <see cref="RoosterCommandService"/>.
		/// </summary>
		protected virtual void AddModules(IServiceProvider services, RoosterCommandService commandService) { }

		internal DependencyResult CheckDependenciesInternal(IEnumerable<Component> components) => CheckDependencies(components);
		internal void AddServicesInternal(IServiceCollection services, string configPath) => AddServices(services, configPath);
		internal void AddModulesInternal(IServiceProvider services, RoosterCommandService commandService) => AddModules(services, commandService);
		
		#region IDisposable Support
		/// <summary>
		/// Called when RoosterBot shuts down.
		/// </summary>
		/// <param name="disposing">Will always be true; added as part of the Dispose Pattern.</param>
		/// <remarks>
		/// See <see href="https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/implementing-dispose#implementing-the-dispose-pattern-for-a-derived-class">this page</see>
		/// for more information on the Dispose Pattern.
		/// </remarks>
		protected virtual void Dispose(bool disposing) { }

		/// <summary>
		/// This is called when RoosterBot shuts down. To execute your own code when this happens, override <see cref="Dispose(bool)"/>.
		/// </summary>
		public void Dispose() {
			Dispose(true);
		}
		#endregion
	}
}
