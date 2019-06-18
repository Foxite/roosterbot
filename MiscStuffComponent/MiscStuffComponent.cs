using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;
using MiscStuffComponent.Modules;
using System.IO;
using System;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MiscStuffComponent
{
    public class MiscStuffComponent : ComponentBase
    {
		public string ConfigPath { get; private set; }

		public override Task AddServices(IServiceCollection services, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));
			return Task.CompletedTask;
		}

		public override async Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			await Task.WhenAll(
				commandService.AddModuleAsync<CounterModule>(services),
				commandService.AddModuleAsync<MiscModule>(services)
			);

			string helpText = "Voor de rest zijn er nog deze commands:\n";
			helpText += "`counter <naam van counter>`\n";
			helpText += "`counter reset <naam van counter>`\n";
			helpText += "En nog minstens 4 geheime commands voor de bot owner.";
			help.AddHelpSection("misc", helpText);

			services.GetService<DiscordSocketClient>().MessageReceived += async (msg) => {
				string getNameIfApplicable(ulong userId) {
					switch (userId) {
						case 244147515375484928: return "Kevin";
						case 368317619838779393: return "Lars";
						default: return null;
					}
				}
				string snapUserName = getNameIfApplicable(msg.Author.Id);
				if (snapUserName != null && msg.Content.Contains("snap")) {
					await msg.Channel.SendMessageAsync($"Ja {snapUserName}, leuke pun.");
				}
			};
		}
	}
}
