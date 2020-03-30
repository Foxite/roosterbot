using Qmmands;

namespace RoosterBot.Tools {
	[Name("#MotivationModule_Name")]
	public class MotivationModule : RoosterModule {
		public MotivationService Service { get; set; } = null!;

		[Command("#MotivationModule_Command_Name"), Description("#MotivationModule_Command_Description"), IgnoresExtraArguments]
		public CommandResult GetQuoteCommand() {
			return TextResult.Info(Service.GetQuote(Culture));
		}
	}
}
