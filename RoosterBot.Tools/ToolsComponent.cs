using System;
using System.Threading.Tasks;

namespace RoosterBot.Tools {
	// TODO (localize) This component
	public class ToolsComponent : Component {
		public override Version ComponentVersion => new Version(0, 2, 0);

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			commandService.AddModule<YoutubeModule>();
			commandService.AddModule<EmoteTheftModule>();
			commandService.AddModule<UserListModule>();
			return Task.CompletedTask;
		}
	}
}
