using System;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract void AddServices(ref IServiceCollection services, string configPath);
		public abstract void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help);
	}
}
