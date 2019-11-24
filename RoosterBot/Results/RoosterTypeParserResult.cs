using System;
using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	public class RoosterTypeParserResult<T> : TypeParserResult<T>, IRoosterTypeParserResult {
		public IReadOnlyList<object> ErrorReasonObjects { get; }
		public ComponentBase? ErrorReasonComponent { get; }

		private RoosterTypeParserResult(T value) : base(value) {
			ErrorReasonObjects = Array.Empty<object>();
		}

		private RoosterTypeParserResult(string reason, ComponentBase? errorReasonComponent, params object[] errorReasonObjects) : base(reason) {
			ErrorReasonObjects = errorReasonObjects;
			ErrorReasonComponent = errorReasonComponent;
		}

		public static new RoosterTypeParserResult<T> Successful(T result) => new RoosterTypeParserResult<T>(result);

		public static RoosterTypeParserResult<T> Unsuccessful(string reason, ComponentBase? errorReasonComponent, params object[] errorReasonObjects) {
			return new RoosterTypeParserResult<T>(reason, errorReasonComponent, errorReasonObjects);
		}

		internal static RoosterTypeParserResult<T> UnsuccessfulBuiltIn(string reason, params object[] errorReasonObjects) {
			return new RoosterTypeParserResult<T>(reason, null, errorReasonObjects);
		}
	}

	internal interface IRoosterTypeParserResult : IRoosterResult {
		public string Reason { get; }
		public bool HasValue { get; }
	}
}
