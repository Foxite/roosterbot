using Qmmands;

namespace RoosterBot.Meta {
	[Name("#Meta_Name")]
	public class HelpModule : RoosterModule {
		public HelpService Help { get; set; } = null!;

		[Command("#HelpCommand"), Description("#HelpCommand_Section_Summary")]
		public CommandResult HelpCommand([Remainder, Name("#HelpCommand_Section")] string? section = null) {
			if (section == null) {
				return new TextResult(null,
					GetString("HelpCommand_HelpPretext", ChannelConfig.CommandPrefix) + "\n\n"
					+ GetString("HelpCommand_HelpSectionsPretext", ChannelConfig.CommandPrefix) + "\n"
					+ string.Join(", ", Help.GetSectionNames(Culture)) + "\n\n"
					+ GetString("HelpCommand_PostText", ChannelConfig.CommandPrefix));
			} else if (Help.HelpSectionExists(Culture, section)) {
				return new TextResult(null, string.Format(Help.GetHelpSection(Culture, section), ChannelConfig.CommandPrefix));
			} else {
				return new TextResult(null,
					GetString("HelpCommand_ChapterDoesNotExist") + "\n\n"
					+ GetString("HelpCommand_HelpSectionsPretext", ChannelConfig.CommandPrefix) + "\n"
					+ string.Join(", ", Help.GetSectionNames(Culture)));
			}
		}
	}
}