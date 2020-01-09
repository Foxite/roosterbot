using System;
using System.IO;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.DiscordNet {
	public class DiscordNetComponent : PlatformComponent {
		private string m_Token = null!;

		public static DiscordNetComponent Instance { get; private set; } = null!;

		public BaseSocketClient Client { get; set; } = null!;
		public override string PlatformName => "Discord";
		public override Version ComponentVersion => new Version(0, 1, 0);

		public DiscordNetComponent() {
			Instance = this;
		}

		protected override Task AddServicesAsync(IServiceCollection services, string configPath) {
			m_Token = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")))["token"]!.ToObject<string>()!;

			Client = new DiscordSocketClient(new DiscordSocketConfig() {

			});

			services.AddSingleton(Client);

			return Task.CompletedTask;
		}

		protected override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			// TODO create handlers

			Client.Log += (msg) => {
				Action<string, string, Exception?> logFunc = msg.Severity switch {
					Discord.LogSeverity.Verbose  => Logger.Verbose,
					Discord.LogSeverity.Debug    => Logger.Debug,
					Discord.LogSeverity.Info     => Logger.Info,
					Discord.LogSeverity.Warning  => Logger.Warning,
					Discord.LogSeverity.Error    => Logger.Error,
					Discord.LogSeverity.Critical => Logger.Critical,
					_                            => Logger.Info,
				};
				logFunc(msg.Source, msg.Message, msg.Exception);
				return Task.CompletedTask;
			};

			Client.MessageReceived += async (msg) => {
				if (msg is Discord.IUserMessage && msg.Source == Discord.MessageSource.User && msg.Content.ToLower() == "ping") {
					await msg.Channel.SendMessageAsync("Pong!");
				}
			};

			return Task.CompletedTask;
		}

		protected async override Task ConnectAsync(IServiceProvider services) {
			await Client.LoginAsync(Discord.TokenType.Bot, m_Token);
			await Client.StartAsync();
		}

		protected async override Task DisconnectAsync() {
			await Client.StopAsync();
			await Client.LogoutAsync();
		}

		public override object GetSnowflakeIdFromString(string input) => ulong.Parse(input);
	}
}
