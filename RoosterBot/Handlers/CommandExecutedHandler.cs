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
					string response = rcr.ToString();
					if (response.Length == 0) {
						response = rcc.ServiceProvider.GetService<ResourceService>().GetString(rcc.Culture, "CommandHandling_Empty");
					}

					if (rcr.UploadFilePath == null) {
						await rcc.RespondAsync(response);
					} else {
						await rcc.RespondAsync(response, rcr.UploadFilePath);
					}
				} else {
					if (!(args.Result is null)) {
						Logger.Warning("CommandHandler", $"A command has returned an unknown result of type {args.Result.GetType().Name}. It cannot be handled.");
					}
					
					string response = rcc.ServiceProvider.GetService<ResourceService>().GetString(rcc.Culture, "CommandHandling_Empty");
					await rcc.RespondAsync(TextResult.Info(response).ToString());
				}
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
