using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DateTimeUtils {
	public class DateTimeUtilsComponent : ComponentBase {
		public override Version ComponentVersion => new Version(1, 0, 0);

		public override IEnumerable<string> Tags => new[] { "DayOfWeekReader" };
		
#nullable disable
		internal static ResourceService ResourceService { get; private set; }
#nullable restore

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help, RegisterModules registerModuleFunction) {
			ResourceService = services.GetService<ResourceService>();
			ResourceService.RegisterResources("RoosterBot.DateTimeUtils.Resources");

			// This is bullshit, but it works, trust me.
			// https://github.com/discord-net/Discord.Net/issues/1425
			commandService.AddTypeParser<DayOfWeek>(new DayOfWeekReader(), false);

			return Task.CompletedTask;
		}
	}
}
