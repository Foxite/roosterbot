using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.PublicTransit {
	public class StationInfoParser : RoosterTypeParser<StationInfo> {
		public override string TypeDisplayName => "treinstation";

		protected override ValueTask<RoosterTypeParserResult<StationInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			StationInfoService sis = context.ServiceProvider.GetService<StationInfoService>();
			RoosterTypeParserResult<StationInfo> result;
			if (input.StartsWith("$")) {
				StationInfo? lookupResult = sis.GetByCode(input.Substring(1));
				if (lookupResult != null) {
					result = Successful(lookupResult);
				} else {
					result = Unsuccessful(true, context, "Die code bestaat niet.");
				}
			} else {
				StationInfo? stationResult = sis.Lookup(input, 1).SingleOrDefault()?.Station;
				if (stationResult != null) {
					result = Successful(stationResult);
				} else {
					result = Unsuccessful(false, context, "Ik ken dat station niet. Ik ken geen busstations, en steden zonder treinstations staan niet in mijn lijst.");
				}
			}
			return new ValueTask<RoosterTypeParserResult<StationInfo>>(result);
		}
	}
}
