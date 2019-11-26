using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandSuccessHandler : PostCommandHandler {
		internal CommandSuccessHandler(RoosterCommandService commands) {
			commands.CommandExecuted += OnCommandExecuted;
		}

		public async Task OnCommandExecuted(CommandExecutedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				if (args.Result != null) {
					if (args.Result is RoosterCommandResult result) {
						await rcc.RespondAsync(result.Present());
					} else {
						Logger.Warning("CommandHandler", "A successful command has returned an unknown Result type: " + args.Result.GetType().FullName + ". It will not be used.");
					}
				}
				await rcc.UserConfig.UpdateAsync();
			} else {
				ForeignContext(args.Result.Command, args.Context, args.Result);
			}
		}
	}
}
