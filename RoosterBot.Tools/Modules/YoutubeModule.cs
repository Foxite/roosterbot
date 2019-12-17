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
	[Name("#YoutubeModule_Name")]
	public class YoutubeModule : RoosterModule {
		public YoutubeClient Client { get; set; } = null!;
		public YoutubeConverter Converter { get; set; } = null!;

		[Command("#YoutubeModule_Convert"), Description("#YoutubeModule_Convert_Description")]
		public async Task<CommandResult> DownloadYoutubeAudioCommand([Name("#YoutubeModule_Convert_Format")] string format, [Name("#YoutubeModule_Convert_Url")] string url) {
			string[] formats = new[] { "mp3", "ogg" };
			if (formats.Contains(format)) {
				using IDisposable typingState = Context.Channel.EnterTypingState();

				string id = YoutubeClient.ParseVideoId(url);
				Video video = await Client.GetVideoAsync(id);

				MediaStreamInfoSet streams = await Client.GetVideoMediaStreamInfosAsync(id);
				if (streams.Audio.Any()) {
					DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
					string filePath = Path.Combine(Path.GetTempPath(), directory.FullName, video.Title + "." + format);
					await Converter.DownloadAndProcessMediaStreamsAsync(streams.Audio, filePath, format);

					if (new FileInfo(filePath).Length > 8e6) {
						return TextResult.Error(GetString("YoutubeModule_Convert_Fail_Filesize"));
					} else {
						var ret = TextResult.Success(GetString("YoutubeModule_Convert_Success", video.Title, video.Author, video.Duration.ToString("c", Culture)));
						ret.UploadFilePath = filePath;
						return ret;
					}
				} else {
					return TextResult.Error(GetString("YoutubeModule_Convert_Fail_Streams"));
				}
			} else {
				return TextResult.Error(GetString("YoutubeModule_Convert_Fail_Format", string.Join(", ", formats)));
			}
		}
	}
}
