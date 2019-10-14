using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	public sealed class RangeAttribute : ParameterPreconditionAttribute {
		public int Min { get; }
		public int Max { get; }

		public RangeAttribute(int min, int max) {
			Min = min;
			Max = max;
		}

		public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {
			bool ret = (dynamic) value >= Min && (dynamic) value <= Max;
			if (ret) {
				return PreconditionResult.FromSuccess();
			} else {
				CultureInfo culture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild)).Culture;
				return PreconditionResult.FromError(services.GetService<ResourceService>().GetString(culture, "RangeAttribute_CheckFailed"));
			}
		}
	}
}
