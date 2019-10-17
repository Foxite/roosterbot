using Discord.Commands;
using MiscStuffComponent.Services;
using RoosterBot.Modules;
using System.Threading.Tasks;

namespace MiscStuffComponent.Modules {
	[Name("darkside")]
	public class PropagationModule : EditableCmdModuleBase {
		public PropagationService Service { get; set; }

		[Command("propagation stats"), Alias("darkside", "dark side"), Summary("Statistieken over de Dark Side.")]
		public async Task PropagationStatsCommand() {
			PropagationStats stats = await Service.GetPropagationStats(Context.Guild);


			string response = $"{Context.Guild.Name} infection status\n";
			response += stats.Present();
			await ReplyAsync(response);
		}
	}
}
