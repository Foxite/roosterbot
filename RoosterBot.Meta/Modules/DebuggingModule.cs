using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[HiddenFromList, RequireBotManager, Group("debug")]
	public class DebuggingModule : RoosterModule {
		public UserConfigService UserConfigService { get; set; } = null!;

		[Command("userconfig")]
		public async Task<CommandResult> ShowUserConfig(IUser user) {
			UserConfig config;
			if (user.Platform.Name == Context.Platform.Name && user.Id == Context.User.Id) {
				config = Context.UserConfig;
			} else {
				config = await UserConfigService.GetConfigAsync(user.GetReference());
			}

			return TextResult.Info(config.GetRawData().ToString());
		}
	}
}
