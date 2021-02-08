using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class MetaComponent : Component {
		internal const string LogTag = "Meta";

		private bool m_EnableCommandsList;
		private EmailSettings? m_EmailSettings;

		public override Version ComponentVersion => new Version(1, 4);

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				ConfigProvider = "None",
				EnableCommandsList = true,
				DefaultCommandPrefix = "!",
				DefaultCulture = "en-US",
				EmailSettings = new EmailSettings(),
				DatabaseProvider = new PostgresProvider("", 5432, "", "", "")
			});

			if (config.ConfigProvider == "Json") {
				services.AddSingleton<UserConfigService   >(new JsonUserConfigService   (Path.Combine(configPath, "Users.json")));
				services.AddSingleton<ChannelConfigService>(new JsonChannelConfigService(Path.Combine(configPath, "Channels.json"), config.DefaultCommandPrefix, CultureInfo.GetCultureInfo(config.DefaultCulture)));
			} else if (config.ConfigProvider == "EntityFramework") {
				services.AddSingleton<UserConfigService   >(new EFUserConfigService(config.DatabaseProvider));
				services.AddSingleton<ChannelConfigService>(new EFChannelConfigService(config.DatabaseProvider, config.DefaultCommandPrefix, CultureInfo.GetCultureInfo(config.DefaultCulture)));
			} else if (config.ConfigProvider != "None") {
				Logger.Warning(LogTag, $"Unrecognized config provider {config.ConfigProvider}. Valid options are None, Json, and EntityFramework.");
			}

			m_EnableCommandsList = config.EnableCommandsList;
			m_EmailSettings = config.EmailSettings;
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");

			commandService.AddTypeParser(new CultureInfoParser());
			commandService.AddTypeParser(new UriParser());

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

			if (m_EnableCommandsList) {
				commandService.AddModule<CommandsListModule>();
			}

			commandService.AddAllModules();

			if (m_EmailSettings is not null && !m_EmailSettings.IsEmpty) {
				new EmailNotificationHandler(services.GetRequiredService<NotificationService>(), m_EmailSettings);
			}
		}
	}
}
