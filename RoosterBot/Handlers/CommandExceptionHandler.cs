using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExceptionHandler {
		private readonly ConfigService m_Config;
		private readonly ResourceService m_Resources;

		public CommandExceptionHandler(ConfigService config, ResourceService resources, RoosterCommandService commands) {
			m_Config = config;
			m_Resources = resources;
			commands.CommandExecutionFailed += HandleError;
		}

		private async Task HandleError(CommandExecutionFailedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				await m_Config.BotOwner.SendMessageAsync($"Exception during {args.Result.CommandExecutionStep.ToString()}\n{args.Result.Exception.ToStringDemystified()}");
				await rcc.RespondAsync(Util.Error + m_Resources.GetString(rcc.Culture, "RoosterBot_FatalError"));
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
