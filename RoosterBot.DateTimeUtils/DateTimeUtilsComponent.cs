using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DateTimeUtils {
	public class DateTimeUtilsComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);

		public override string[] Tags => new[] { "DayOfWeekReader" };

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, Action<ModuleInfo[]> registerModuleFunction) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.DateTimeUtils.Resources");

			commandService.AddTypeReader<DayOfWeek>(new DayOfWeekReader());

			return Task.CompletedTask;
		}
	}
}
