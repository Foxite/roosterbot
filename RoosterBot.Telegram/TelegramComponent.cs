using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace RoosterBot.Telegram {
	public class TelegramComponent : PlatformComponent {
		private string m_Token = "";

		public static TelegramComponent Instance { get; private set; } = null!;

		public override string PlatformName => "Telegram";
		public override Version ComponentVersion => new Version(0, 1, 0);

		public TelegramBotClient Client { get; private set; } = null!;

		public TelegramComponent() {
			Instance = this;
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				Token = ""
			});

			m_Token = config.Token;

			
			Client = new TelegramBotClient(m_Token);
			Logger.Info("Telegram", "Username is " + Client.GetMeAsync().Result.Username);
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			new MessageReceivedHandler(services);
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
