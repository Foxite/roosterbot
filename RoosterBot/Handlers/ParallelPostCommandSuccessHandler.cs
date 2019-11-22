using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	internal sealed class ParallelPostCommandSuccessHandler : ParallelPostCommandHandler {
		internal ParallelPostCommandSuccessHandler(RoosterCommandService commands) {
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
