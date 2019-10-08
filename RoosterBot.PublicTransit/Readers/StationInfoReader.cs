using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.PublicTransit {
	public class StationInfoReader : RoosterTypeReaderBase {
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
				IEnumerable<StationMatchInfo> matchResults = sis.Lookup(input, 1);
				if (matchResults.Any()) {
					result = TypeReaderResult.FromSuccess(
						from lookupResult in matchResults
						let floatScore = lookupResult.Score == 0 ? 1.0f : (1.0f / lookupResult.Score)
						select new TypeReaderValue(lookupResult.Station, lookupResult.Score)
					);
				} else {
					result = TypeReaderResult.FromError(CommandError.ParseFailed, "Ik ken dat station niet. Ik ken geen busstations, en steden zonder treinstations staan niet in mijn lijst.");
				}
			}
			return Task.FromResult(result);
		}
	}
}
