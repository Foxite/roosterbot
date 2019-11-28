using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.GLU {
	public class NewUserHandler {
		public NewUserHandler(DiscordSocketClient client) {
			client.UserJoined += WelcomeUser;
		}

		private async Task WelcomeUser(SocketGuildUser user) {
			if (user.Guild.Id == GLUComponent.GLUGuildId &&
				user.Guild.Channels.SingleOrDefault(channel => channel.Name == "welcome") is SocketTextChannel welcomeChannel) {
				string botCommandsMention = (user.Guild.Channels.Single(channel => channel.Name == "bot-commands") as SocketTextChannel)!.Mention;

				string text = $"Welkom {user.Mention},\n";
				text += "Je bent bijna klaar je hoeft alleen het volgende nog te doen.\n";
				text += "- Geef je naam en achternaam door in dit kanaal zodat een admin of mod je naam kan veranderen\n";
				text += $"- Stel in {botCommandsMention} jouw klas in door `!mijn klas <klas>` te sturen: bijvoorbeeld `!mijn klas 2gd1` of `!mijn klas 1ga2` Je krijgt dan automatisch een rang die jouw klas aangeeft.\n";
				text += $"Voor meer rangen kan je `?ranks` invoeren in {botCommandsMention}\n";
				text += "Verder ben ik altijd beschikbaar om het rooster te laten zien. Gebruik `!help rooster` of `!commands rooster` om te zien hoe ik werk.";

				await welcomeChannel.SendMessageAsync(text);
			}
		}
	}
}