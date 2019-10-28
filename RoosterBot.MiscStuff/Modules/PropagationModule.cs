using Discord.Commands;
using System.Threading.Tasks;

namespace RoosterBot.MiscStuff {
	[HiddenFromList]
	public class PropagationModule : RoosterModuleBase {
		[Command("propagation stats"), Alias("darkside", "dark side")]
		public async Task PropagationStatsCommand() {
			await ReplyAsync("Everybody is infected. And when everyone's infected... https://www.youtube.com/watch?v=fmSO2cz2ozQ&feature=youtu.be&t=3");
		}
	}
}
