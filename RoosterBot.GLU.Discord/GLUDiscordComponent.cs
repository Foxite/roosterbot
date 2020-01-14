using System;

namespace RoosterBot.GLU.Discord {
	// This component has a lot of hardcoded snowflake IDs. Normally I'd get all that from a config file, but this component is specifically made for a particular guild,
	//  so generalizing the code does not make a lot of sense.
	public class GLUDiscordComponent : Component {
		public const long GLUGuildId = 278586698877894657;

		public override Version ComponentVersion => new Version(0, 1, 0);

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			commandService.AddModule<GLUModule>();

			new RoleAssignmentHandler();
			new ManualRanksHintHandler();
			new NewUserHandler();
		}
	}
}
