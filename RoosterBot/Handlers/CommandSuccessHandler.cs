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
						await result.PresentAsync(rcc);
					} else {
						Logger.Warning("CommandHandler", $"A successful command has returned Result that does not derive from {nameof(RoosterCommandResult)}: {args.Result.GetType().FullName}. It will not be used.");
					}
				}
				await rcc.UserConfig.UpdateAsync();
			} else {
				ForeignContext(args.Result.Command, args.Context, args.Result);
			}
		}
	}
}
