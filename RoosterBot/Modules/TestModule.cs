#if DEBUG
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Services;
using RoosterBot.Preconditions;
using System.Threading.Tasks;

namespace RoosterBot.Modules {
	[LogTag("TestModule"), HiddenFromList]
	public class TestModule : EditableCmdModuleBase {
		public SNSService SNS { get; set; }

		[Command("sns"), RequireOwner, HiddenFromList]
		public async Task TestErrorCommand() {
			await SNS.SendCriticalErrorNotificationAsync("test message");
		}
	}
}
#endif