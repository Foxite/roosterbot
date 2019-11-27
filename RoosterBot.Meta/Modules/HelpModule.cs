using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#MetaCommandsModule_Name")]
	public class HelpModule : RoosterModuleBase {
		public HelpService Help { get; set; } = null!;
		public RoosterCommandService CmdService { get; set; } = null!;

		[Command("help"), Description("#MetaCommandsModule_HelpCommand_Summary")]
		public Task<CommandResult> HelpCommand() {
			string response = GetString("MetaCommandsModule_HelpCommand_HelpPretext", GuildConfig.CommandPrefix) + "\n\n";
			response += GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n";
			response += string.Join(", ", Help.GetSectionNames(Culture)) + "\n\n";

			response += GetString("MetaCommandsModule_HelpCommand_PostText", GuildConfig.CommandPrefix);

			return Result(new TextResult(null, response));
		}

		[Command("help"), Description("#MetaCommandsModule_HelpCommand_Section_Summary")]
		public Task<CommandResult> HelpCommand([Remainder, Name("#MetaCommandsModule_HelpCommand_Section")] string section) {
			string response = "";
			if (Help.HelpSectionExists(Culture, section)) {
				string helpText = Help.GetHelpSection(Culture, section);
				response += string.Format(helpText, GuildConfig.CommandPrefix);
			} else {
				response += GetString("MetaCommandsModule_HelpCommand_ChapterDoesNotExist") + "\n\n";
				response += GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n";
				response += string.Join(", ", Help.GetSectionNames(Culture));
			}
			
			return Result(new TextResult(null, response));
		}

		[Command("commands"), Description("#MetaCommandsModule_CommandListCommand_Summary")]
		public Task<CommandResult> CommandListCommand() {
			string response = GetString("MetaCommandsModule_CommandListCommand_CategoriesPretext", GuildConfig.CommandPrefix) + "\n";
			response += string.Join(", ", GetCategories().Select(grouping => grouping.Key));
			
			return Result(new TextResult(null, response));
		}

		[Command("commands"), Description("#MetaCommandsModule_CommandListCommand_Category_Summary")]
		public Task<CommandResult> CommandListCommand([Remainder, Name("#MetaCommandsModule_CommandListCommand_ModuleName")] string query) {
			query = query.ToLower();
			string response;

			IEnumerable<Command>? commands = GetCategories()
				.Where(category => category.Key.ToLower() == query)
				.SingleOrDefault();

			if (commands == null) {
				response = GetString("MetaCommandsModule_CommandListCommand_CategoryDoesNotExist") + "\n\n";
				response += GetCategories();
			} else {
				response = "";
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
				
			}
			
			return Result(new TextResult(null, response));
		}

		private IEnumerable<IGrouping<string, Command>> GetCategories() {
			bool shouldNotHide(dynamic moduleOrCommand) {
				return !(((IEnumerable<Attribute>) moduleOrCommand.Attributes).OfType<HiddenFromListAttribute>().Any() || 
						 ((IEnumerable<CheckAttribute>) moduleOrCommand.Checks).OfType<RequireCultureAttribute>().Any(attr => attr.Culture != Culture));
			}

			return
				from module in CmdService.GetAllModules(Context.Culture)
				from command in module.Commands
				where shouldNotHide(command.Module)
				where shouldNotHide(command)
				group command by command.Module.Name;
		}
	}
}
