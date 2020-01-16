using System.Globalization;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Group("#ChannelConfigModule_Group"), HiddenFromList]
	public class ChannelConfigModule : RoosterModule {
		public CultureNameService CultureNameService { get; set; } = null!;

		[Command("#ChannelConfigModule_Prefix"), RequireBotManager]
		public async Task<CommandResult> CommandPrefix(string? prefix = null) {
			if (prefix == null) {
				return TextResult.Info(GetString("ChannelConfigModule_GetPrefix", ChannelConfig.CommandPrefix));
			} else {
				ChannelConfig.CommandPrefix = prefix;
				await ChannelConfig.UpdateAsync();
				return TextResult.Success(GetString("ChannelConfigModule_SetPrefix", ChannelConfig.CommandPrefix));
			}
		}

		[Command("#ChannelConfigModule_Language"), RequireBotManager]
		public async Task<CommandResult> Language(CultureInfo? culture = null) {
			if (culture == null) {
				return TextResult.Info(GetString("ChannelConfigModule_GetLanguage", CultureNameService.GetLocalizedName(ChannelConfig.Culture, ChannelConfig.Culture)));
			} else {
				ChannelConfig.Culture = culture;
				await ChannelConfig.UpdateAsync();
				return TextResult.Success(GetString("ChannelConfigModule_SetLanguage", CultureNameService.GetLocalizedName(ChannelConfig.Culture, ChannelConfig.Culture)));
			}
		}
	}
}
