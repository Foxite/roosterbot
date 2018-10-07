using Discord.Commands;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public class TestModule : EditableCmdModuleBase {
		public TestModule() : base() {
			LogTag = "TestModule";
		}
	}
}
