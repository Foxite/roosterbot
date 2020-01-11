using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Statistics {
	public class StatisticsComponent : Component {
		public override Version ComponentVersion => new Version(0, 1, 0);

		protected override void AddServices(IServiceCollection services, string configPath) {
			services.AddSingleton((isp) => new StatisticsService(isp.GetService<ResourceService>()));
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
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
		}
	}
}
