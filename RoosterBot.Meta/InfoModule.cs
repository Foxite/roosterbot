using Discord.Commands;
using System.Threading.Tasks;

namespace RoosterBot.Meta {
	[Name("info")]
	public class InfoModule : RoosterModuleBase {
		[Command("info"), Summary("#InfoModule_InfoCommand_Summary")]
		public Task InfoCommand() {
			string response = GetString("InfoModule_InfoCommand_Pretext", Constants.VersionString);
			foreach (ComponentBase component in Program.Instance.Components.GetComponents()) {
				response += "\n" + GetString("InfoModule_InfoCommand_ListItem", component.Name, component.ComponentVersion.ToString());
			}
			response += "\n" + GetString("InfoModule_InfoCommand_PostText", "https://github.com/Foxite/roosterbot"); // Hardcoded for now
			//response += "\nAlle code is beschikbaar op Github: ";
			ReplyDeferred(response);

			return Task.CompletedTask;
		}
	}
}
