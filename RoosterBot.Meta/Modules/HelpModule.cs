using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#MetaCommandsModule_Name")]
	public class HelpModule : RoosterModule {
		public HelpService Help { get; set; } = null!;

		[Command("#HelpModule_HelpCommand"), Description("#MetaCommandsModule_HelpCommand_Summary")]
		public Task<CommandResult> HelpCommand() {
			return Result(new TextResult(null,
				GetString("MetaCommandsModule_HelpCommand_HelpPretext", GuildConfig.CommandPrefix) + "\n\n"
				+ GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n"
				+ string.Join(", ", Help.GetSectionNames(Culture)) + "\n\n"
				+ GetString("MetaCommandsModule_HelpCommand_PostText", GuildConfig.CommandPrefix)));
		}

		[Command("#HelpModule_HelpCommand"), Description("#MetaCommandsModule_HelpCommand_Section_Summary")]
		public Task<CommandResult> HelpCommand([Remainder, Name("#MetaCommandsModule_HelpCommand_Section")] string section) {
			if (Help.HelpSectionExists(Culture, section)) {
				return Result(new TextResult(null, string.Format(Help.GetHelpSection(Culture, section), GuildConfig.CommandPrefix)));
			} else {
				return Result(new TextResult(null,
					GetString("MetaCommandsModule_HelpCommand_ChapterDoesNotExist") + "\n\n"
					+ GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n"
					+ string.Join(", ", Help.GetSectionNames(Culture))));
			}
		}
	}
}
