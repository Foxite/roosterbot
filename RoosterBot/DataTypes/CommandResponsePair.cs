namespace RoosterBot {
	/// <summary>
	/// A serialized reference to a user command and RoosterBot's response.
	/// </summary>
	public class CommandResponsePair {
		/// <summary>
		/// The command sent by the user.
		/// </summary>
		public SnowflakeReference Command { get; internal set; }

		/// <summary>
		/// The response sent by the RoosterBot.
		/// </summary>
		public SnowflakeReference Response { get; internal set; }

		/// <summary>
		/// Construct a new instance of CommandResponsePair.
		/// </summary>
		public CommandResponsePair(SnowflakeReference command, SnowflakeReference response) {
			Command = command;
			Response = response;
		}
	}
}
