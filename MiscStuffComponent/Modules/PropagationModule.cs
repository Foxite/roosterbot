using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using System.Threading.Tasks;

namespace MiscStuffComponent.Modules {
	[HiddenFromList]
	public class PropagationModule : EditableCmdModuleBase {
		[Command("propagation stats"), Alias("darkside", "dark side")]
		public async Task PropagationStatsCommand() {
			await ReplyAsync("Everybody is infected. And when everyone's infected... https://www.youtube.com/watch?v=fmSO2cz2ozQ&feature=youtu.be&t=3");
		}
	}
}
