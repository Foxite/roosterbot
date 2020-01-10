namespace RoosterBot {
	public class CommandResponsePair {
		public SnowflakeReference Command { get; internal set; }
		public SnowflakeReference Response { get; internal set; }

		public CommandResponsePair(SnowflakeReference command, SnowflakeReference response) {
			Command = command;
			Response = response;
		}
	}
}
