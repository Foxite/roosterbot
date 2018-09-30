using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class MetaCommandsModule : ModuleBase {
		protected ConfigService Config { get; }
		protected CommandService CmdService { get;  }

		public MetaCommandsModule(ConfigService config, CommandService cmdService) {
			CmdService = cmdService;
			Config = config;
		}

		[Command("help"), Summary("Lijst van alle commands")]
		public async Task HelpCommand() {
			// Print list of commands
			string response = "Commands die je bij mij kan gebruiken:\n";
			foreach (CommandInfo cmd in CmdService.Commands) {
				response += "- `" + Config.CommandPrefix + cmd.Name;
				foreach (ParameterInfo parameter in cmd.Parameters) {
					response += $" <{parameter.Name}>";
				}
				response += $"`: {cmd.Summary}\n";
			}
			await ReplyAsync(response);
		}
	}
}
