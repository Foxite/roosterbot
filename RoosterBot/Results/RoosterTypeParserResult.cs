using Qmmands;

namespace RoosterBot {
	public class RoosterTypeParserResult<T> : TypeParserResult<T>, IRoosterTypeParserResult {
		/// <summary>
		/// true if the input was valid for parsing <typeparamref name="T"/>, but no object could be returned due to another reason. false if the input was invalid. Undefined otherwise.
		/// </summary>
		public bool InputValid { get; }

		object? IRoosterTypeParserResult.Value => Value;

		protected RoosterTypeParserResult(T value) : base(value) { }

		protected RoosterTypeParserResult(string reason, bool inputValid) : base(reason) {
			InputValid = inputValid;
		}

		public static new RoosterTypeParserResult<T> Successful(T result) => new RoosterTypeParserResult<T>(result);

		public static RoosterTypeParserResult<T> Unsuccessful(bool inputValid, string reason) {
			return new RoosterTypeParserResult<T>(reason, inputValid);
		}

		internal static RoosterTypeParserResult<T> UnsuccessfulBuiltIn(bool inputValid, string reason) {
			return new RoosterTypeParserResult<T>(reason, inputValid);
		}
	}

	public interface IRoosterTypeParserResult {
		public bool IsSuccessful { get; }
		public string Reason { get; }
		public bool HasValue { get; }
		public bool InputValid { get; }
		public object? Value { get; }
	}
}
