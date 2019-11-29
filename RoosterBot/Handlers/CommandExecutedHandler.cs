using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExecutedHandler {
		public CommandExecutedHandler(RoosterCommandService commands) {
			commands.CommandExecuted += HandleResultAsync;
		}

		public async Task HandleResultAsync(CommandExecutedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				if (args.Result is RoosterCommandResult rcr) {
					await rcr.PresentAsync(rcc);
				}
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
