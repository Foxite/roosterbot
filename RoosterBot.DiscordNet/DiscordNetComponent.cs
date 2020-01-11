using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.DiscordNet {
	public class DiscordNetComponent : PlatformComponent {
		public static DiscordNetComponent Instance { get; private set; } = null!;

		private string m_Token = null!;
		private string m_GameString = "";
		private ActivityType m_Activity;
		private bool m_ReportVersion;
		private ulong m_BotOwnerId;

		public BaseSocketClient Client { get; set; } = null!;
		public override string PlatformName => "Discord";
		public override Version ComponentVersion => new Version(0, 1, 0);

		public DiscordNetComponent() {
			Instance = this;
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				Token = "",
				GameString = "",
				Activity = ActivityType.Playing,
				ReportStartupVersionToOwner = true,
				BotOwnerId = 0UL
			});
			m_Token = config.Token;
			m_GameString = config.GameString;
			m_Activity = config.Activity;
			m_ReportVersion = config.ReportStartupVersionToOwner;
			m_BotOwnerId = config.BotOwnerId;

			// TODO full support for DiscordSocketConfig through the config file
			Client = new DiscordSocketClient(new DiscordSocketConfig() {
				MessageCacheSize = 10
			});

			services.AddSingleton(Client);
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			#region Handlers
			Client.Log += (msg) => {
				Action<string, string, Exception?> logFunc = msg.Severity switch {
					LogSeverity.Verbose  => Logger.Verbose,
					LogSeverity.Debug    => Logger.Debug,
					LogSeverity.Info     => Logger.Info,
					LogSeverity.Warning  => Logger.Warning,
					LogSeverity.Error    => Logger.Error,
					LogSeverity.Critical => Logger.Critical,
					_                            => Logger.Info,
				};
				logFunc(msg.Source, msg.Message, msg.Exception);
				return Task.CompletedTask;
			};

			Client.MessageReceived += async (msg) => {
				if (msg is IUserMessage && msg.Source == MessageSource.User && msg.Content.ToLower() == "ping") {
					await msg.Channel.SendMessageAsync("Pong!");
				}
			};

			new MessageReceivedHandler(services);
			new MessageUpdatedHandler (services);
			new MessageDeletedHandler (services);
			new ReadyHandler          (services, m_GameString, m_Activity, m_ReportVersion, m_BotOwnerId);
			#endregion Handlers
			
			#region Discord parsers
			var userParser = new UserParser<Discord.IUser>();
			var messageParser = new MessageParser<IUserMessage>();
			var channelParser = new ChannelParser<IMessageChannel>();

			commandService.AddTypeParser(userParser);
			commandService.AddTypeParser(messageParser);
			commandService.AddTypeParser(channelParser);
			
			commandService.AddTypeParser(new RoleParser<IRole>());

			commandService.AddTypeParser(new UserParser<IGuildUser>());
			commandService.AddTypeParser(new UserParser<IGroupUser>());
			commandService.AddTypeParser(new UserParser<IWebhookUser>());
			
			commandService.AddTypeParser(new MessageParser<ISystemMessage>());
			commandService.AddTypeParser(new MessageParser<Discord.IMessage>());
			
			commandService.AddTypeParser(new ChannelParser<IDMChannel>());
			commandService.AddTypeParser(new ChannelParser<ITextChannel>());
			commandService.AddTypeParser(new ChannelParser<IAudioChannel>());
			commandService.AddTypeParser(new ChannelParser<IGroupChannel>());
			commandService.AddTypeParser(new ChannelParser<IGuildChannel>());
			commandService.AddTypeParser(new ChannelParser<IVoiceChannel>());
			commandService.AddTypeParser(new ChannelParser<INestedChannel>());
			commandService.AddTypeParser(new ChannelParser<IPrivateChannel>());
			commandService.AddTypeParser(new ChannelParser<ICategoryChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.IChannel>());

			commandService.AddTypeParser(new ConversionParser<Discord.IUser, IUser>("Discord user", userParser, discordUser => new DiscordUser(discordUser)));
			commandService.AddTypeParser(new ConversionParser<IUserMessage, IMessage>("Discord message", messageParser, discordMessage => new DiscordMessage(discordMessage)));
			commandService.AddTypeParser(new ConversionParser<IMessageChannel, IChannel>("Discord channel", channelParser, discordChannel => new DiscordChannel(discordChannel)));
			#endregion
			
			commandService.AddModule<EmoteTheftModule>();
			commandService.AddModule<UserListModule>();

			var emotes = services.GetService<EmoteService>();
			emotes.RegisterEmote(this, "Error",   new DiscordEmote("<:error:636213609919283238>"));
			emotes.RegisterEmote(this, "Success", new DiscordEmote("<:ok:636213617825546242>"));
			emotes.RegisterEmote(this, "Warning", new DiscordEmote("<:warning:636213630114856962>"));
			emotes.RegisterEmote(this, "Unknown", new DiscordEmote("<:unknown:636213624460935188>"));
			emotes.RegisterEmote(this, "Info",    new DiscordEmote("<:info:644251874010202113>"));
		}

		// Async void, but does it matter? StartAsync spawns a thread that manages the connection and returns immediately.
		// If this was actually awaited it would hardly matter.
		protected async override void Connect(IServiceProvider services) {
			await Client.LoginAsync(TokenType.Bot, m_Token);
			await Client.StartAsync();
		}

		protected async override void Disconnect() {
			await Client.StopAsync();
			await Client.LogoutAsync();
		}

		public override object GetSnowflakeIdFromString(string input) => ulong.Parse(input);
	}
}
