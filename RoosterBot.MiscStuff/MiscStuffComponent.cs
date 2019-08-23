using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.MiscStuff {
	public class MiscStuffComponent : ComponentBase {
		public string ConfigPath { get; private set; }

		public override string VersionString => "1.0.0";

		public override Task AddServices(IServiceCollection services, string configPath) {
			ResourcesType = typeof(Resources);

			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));
			return Task.CompletedTask;
		}

		public override async Task AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<CounterModule>(services)
			));

			string helpText = Resources.MiscStuffComponent_HelpText;
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
