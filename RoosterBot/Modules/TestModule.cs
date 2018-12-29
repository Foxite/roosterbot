#if DEBUG
using Discord.Commands;
using RoosterBot.Services;
using RoosterBot.Modules.Preconditions;
using System.Threading.Tasks;

namespace RoosterBot.Modules {
	[Attributes.LogTag("TestModule")]
	public class TestModule : EditableCmdModuleBase {
		public SNSService SNS { get; set; }

		[Command("sns"), RequireOwner]
		public async Task TestErrorCommand() {
			await SNS.SendCriticalErrorNotificationAsync("test message");
		}
	}
}
#endif