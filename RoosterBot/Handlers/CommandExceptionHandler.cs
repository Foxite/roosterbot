using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExceptionHandler : RoosterHandler {
		public ConfigService Config { get; set; } = null!;
		public ResourceService Resources { get; set; } = null!;
		public RoosterCommandService Commands { get; set; } = null!;
		public NotificationService Notification { get; set; } = null!;
		public EmoteService Emotes { get; set; } = null!;

		public CommandExceptionHandler(IServiceProvider isp) : base(isp) {
			Commands.CommandExecutionFailed += HandleError;
		}

		private async Task HandleError(CommandExecutionFailedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				string report = $"{rcc.ToString()}\nException during {args.Result.CommandExecutionStep.ToString()}\n{args.Result.Exception.ToStringDemystified()}";
				Logger.Error("CommandException", report);
				if (report.Length > 2000) {
					const string TooLong = "The error message was longer than 2000 characters. This is the first section:\n";
					report = TooLong + report.Substring(0, 1999 - TooLong.Length);
				}
				await Notification.AddNotificationAsync(report);
				await rcc.RespondAsync(Emotes.Error(rcc.Platform) + Resources.GetString(rcc.Culture, "CommandHandling_FatalError"));
				await rcc.UserConfig.UpdateAsync();
			}
		}
	}
}
