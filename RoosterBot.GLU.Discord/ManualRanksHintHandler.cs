using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using RoosterBot.DiscordNet;

namespace RoosterBot.GLU.Discord {
	internal sealed class ManualRanksHintHandler {
		public ManualRanksHintHandler() {
			DiscordNetComponent.Instance.Client.MessageReceived += HintManualRanks;
		}

		private async Task HintManualRanks(SocketMessage msg) {
			if (msg is SocketUserMessage sum && msg.Channel is IGuildChannel igu && igu.GuildId == GLUDiscordComponent.GLUGuildId) {
				int argPos = 0;
				if (sum.Content.StartsWith("?rank ")) {
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
						   "Dit wordt nu automatisch gedaan als je je klas instelt met `!ik zit in <klas>`.");
					}
				}
			}
		}
	}
}
