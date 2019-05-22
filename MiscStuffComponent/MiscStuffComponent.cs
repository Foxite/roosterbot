using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;
using MiscStuffComponent.Modules;
using System.IO;
using System;
using Discord.WebSocket;

namespace MiscStuffComponent
{
    public class MiscStuffComponent : ComponentBase
    {
		public string ConfigPath { get; private set; }

		public override void AddServices(ref IServiceCollection services, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));
		}

		public override void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			commandService.AddModuleAsync<CounterModule>(services);
			commandService.AddModuleAsync<MiscModule>(services);

			string helpText = "Voor de rest zijn er nog deze commands:\n";
			helpText += "`counter <naam van counter>`\n";
			helpText += "`counter reset <naam van counter>`\n";
			helpText += "En nog minstens 4 geheime commands voor de bot owner.";
			help.AddHelpSection("misc", helpText);

			services.GetService<DiscordSocketClient>().MessageReceived += async (msg) => {
				if (msg.Author.Id == 244147515375484928 // Kevin
				 && msg.Content.Contains("snap")) {
					await msg.Channel.SendMessageAsync("Ja Kevin, leuke pun.");
				}
			};
		}
	}
}
