using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class ScheduleComponent : ComponentBase {
#nullable disable // None of the code in this component executes before this is assigned
		internal static ScheduleComponent Instance { get; private set; }
#nullable enable

		// TODO (refactor) need to find a better way to do this, a service just to serve this variable to ScheduleModule seems a bit too much
		internal MultiParser<IdentifierInfo> IdentifierReaders { get; }

		public override Version ComponentVersion => new Version(2, 0, 0);

		public ScheduleComponent() {
			Instance = this;
			IdentifierReaders = new MultiParser<IdentifierInfo>("#ScheduleModule_ReplyErrorMessage_UnknownIdentifier", "#IdentifierInfo_MultiReader_TypeDisplayName", this);
		}

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
			return DependencyResult.Build(components)
				.RequireTag("ScheduleProvider")
				.RequireTag("DayOfWeekReader")
				.Check();
		}

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			services
				.AddSingleton<TeacherNameService>()
				.AddSingleton<ScheduleService>()
				.AddSingleton<IdentifierValidationService>();

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Schedule.Resources");

			var ssir = new StudentSetInfoParser();
			commandService.AddTypeParser(ssir);

			// Long-term todo: allow other components to use their own IdentifierInfo.
			// Currently the codebase *probably* allows this, but I haven't really looked into it.
			commandService.AddTypeParser(IdentifierReaders);
			IdentifierReaders.AddReader(new TeacherInfoParser());
			IdentifierReaders.AddReader(ssir);
			IdentifierReaders.AddReader(new RoomInfoParser());

			commandService.AddModule<TeacherListModule>();
			commandService.AddLocalizedModule<ScheduleModule>();
			commandService.AddLocalizedModule<UserClassModule>();

			help.AddHelpSection(this, "#ScheduleComponent_HelpName_Schedule", "#ScheduleComponent_HelpText_Rooster");
			help.AddHelpSection(this, "#ScheduleComponent_HelpName_Class", "#ScheduleComponent_HelpText_Class");

			return Task.CompletedTask;
		}
	}
}
