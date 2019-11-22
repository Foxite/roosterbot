using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandSuccessHandler : PostCommandHandler {
		internal CommandSuccessHandler(RoosterCommandService commands) {
			commands.CommandExecuted += OnCommandExecuted;
		}

		public async Task OnCommandExecuted(CommandExecutedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				await rcc.UserConfig.UpdateAsync();
			} else {
				ForeignContext(args.Result.Command, args.Context, args.Result);
			}
		}
	}
}
