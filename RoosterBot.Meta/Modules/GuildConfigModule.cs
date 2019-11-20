using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Meta {
	[Group("config"), HiddenFromList]
	public class GuildConfigModule : RoosterModuleBase {
		[Command("prefix"), RequireBotManager]
		public Task GetCommandPrefix() {
			ReplyDeferred($"This guild's command prefix is `{GuildConfig.CommandPrefix}`");
			return Task.CompletedTask;
		}

		[Command("prefix"), RequireBotManager]
		public async Task SetCommandPrefix(string prefix) {
			GuildConfig.CommandPrefix = prefix;
			await GuildConfig.UpdateAsync();
			ReplyDeferred($"{Util.Success}This guild's command prefix is now `{GuildConfig.CommandPrefix}`");
		}

		[Command("language"), RequireBotManager]
		public Task GetLanguage() {
			ReplyDeferred($"This guild's language is {GuildConfig.Culture.Name}");
			return Task.CompletedTask;
		}

		[Command("language"), RequireBotManager]
		public async Task SetLanguage(string name) {
			GuildConfig.Culture = CultureInfo.GetCultureInfo(name);
			await GuildConfig.UpdateAsync();
			ReplyDeferred($"{Util.Success}This guild's language is now {GuildConfig.Culture.Name}");
		}
	}
}
