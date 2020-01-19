using System;
using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all <see cref="CheckResult"/> used within RoosterBot.
	/// </summary>
	public class RoosterCheckResult : CheckResult {
		/// <summary>
		/// The list of objects used to format the <see cref="CheckResult.Reason"/>.
		/// </summary>
		public IReadOnlyList<object> ErrorReasonObjects { get; }

		/// <summary>
		/// The Component used when resolving the <see cref="CheckResult.Reason"/>.
		/// </summary>
		public Component? ErrorReasonComponent { get; }

		/// <summary>
		/// Get a successful result.
		/// </summary>
		public static new RoosterCheckResult Successful => new RoosterCheckResult();

		private RoosterCheckResult(string errorReason, Component? errorReasonComponent, params object[] errorReasonObjects) : base(errorReason) {
			ErrorReasonObjects = errorReasonObjects;
			ErrorReasonComponent = errorReasonComponent;
		}

		private RoosterCheckResult() : base(null) {
			ErrorReasonObjects = Array.Empty<object>();
		}

		/// <summary>
		/// Get an unsuccessful result with an error reason, the <see cref="Component"/> used when resolving the error reason, and the objects used to format the resolved string.
		/// </summary>
		public static RoosterCheckResult Unsuccessful(string errorReason, Component errorReasonComponent, params object[] errorReasonObjects) {
			return new RoosterCheckResult(errorReason, errorReasonComponent, errorReasonObjects);
		}

		internal static RoosterCheckResult UnsuccessfulBuiltIn(string errorReason, params object[] errorReasonObjects) {
			return new RoosterCheckResult(errorReason, null, errorReasonObjects);
		}
	}
}
