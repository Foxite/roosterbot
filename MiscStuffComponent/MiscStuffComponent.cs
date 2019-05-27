using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;
using MiscStuffComponent.Modules;
using System.IO;
using System;
using Discord.WebSocket;
using System.Collections.Generic;

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
				Dictionary<ulong, string> usersByName = new Dictionary<ulong, string>() {
					{ 244147515375484928, "Kevin" },
					{ 368317619838779393, "Lars" }
				};

				if (usersByName.TryGetValue(msg.Author.Id, out string name) && msg.Content.Contains("snap")) {
					await msg.Channel.SendMessageAsync($"Ja {name}, leuke pun.");
				}
			};
		}
	}
}
