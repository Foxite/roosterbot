namespace RoosterBot {
	public class CommandResponsePair {
		public ulong CommandId { get; internal set; }
		public ulong ResponseId { get; internal set; }

		public CommandResponsePair(ulong commandId, ulong responseId) {
			CommandId = commandId;
			ResponseId = responseId;
		}
	}
}
