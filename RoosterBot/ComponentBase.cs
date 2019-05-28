using System;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.Services;

namespace RoosterBot {
	public abstract class ComponentBase {
		public abstract void AddServices(ref IServiceCollection services, string configPath);
		public abstract void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help);
		public virtual bool HandleCommandError(CommandInfo command, ICommandContext context, IResult result) { return false; }
	}
}
