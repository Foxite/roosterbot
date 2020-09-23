using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

namespace RoosterBot.DiscordNet {
	public class AudioService {
		private ConcurrentDictionary<IVoiceChannel, (Stream AudioStream, CancellationTokenSource CTS)> m_Streams;

		public AudioService() {
			m_Streams = new ConcurrentDictionary<IVoiceChannel, (Stream AudioStream, CancellationTokenSource CTS)>();
		}

		public async Task PlayAudio(IVoiceChannel channel, Stream audioStream) {
			if (!m_Streams.TryGetValue(channel, out (Stream AudioStream, CancellationTokenSource CTS) tuple)) {
				IAudioClient audioClient = await channel.ConnectAsync();
				AudioOutStream discordStream = audioClient.CreatePCMStream(AudioApplication.Music);
				await AudioClient_StreamCreated(channel, discordStream, audioStream);
			}
		}

		private async Task AudioClient_StreamCreated(IVoiceChannel channel, AudioOutStream discordStream, Stream audioStream) {
			var cts = new CancellationTokenSource();
			m_Streams.AddOrUpdate(channel, old => (audioStream, cts), (key, old) => (audioStream, cts));
			try {
				await audioStream.CopyToAsync(discordStream, cts.Token);
			} catch (OperationCanceledException) {
				// Pass
			} finally {
				await audioStream.FlushAsync();
			}
		}

		public async Task StopAudio(IVoiceChannel channel) {
			if (m_Streams.TryGetValue(channel, out (Stream AudioStream, CancellationTokenSource CTS) tuple)) {
				tuple.CTS.Cancel();
				await channel.DisconnectAsync();
				await tuple.AudioStream.DisposeAsync();
			}
		}
	}
}
