using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Meta {
	[Name("#MetaCommandsModule_Name")]
	public class InfoModule : RoosterModuleBase {
		[Command("#InfoModule_InfoCommand"), Summary("#InfoModule_InfoCommand_Summary")]
		public Task InfoCommand() {
			string response = GetString("InfoModule_InfoCommand_Pretext", Constants.VersionString);
			foreach (ComponentBase component in Program.Instance.Components.GetComponents()) {
				response += "\n" + GetString("InfoModule_InfoCommand_ListItem", component.Name, component.ComponentVersion.ToString());
			}
			response += "\n" + GetString("InfoModule_InfoCommand_PostText", "https://github.com/Foxite/roosterbot"); // Hardcoded for now
			ReplyDeferred(response);

			return Task.CompletedTask;
		}
	}
}
