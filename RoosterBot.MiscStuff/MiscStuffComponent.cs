﻿using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace RoosterBot.MiscStuff {
	public class MiscStuffComponent : ComponentBase {
		public string ConfigPath { get; private set; }

		public override Version ComponentVersion => new Version(1, 0, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			ConfigPath = configPath;

			services.AddSingleton(new CounterService(Path.Combine(configPath, "counters")));
			return Task.CompletedTask;
		}

		public override async Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.MiscStuff.Resources");

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<CounterModule>(services),
				commandService.AddModuleAsync<ModerationModule>(services)
			));

			string helpText = "#MiscStuffComponent_HelpText";
			help.AddHelpSection(this, "misc", helpText);

			services.GetService<DiscordSocketClient>().UserJoined += WelcomeUser;
		}

		private async Task WelcomeUser(SocketGuildUser user) {
			// TODO only in specific guilds
			if (user.Guild.Channels.SingleOrDefault(channel => channel.Name == "welcome") is SocketTextChannel welcomeChannel) {
				string botCommandsMention = (user.Guild.Channels.Single(channel => channel.Name == "bot-commands") as SocketTextChannel).Mention;

				string text = $"Welkom {user.Mention},\n";
				text +=  "Je bent bijna klaar je hoeft alleen het volgende nog te doen.\n";
				text +=  "- Geef je naam en achternaam door in de welcome chat zodat een admin of mod je naam kan veranderen\n";
				text += $"- Stel in {botCommandsMention} jouw klas in door `!ik <jouw klas>` te sturen: bijvoorbeeld `!ik 2gd1` of `!ik 1ga2` Je krijgt dan automatisch een rang die jouw klas aangeeft.\n";
				text += $"Voor meer rangen kan je `?ranks` invoeren in {botCommandsMention}\n";
				text +=  "Verder ben ik altijd beschikbaar om het rooster te laten zien. Gebruik `!help rooster` of `!commands rooster` om te zien hoe ik werk.";


				await welcomeChannel.SendMessageAsync(text);
			}
		}
	}
}
