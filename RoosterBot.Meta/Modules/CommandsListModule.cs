using System;
using System.Collections.Generic;
using System.Linq;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#Meta_Name")]
	public class CommandsListModule : RoosterModule {
		public RoosterCommandService CmdService { get; set; } = null!;

		[Command("#Commands"), Description("#Commands_Category_Summary")]
		public CommandResult Commands([Remainder, Name("#Commands_ModuleName")] string? query = null) {
			if (query == null) {
				return new TextResult(null,
					GetString("Commands_CategoriesPretext", ChannelConfig.CommandPrefix) + "\n"
					+ string.Join(", ", GetCategories()));
			} else {
				query = query.ToLower();

				IEnumerable<Command> commands = GetCommands(query);

				if (commands.Any()) {
					string response = "";
					bool containsOptionalParameters = false;

					foreach (Command command in commands) {
						if (!containsOptionalParameters) {
							containsOptionalParameters = command.Parameters.Any(param => param.IsOptional);
						}
						response += "`" + ChannelConfig.CommandPrefix + command.GetSignature() + "`";
						if (command.Description != null) {
							response += ": " + string.Format(command.Description, ChannelConfig.CommandPrefix);
						}
						response += "\n";
					}

					if (containsOptionalParameters) {
						response += GetString("Commands_OptionalHint");
					}
					return new TextResult(null, response);
				} else {
					return new TextResult(null,
						GetString("Commands_CategoryDoesNotExist") + "\n\n"
						+ GetString("Commands_CategoriesPretext", ChannelConfig.CommandPrefix) + "\n"
						+ string.Join(", ", GetCategories()));
				}
			}
		}

		private bool ShouldNotHide(dynamic moduleOrCommand) {
			var hiddenFromList = ((IEnumerable<Attribute>) moduleOrCommand.Attributes).OfType<HiddenFromListAttribute>();

			// If the module/command has a HiddenFromListAttribute that explicitly allows the current culture, ignore the other rules.
			if (hiddenFromList.Any(attr => attr.VisibleInCultures.Contains(Culture))) {
				return true;
			}

			// Otherwise, hide this item if it has a HiddenFromListAttribute (that does not contain the current culture), or a RequireCultureAttribute that does not match the current culture.
			return !(
				hiddenFromList.Any() ||
				((IEnumerable<CheckAttribute>) moduleOrCommand.Checks).OfType<RequireCultureAttribute>().Any(attr => attr.Culture != Culture)
			);
		}

		private IEnumerable<Command> GetCommands(string category) =>
			from module in CmdService.GetAllModules(Context.Culture)
			where ShouldNotHide(module) && module.Name.ToLower() == category
			from command in module.Commands
			where ShouldNotHide(command)
			select command;

		private IEnumerable<string> GetCategories() => (
			from module in CmdService.GetAllModules(Context.Culture)
			where ShouldNotHide(module) && module.Commands.Any(command => ShouldNotHide(command))
			select module.Name
		).Distinct();
	}
}
