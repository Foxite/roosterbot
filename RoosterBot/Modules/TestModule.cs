using Discord.Commands;
using RoosterBot.Services;
using RoosterBot.Modules.Preconditions;
using System.Threading.Tasks;

namespace RoosterBot.Modules {
	internal class TestModule : EditableCmdModuleBase {
		public TestModule() : base() {
			LogTag = "TestModule";
		}
	}
}
