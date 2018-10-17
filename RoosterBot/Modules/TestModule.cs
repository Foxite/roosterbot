using Discord.Commands;
using RoosterBot.Services;
using RoosterBot.Modules.Preconditions;
using System.Threading.Tasks;

namespace RoosterBot.Modules {
	public class TestModule : EditableCmdModuleBase {
		public TestModule() : base() {
			LogTag = "TestModule";
		}
	}
}
