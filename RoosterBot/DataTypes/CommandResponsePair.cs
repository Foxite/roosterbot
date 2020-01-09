namespace RoosterBot {
	public class CommandResponsePair {
		// TODO snowflakereference
		public object CommandId { get; internal set; }
		public object ResponseId { get; internal set; }

		public CommandResponsePair(object commandId, object responseId) {
			CommandId = commandId;
			ResponseId = responseId;
		}
	}
}
