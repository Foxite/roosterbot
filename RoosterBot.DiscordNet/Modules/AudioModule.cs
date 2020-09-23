using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using Qmmands;
using Discord;
using System.Net;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	[RequirePrivate(false)]
	public class AudioModule : RoosterModule<DiscordCommandContext> {
		public AudioService AudioService { get; set; } = null!;

		[Command("play"), MessageHasAttachment("mp3")]
		public async Task PlayAudio() {
			string path = Path.GetTempFileName();
			IAttachment attachment = Context.Message.Attachments.First();
			using var client = new WebClient();
			byte[] data = await client.DownloadDataTaskAsync(new Uri(attachment.Url));

			var process = Process.Start(new ProcessStartInfo {
				FileName = "ffmpeg",
				Arguments = $"-hide_banner -loglevel panic -i pipe: -ac 2 -f s16le -ar 48000 pipe:1",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardInput = true
			});
			_ = new MemoryStream(data).CopyToAsync(process.StandardInput.BaseStream);
			await AudioService.PlayAudio((await Context.Guild!.GetUserAsync(Context.User.Id)).VoiceChannel, process.StandardOutput.BaseStream);
		}

		[Command("stop")]
		public async void StopPlaying() {
			await AudioService.StopAudio((await Context.Guild!.GetUserAsync(Context.User.Id)).VoiceChannel);
		}
	}
}
