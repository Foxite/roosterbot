using System.Globalization;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[LocalizedModule("nl-NL", "en-US"), Group("#GuildConfigModule_Group"), HiddenFromList]
	public class GuildConfigModule : RoosterModuleBase {
		public CultureNameService CultureNameService { get; set; } = null!;

		[Command("#GuildConfigModule_Prefix"), RequireBotManager]
		public async Task<CommandResult> CommandPrefix(string? prefix = null) {
			if (prefix == null) {
				return TextResult.Info(GetString("GuildConfigModule_GetPrefix", GuildConfig.CommandPrefix));
			} else {
				GuildConfig.CommandPrefix = prefix;
				await GuildConfig.UpdateAsync();
				return TextResult.Success(GetString("GuildConfigModule_SetPrefix", GuildConfig.CommandPrefix));
			}
		}

		[Command("#GuildConfigModule_Language"), RequireBotManager]
		public async Task<CommandResult> Language(CultureInfo? culture = null) {
			if (culture == null) {
				return TextResult.Info(GetString("GuildConfigModule_GetLanguage", CultureNameService.GetLocalizedName(GuildConfig.Culture, GuildConfig.Culture)));
			} else {
				GuildConfig.Culture = culture;
				await GuildConfig.UpdateAsync();
				return TextResult.Success(Util.Success + GetString("GuildConfigModule_SetLanguage", CultureNameService.GetLocalizedName(GuildConfig.Culture, GuildConfig.Culture)));
			}
		}
	}
}
