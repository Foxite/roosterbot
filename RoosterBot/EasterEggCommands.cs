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
			
			if (message.Content == "?slots" && Util.RNG.NextDouble() < 0.02) {
				// Play slots
				await Task.Delay(1500);
				await command.Channel.SendMessageAsync("?slots");
			} else if (message.Author.Id == 244147515375484928 /* Kevin Rommes#1429 */) {
				string contentLower = message.Content.ToLower();
				if (contentLower.Contains("snappie") ||
					contentLower.Contains("snap je") ||
					contentLower.Contains("snapje")/*||
					Regex.Match(message.Content, @"(.*)\*\*(.+)\*\*(.*)").Success*/) { // Text in bold - he usually does that when making a pun but it's probably better to wait for the "get it"

					if ((DateTime.Now - m_LastKevinPunResponse).TotalSeconds >= 3) {
						m_LastKevinPunResponse = DateTime.Now;
						await command.Channel.SendMessageAsync("Ja Kevin, leuke pun.");
					}
				}
			}
		}
	}
}
