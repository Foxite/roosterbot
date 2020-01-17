using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Weather {
	public class CityInfoParser : RoosterTypeParser<CityInfo> {
		public override string TypeDisplayName => "#CityInfo_TypeDisplayName";

		public async override ValueTask<RoosterTypeParserResult<CityInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			CityService cities = context.ServiceProvider.GetRequiredService<CityService>();
			CityInfo? cityResult = await cities.Lookup(input);

			if (cityResult == null) {
				return Unsuccessful(false, context, "#CityInfoReader_ParseFailed");
			} else {
				return Successful(cityResult);
			}
		}
	}
}