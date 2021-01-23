using System.Threading.Tasks;
using Discord;

namespace RoosterBot.DiscordNet {
	public class MediaResultAdapter : DiscordResultAdapter<MediaResult> {
		protected override Task<IUserMessage> HandleResult(DiscordCommandContext context, MediaResult result) {
			using System.IO.Stream stream = result.GetStream();
			// TODO SetResponse and shit
			return context.Channel.SendFileAsync(stream, result.Filename, result.Message, messageReference: context.Message.GetReference());
		}
	}
}
