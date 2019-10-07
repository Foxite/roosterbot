using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class ScheduleComponent : ComponentBase {
		internal static MultiReader s_IdentifierReaders; // Experiment, flesh out before pushing

		public override Version ComponentVersion => new Version(2, 0, 0);

		public override DependencyResult CheckDependencies(IEnumerable<ComponentBase> components) {
			return DependencyResult.Build(components)
				.RequireTag("ScheduleProvider")
				.RequireTag("UserClassesService")
				.RequireTag("DayOfWeekReader")
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

			// Long-term todo: allow other components to use their own IdentifierInfo.
			// Currently the codebase *probably* allows this, but I haven't really looked into it.
			s_IdentifierReaders = new MultiReader(new RoosterTypeReaderBase[] {
				new TeacherInfoReader(),
				new StudentSetInfoReader(),
				new RoomInfoReader()
			}, "#DefaultScheduleModule_ReplyErrorMessage_UnknownIdentifier", this);
			commandService.AddTypeReader<IdentifierInfo>(s_IdentifierReaders);

			registerModules(await Task.WhenAll(
				//commandService.AddModuleAsync<DefaultScheduleModule>(services),
				commandService.AddModuleAsync<TeacherListModule>(services),
				commandService.AddModuleAsync<UserClassModule>(services)
			));

			registerModules(await commandService.AddLocalizedModuleAsync<ScheduleModule>());

			// TODO rewrite documentation everywhere, it is outdated, unclear, and possibly wrong since updates
			help.AddHelpSection(this, "rooster", "#ScheduleComponent_HelpText_Rooster");

			help.AddHelpSection(this, "klas", "#ScheduleComponent_HelpText_Class");
		}
	}
}
