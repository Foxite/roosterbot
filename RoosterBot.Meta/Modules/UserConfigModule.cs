using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Meta {
	// TODO (localize) this module
	[/*LocalizedModule("nl-NL", "en-US"), */Group("i")]
	public class UserConfigModule : RoosterModuleBase {
		[Command("speak")]
		public Task GetLanguage() {
			if (UserConfig.Culture != null) {
				ReplyDeferred("Your language is " + UserConfig.Culture.EnglishName);
			} else {
				ReplyDeferred($"You do not have a language set; the guild's language {GuildConfig.Culture.EnglishName} will be used.");
			}
			return Task.CompletedTask;
		}

		[Command("speak")]
		public async Task SetLanguage(string language) {
			// TODO (feature) Users should not have to pass the language's internal name for this, use CultureNameService
			UserConfig.Culture = CultureInfo.GetCultureInfo(language);
			await UserConfig.UpdateAsync();
			ReplyDeferred($"{Util.Success}Your language is now {UserConfig.Culture.EnglishName}");
		}
	}
}
