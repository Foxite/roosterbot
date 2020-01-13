using System;
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
				EnableCommandsList = true
			});

			if (config.UseFileConfig) {
				services.AddSingleton<ChannelConfigService, FileChannelConfigService>(isp => new FileChannelConfigService(isp.GetRequiredService<GlobalConfigService>(), Path.Combine(configPath, "Guilds.json")));
				services.AddSingleton<UserConfigService,    FileUserConfigService   >(isp => new FileUserConfigService   (isp.GetRequiredService<GlobalConfigService>(), Path.Combine(configPath, "Users.json")));
			}

			m_EnableHelp = config.EnableHelp;
			m_EnableCommandsList = config.EnableCommandsList;
		}

		protected override void AddModules(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
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

			if (m_EnableCommandsList) {
				commandService.AddModule<CommandsListModule>();
			}
			if (m_EnableHelp) {
				commandService.AddModule<HelpModule>();
			}

			commandService.AddModule<ControlModule>();
			commandService.AddModule<GuildConfigModule>();
			commandService.AddModule<UserConfigModule>();
			commandService.AddModule<InfoModule>();
			commandService.AddModule<TestModule>();

			help.AddHelpSection(this, "#Meta_HelpName_Edit", "#Meta_HelpText_Edit");
		}
	}
}
