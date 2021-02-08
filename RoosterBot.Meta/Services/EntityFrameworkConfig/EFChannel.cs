using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	internal class EFChannel {
		public string Platform { get; set; } = null!;
		public string PlatformId { get; set; } = null!;
		public string CommandPrefix { get; set; } = null!;
		public string Culture { get; set; } = null!;
		public string DisabledModules { get; set; } = null!;
		public string CustomDataJson { get; set; } = null!;

		public ChannelConfig ToRealConfig(ChannelConfigService service) {
			PlatformComponent? pfc = Program.Instance.Components.GetPlatform(Platform);

			if (pfc is null) {
				throw new InvalidOperationException($"Trying to convert an EFChannel to ChannelConfig, but the platform ({Platform}) is missing.");
			} else {
				return new ChannelConfig(
					service,
					CommandPrefix,
					CultureInfo.GetCultureInfo(Culture),
					new SnowflakeReference(pfc, pfc.GetSnowflakeIdFromString(PlatformId)),
					JObject.Parse(CustomDataJson)!, // Null forgiveness, RoosterBot does not permit using null values inside CC or UC custom data
					DisabledModules.Split(';')
				);
			}
		}

		public static EFChannel FromRealConfig(ChannelConfig config) {
			return new EFChannel() {
				Platform = config.ChannelReference.Platform.PlatformName,
				PlatformId = config.ChannelReference.Id.ToString()!,
				CommandPrefix = config.CommandPrefix,
				Culture = config.Culture.Name,
				DisabledModules = string.Join(';', config.DisabledModules),
				CustomDataJson = config.GetRawData().ToString(Formatting.None)
			};
		}
	}
}
