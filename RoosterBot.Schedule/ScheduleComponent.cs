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
			services
				.AddSingleton(new TeacherNameService())
				.AddSingleton(new ScheduleService())
				.AddSingleton(new LastScheduleCommandService())
				.AddSingleton(new IdentifierValidationService());

			Logger.Debug("ScheduleComponent", "Started services");

			return Task.CompletedTask;
		}

		public async override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Schedule.Resources");

			// TODO allow other components to use their own IdentifierInfo
			// Currently the codebase *probably* allows this, but I haven't really looked into it.
			// In any case there needs to be a way to add custom TypeReaders to this MultiReader.
			commandService.AddTypeReader<IdentifierInfo>(new MultiReader(new RoosterTypeReaderBase[] {
				new TeacherInfoReader(),
				new StudentSetInfoReader(),
				new RoomInfoReader()
			}, "#DefaultScheduleModule_ReplyErrorMessage_UnknownIdentifier", this));

			commandService.AddTypeReader<DayOfWeek>(new DayOfWeekReader());

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<DefaultScheduleModule>(services),
				commandService.AddModuleAsync<ScheduleModule>(services),
				commandService.AddModuleAsync<TeacherListModule>(services),
				commandService.AddModuleAsync<UserClassModule>(services)
			));

			help.AddHelpSection(this, "rooster", "#ScheduleComponent_HelpText_Rooster");

			help.AddHelpSection(this, "klas", "#ScheduleComponent_HelpText_Class");
		}
	}
}
