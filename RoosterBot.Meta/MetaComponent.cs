using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	public class MetaComponent : Component {
		public override Version ComponentVersion => new Version(1, 2, 0);

		public override Task AddServicesAsync(IServiceCollection services, string configPath) {
			var jsonConfig = JObject.Parse(File.ReadAllText(Path.Combine(configPath, "Config.json")));

			if (jsonConfig["useFileConfig"].ToObject<bool>()) {
				services.AddSingleton<GuildConfigService, FileGuildConfigService>(isp => new FileGuildConfigService(isp.GetRequiredService<ConfigService>(), Path.Combine(configPath, "Guilds.json")));
				services.AddSingleton<UserConfigService, FileUserConfigService>(isp => new FileUserConfigService(Path.Combine(configPath, "Users.json")));
			}

			services.AddSingleton(new MetaInfoService(jsonConfig["githubLink"].ToObject<string>(), jsonConfig["discordLink"].ToObject<string>()));

			return Task.CompletedTask;
		}

		public override Task AddModulesAsync(IServiceProvider services, RoosterCommandService commandService, HelpService help) {
			services.GetService<ResourceService>().RegisterResources("RoosterBot.Meta.Resources");

			void addPrimitiveParser<T>(string typeKey) {
				// https://riptutorial.com/csharp/example/17807/get-a-strongly-typed-delegate-to-a-method-or-property-via-reflection
				MethodInfo tryParseFunction = typeof(T).GetMethod("TryParse", new Type[] { typeof(string), typeof(T).MakeByRefType() })!;
				var tryParseDelegate = (TryParsePrimitive<T>) Delegate.CreateDelegate(typeof(TryParsePrimitive<T>), null, tryParseFunction);
				commandService.AddTypeParser(new PrimitiveParser<T>(tryParseDelegate, typeKey), true);
			}
			
			addPrimitiveParser<byte   >("Integer");
			addPrimitiveParser<short  >("Integer");
			addPrimitiveParser<int    >("Integer");
			addPrimitiveParser<long   >("Integer");
			addPrimitiveParser<float  >("Decimal");
			addPrimitiveParser<double >("Decimal");
			addPrimitiveParser<decimal>("Decimal");
			commandService.AddTypeParser(new CharParser());
			commandService.AddTypeParser(new BoolParser());
			commandService.AddTypeParser(new CultureInfoParser());

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
