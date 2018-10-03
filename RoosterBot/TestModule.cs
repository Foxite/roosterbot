using Discord.Commands;

namespace RoosterBot {
	public class TestModule : EditableCmdModuleBase {
		private ConfigService Config;

		public TestModule(EditedCommandService ecs, ConfigService configService) : base(ecs) {
			Config = configService;
		}

	}
}
