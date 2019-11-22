using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.PublicTransit {
	public class StationInfoReader : RoosterTypeParser<StationInfo> {
		public override string TypeDisplayName => "treinstation";

		protected override ValueTask<TypeParserResult<StationInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			StationInfoService sis = context.ServiceProvider.GetService<StationInfoService>();
			TypeParserResult<StationInfo> result;
			if (input.StartsWith("$")) {
				StationInfo? lookupResult = sis.GetByCode(input.Substring(1));
				if (lookupResult != null) {
					result = TypeParserResult<StationInfo>.Successful(lookupResult);
				} else {
					result = TypeParserResult<StationInfo>.Unsuccessful("Die code bestaat niet.");
				}
			} else {
				StationInfo? stationResult = sis.Lookup(input, 1).SingleOrDefault()?.Station;
				if (stationResult != null) {
					result = TypeParserResult<StationInfo>.Successful(stationResult);
				} else {
					result = TypeParserResult<StationInfo>.Unsuccessful("Ik ken dat station niet. Ik ken geen busstations, en steden zonder treinstations staan niet in mijn lijst.");
				}
			}
			return new ValueTask<TypeParserResult<StationInfo>>(result);
		}
	}
}
