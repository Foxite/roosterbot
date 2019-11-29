using System;
using System.Threading.Tasks;

namespace RoosterBot.Tools {
	public class ToolsComponent : ComponentBase {
		public override Version ComponentVersion => new Version(0, 1, 0);

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			commandService.AddModule<YoutubeModule>();
			return Task.CompletedTask;
		}
	}
}
