using System.Globalization;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[LocalizedModule("nl-NL", "en-US"), Name("#UserConfigModule_Name"), Group("#UserConfigModule_Group")]
	public class UserConfigModule : RoosterModuleBase {
		public CultureNameService CultureNameService { get; set; } = null!;

		[Command("#UserConfigModule_Language")]
		public Task Language([Name("#UserConfigModule_Language_ParamName")] CultureInfo? culture = null) {
			if (culture == null) {
				if (UserConfig.Culture != null) {
					ReplyDeferred(GetString("UserConfigModule_GetLanguage", CultureNameService.GetLocalizedName(UserConfig.Culture, UserConfig.Culture)));
				} else {
					ReplyDeferred(GetString("UserConfigModule_GetLanguage_NoneSet", CultureNameService.GetLocalizedName(GuildConfig.Culture, GuildConfig.Culture)));
				}
			} else {
				UserConfig.Culture = culture;
				ReplyDeferred(Util.Success + GetString("UserConfigModule_SetLanguage", CultureNameService.GetLocalizedName(UserConfig.Culture, UserConfig.Culture)));
			}
			return Task.CompletedTask;
		}
	}
}
