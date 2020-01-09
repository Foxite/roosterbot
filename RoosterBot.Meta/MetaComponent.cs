using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class MetaComponent : Component {
		public override Version ComponentVersion => new Version(1, 2, 0);

		protected override Task AddServicesAsync(IServiceCollection services, string configPath) {
			var jsonConfig = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			// TODO Json deserialization
			if (jsonConfig["useFileConfig"]!.ToObject<bool>()) {
				services.AddSingleton<ChannelConfigService, FileChannelConfigService>(isp => new FileChannelConfigService(isp.GetRequiredService<ConfigService>(), Path.Combine(configPath, "Guilds.json")));
				services.AddSingleton<UserConfigService, FileUserConfigService>(isp => new FileUserConfigService(Path.Combine(configPath, "Users.json")));
			}

			services.AddSingleton(new MetaInfoService(jsonConfig["githubLink"]!.ToObject<string>()!, jsonConfig["discordLink"]!.ToObject<string>()!));

			return Task.CompletedTask;
		}

		protected override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");

			commandService.AddTypeParser(new CultureInfoParser());

			#region Primitive types
			void addPrimitive<T>(string typeKey) {
				commandService.AddTypeParser(new PrimitiveParser<T>(typeKey), true);
			}

			addPrimitive<byte   >("Integer");
			addPrimitive<short  >("Integer");
			addPrimitive<int    >("Integer");
			addPrimitive<long   >("Integer");
			addPrimitive<float  >("Decimal");
			addPrimitive<double >("Decimal");
			addPrimitive<decimal>("Decimal");
			commandService.AddTypeParser(new CharParser(), true);
			commandService.AddTypeParser(new BoolParser(), true);
			#endregion

			#region Discord entities
			/*
			void addChannelParser<T>() where T : class, IChannel {
				commandService.AddTypeParser(new ChannelParser<T>());
			}

			addChannelParser<IAudioChannel>();
			addChannelParser<ICategoryChannel>();
			addChannelParser<IChannel>();
			addChannelParser<IDMChannel>();
			addChannelParser<IGroupChannel>();
			addChannelParser<IGuildChannel>();
			addChannelParser<IMessageChannel>();
			addChannelParser<INestedChannel>();
			addChannelParser<IPrivateChannel>();
			addChannelParser<ITextChannel>();
			addChannelParser<IVoiceChannel>();
			
			commandService.AddTypeParser(new UserParser<IUser>());
			commandService.AddTypeParser(new UserParser<IGuildUser>());
			commandService.AddTypeParser(new UserParser<IGroupUser>());
			commandService.AddTypeParser(new UserParser<IWebhookUser>());
			
			commandService.AddTypeParser(new MessageParser<IMessage>());
			commandService.AddTypeParser(new MessageParser<ISystemMessage>());
			commandService.AddTypeParser(new MessageParser<IUserMessage>());

			commandService.AddTypeParser(new RoleParser<IRole>());
			*/
			#endregion

			commandService.AddModule<CommandsListModule>();
			commandService.AddModule<HelpModule>();
			commandService.AddModule<ControlModule>();
			commandService.AddModule<GuildConfigModule>();
			commandService.AddModule<UserConfigModule>();
			commandService.AddModule<InfoModule>();

			help.AddHelpSection(this, "#Meta_HelpName_Edit", "#Meta_HelpText_Edit");

			return Task.CompletedTask;
		}
	}
}
