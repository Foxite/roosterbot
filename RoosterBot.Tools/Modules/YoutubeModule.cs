using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;

namespace RoosterBot.Tools {
	[Name("#YoutubeModule_Name")]
	public class YoutubeModule : RoosterModule {
		public YoutubeClient Client { get; set; } = null!;

		[Command("#YoutubeModule_Convert"), Description("#YoutubeModule_Convert_Description")]
		public async Task<CommandResult> DownloadYoutubeAudioCommand([Name("#YoutubeModule_Convert_Format")] string format, [Name("#YoutubeModule_Convert_Url")] string url) {
			string[] formats = new[] { "mp3", "ogg" };
			if (formats.Contains(format)) {

				var id = new VideoId(url);
				Video video = await Client.Videos.GetAsync(id);

				var streams = await Client.Videos.Streams.GetManifestAsync(id);
				if (streams.GetAudioOnly().Any()) {
					// Would make more sense to use a stream directly, but then I have to manually wrangle FFMPEG and I'm too lazy for that right now.
					DirectoryInfo directory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
					string filePath = Path.Combine(Path.GetTempPath(), directory.FullName, video.Title + "." + format);
					await Client.Videos.DownloadAsync(id, new ConversionRequest(ToolsComponent.Instance.PathToFFMPEG, filePath, new ConversionFormat(format), ConversionPreset.Slow));

					if (new FileInfo(filePath).Length > 8e6) {
						return TextResult.Error(GetString("YoutubeModule_Convert_Fail_Filesize"));
					} else {
						var ret = new MediaResult(
							TextResult.Success(GetString("YoutubeModule_Convert_Success", video.Title, video.Author, video.Duration.ToString("c", Culture))).Response,
							video.Title + "." + format,
							() => File.OpenRead(filePath)
						);
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
