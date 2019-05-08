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
			
			string helpText = "Ik gebruik een machine-learning systeem waarmee ik spreektaal kan begrijpen.\n";
			helpText += "Om dit te gebruiken, mention je me aan het begin van een command en stel je me een vraag.\n";
			helpText += "Bijvoorbeeld:\n";
			helpText += "- `@RoosterBot wat heeft 2gd1 morgen?`\n";
			helpText += "- `@RoosterBot wat is er nu in a224`\n";
			helpText += "- `@RoosterBot waar is lance straks?`\n";
			helpText += "- `@RoosterBot wat heb ik hierna`\n";
			helpText += "- `@RoosterBot en daarna?`\n";
			helpText += "Dit wordt automatisch omgezet naar de goede command. Als ik je niet begrijp, reageer ik met een vraagteken.\n";
			helpText += "Dit systeem is nog in de betafase en is soms raar. Lees goed de eerste regel van het antwoord, zodat je zeker weet dat ik niet " +
				"dacht dat je \"nu\" bedoelde in plaats van \"straks\".";
			help.AddHelpSection("taal", helpText);

		}

		private Task ProcessNaturalLanguageCommandsAsync(SocketMessage socketMsg) {
			if (socketMsg is IUserMessage msg && !msg.Author.IsBot) {
				int argPos = 0;
				bool process = false;
				if (msg.Channel is IDMChannel && !msg.Content.StartsWith(m_Config.CommandPrefix)) {
					Logger.Log(LogSeverity.Info, "WatsonComponent", $"Processing natlang command from {socketMsg.Author.Username}#{socketMsg.Author.Discriminator} in DM channel: {socketMsg.Content}");
					process = true;
				} else if (msg.Channel is IGuildChannel gch && msg.HasMentionPrefix(m_Client.CurrentUser, ref argPos)) {
					Logger.Log(LogSeverity.Info, "WatsonComponent", $"Processing natlang command from {socketMsg.Author.Username}#{socketMsg.Author.Discriminator} in `{gch.Guild.Name}` channel `{gch.Name}`: {socketMsg.Content.Substring(argPos)}");
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
