using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RoosterBot;
using RoosterBot.Services;

namespace WatsonComponent {
	public class WatsonComponent : ComponentBase {
		private WatsonClient m_Watson;
		private DiscordSocketClient m_Client;
		private ConfigService m_Config;

		public override void AddServices(ref IServiceCollection services, string configPath) {
			string jsonFile = File.ReadAllText(Path.Combine(configPath, "Config.json"));
			JObject jsonConfig = JObject.Parse(jsonFile);

			m_Watson = new WatsonClient(m_Client, jsonConfig["watsonkey"].ToObject<string>(), jsonConfig["watsonid"].ToObject<string>());
		}

		public override void AddModules(IServiceProvider services, EditedCommandService commandService, HelpService help) {
			m_Config = services.GetService<ConfigService>();
			m_Client = services.GetService<DiscordSocketClient>();
			
			m_Client.MessageReceived += ProcessNaturalLanguageCommandsAsync;
		}

		private Task ProcessNaturalLanguageCommandsAsync(SocketMessage socketMsg) {
			if (socketMsg is IUserMessage msg && !msg.Author.IsBot) {
				int argPos = 0;
				bool process = false;
				if (msg.Channel is IDMChannel && !msg.Content.StartsWith(m_Config.CommandPrefix)) {
					Logger.Log(LogSeverity.Info, "ScheduleComponent", $"Processing natlang command from {socketMsg.Author.Username}#{socketMsg.Author.Discriminator} in DM channel {socketMsg.Content}");
					process = true;
				} else if (msg.Channel is IGuildChannel gch && msg.HasMentionPrefix(m_Client.CurrentUser, ref argPos)) {
					Logger.Log(LogSeverity.Info, "ScheduleComponent", $"Processing natlang command from {socketMsg.Author.Username}#{socketMsg.Author.Discriminator} in `{gch.Guild.Name}` channel `{gch.Name}`: {socketMsg.Content.Substring(argPos)}");
					process = true;
				}

				if (process) {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
					m_Watson.ProcessCommandAsync(msg, msg.Content.Substring(argPos));
#pragma warning restore CS4014
				}
			}
			return Task.CompletedTask;
		}
	}
}
