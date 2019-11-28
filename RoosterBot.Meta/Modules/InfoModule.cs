using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#MetaCommandsModule_Name"), LocalizedModule("nl-NL", "en-US")]
	public class InfoModule : RoosterModuleBase {
		public MetaInfoService InfoService { get; set; } = null!;

		[Command("#InfoModule_InfoCommand"), Description("#InfoModule_InfoCommand_Summary")]
		public Task<CommandResult> InfoCommand() {
			string response = GetString("InfoModule_InfoCommand_Pretext", Constants.VersionString);
			foreach (ComponentBase component in Program.Instance.Components.GetComponents()) {
				response += "\n" + GetString("InfoModule_InfoCommand_ListItem", component.Name, component.ComponentVersion.ToString());
			}
			return Result(TextResult.Info(response));
		}
		
		[Command("#InfoModule_Uptime"), Description("#InfoModule_Uptime_Summary")]
		public Task<CommandResult> UptimeCommand() {
			TimeSpan uptime = DateTime.Now - Program.Instance.StartTime;
			return Result(TextResult.Info(GetString("InfoModule_Uptime_Response", (int) uptime.TotalDays, uptime.Hours, uptime.Minutes, uptime.Seconds)));
		}

		[Command("#InfoModule_DiscordInvite_CommandName"), Description("#InfoModule_DiscordInvite_Description")]
		public Task<CommandResult> DiscordServerLinkCommand() {
			return Result(TextResult.Info(GetString("InfoModule_DiscordInvite", InfoService.DiscordLink)));
		}
	}
}
