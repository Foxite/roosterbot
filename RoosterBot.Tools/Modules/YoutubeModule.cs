using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace RoosterBot.Tools {
	[Group("youtube"), Name("Youtube")]
	public class YoutubeModule : RoosterModule {
		[Command("mp3")]
		public async Task<CommandResult> DownloadYoutubeAudioCommand(string url) {
			using IDisposable typingState = Context.Channel.EnterTypingState();

			string id = YoutubeClient.ParseVideoId(url);
			var client = new YoutubeClient();
			Video video = await client.GetVideoAsync(id);

			MediaStreamInfoSet streams = await client.GetVideoMediaStreamInfosAsync(id);
			if (streams.Audio.Any()) {
				AudioStreamInfo selectedStream = streams.Audio.First();
				foreach (AudioStreamInfo stream in streams.Audio.Skip(1)) {
					if (stream.Bitrate > selectedStream.Bitrate) {
						selectedStream = stream;
					}
				}
				string filePath = Path.Combine(Path.GetTempPath(), video.Title + "." + selectedStream.Container.GetFileExtension());
				await client.DownloadMediaStreamAsync(selectedStream, filePath);

				if (new FileInfo(filePath).Length > 8e6) {
					return TextResult.Error("Unable to upload audio: file size exceeds 8 MB");
				} else {
					var ret = TextResult.Success($"{video.Title} by {video.Author} ({video.Duration.ToString("c", Culture)})");
					ret.UploadFilePath = filePath;
					return ret;
				}
			} else {
				return TextResult.Error("Unable to download audio: no audio streams");
			}
		}
	}
}
