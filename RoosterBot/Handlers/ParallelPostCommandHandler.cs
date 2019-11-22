using System;
using Qmmands;

namespace RoosterBot {
	internal abstract class ParallelPostCommandHandler {
		protected void ForeignContext(Command? command, CommandContext context, IResult result) {
			var nse = new NotSupportedException($"A command has been executed that used a context of type {context.GetType().Name}. RoosterBot does not support this as of version 2.1. " +
								"All command context objects must be derived from RoosterCommandContext. " +
								$"Starting in RoosterBot 3.0, it will no longer be possible to add modules that are not derived from {nameof(RoosterModuleBase)}. " +
								"This exception object contains useful information in its Data property; use a debugger to see where this error came from.");
			nse.Data.Add("command", command);
			nse.Data.Add("context", context);
			nse.Data.Add("result", result);
			throw nse;
		}
	}
}
