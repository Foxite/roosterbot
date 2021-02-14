using System;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RoosterBot.Meta {
	internal class EFUser {
		public string Platform { get; set; } = null!;
		public string PlatformId { get; set; } = null!;
		public string? Culture { get; set; }
		public string CustomDataJson { get; set; } = null!;

		public UserConfig ToRealConfig(UserConfigService service) {
			PlatformComponent? pfc = Program.Instance.Components.GetPlatform(Platform);

			if (pfc is null) {
				throw new InvalidOperationException($"Trying to convert an EFChannel to ChannelConfig, but the platform ({Platform}) is missing.");
			} else {
				return new UserConfig(
					service,
					Culture is null ? null : CultureInfo.GetCultureInfo(Culture),
					new SnowflakeReference(pfc, pfc.GetSnowflakeIdFromString(PlatformId)),
					JObject.Parse(CustomDataJson)!
				);
			}
		}
		
		public static EFUser FromRealConfig(UserConfig config) {
			return new EFUser() {
				Platform = config.UserReference.Platform.PlatformName,
				PlatformId = config.UserReference.Id.ToString()!,
				Culture = config.Culture?.Name,
				CustomDataJson = config.GetRawData().ToString(Formatting.None),
			};
		}
	}
}
