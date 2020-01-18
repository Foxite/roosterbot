using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The base class for all <see cref="TypeParserResult{T}"/>s used within RoosterBot.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public class RoosterTypeParserResult<T> : TypeParserResult<T>, IRoosterTypeParserResult {
		/// <summary>
		/// true if the input was valid for parsing <typeparamref name="T"/>, but no object could be returned due to another reason. false if the input was invalid. Undefined otherwise.
		/// </summary>
		public bool InputValid { get; }

		/// <summary>
		/// The non-generic value.
		/// </summary>
		object? IRoosterTypeParserResult.Value => Value;

		/// <summary>
		/// Construct a new successful <see cref="RoosterTypeParserResult{T}"/>.
		/// </summary>
		protected RoosterTypeParserResult(T value) : base(value) { }

		/// <summary>
		/// Construct a new unsuccessful <see cref="RoosterTypeParserResult{T}"/>.
		/// </summary>
		protected RoosterTypeParserResult(string reason, bool inputValid) : base(reason) {
			InputValid = inputValid;
		}

		/// <summary>
		/// Get a successful result.
		/// </summary>
		public static new RoosterTypeParserResult<T> Successful(T result) => new RoosterTypeParserResult<T>(result);

		/// <summary>
		/// Get an unsuccessful result.
		/// </summary>
		public static RoosterTypeParserResult<T> Unsuccessful(bool inputValid, string reason) {
			return new RoosterTypeParserResult<T>(reason, inputValid);
		}

		internal static RoosterTypeParserResult<T> UnsuccessfulBuiltIn(bool inputValid, string reason) {
			return new RoosterTypeParserResult<T>(reason, inputValid);
		}
	}

	/// <summary>
	/// A non-generic interface for <see cref="RoosterTypeParserResult{T}"/>.
	/// </summary>
	public interface IRoosterTypeParserResult {
		/// <summary>
		/// Indicates if the result is successful.
		/// </summary>
		public bool IsSuccessful { get; }

		/// <summary>
		/// If <see cref="IsSuccessful"/> is <see langword="false"/>, then this is the human-friendly reason for failure.
		/// </summary>
		public string Reason { get; }
		
		/// <summary>
		/// Indicates if the result has a value.
		/// </summary>
		public bool HasValue { get; }

		/// <summary>
		/// If <see cref="IsSuccessful"/> is <see langword="false"/>, then this indicates if the failure occurred because of an unspecified resolution error, rather than invalid input.
		/// </summary>
		public bool InputValid { get; }

		/// <summary>
		/// The value for the result.
		/// </summary>
		public object? Value { get; }
	}
}
