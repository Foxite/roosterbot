using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class ScheduleComponent : Component {
		public override Version ComponentVersion => new Version(2, 2, 0);

		public override IEnumerable<string> RequiredTags { get; } = new[] { "ScheduleProvider", "DayOfWeekReader" };

		protected override void AddServices(IServiceCollection services, string configPath) {
			services
				.AddSingleton<ScheduleService>()
				.AddSingleton<IdentifierValidationService>();
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.Schedule.Resources");

			// Long-term todo: allow other components to use their own IdentifierInfo.
			// Currently the codebase *probably* allows this, but I haven't really looked into it.
			var identifierReaders = new MultiParser<IdentifierInfo>(this, "#ScheduleModule_ReplyErrorMessage_UnknownIdentifier", "#IdentifierInfo_MultiReader_TypeDisplayName");
			commandService.AddTypeParser(identifierReaders);

			commandService.AddModule<ScheduleModule>();
			commandService.AddModule<UserIdentifierModule>();
		}
	}
}
