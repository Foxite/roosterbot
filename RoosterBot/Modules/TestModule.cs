#if DEBUG
using Discord.Commands;
using RoosterBot.Services;
using RoosterBot.Modules.Preconditions;
using System.Threading.Tasks;

namespace RoosterBot.Modules {
	[Attributes.LogTag("TestModule")]
	public class TestModule : EditableCmdModuleBase {
		[Command("error")]
		public Task TestErrorCommand() {
			throw new System.Exception("test error");
		}
	}
}
#endif