using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace RoosterBot.Tools {
	[Name("Youtube")]
	public class YoutubeModule : RoosterModule {
		[Command("youtube")]
		public async Task<CommandResult> DownloadYoutubeAudioCommand(string format, string url) {
			string[] formats = new[] { "mp3", "m4a", "wav", "wma", "ogg", "aac", "opus" };
			if (formats.Contains(format)) {
				using IDisposable typingState = Context.Channel.EnterTypingState();

				string id = YoutubeClient.ParseVideoId(url);
				var client = new YoutubeClient();
				Video video = await client.GetVideoAsync(id);

				MediaStreamInfoSet streams = await client.GetVideoMediaStreamInfosAsync(id);
				if (streams.Audio.Any()) {
					DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
					string filePath = Path.Combine(Path.GetTempPath(), directory.FullName, video.Title + ".mp3");
					// TODO (feature) Get this hardcoded exe path from config
					await new YoutubeConverter(client, "C:/RoosterBot/ffmpeg.exe").DownloadAndProcessMediaStreamsAsync(streams.Audio, filePath, "mp3");

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
			} else {
				return TextResult.Error("Unable to download audio: unrecognized format\nValid formats: " + string.Join(", ", formats));
			}
		}
	}
}
