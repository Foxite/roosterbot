using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Weather {
	public class CityInfoReader : RoosterTypeReaderBase {
		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			CityService cities = services.GetService<CityService>();
			// TODO use Lookup and convert to TypeReaderResult enumerable, this is for testing
			return TypeReaderResult.FromSuccess(await cities.GetByWeatherBitIdAsync(2743477)); // Zwolle, Overijssel
		}
	}
}