using System;
using System.Collections.Generic;
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

		public ComponentBase() { }

		public virtual DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) => new DependencyResult() {
			OK = true
		};
		public virtual Task AddServicesAsync(IServiceCollection services, string configPath) => Task.CompletedTask;
		public virtual Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) => Task.CompletedTask;
		public virtual Task ShutdownAsync() => Task.CompletedTask;
	}
}
