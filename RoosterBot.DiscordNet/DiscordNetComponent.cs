using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qommon.Collections;

namespace RoosterBot.DiscordNet {
	public class DiscordNetComponent : PlatformComponent {
		public static DiscordNetComponent Instance { get; private set; } = null!;

		private Dictionary<string, DiscordEmote> m_Emotes = new Dictionary<string, DiscordEmote>();
		private string m_Token = null!;
		private string m_GameString = "";
		private ActivityType m_Activity;
		private ulong[] m_NotifyReady;
		private ulong[] m_BotOwnerIds;

		public IReadOnlyList<ulong> EmoteStorageGuilds { get; private set; }
		public BaseSocketClient Client { get; private set; } = null!;
		public string DiscordLink { get; private set; } = null!;
		public IReadOnlyList<ulong> BotAdminIds => new ReadOnlyList<ulong>(m_BotOwnerIds);
		public IReadOnlyList<SocketUser> BotAdmins => BotAdminIds.ListSelect(id => Client.GetUser(id));

		public override string PlatformName => "Discord";
		public override Version ComponentVersion => new Version(1, 3, 0);

		public override Type SnowflakeIdType => typeof(ulong);

		public DiscordNetComponent() {
			Instance = this;
			m_BotOwnerIds = Array.Empty<ulong>();
			m_NotifyReady = Array.Empty<ulong>();
			EmoteStorageGuilds = Array.Empty<ulong>();
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			#region Config
			var discordConfig = new DiscordSocketConfig();

			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				Token = "",
				GameString = "",
				Activity = ActivityType.Playing,
				NotifyReady = Array.Empty<ulong>(),
				BotOwnerIds = Array.Empty<ulong>(),
				DiscordLink = "",
				Emotes = new {
					Info = ":information_source:",
					Unknown = ":question:",
					Warning = ":exclamation:",
					Success = ":white_check_mark:",
					Error = ":x:"
				},
				EmoteStorageGuilds = Array.Empty<ulong>(),
				Discord = new {
					// Does not include RestClientProvider, WebSocketProvider, UdpSocketProvider
					discordConfig.GatewayHost,
					discordConfig.ConnectionTimeout,
					discordConfig.ShardId,
					discordConfig.TotalShards,
					discordConfig.MessageCacheSize,
					discordConfig.LargeThreshold,
					discordConfig.AlwaysDownloadUsers,
					discordConfig.HandlerTimeout,
					discordConfig.ExclusiveBulkDelete,
					discordConfig.DefaultRetryMode,
					discordConfig.LogLevel
				}
			});
			m_Token = config.Token;
			m_GameString = config.GameString;
			m_Activity = config.Activity;
			m_NotifyReady = config.NotifyReady;
			m_BotOwnerIds = config.BotOwnerIds;
			DiscordLink = config.DiscordLink;

			EmoteStorageGuilds = config.EmoteStorageGuilds;

			// Reflection, but it beats doing this:
			//  discordConfig.GatewayHost = config.Discord.GatewayHost;
			// For every property of the anonymous type
			foreach (PropertyInfo prop in config.Discord.GetType().GetProperties()) {
				discordConfig.GetType().GetProperty(prop.Name)!.SetValue(discordConfig, prop.GetValue(config.Discord));
			}

			m_Emotes = new Dictionary<string, DiscordEmote>() {
				{ "Error",    new DiscordEmote(config.Emotes.Error) },
				{ "Info",     new DiscordEmote(config.Emotes.Info) },
				{ "Success",  new DiscordEmote(config.Emotes.Success) },
				{ "Warning",  new DiscordEmote(config.Emotes.Warning) },
				{ "Unknown",  new DiscordEmote(config.Emotes.Unknown) }
			};
			#endregion

			Client = new DiscordSocketClient(discordConfig);

			services.AddSingleton(Client);
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.DiscordNet.Resources");

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

			commandService.GetPlatformSpecificParser<IUser>().RegisterParser(this, new ConversionParser<Discord.IUser, IUser>("Discord user", userParser, discordUser => new DiscordUser(discordUser)));
			commandService.GetPlatformSpecificParser<IMessage>().RegisterParser(this, new ConversionParser<IUserMessage, IMessage>("Discord message", messageParser, discordMessage => new DiscordMessage(discordMessage)));
			commandService.GetPlatformSpecificParser<IChannel>().RegisterParser(this, new ConversionParser<IMessageChannel, IChannel>("Discord channel", channelParser, discordChannel => new DiscordChannel(discordChannel)));
			#endregion
			
			commandService.AddModule<EmoteModule>();
			commandService.AddModule<UserListModule>();
			commandService.AddModule<InfoModule>();

			var emoteService = services.GetRequiredService<EmoteService>();
			foreach (var item in m_Emotes) {
				emoteService.RegisterEmote(this, item.Key, item.Value);
			}
			
			new DiscordNotificationHandler(services.GetRequiredService<NotificationService>());
			new MessageReceivedHandler    (services);
			new MessageUpdatedHandler     (services);
			new MessageDeletedHandler     (services);
			new VoteToPinHandler          (services);
			new ReadyHandler              (m_GameString, m_Activity, m_NotifyReady);
			new LogHandler                (Client);
		}

		protected override void Connect(IServiceProvider services) {
			Client.LoginAsync(TokenType.Bot, m_Token).GetAwaiter().GetResult();
			Client.StartAsync().GetAwaiter().GetResult();
		}

		protected override void Disconnect() {
			Client.StopAsync().GetAwaiter().GetResult();
			Client.LogoutAsync().GetAwaiter().GetResult();
		}

		public override object GetSnowflakeIdFromString(string input) => ulong.Parse(input);
	}
}
