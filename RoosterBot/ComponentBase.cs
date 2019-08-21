using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
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

		public abstract Task AddServices(IServiceCollection services, string configPath);
		public abstract Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help);
		public virtual Task OnShutdown() { return Task.CompletedTask; }
	}
}
