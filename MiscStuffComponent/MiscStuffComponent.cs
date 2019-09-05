﻿using RoosterBot;
using RoosterBot.Services;
using Microsoft.Extensions.DependencyInjection;
using MiscStuffComponent.Services;
using MiscStuffComponent.Modules;
using System.IO;
using System;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;

namespace MiscStuffComponent {
	public class MiscStuffComponent : ComponentBase {
		public string ConfigPath { get; private set; }

		public override string VersionString => "1.0.0";

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

			services.GetService<DiscordSocketClient>().UserJoined += WelcomeUser;
		}

		private async Task WelcomeUser(SocketGuildUser user) {
			if (user.Guild.Channels.SingleOrDefault(channel => channel.Name == "welcome") is SocketTextChannel welcomeChannel) {
				string text = $"Welkom {user.Mention},\n";
				text += "Je ben bijna klaar je hoeft alleen het volgende nog te doen.\n";
				text += "- Geef je naam door in de welcome chat zodat een admin of mod je naam kan veranderen\n";
				text += "- Voer in bot-commands het command `?rank developer` of `?rank artist` in om een rang te krijgen\n\n";

				string botCommandsMention = (user.Guild.Channels.Single(channel => channel.Name == "bot-commands") as SocketTextChannel).Mention;
				text += $"Voor meer rangen kan je `?ranks` invoeren in {botCommandsMention}";

				await welcomeChannel.SendMessageAsync(text);
			}
		}
	}
}
