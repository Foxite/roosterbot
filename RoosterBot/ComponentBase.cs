using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract string VersionString { get; }

		public abstract Task AddServices(IServiceCollection services, string configPath);
		public abstract Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help);
		public virtual Task OnShutdown() { return Task.CompletedTask; }
	}
}
