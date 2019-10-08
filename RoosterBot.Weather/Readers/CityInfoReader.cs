using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Weather {
	public class CityInfoReader : RoosterTypeReaderBase {
		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			CityService cities = services.GetService<CityService>();
			CityInfo cityResult = await cities.Lookup(input);

			if (cityResult == null) {
				return TypeReaderResult.FromError(CommandError.ParseFailed, "Die stad ken ik niet.");
			} else {
				return TypeReaderResult.FromSuccess(cityResult);
			}
		}
	}
}