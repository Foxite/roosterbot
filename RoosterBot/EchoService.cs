using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

namespace RoosterBot {
	public class EchoService {
		private readonly ConcurrentDictionary<ulong, AudioConnection> ConnectedChannels = new ConcurrentDictionary<ulong, AudioConnection>();

		public async Task Join(IGuild guild, IVoiceChannel channel) {
			if (ConnectedChannels.ContainsKey(guild.Id)) {
				return;
			}
			if (channel.Guild.Id != guild.Id) {
				return;
			}

			AudioConnection connection = new AudioConnection();
			IAudioClient audioClient = await channel.ConnectAsync((client) => {
				client.StreamCreated += (ul, stream) => {
					connection.Input = stream;
					return Task.CompletedTask;
				};
			});

			connection.Client = audioClient;

			if (ConnectedChannels.TryAdd(guild.Id, connection)) {
				Logger.Log(LogSeverity.Info, "EchoService", $"Connected to voice on {guild.Name}.");
			}
		}

		public async Task Leave(IGuild guild) {
			if (ConnectedChannels.TryRemove(guild.Id, out AudioConnection connection)) {
				await connection.Client.StopAsync();
				Logger.Log(LogSeverity.Info, "EchoService", $"Disconnected from voice on {guild.Name}.");
				connection.Dispose();
			}
		}

		public async Task Echo(IGuild guild) {
			if (ConnectedChannels.TryGetValue(guild.Id, out AudioConnection connection)) {
				connection.Output = connection.Client.CreatePCMStream(AudioApplication.Mixed, 1920);
				try {
					Console.WriteLine("aaaa");
					CancellationTokenSource cts = new CancellationTokenSource();
					Task echo = connection.Input.CopyToAsync(connection.Output, cts.Token);
					connection.Closed += (o, e) => {
						Console.WriteLine("cccc");
						cts.Cancel();
						Console.WriteLine("dddd");
					};
					await echo;
					Console.WriteLine("bbbb");
				} catch (OperationCanceledException) { } // Pass
			}
		}
	}

	public class AudioConnection : IDisposable {
		public IAudioClient Client;
		public AudioInStream Input;
		public AudioOutStream Output;

		public event EventHandler Closed;

		public void Dispose() {
			Closed?.Invoke(this, null);

			Input?.Dispose();
			Output?.Dispose();
			Client?.Dispose();
		}
	}
}
