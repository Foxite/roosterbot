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
			#region Handlers
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

			new MessageReceivedHandler(services);
			new MessageUpdatedHandler (services);
			new MessageDeletedHandler (services);
			#endregion Handlers

			#region Discord entities
			var userParser = new UserParser<Discord.IUser>();
			var messageParser = new MessageParser<Discord.IUserMessage>();
			var channelParser = new ChannelParser<Discord.IMessageChannel>();

			commandService.AddTypeParser(userParser);
			commandService.AddTypeParser(messageParser);
			commandService.AddTypeParser(channelParser);

			commandService.AddTypeParser(new ChannelParser<Discord.IAudioChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.ICategoryChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.IChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.IDMChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.IGroupChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.IGuildChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.INestedChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.IPrivateChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.ITextChannel>());
			commandService.AddTypeParser(new ChannelParser<Discord.IVoiceChannel>());

			commandService.AddTypeParser(new UserParser<Discord.IGuildUser>());
			commandService.AddTypeParser(new UserParser<Discord.IGroupUser>());
			commandService.AddTypeParser(new UserParser<Discord.IWebhookUser>());
			
			commandService.AddTypeParser(new MessageParser<Discord.IMessage>());
			commandService.AddTypeParser(new MessageParser<Discord.ISystemMessage>());

			commandService.AddTypeParser(new RoleParser<Discord.IRole>());

			commandService.AddTypeParser(new ConversionParser<Discord.IUser, IUser>("Discord user", userParser, discordUser => new DiscordUser(discordUser)));
			commandService.AddTypeParser(new ConversionParser<Discord.IUserMessage, IMessage>("Discord message", messageParser, discordMessage => new DiscordMessage(discordMessage)));
			commandService.AddTypeParser(new ConversionParser<Discord.IMessageChannel, IChannel>("Discord channel", channelParser, discordChannel => new DiscordChannel(discordChannel)));
			#endregion
			
			commandService.AddModule<EmoteTheftModule>();
			commandService.AddModule<UserListModule>();

			var emotes = services.GetService<EmoteService>();
			emotes.RegisterEmote(this, "Error",   new DiscordEmote("<:error:636213609919283238>"));
			emotes.RegisterEmote(this, "Success", new DiscordEmote("<:ok:636213617825546242>"));
			emotes.RegisterEmote(this, "Warning", new DiscordEmote("<:warning:636213630114856962>"));
			emotes.RegisterEmote(this, "Unknown", new DiscordEmote("<:unknown:636213624460935188>"));
			emotes.RegisterEmote(this, "Info",    new DiscordEmote("<:info:644251874010202113>"));

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
