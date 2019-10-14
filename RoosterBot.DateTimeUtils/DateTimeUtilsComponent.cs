using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DateTimeUtils {
	public class DateTimeUtilsComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);

		public override IEnumerable<string> Tags => new[] { "DayOfWeekReader" };

		internal static ResourceService ResourceService { get; private set; }

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) {
			ResourceService = services.GetService<ResourceService>();
			ResourceService.RegisterResources("RoosterBot.DateTimeUtils.Resources");

			commandService.AddTypeReader<DayOfWeek>(new DayOfWeekReader());

			return Task.CompletedTask;
		}
	}
}
