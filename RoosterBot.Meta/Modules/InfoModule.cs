using System;
using System.Diagnostics;
using Qmmands;

namespace RoosterBot.Meta {
	[Name("#Meta_Name")]
	public class InfoModule : RoosterModule {
		[Command("#InfoModule_InfoCommand"), Description("#InfoModule_InfoCommand_Summary")]
		public CommandResult InfoCommand() {
			string response = GetString("InfoModule_InfoCommand_Pretext", Program.Version.ToString());
			foreach (Component component in Program.Instance.Components.GetComponents()) {
				response += "\n" + GetString("InfoModule_InfoCommand_ListItem", component.Name, component.ComponentVersion.ToString());
			}
			return TextResult.Info(response);
		}
		
		[Command("#InfoModule_Uptime"), Description("#InfoModule_Uptime_Summary")]
		public CommandResult UptimeCommand() {
			TimeSpan uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;
			return TextResult.Info(GetString("InfoModule_Uptime_Response", (int) uptime.TotalDays, uptime.Hours, uptime.Minutes, uptime.Seconds));
		}

		[Command("test"), RequirePrivate(true)]
		public CommandResult Test() {
			return TextResult.Info("Success");
		}
	}
}
