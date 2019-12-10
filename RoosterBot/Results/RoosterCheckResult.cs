using System;
using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	public class RoosterCheckResult : CheckResult {
		public IReadOnlyList<object> ErrorReasonObjects { get; }
		public Component? ErrorReasonComponent { get; }

		public static new RoosterCheckResult Successful => new RoosterCheckResult();

		private RoosterCheckResult(string errorReason, Component? errorReasonComponent, params object[] errorReasonObjects) : base(errorReason) {
			ErrorReasonObjects = errorReasonObjects;
			ErrorReasonComponent = errorReasonComponent;
		}

		private RoosterCheckResult() : base(null) {
			ErrorReasonObjects = Array.Empty<object>();
		}

		public static RoosterCheckResult Unsuccessful(string errorReason, Component errorReasonComponent, params object[] errorReasonObjects) {
			return new RoosterCheckResult(errorReason, errorReasonComponent, errorReasonObjects);
		}

		internal static RoosterCheckResult UnsuccessfulBuiltIn(string errorReason, params object[] errorReasonObjects) {
			return new RoosterCheckResult(errorReason, null, errorReasonObjects);
		}
	}
}
