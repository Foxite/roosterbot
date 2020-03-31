using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using RoosterBot.DiscordNet;

namespace RoosterBot.GLU.Discord {
	// TODO if we're going to deploy to the official school guild, then we need to make sure there are no irrelevant commands there that can be used.
	// Exclude everything except:
	// - This module
	// - ScheduleModule (not all other things from Schedule)
	// - Meta modules
	public class OnlineClassesModule : RoosterModule {
		[Command("presentielijst"), RequirePrivate(false), RequireTeacher, HiddenFromList]
		public async Task<CommandResult> GetPresentStudents() {
			IVoiceChannel? userChannel = ((Context.User as DiscordUser)!.DiscordEntity as IGuildUser)!.VoiceChannel;

			if (userChannel != null) {
				return TextResult.Info("Presente leerlingen:\n- " + string.Join("\n- ",
					// Null-forgiveness, but the preconditions will prevent null from showing up here.
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
