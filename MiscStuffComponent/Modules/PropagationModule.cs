using Discord.Commands;
using MiscStuffComponent.Services;
using RoosterBot.Modules;
using System.Threading.Tasks;

namespace MiscStuffComponent.Modules {
	public class PropagationModule : EditableCmdModuleBase {
		public PropagationService Service { get; set; }

		[Command("propagation stats"), Alias("darkside", "dark side")]
		public async Task PropagationStatsCommand() {
			PropagationStats stats = await Service.GetPropagationStats(Context.Guild);


			string response = $"{Context.Guild.Name} infection status\n";
			response += stats.Present();
			await ReplyAsync(response);
		}
	}
}
