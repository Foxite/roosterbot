using Discord.Commands;

namespace RoosterBot {
	public class TestModule : EditableCmdModuleBase {
		public ConfigService Config { get; set; }
		
		private readonly string LogTag;

		public TestModule() : base() {
			LogTag = "TestModule";
		}
	}
}
