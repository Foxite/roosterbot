using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.DiscordNet {
	[Name("#InfoModule_Name")]
	public class InfoModule : RoosterModule {
		[Command("#InfoModule_DiscordInvite_CommandName"), Description("#InfoModule_DiscordInvite_Description")]
		public CommandResult DiscordServerLinkCommand() {
			return TextResult.Info(GetString("InfoModule_DiscordInvite", DiscordNetComponent.Instance.DiscordLink));
		}

		[Command("poll"), HiddenFromList, UserIsModerator]
		public async Task MakePoll([Remainder] string question) {
			IUserMessage msg = await ((DiscordCommandContext) Context).Channel.SendMessageAsync(question);
			await msg.AddReactionAsync(new Discord.Emoji("👍"));
			await msg.AddReactionAsync(new Discord.Emoji("👎"));
		}
	}
}
