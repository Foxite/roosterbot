using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Statistics {
	public class StatisticsComponent : Component {
		public override Version ComponentVersion => new Version(0, 1, 0);

		protected override Task AddServicesAsync(IServiceCollection services, string configPath) {
			services.AddSingleton((isp) => new StatisticsService(isp.GetService<ResourceService>()));

			return Task.CompletedTask;
		}

		protected override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			StatisticsService stats = services.GetService<StatisticsService>();


			var commandsExecuted = new TimeStatistic(this, "Commands executed");

			commandService.CommandExecuted += e => {
				commandsExecuted.Increment();
				return Task.CompletedTask;
			};

			// TODO discord statistics
			//DiscordSocketClient client = services.GetService<DiscordSocketClient>();
			//stats.AddStatistic(new ExternalStatistic(() => client.Guilds.Count, this, "Guilds served"));
			stats.AddStatistic(commandsExecuted);

			commandService.AddModule<StatisticsModule>();

			return Task.CompletedTask;
		}
	}
}
