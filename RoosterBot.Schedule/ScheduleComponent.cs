using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class ScheduleComponent : ComponentBase {
		public override Version ComponentVersion => new Version(2, 0, 0);

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
			return DependencyResult.Build(components)
				.RequireTag("ScheduleProvider")
				.RequireTag("UserClassesService")
				.Check();
		}

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			ResourcesType = typeof(Resources);

			services
				.AddSingleton(new TeacherNameService())
				.AddSingleton(new ScheduleService())
				.AddSingleton(new LastScheduleCommandService())
				.AddSingleton(new IdentifierValidationService());

			Logger.Debug("ScheduleComponent", "Started services");

			return Task.CompletedTask;
		}

		public async override Task AddModulesAsync(IServiceProvider services, EditedCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			commandService.AddTypeReader<StudentSetInfo>(new StudentSetInfoReader());
			commandService.AddTypeReader<TeacherInfo[]>(new TeacherInfoReader());
			commandService.AddTypeReader<RoomInfo>(new RoomInfoReader());
			commandService.AddTypeReader<DayOfWeek>(new DayOfWeekReader());

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<DefaultScheduleModule>(services),
				commandService.AddModuleAsync<AfterScheduleModule>(services),
				commandService.AddModuleAsync<StudentScheduleModule>(services),
				commandService.AddModuleAsync<TeacherScheduleModule>(services),
				commandService.AddModuleAsync<RoomScheduleModule>(services),
				commandService.AddModuleAsync<TeacherListModule>(services),
				commandService.AddModuleAsync<UserClassModule>(services)
			));

			help.AddHelpSection("rooster", Resources.ScheduleComponent_HelpText_Rooster);

			help.AddHelpSection("klas", Resources.ScheduleComponent_HelpText_Class);
		}
	}
}
