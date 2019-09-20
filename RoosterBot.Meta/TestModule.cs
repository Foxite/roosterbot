using Discord.Commands;
using System.Threading.Tasks;

namespace RoosterBot.Meta {
	[LocalizedModule("nl-NL", "en-US")]
	public class TestModule : RoosterModuleBase<RoosterCommandContext> {
		[Command("#TestModule_TestCommand_Name")]
		public Task TestCommand() {
			return ReplyAsync(GetString("MetaCommandsModule_HelpCommand_Summary"));
		}
	}
}
