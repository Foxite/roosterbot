﻿using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace RoosterBot.Schedule.GLU {
	internal sealed class ManualRanksHintHandler {
		public ManualRanksHintHandler(DiscordSocketClient client) {
			client.MessageReceived += HintManualRanks;
		}

		private async Task HintManualRanks(SocketMessage msg) {
			if (msg is SocketUserMessage sum) {
				int argPos = 0;
				if (sum.HasStringPrefix("?rank ", ref argPos)) {
					string normalContent = sum.Content.ToLower();
					bool respond = false;
					if (normalContent.EndsWith("e jaar")) {
						respond = true;
					} else {
						string manualRank = normalContent.Substring(argPos);
						if (manualRank == "developer" || manualRank == "artist") {
							respond = true;
						}
					}

					if (respond) {
						await msg.Channel.SendMessageAsync(msg.Author.Mention +
							", je hoeft niet meer handmatig aan te geven aan Dyno in welke jaar/opleiding je zit. " +
						   "Dit wordt nu automatisch gedaan als je je klas instelt met `!ik <jouw klas>`.");
					}
				}
			}
		}
	}
}
