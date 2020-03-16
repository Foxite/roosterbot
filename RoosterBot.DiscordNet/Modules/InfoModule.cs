﻿using Qmmands;

namespace RoosterBot.DiscordNet {
	[Name("#InfoModule_Name")]
	public class InfoModule : RoosterModule {
		[Command("#InfoModule_DiscordInvite_CommandName"), Description("#InfoModule_DiscordInvite_Description")]
		public CommandResult DiscordServerLinkCommand() {
			return TextResult.Info(GetString("InfoModule_DiscordInvite", DiscordNetComponent.Instance.DiscordLink));
		}
	}
}
