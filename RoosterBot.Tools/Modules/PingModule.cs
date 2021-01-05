using System;
using Qmmands;

namespace RoosterBot.Tools {
	[HiddenFromList]
	public class PingModule : RoosterModule {
		[Command("ping"), IgnoresExtraArguments]
		public RoosterCommandResult Ping() {
			return TextResult.Info(((int) (DateTimeOffset.Now - Context.Message.SentAt).TotalMilliseconds) + " ms");
		}
	}
}
