using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace RoosterBot.Schedule {
	public class IdentifierInfoReaderBase<T> : RoosterTypeReaderBase where T : IdentifierInfo {
		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			T result = await services.GetService<IdentifierValidationService>().ValidateAsync<T>(context as RoosterCommandContext, input);
			if (result is null) {
				CultureInfo culture = (await services.GetService<GuildConfigService>().GetConfigAsync(context.Guild)).Culture;
				return TypeReaderResult.FromError(CommandError.ParseFailed, services.GetService<ResourceService>().GetString(culture, "IdentifierInfoReaderBase_ErrorMessage"));
			} else {
				return TypeReaderResult.FromSuccess(result);
			}
		}
	}
}
