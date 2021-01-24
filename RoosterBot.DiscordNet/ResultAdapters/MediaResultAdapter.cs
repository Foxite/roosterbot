using System.Threading.Tasks;
using Discord;

namespace RoosterBot.DiscordNet {
	public class MediaResultAdapter : DiscordResultAdapter<MediaResult> {
		protected async override Task<IUserMessage> HandleResult(DiscordCommandContext context, MediaResult result) {
			await using System.IO.Stream stream = result.GetStream();
			// TODO SetResponse and shit
			// Must await it because otherwise, the stream will be disposed immediately
			return await SendMessage(context, text: result.Message, attachmentStream: stream, filename: result.Filename);
		}
	}
}
