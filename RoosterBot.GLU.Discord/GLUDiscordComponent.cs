using System;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.GLU.Discord {
	// This component has a lot of hardcoded snowflake IDs. Normally I'd get all that from a config file, but this component is specifically made for a particular guild,
	//  so generalizing the code does not make a lot of sense.
	public class GLUDiscordComponent : Component {
		public const long GLUGuildId = 278586698877894657; //346682476149866497;

		public override Version ComponentVersion => new Version(1, 1, 0);

		internal static GLUDiscordComponent Instance { get; private set; } = null!;

		public GLUDiscordComponent() {
			Instance = this;
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			commandService.AddModule<GLUModule>();
			commandService.AddModule<OnlineClassesModule>();

			new RoleAssignmentHandler();
			new ManualRanksHintHandler();
			new NewUserHandler(services.GetService<UserConfigService>());
			new NicknameChangedHandler(services.GetService<UserConfigService>());
		}
	}
}
