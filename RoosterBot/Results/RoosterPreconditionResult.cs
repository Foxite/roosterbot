using System;
using System.Collections.Generic;
using Discord.Commands;

namespace RoosterBot {
	public class RoosterPreconditionResult : PreconditionResult, IRoosterResult {
		public IReadOnlyList<object> ErrorReasonObjects { get; }
		public ComponentBase? ErrorReasonComponent { get; }

		private RoosterPreconditionResult(CommandError error, string errorReason, ComponentBase? errorReasonComponent, params object[] errorReasonObjects) : base(error, errorReason) {
			ErrorReasonObjects = errorReasonObjects;
			ErrorReasonComponent = errorReasonComponent;
		}

		private RoosterPreconditionResult() : base(null, null) {
			ErrorReasonObjects = Array.Empty<object>();
		}

		public static RoosterPreconditionResult FromError(string errorReason, ComponentBase errorReasonComponent, params object[] errorReasonObjects) {
			return new RoosterPreconditionResult(CommandError.UnmetPrecondition, errorReason, errorReasonComponent, errorReasonObjects);
		}

		internal static RoosterPreconditionResult FromErrorBuiltin(string errorReason, params object[] errorReasonObjects) {
			return new RoosterPreconditionResult(CommandError.UnmetPrecondition, errorReason, null, errorReasonObjects);
		}

		public static new RoosterPreconditionResult FromSuccess() => new RoosterPreconditionResult();
	}
}
