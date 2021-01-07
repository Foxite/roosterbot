using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace RoosterBot.Telegram {
	public class TelegramComponent : PlatformComponent {
		private string m_Token = "";

		public override string PlatformName => "Telegram";
		public override Version ComponentVersion => new Version(0, 2, 1);

		public TelegramBotClient Client { get; private set; } = null!;
		public int BotOwnerId { get; private set; }

		public static TelegramComponent Instance { get; private set; } = null!;

		public override Type SnowflakeIdType => typeof(long);

		public TelegramComponent() {
			Instance = this;
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				Token = "",
				BotOwnerId = 0
			});

			m_Token = config.Token;
			BotOwnerId = config.BotOwnerId;
			
			Client = new TelegramBotClient(m_Token);
			Logger.Info("Telegram", "Username is " + Client.GetMeAsync().Result.Username);
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			new MessageReceivedHandler(services);
			new MessageEditedHandler  (services);
		}

		protected override void Connect(IServiceProvider services) {
			Client.StartReceiving();
		}

		protected override void Disconnect() {
			Client.StopReceiving();
		}

		public override object GetSnowflakeIdFromString(string input) => long.Parse(input);
	}
}
