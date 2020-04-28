using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using RoosterBot.DiscordNet;

namespace RoosterBot.GLU.Discord {
	public class OnlineClassesModule : RoosterModule {
		[Command("presentielijst"), RequirePrivate(false), RequireTeacher, HiddenFromList]
		public async Task<CommandResult> GetPresentStudents() {
			// Null-forgiveness, but the preconditions will prevent null from showing up here.
			IVoiceChannel? userChannel = ((Context.User as DiscordUser)!.DiscordEntity as IGuildUser)!.VoiceChannel;

			if (userChannel != null) {
				return TextResult.Info("Presente leerlingen:\n\n" + string.Join("\n",
					from user in await userChannel.GetUsersAsync().FlattenAsync()
					// Skip teachers
					where !user.RoleIds.Contains(688839445981560883ul)
					// Remove class
					let fullName = user.Nickname ?? user.Username
					select fullName.EndsWith(')') ? fullName.Substring(0, fullName.LastIndexOf('(')).Trim() : fullName
				));
			} else {
				return TextResult.Error("U zit niet in een voice-kanaal.");
			}
		}
	}
}
