using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#Meta_Name")]
	public class HelpModule : RoosterModule {
		public HelpService Help { get; set; } = null!;

		[Command("#HelpCommand"), Description("#HelpCommand_Section_Summary")]
		public Task<CommandResult> HelpCommand([Remainder, Name("#HelpCommand_Section")] string? section = null) {
			if (section == null) {
				return Result(new TextResult(null,
					GetString("HelpCommand_HelpPretext", GuildConfig.CommandPrefix) + "\n\n"
					+ GetString("HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n"
					+ string.Join(", ", Help.GetSectionNames(Culture)) + "\n\n"
					+ GetString("HelpCommand_PostText", GuildConfig.CommandPrefix)));
			} else if (Help.HelpSectionExists(Culture, section)) {
				return Result(new TextResult(null, string.Format(Help.GetHelpSection(Culture, section), GuildConfig.CommandPrefix)));
			} else {
				return Result(new TextResult(null,
					GetString("HelpCommand_ChapterDoesNotExist") + "\n\n"
					+ GetString("HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n"
					+ string.Join(", ", Help.GetSectionNames(Culture))));
			}
		}
	}
}