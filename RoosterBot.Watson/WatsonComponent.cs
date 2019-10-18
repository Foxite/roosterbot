using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Watson {
	public class WatsonComponent : ComponentBase {
		private WatsonClient m_Watson;
		private DiscordSocketClient m_Client;
		private ConfigService m_Config;
		private string m_WatsonID;
		private string m_WatsonKey;

		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			m_WatsonID = jsonConfig["watsonid"].ToObject<string>();
			m_WatsonKey = jsonConfig["watsonkey"].ToObject<string>();
			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> unused) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Watson.Resources");

			m_Config = services.GetService<ConfigService>();
			m_Client = services.GetService<DiscordSocketClient>();
			
			m_Watson = new WatsonClient(m_WatsonKey, m_WatsonID,
				services.GetService<GuildConfigService>(),
				services.GetService<ResourceService>(),
				m_Client,
				commandService,
				services.GetService<CommandResponseService>());

			m_Client.MessageReceived += async (SocketMessage msg) => {
				string commandPrefix;

				if (msg.Channel is IGuildChannel guildChannel) {
					commandPrefix = (await services.GetService<GuildConfigService>().GetConfigAsync(guildChannel.Guild)).CommandPrefix;
				} else {
					commandPrefix = m_Config.DefaultCommandPrefix;
				}

				// Do not await this task on the gateway thread because it can take very long.
				_ = ProcessNaturalLanguageCommandsAsync(msg, commandPrefix);
			};
			
			help.AddHelpSection(this, "taal", "#WatsonComponent_HelpText");

			return Task.CompletedTask;
		}

		private async Task ProcessNaturalLanguageCommandsAsync(SocketMessage socketMsg, string commandPrefix) {
			if (socketMsg is IUserMessage msg && !msg.Author.IsBot) {
				int argPos = 0;
				bool process = false;
				if (msg.Channel is IDMChannel && !msg.Content.StartsWith(commandPrefix)) {
					Logger.Info("WatsonComponent", $"Processing natlang command from {socketMsg.Author.Username}#{socketMsg.Author.Discriminator} in DM channel: {socketMsg.Content}");
					process = true;
				} else if (msg.Channel is IGuildChannel gch && msg.HasMentionPrefix(m_Client.CurrentUser, ref argPos)) {
					Logger.Info("WatsonComponent", $"Processing natlang command from {socketMsg.Author.Username}#{socketMsg.Author.Discriminator} in `{gch.Guild.Name}` channel `{gch.Name}`: {socketMsg.Content.Substring(argPos)}");
					process = true;
				}

				if (process) {
					await m_Watson.ProcessCommandAsync(msg, msg.Content.Substring(argPos));
				}
			}
		}
	}
}
