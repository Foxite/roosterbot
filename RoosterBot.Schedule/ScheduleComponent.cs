using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class ScheduleComponent : Component {
		public override Version ComponentVersion => new Version(2, 1, 0);

		protected override DependencyResult CheckDependencies(IEnumerable<Component> components) {
			return DependencyResult.Build(components)
				.RequireTag("ScheduleProvider")
				.RequireTag("DayOfWeekReader")
				.Check();
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			services
				.AddSingleton<TeacherNameService>()
				.AddSingleton<ScheduleService>()
				.AddSingleton<IdentifierValidationService>();
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Schedule.Resources");

			var ssir = new StudentSetInfoParser();
			commandService.AddTypeParser(ssir);

			// Long-term todo: allow other components to use their own IdentifierInfo.
			// Currently the codebase *probably* allows this, but I haven't really looked into it.
			var identifierReaders = new MultiParser<IdentifierInfo>(this, "#ScheduleModule_ReplyErrorMessage_UnknownIdentifier", "#IdentifierInfo_MultiReader_TypeDisplayName");
			identifierReaders.AddReader(ssir);
			identifierReaders.AddReader(new TeacherInfoParser());
			identifierReaders.AddReader(new RoomInfoParser());
			commandService.AddTypeParser(identifierReaders);

			commandService.AddModule<TeacherListModule>();
			commandService.AddModule<ScheduleModule>();
			commandService.AddModule<UserClassModule>();

			help.AddHelpSection(this, "#ScheduleComponent_HelpName_Schedule", "#ScheduleComponent_HelpText_Rooster");
			help.AddHelpSection(this, "#ScheduleComponent_HelpName_Class", "#ScheduleComponent_HelpText_Class");
		}
	}
}
