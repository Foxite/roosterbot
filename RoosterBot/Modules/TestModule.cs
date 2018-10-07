using Discord.Commands;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public class TestModule : EditableCmdModuleBase {
		public ConfigService Config { get; set; }
		
		private readonly string LogTag;

		public TestModule() : base() {
			LogTag = "TestModule";
		}
	}
}
