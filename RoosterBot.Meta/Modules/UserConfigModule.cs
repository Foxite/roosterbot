using System.Globalization;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#UserConfigModule_Name"), Group("#UserConfigModule_Group")]
	public class UserConfigModule : RoosterModule {
		public CultureNameService CultureNameService { get; set; } = null!;

		[Command("#UserConfigModule_Language"), Description("#UserConfigModule_Language_Description")]
		public Task<CommandResult> Language([Name("#UserConfigModule_Language_ParamName")] CultureInfo? culture = null) {
			if (culture == null) {
				if (UserConfig.Culture != null) {
					return Result(TextResult.Info(GetString("UserConfigModule_GetLanguage", CultureNameService.GetLocalizedName(UserConfig.Culture, UserConfig.Culture))));
				} else {
					return Result(TextResult.Info(GetString("UserConfigModule_GetLanguage_NoneSet", CultureNameService.GetLocalizedName(GuildConfig.Culture, GuildConfig.Culture))));
				}
			} else {
				UserConfig.Culture = culture;
				return Result(TextResult.Success(GetString("UserConfigModule_SetLanguage", CultureNameService.GetLocalizedName(UserConfig.Culture, UserConfig.Culture))));
			}
		}
	}
}
