using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Meta {
	[LogTag("MetaModule"), Name("#" + nameof(Resources.MetaCommandsModule_Name))]
	public class HelpModule : EditableCmdModuleBase {
		public HelpService Help { get; set; }

		[Command("help"), Summary("#" + nameof(Resources.MetaCommandsModule_HelpCommand_Summary))]
		public async Task HelpCommand() {
			string response = string.Format(Resources.MetaCommandsModule_HelpCommand_HelpPretext, Config.CommandPrefix);

			bool notFirst = false;
			foreach (string helpSection in Help.GetSectionNames()) {
				if (notFirst) {
					response += ", ";
				}
				response += helpSection;
				notFirst = true;
			}

			response += Resources.MetaCommandsModule_HelpCommand_PostText;

			await ReplyAsync(response);
		}

		[Command("help"), Summary("#" + nameof(Resources.MetaCommandsModule_HelpCommand_Section_Summary))]
		public async Task HelpCommand([Remainder, Name("#" + nameof(Resources.MetaCommandsModule_HelpCommand_Section))] string section) {
			string response = "";
			if (Help.HelpSectionExists(section)) {
				response += Help.GetHelpSection(section);
			} else {
				response += Resources.MetaCommandsModule_HelpCommand_ChapterDoesNotExist;
			}
			await ReplyAsync(response);
		}
	}
}
