using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.PublicTransit {
	public class StationInfoReader : RoosterTypeReader {
		protected override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			StationInfoService sis = services.GetService<StationInfoService>();
			TypeReaderResult result;
			if (input.StartsWith("$")) {
				StationInfo lookupResult = sis.GetByCode(input.Substring(1));
				if (lookupResult != null) {
					result = TypeReaderResult.FromSuccess(lookupResult);
				} else {
					result = TypeReaderResult.FromError(CommandError.ParseFailed, "Die code bestaat niet.");
				}
			} else {
				StationInfo stationResult = sis.Lookup(input, 1).SingleOrDefault()?.Station;
				if (stationResult != null) {
					result = TypeReaderResult.FromSuccess(stationResult);
				} else {
					result = TypeReaderResult.FromError(CommandError.ParseFailed, "Ik ken dat station niet. Ik ken geen busstations, en steden zonder treinstations staan niet in mijn lijst.");
				}
			}
			return Task.FromResult(result);
		}
	}
}
