using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	public class EchoModule : ModuleBase {
		public EchoService Service { get; private set; }

		public EchoModule(EchoService serv) {
			Service = serv;
		}
		
		[Command("echo", RunMode = RunMode.Async)]
		public async Task EchoCommand() {
			IVoiceChannel voiceChannel = (Context.User as IGuildUser)?.VoiceChannel;

			if (voiceChannel == null) {
				await ReplyAsync(Context.User.Mention + ", you need to be in a voice channel.");
			} else {
				await Service.Join(Context.Guild, voiceChannel);
				await Service.Echo(Context.Guild);
			}
		}

		[Command("leave", RunMode = RunMode.Async)]
		public async Task LeaveCommand() {
			await Service.Leave(Context.Guild);
		}
	}
}
