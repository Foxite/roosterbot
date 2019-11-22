﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#MetaCommandsModule_Name")]
	public class HelpModule : RoosterModuleBase {
		public HelpService Help { get; set; } = null!;

		[Command("help"), Description("#MetaCommandsModule_HelpCommand_Summary")]
		public Task HelpCommand() {
			string response = GetString("MetaCommandsModule_HelpCommand_HelpPretext", GuildConfig.CommandPrefix) + "\n\n";
			response += GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n";
			response += string.Join(", ", Help.GetSectionNames(Culture)) + "\n\n";

			response += GetString("MetaCommandsModule_HelpCommand_PostText", GuildConfig.CommandPrefix);

			ReplyDeferred(response);

			return Task.CompletedTask;
		}

		[Command("help"), Description("#MetaCommandsModule_HelpCommand_Section_Summary")]
		public Task HelpCommand([Remainder, Name("#MetaCommandsModule_HelpCommand_Section")] string section) {
			string response = "";
			if (Help.HelpSectionExists(Culture, section)) {
				string helpText = Help.GetHelpSection(Culture, section);
				response += string.Format(helpText, GuildConfig.CommandPrefix);
			} else {
				response += GetString("MetaCommandsModule_HelpCommand_ChapterDoesNotExist") + "\n\n";
				response += GetString("MetaCommandsModule_HelpCommand_HelpSectionsPretext", GuildConfig.CommandPrefix) + "\n";
				response += string.Join(", ", Help.GetSectionNames(Culture));
			}
			ReplyDeferred(response);

			return Task.CompletedTask;
		}

		[Command("commands"), Description("#MetaCommandsModule_CommandListCommand_Summary")]
		public Task CommandListCommand() {
			string ret = GetString("MetaCommandsModule_CommandListCommand_CategoriesPretext", GuildConfig.CommandPrefix) + "\n";
			ret += string.Join(", ", GetCategories().Select(grouping => grouping.Key));

			ReplyDeferred(ret);
			return Task.CompletedTask;
		}

		[Command("commands"), Description("#MetaCommandsModule_CommandListCommand_Category_Summary")]
		public Task CommandListCommand([Remainder, Name("#MetaCommandsModule_CommandListCommand_ModuleName")] string query) {
			query = query.ToLower();

			IEnumerable<(Command command, ComponentBase component)>? commands = GetCategories()
				.Where(category => category.Key.ToLower() == query)
				.Select(category => category as IEnumerable<(Command command, ComponentBase component)>)
				.SingleOrDefault();
			
			if (commands == null) {
				string response = GetString("MetaCommandsModule_CommandListCommand_CategoryDoesNotExist") + "\n\n";
				response += GetCategories();
				base.ReplyDeferred(response);
			} else {
				string response = "";
				bool containsOptionalParameters = false;

				foreach ((Command command, ComponentBase component) in commands) {
					if (!containsOptionalParameters) {
						containsOptionalParameters = command.Parameters.Any(param => param.IsOptional);
					}
					response += "`" + GuildConfig.CommandPrefix + command.GetSignature(ResourcesService, Culture) + "`";
					if (command.Description != null) {
						response += ": " + string.Format(ResourcesService.ResolveString(Culture, component, command.Description), GuildConfig.CommandPrefix);
					}
					response += "\n";
				}

				if (containsOptionalParameters) {
					response += GetString("MetaCommandsModule_CommandListCommand_OptionalHint");
				}
				ReplyDeferred(response);
			}
			return Task.CompletedTask;
		}

		private IEnumerable<IGrouping<string, (Command command, ComponentBase component)>> GetCategories() {
			bool shouldNotHide(dynamic moduleOrCommand) {
				return !((IEnumerable<Attribute>) moduleOrCommand.Attributes).Any(attr => attr is HiddenFromListAttribute) 
					&& !((IEnumerable<CheckAttribute>) moduleOrCommand.Preconditions).Any(attr => attr is RequireCultureAttribute rca && rca.Culture != Culture);
			}

			return
				from module in CmdService.TopLevelModules
				from command in module.Commands
				// TODO (feature) submodules
				where shouldNotHide(command.Module)
				where shouldNotHide(command)
				let component = Program.Instance.Components.GetComponentForModule(command.Module)
				let moduleName = ResourcesService.ResolveString(Culture, component, command.Module.Name)
				group (command, component) by moduleName;
		}
	}
}
