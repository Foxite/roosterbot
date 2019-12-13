namespace RoosterBot {
	/// <summary>
	/// Represents a RoosterTypeParser that gets its ErrorReason string from an external Component.
	/// </summary>
	public interface IExternalResultStringParser {
		Component ErrorReasonComponent { get; }
	}
}
