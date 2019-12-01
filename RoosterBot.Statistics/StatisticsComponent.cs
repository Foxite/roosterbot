using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Statistics {
	public class StatisticsComponent : Component {
		public override Version ComponentVersion => new Version(0, 1, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			services.AddSingleton((isp) => new StatisticsService(isp.GetService<ResourceService>()));

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			StatisticsService stats = services.GetService<StatisticsService>();
			DiscordSocketClient client = services.GetService<DiscordSocketClient>();

			var commandsExecuted = new TimeStatistic(this, "Commands executed");

			commandService.CommandExecuted += e => {
				commandsExecuted.Increment();
				return Task.CompletedTask;
			};

			stats.AddStatistic(commandsExecuted);
			stats.AddStatistic(new ExternalStatistic(() => client.Guilds.Count, this, "Guilds served"));

			commandService.AddModule<StatisticsModule>();

			return Task.CompletedTask;
		}
	}
}
