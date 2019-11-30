namespace RoosterBot.Schedule {
	public class RoomInfoParser : IdentifierInfoParserBase<RoomInfo> {
		public override string TypeDisplayName => "#RoomInfo_TypeDisplayName";

		public RoomInfoParser(Component component) : base (component) { }
	}
}
