using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#MetaCommandsModule_Name")]
	public class CommandsListModule : RoosterModule {
		public RoosterCommandService CmdService { get; set; } = null!;

		[Command("#HelpModule_CommandsListCommand"), Description("#MetaCommandsModule_CommandListCommand_Summary")]
		public Task<CommandResult> CommandListCommand() {
			return Result(new TextResult(null,
				GetString("MetaCommandsModule_CommandListCommand_CategoriesPretext", GuildConfig.CommandPrefix) + "\n"
				+ string.Join(", ", GetCategories())));
		}

		[Command("#HelpModule_CommandsListCommand"), Description("#MetaCommandsModule_CommandListCommand_Category_Summary")]
		public Task<CommandResult> CommandListCommand([Remainder, Name("#MetaCommandsModule_CommandListCommand_ModuleName")] string query) {
			query = query.ToLower();

			IEnumerable<Command> commands = GetCommands(query);

			if (commands.Any()) {
				string response = "";
				bool containsOptionalParameters = false;

				foreach (Command command in commands) {
					if (!containsOptionalParameters) {
						containsOptionalParameters = command.Parameters.Any(param => param.IsOptional);
					}
					response += "`" + GuildConfig.CommandPrefix + command.GetSignature() + "`";
					if (command.Description != null) {
						response += ": " + string.Format(command.Description, GuildConfig.CommandPrefix);
					}
					response += "\n";
				}

				if (containsOptionalParameters) {
					response += GetString("MetaCommandsModule_CommandListCommand_OptionalHint");
				}
				return Result(new TextResult(null, response));
			} else {
				return Result(new TextResult(null,
					GetString("MetaCommandsModule_CommandListCommand_CategoryDoesNotExist") + "\n\n"
					+ GetString("MetaCommandsModule_CommandListCommand_CategoriesPretext", GuildConfig.CommandPrefix) + "\n"
					+ string.Join(", ", GetCategories())));
			}
		}

		private bool ShouldNotHide(dynamic moduleOrCommand) => !(
			((IEnumerable<Attribute>)  moduleOrCommand.Attributes).OfType<HiddenFromListAttribute>().Any() ||
			((IEnumerable<CheckAttribute>) moduleOrCommand.Checks).OfType<RequireCultureAttribute>().Any(attr => attr.Culture != Culture)
		);

		private IEnumerable<Command> GetCommands(string moduleName) =>
			from module in CmdService.GetAllModules(Context.Culture)
			where ShouldNotHide(module) && module.Name.ToLower() == moduleName
			from command in module.Commands
			where ShouldNotHide(command)
			select command;

		private IEnumerable<string> GetCategories() => (
			from module in CmdService.GetAllModules(Context.Culture)
			where ShouldNotHide(module)
			select module.Name
		).Distinct();
	}
}
