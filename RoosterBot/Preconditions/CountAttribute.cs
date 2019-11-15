using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	// TODO (feature) RoosterParameterPreconditionAttribute that only accepts RoosterCommandContext
	// If you do this make sure not to violate DRY; RoosterPreconditionAttribute and RoosterTypeReader contain the same code to check if the context is RCC
	public sealed class CountAttribute : ParameterPreconditionAttribute {
		public int Min { get; }
		public int Max { get; }

		public CountAttribute(int min, int max) {
			Min = min;
			Max = max;
		}

		public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {
			if (!parameter.Type.IsArray) {
				throw new InvalidOperationException("CountAttribute can only be used on array parameters.");
			} else {
				int length = ((Array) value).Length;
				if (length >= Min && length <= Max) {
					return PreconditionResult.FromSuccess();
				} else {
					CultureInfo culture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild)).Culture;
					return PreconditionResult.FromError(services.GetService<ResourceService>().GetString(culture, "Program_OnCommandExecuted_BadArgCount"));
				}
			}
		}
	}
}
