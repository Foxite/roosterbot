namespace RoosterBot {
	/// <summary>
	/// An endpoint for the <see cref="Logger"/> class. All messages logged using <see cref="Logger"/> will be passed to every registered instance of this class.
	/// </summary>
	public abstract class LogEndpoint {
		/// <summary>
		/// Send a <see cref="LogMessage"/> to this endpoint.
		/// </summary>
		public abstract void Log(LogMessage message);
	}
}