using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Meta {
	public class MetaComponent : Component {
		private bool m_EnableHelp;
		private bool m_EnableCommandsList;

		public override Version ComponentVersion => new Version(1, 2, 1);

#nullable disable
		public static MetaComponent Instance { get; private set; }
#nullable restore

		public MetaComponent() {
			Instance = this;
		}

		protected override void AddServices(IServiceCollection services, string configPath) {
			var config = Util.LoadJsonConfigFromTemplate(Path.Combine(configPath, "Config.json"), new {
				UseFileConfig = false,
				EnableHelp = true,
				EnableCommandsList = true,
				DefaultCommandPrefix = "!",
				DefaultCulture = "en-US"
			});

			if (config.UseFileConfig) {
				services.AddSingleton<UserConfigService   >(new FileUserConfigService   (Path.Combine(configPath, "Users.json")));
				services.AddSingleton<ChannelConfigService>(new FileChannelConfigService(Path.Combine(configPath, "Channels.json"),
					config.DefaultCommandPrefix, CultureInfo.GetCultureInfo(config.DefaultCulture)));
			}

			m_EnableHelp = config.EnableHelp;
			m_EnableCommandsList = config.EnableCommandsList;
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService) {
			services.GetRequiredService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");

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

			if (m_EnableCommandsList) {
				commandService.AddModule<CommandsListModule>();
			}
			if (m_EnableHelp) {
				commandService.AddModule<HelpModule>();
			}

			commandService.AddModule<ControlModule>();
			commandService.AddModule<ChannelConfigModule>();
			commandService.AddModule<UserConfigModule>();
			commandService.AddModule<InfoModule>();

			services.GetRequiredService<HelpService>().AddHelpSection(this, "#Meta_HelpName_Edit", "#Meta_HelpText_Edit");
		}
	}
}
