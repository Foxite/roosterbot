﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using YoutubeExplode;
using YoutubeExplode.Models;
using YoutubeExplode.Models.MediaStreams;

namespace RoosterBot.Tools {
	[Group("youtube")]
	public class YoutubeModule : RoosterModuleBase {
		[Command("mp3"), RunMode(RunMode.Parallel)]
		public async Task DownloadYoutubeAudioCommand(string url) {
			string id = YoutubeClient.ParseVideoId(url);
			var client = new YoutubeClient();
			Video video = await client.GetVideoAsync(id);

			int totalHours = (int) video.Duration.TotalHours;

			MediaStreamInfoSet streams = await client.GetVideoMediaStreamInfosAsync(id);
			if (streams.Audio.Any()) {
				using IDisposable typingState = Context.Channel.EnterTypingState();
				AudioStreamInfo selectedStream = streams.Audio.First();
				foreach (AudioStreamInfo stream in streams.Audio.Skip(1)) {
					if (stream.Bitrate > selectedStream.Bitrate) {
						selectedStream = stream;
					}
				}
				//var audioStream = new MemoryStream();
				string filePath = Path.Combine(Path.GetTempPath(), video.Title + "." + selectedStream.Container.GetFileExtension());
				await client.DownloadMediaStreamAsync(selectedStream, filePath);
				typingState.Dispose();
				if (new FileInfo(filePath).Length > 8e6) {
					await Context.RespondAsync(Util.Error + " Unable to upload audio: file size exceeds 8 MB");
				} else {
					await Context.Channel.SendFileAsync(filePath, Context.User.Mention + $" {video.Title} by {video.Author} ({(totalHours == 0 ? "" : totalHours.ToString() + ":")}{video.Duration.Minutes}:{video.Duration.Seconds})");
				}
			} else {
				await Context.RespondAsync(Util.Error + " Unable to download audio: no audio streams");
			}
		}
	}
}
