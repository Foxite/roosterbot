using System;
using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	public class RoosterCheckResult : CheckResult, IRoosterResult {
		public IReadOnlyList<object> ErrorReasonObjects { get; }
		public ComponentBase? ErrorReasonComponent { get; }

		private RoosterCheckResult(CommandError error, string errorReason, ComponentBase? errorReasonComponent, params object[] errorReasonObjects) : base(error, errorReason) {
			ErrorReasonObjects = errorReasonObjects;
			ErrorReasonComponent = errorReasonComponent;
		}

		private RoosterCheckResult() : base(null, null) {
			ErrorReasonObjects = Array.Empty<object>();
		}

		public static RoosterCheckResult FromError(string errorReason, ComponentBase errorReasonComponent, params object[] errorReasonObjects) {
			return new RoosterCheckResult(CommandError.UnmetPrecondition, errorReason, errorReasonComponent, errorReasonObjects);
		}

		internal static RoosterCheckResult FromErrorBuiltin(string errorReason, params object[] errorReasonObjects) {
			return new RoosterCheckResult(CommandError.UnmetPrecondition, errorReason, null, errorReasonObjects);
		}

		public static new RoosterCheckResult FromSuccess() => new RoosterCheckResult();
	}
}
