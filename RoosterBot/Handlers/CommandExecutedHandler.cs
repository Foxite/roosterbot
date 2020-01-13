using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExecutedHandler : RoosterHandler {
		public CommandExecutedHandler(IServiceProvider isp) : base(isp) {
			isp.GetService<RoosterCommandService>().CommandExecuted += HandleResultAsync;
		}

		public async Task HandleResultAsync(CommandExecutedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				if (args.Result is RoosterCommandResult rcr) {
					string response = rcr.ToString(rcc);
					if (response.Length == 0) {
						await rcc.RespondAsync(TextResult.Info(rcc.ServiceProvider.GetService<ResourceService>().GetString(rcc.Culture, "CommandHandling_Empty")));
					} else {
						await rcc.RespondAsync(rcr);
					}
				} else {
					if (!(args.Result is null)) {
						Logger.Warning("CommandHandler", $"A command has returned an unknown result of type {args.Result.GetType().Name}. It cannot be handled.");
					}
					
					string response = rcc.ServiceProvider.GetService<ResourceService>().GetString(rcc.Culture, "CommandHandling_Empty");
					await rcc.RespondAsync(TextResult.Info(response));
				}
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
