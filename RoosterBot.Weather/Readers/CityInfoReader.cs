using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Weather {
	public class CityInfoReader : RoosterTypeReader {
		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			CityService cities = services.GetService<CityService>();
			CityInfo cityResult = await cities.Lookup(input);

			if (cityResult == null) {
				// TODO problem-reporting command, this could be a useful alternative to "contacting the bot owner" if we ever go beyond servers that any dev is in
				return TypeReaderResult.FromError(CommandError.ParseFailed, "Die stad ken ik niet. Bedoelde je een stad die officiëel een andere naam heeft? Geef het door aan de bot eigenaar.");
			} else {
				return TypeReaderResult.FromSuccess(cityResult);
			}
		}
	}
}