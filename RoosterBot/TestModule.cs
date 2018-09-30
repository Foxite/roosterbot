using Discord.Commands;

namespace RoosterBot {
	public class TestModule : ModuleBase {
		private ConfigService Config;

		public TestModule(ConfigService configService) {
			Config = configService;
		}

	}
}
