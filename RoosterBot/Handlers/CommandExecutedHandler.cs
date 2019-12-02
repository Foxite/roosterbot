using System;
using System.Threading.Tasks;
using Qmmands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	internal sealed class CommandExecutedHandler : RoosterHandler {
		public CommandExecutedHandler(IServiceProvider isp) : base(isp) {
			isp.GetService<RoosterCommandService>().CommandExecuted += HandleResultAsync;
		}

		public async Task HandleResultAsync(CommandExecutedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				if (args.Result is RoosterCommandResult rcr) {
					if (rcr.UploadFilePath == null) {
						await rcc.RespondAsync(rcr.ToString());
					} else {
						await rcc.RespondAsync(rcr.ToString(), rcr.UploadFilePath);
					}
				} else {
					if (!(args.Result is null)) {
						Logger.Warning("CommandHandler", $"A command has returned an unknown result of type {args.Result.GetType().Name}. It cannot be handled.");
					}
					await rcc.RespondAsync(TextResult.Info("").ToString()); // This will produce a "No result" message
				}
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
