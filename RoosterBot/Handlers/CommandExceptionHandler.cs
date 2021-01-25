using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExceptionHandler {
		private NotificationService Notification { get; }

		public CommandExceptionHandler(IServiceProvider isp) {
			Notification = isp.GetRequiredService<NotificationService>();

			isp.GetRequiredService<RoosterCommandService>().CommandExecutionFailed += HandleError;
		}

		private async Task HandleError(CommandExecutionFailedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				string report = $"{rcc}\nException during {args.Result.CommandExecutionStep}";
				Logger.Error(Logger.Tags.Pipeline, report, args.Result.Exception);
				await Notification.AddNotificationAsync(report + "\n" + args.Result.Exception.ToStringDemystified());
				await rcc.RespondAsync(TextResult.Error(rcc.GetString("CommandHandling_FatalError")));
			}
		}
	}
}
