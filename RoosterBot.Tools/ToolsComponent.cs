using System;
using System.Threading.Tasks;

namespace RoosterBot.Tools {
	public class ToolsComponent : Component {
		public override Version ComponentVersion => new Version(0, 1, 0);

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			commandService.AddModule<YoutubeModule>();
			commandService.AddModule<EmoteTheftModule>();
			return Task.CompletedTask;
		}
	}
}
