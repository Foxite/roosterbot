using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExceptionHandler : RoosterHandler {
		public ConfigService Config { get; set; } = null!;
		public ResourceService Resources { get; set; } = null!;
		public RoosterCommandService Commands { get; set; } = null!;

		public CommandExceptionHandler(IServiceProvider isp) : base(isp) {
			Commands.CommandExecutionFailed += HandleError;
		}

		private async Task HandleError(CommandExecutionFailedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				await Config.BotOwner.SendMessageAsync($"Exception during {args.Result.CommandExecutionStep.ToString()}\n{args.Result.Exception.ToStringDemystified()}");
				await rcc.RespondAsync(Util.Error + Resources.GetString(rcc.Culture, "RoosterBot_FatalError"));
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
