using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Qommon.Collections;

namespace RoosterBot.DiscordNet {
	public class DiscordNetComponent : PlatformComponent {
		public static DiscordNetComponent Instance { get; private set; } = null!;

		private string m_Token = null!;
		private string m_GameString = "";
		private ActivityType m_Activity;
		private ulong[] m_NotifyReady;
		private ulong[] m_BotOwnerIds;

		public BaseSocketClient Client { get; private set; } = null!;
		public string DiscordLink { get; private set; } = null!;
		public IReadOnlyList<ulong> BotAdminIds => new ReadOnlyList<ulong>(m_BotOwnerIds);

		public override string PlatformName => "Discord";
		public override Version ComponentVersion => new Version(1, 0, 0);

		public DiscordNetComponent() {
			Instance = this;
			m_BotOwnerIds = Array.Empty<ulong>();
			m_NotifyReady = Array.Empty<ulong>();
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			#region Config
			var discordConfig = new DiscordSocketConfig();

			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				Token = "",
				GameString = "",
				Activity = ActivityType.Playing,
				NotifyReady = new[] { 0UL },
				BotOwnerIds = new[] { 0UL },
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

			// Reflection, but it beats doing this:
			//  discordConfig.GatewayHost = config.Discord.GatewayHost;
			// For every property of the anonymous type
			foreach (PropertyInfo prop in config.Discord.GetType().GetProperties()) {
				discordConfig.GetType().GetProperty(prop.Name)!.SetValue(discordConfig, prop.GetValue(config.Discord));
			}
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
			
			commandService.AddModule<EmoteTheftModule>();
			commandService.AddModule<UserListModule>();
			commandService.AddModule<InfoModule>();

			var emotes = services.GetRequiredService<EmoteService>();
			emotes.RegisterEmote(this, "Error",   new DiscordEmote("<:error:636213609919283238>"));
			emotes.RegisterEmote(this, "Success", new DiscordEmote("<:ok:636213617825546242>"));
			emotes.RegisterEmote(this, "Warning", new DiscordEmote("<:warning:636213630114856962>"));
			emotes.RegisterEmote(this, "Unknown", new DiscordEmote("<:unknown:636213624460935188>"));
			emotes.RegisterEmote(this, "Info",    new DiscordEmote("<:info:644251874010202113>"));
			
			new MessageReceivedHandler(services);
			new MessageUpdatedHandler (services);
			new MessageDeletedHandler (services);
			new ReadyHandler          (m_GameString, m_Activity, m_NotifyReady);
			new LogHandler            (Client);
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
