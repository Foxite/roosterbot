namespace RoosterBot {
	/// <summary>
	/// Indicates the severity of a <see cref="LogMessage"/>.
	/// </summary>
	public enum LogLevel {
		/// <summary>
		/// Verbose level. Very detailed logging, usually not used in release builds.
		/// </summary>
		Verbose,

		/// <summary>
		/// Debug level. Detailed logging, helps to determine when a program experienced a problem.
		/// </summary>
		Debug,

		/// <summary>
		/// Informational level. Expected events.
		/// </summary>
		Info,

		/// <summary>
		/// Warning level. Unexpected events, that may require attention.
		/// </summary>
		Warning,

		/// <summary>
		/// Error level. Events that should not have happened, for example an exception.
		/// </summary>
		Error,

		/// <summary>
		/// Critical level. The program will not be able to continue running after this event.
		/// </summary>
		Critical
	}
}
