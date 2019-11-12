using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class ScheduleComponent : ComponentBase {
#nullable disable // None of the code in this component executes before this is assigned
		internal static ScheduleComponent Instance { get; private set; }
#nullable enable

		// TODO (refactor) need to find a better way to do this, a service just to serve this variable to ScheduleModule seems a bit too much
		internal MultiReader IdentifierReaders { get; }

		public override Version ComponentVersion => new Version(2, 0, 0);

		public ScheduleComponent() {
			Instance = this;
			IdentifierReaders = new MultiReader("#ScheduleModule_ReplyErrorMessage_UnknownIdentifier", this);
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

			Logger.Debug("ScheduleComponent", "Started services");

			return Task.CompletedTask;
		}

		public async override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModules) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Schedule.Resources");

			// Long-term todo: allow other components to use their own IdentifierInfo.
			// Currently the codebase *probably* allows this, but I haven't really looked into it.
			commandService.AddTypeReader<IdentifierInfo>(IdentifierReaders);
			IdentifierReaders.AddReader(new TeacherInfoReader());
			IdentifierReaders.AddReader(new StudentSetInfoReader());
			IdentifierReaders.AddReader(new RoomInfoReader());

			registerModules(await Task.WhenAll(
				commandService.AddModuleAsync<TeacherListModule>(services),
				commandService.AddModuleAsync<UserClassModule>(services)
			));
			registerModules(await commandService.AddLocalizedModuleAsync<ScheduleModule>());

			help.AddHelpSection(this, "#ScheduleComponent_HelpName_Schedule", "#ScheduleComponent_HelpText_Rooster");
			help.AddHelpSection(this, "#ScheduleComponent_HelpName_Class", "#ScheduleComponent_HelpText_Class");
		}
	}
}
