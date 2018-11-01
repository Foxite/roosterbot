using System;
using System.Text.RegularExpressions;
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

			Console.WriteLine(message.Author.Username + "#" + message.Author.Discriminator + " : ID " + message.Author.Id);
			if (message.Author.Id == 244147515375484928) {
				Console.WriteLine("kevin");
			}

			if (message.Content == "?slots" && Util.RNG.NextDouble() < 0.02) {
				// Play slots
				await Task.Delay(1500);
				await command.Channel.SendMessageAsync("?slots");
			} else if (message.Author.Id == 133798410024255488 /*244147515375484928 /* Kevin Rommes#1429 */) {
				string contentLower = message.Content.ToLower();
				if (contentLower.Contains("snappie") ||
					contentLower.Contains("snap je") ||
					contentLower.Contains("snapje")  ||
					Regex.Match(message.Content, @"(.*)\*\*(.+)\*\*(.*)").Success) { // Text in bold - he usually does that when making a pun

					if ((DateTime.Now - m_LastKevinPunResponse).TotalSeconds >= 3) {
						m_LastKevinPunResponse = DateTime.Now;
						await command.Channel.SendMessageAsync("Ja Kevin, leuke pun.");
					}
				}
			}
		}
	}
}
