using System;
using System.Collections.Generic;
using Qmmands;

namespace RoosterBot {
	public class RoosterTypeParserResult<T> : TypeParserResult<T>, IRoosterTypeParserResult {
		public IReadOnlyList<object> ErrorReasonObjects { get; }
		public Component? ErrorReasonComponent { get; }

		/// <summary>
		/// true if the input was valid for parsing <typeparamref name="T"/>, but no object could be returned due to another reason. false if the input was invalid. Undefined otherwise.
		/// </summary>
		public bool InputValid { get; }

		object? IRoosterTypeParserResult.Value => Value;

		protected RoosterTypeParserResult(T value) : base(value) {
			ErrorReasonObjects = Array.Empty<object>();
		}

		protected RoosterTypeParserResult(string reason, bool inputValid, Component? errorReasonComponent, params object[] errorReasonObjects) : base(reason) {
			ErrorReasonObjects = errorReasonObjects;
			ErrorReasonComponent = errorReasonComponent;
			InputValid = inputValid;
		}

		public static new RoosterTypeParserResult<T> Successful(T result) => new RoosterTypeParserResult<T>(result);

		public static RoosterTypeParserResult<T> Unsuccessful(bool inputValid, string reason, Component? errorReasonComponent, params object[] errorReasonObjects) {
			return new RoosterTypeParserResult<T>(reason, inputValid, errorReasonComponent, errorReasonObjects);
		}

		internal static RoosterTypeParserResult<T> UnsuccessfulBuiltIn(bool inputValid, string reason, params object[] errorReasonObjects) {
			return new RoosterTypeParserResult<T>(reason, inputValid, null, errorReasonObjects);
		}
	}

	public interface IRoosterTypeParserResult : IRoosterResult {
		public string Reason { get; }
		public bool HasValue { get; }
		public bool InputValid { get; }
		public object? Value { get; }
	}
}
