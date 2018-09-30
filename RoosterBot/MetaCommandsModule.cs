using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class MetaCommandsModule : ModuleBase {
		protected CommandService CmdService { get; private set; }

		public MetaCommandsModule(CommandService cmdService) {
			CmdService = cmdService;
		}

		[Command("help"), Summary("Lijst van alle commands")]
		public async Task HelpCommand() {
			// Print list of commands
			string response = "";
			foreach (CommandInfo cmd in CmdService.Commands) {
				response += cmd.Name;
				foreach (ParameterInfo parameter in cmd.Parameters) {
					response += $" <{parameter.Name}>";
				}
				response += "\n";
			}
			await ReplyAsync(response);
		}
	}
}
