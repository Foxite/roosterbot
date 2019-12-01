﻿using System;
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
					await rcr.PresentAsync(rcc);
				} else {
					if (!(args.Result is null)) {
						Logger.Warning("CommandHandler", $"A command has returned an unknown result of type {args.Result.GetType().Name}. It cannot be handled.");
					}
					await TextResult.Info("").PresentAsync(rcc);
				}
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}