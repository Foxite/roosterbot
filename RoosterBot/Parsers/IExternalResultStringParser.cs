namespace RoosterBot {
	/// <summary>
	/// Represents a RoosterTypeParser that gets its ErrorReason string from an external Component.
	/// </summary>
	public interface IExternalResultStringParser {
		/// <summary>
		/// The Component used when resolving a <see cref="Qmmands.TypeParserResult{T}.Reason"/>.
		/// </summary>
		Component ErrorReasonComponent { get; }
	}
}
