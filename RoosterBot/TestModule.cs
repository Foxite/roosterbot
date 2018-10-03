using Discord.Commands;

namespace RoosterBot {
	public class TestModule : EditableCmdModuleBase {
		public ConfigService Config { get; set; }

		public TestModule() : base() { }
	}
}
