using System;
using System.Collections.Generic;
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
		/// The <see cref="IRoosterTypeParser"/> that generated this result.
		/// </summary>
		public IRoosterTypeParser Parser { get; }

		/// <summary>
		/// A list of objects used with <see cref="string.Format(string, object?[])"/> when handling the error, if this result is unsuccessful.
		/// </summary>
		public IReadOnlyList<object> ErrorReasonObjects { get; }

		/// <summary>
		/// Construct a new successful <see cref="RoosterTypeParserResult{T}"/>.
		/// </summary>
		protected RoosterTypeParserResult(IRoosterTypeParser parser, T value) : base(value) {
			Parser = parser;
			ErrorReasonObjects = Array.Empty<object>();
		}

		/// <summary>
		/// Construct a new unsuccessful <see cref="RoosterTypeParserResult{T}"/>.
		/// </summary>
		protected RoosterTypeParserResult(IRoosterTypeParser parser, bool inputValid, string reason, params object[] objects) : base(reason) {
			Parser = parser;
			InputValid = inputValid;
			ErrorReasonObjects = objects;
		}

		internal static RoosterTypeParserResult<T> Successful(IRoosterTypeParser parser, T result) => new RoosterTypeParserResult<T>(parser, result);

		internal static RoosterTypeParserResult<T> Unsuccessful(IRoosterTypeParser parser, bool inputValid, string reason, IReadOnlyList<object> objects) {
			return new RoosterTypeParserResult<T>(parser, inputValid, reason, objects);
		}

		internal static RoosterTypeParserResult<T> Unsuccessful(IRoosterTypeParser parser, bool inputValid, string reason, params object[] objects) {
			return new RoosterTypeParserResult<T>(parser, inputValid, reason, objects);
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
		/// A list of objects used with <see cref="string.Format(string, object?[])"/> when handling the error, if this result is unsuccessful.
		/// </summary>
		public IReadOnlyList<object> ErrorReasonObjects { get; }
		
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

		/// <summary>
		/// The <see cref="IRoosterTypeParser"/> that generated this result.
		/// </summary>
		public IRoosterTypeParser Parser { get; }
	}
}
