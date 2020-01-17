using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExecutedHandler {
		public CommandExecutedHandler(IServiceProvider isp) {
			isp.GetRequiredService<RoosterCommandService>().CommandExecuted += HandleResultAsync;
		}

		public async Task HandleResultAsync(CommandExecutedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				if (args.Result is RoosterCommandResult rcr) {
					await rcc.RespondAsync(rcr);
				} else {
					if (!(args.Result is null)) {
						Logger.Warning("CommandHandler", $"A command has returned an unknown result of type {args.Result.GetType().Name}. It cannot be handled.");
					}
					
					string response = rcc.ServiceProvider.GetRequiredService<ResourceService>().GetString(rcc.Culture, "CommandHandling_Empty");
					await rcc.RespondAsync(TextResult.Info(response));
				}
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
