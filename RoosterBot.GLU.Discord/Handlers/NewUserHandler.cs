using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using RoosterBot.DiscordNet;
using RoosterBot.Schedule;

namespace RoosterBot.GLU.Discord {
	internal sealed class NewUserHandler {
		private readonly UserConfigService m_UCS;

		public NewUserHandler(UserConfigService ucs) {
			DiscordNetComponent.Instance.Client.UserJoined += WelcomeUser;
			m_UCS = ucs;
		}

		private async Task WelcomeUser(SocketGuildUser user) {
			if (user.Guild.Id == GLUDiscordComponent.GLUGuildId &&
				user.Guild.Channels.SingleOrDefault(channel => channel.Name == "welcome") is SocketTextChannel welcomeChannel) {
				string botCommandsMention = (user.Guild.Channels.Single(channel => channel.Name == "bot-commands") as SocketTextChannel)!.Mention;
				//string botCommandsMention = (user.Guild.GetChannel(346682476149866498) as SocketTextChannel)!.Mention;

				string text = $"Welkom {user.Mention},\n";

				UserConfig userConfig = await m_UCS.GetConfigAsync(new SnowflakeReference(DiscordNetComponent.Instance, user.Id));
				StudentSetInfo? ssi = userConfig.GetStudentSet();

				userConfig.TryGetData("glu.discord.nickname", out string? nickname);

				if (ssi == null || nickname == null) {
					text += "Je bent bijna klaar je hoeft alleen het volgende nog te doen.\n";
				}

				if (nickname == null) {
					text += "- Geef je naam en achternaam door in dit kanaal zodat een admin of mod je naam kan veranderen.\n";
				}

				if (ssi == null) {
					text += $"- Stel in {botCommandsMention} jouw klas in door `!ik zit in <klas>` te sturen: bijvoorbeeld `!ik zit in 2gd1` of `!ik zit in 1ga2`. Je krijgt dan automatisch een rang die jouw klas aangeeft.\n";
				}

				if (nickname != null) {
					text += "- Jouw naam is bij mij al bekend, dus die heb ik voor je ingesteld. Als het niet klopt, geef dan je voor- en achternaam door in dit kanaal zodat een admin of mod het voor je kan veranderen.\n";
					await user.ModifyAsync(props => props.Nickname = nickname);
				}

				if (ssi != null) {
					text += $"- Jouw klas is al bij mij bekend, dus ik heb jou de juiste rangen gegeven. Als dit niet klopt, ga dan naar {botCommandsMention} en gebruik `!ik zit in <klas>` om het te veranderen: bijvoorbeeld `!ik zit in 2gd1` of `!ik zit in 1ga2`.\n";
					await user.AddRolesAsync(user.StudentSetRoles(ssi).Where(item => item.Item2 == GluDiscordUtil.RemoveOrAdd.Add).Select(item => item.Role));
				}

				text += "\n";
				text += $"Voor meer rangen kan je `?ranks` invoeren in {botCommandsMention}\n";
				text += "Verder ben ik altijd beschikbaar om het rooster te laten zien. Gebruik `!help rooster` of `!commands rooster` om te zien hoe ik werk.";

				await welcomeChannel.SendMessageAsync(text);
			}
		}
	}
}
