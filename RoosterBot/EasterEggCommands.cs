using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace RoosterBot {
	// Not a module.
	internal class EasterEggCommands {
		private DateTime m_LastKevinPunResponse = DateTime.MinValue; // transient - does not carry over between instances of the bot program

		internal async Task TryEasterEggCommands(IDiscordClient client, SocketMessage command) {
			if (!(command is SocketUserMessage message))
				return;

			if (message.Author.Id == client.CurrentUser.Id) {
				return;
			}
			
			if (message.Content == "?slots" && Util.RNG.NextDouble() < 0.02) {
				// Play slots
				await Task.Delay(1500);
				await command.Channel.SendMessageAsync("?slots");
			}
		}
	}
}
