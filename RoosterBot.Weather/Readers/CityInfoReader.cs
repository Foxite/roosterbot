using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Weather {
	public class CityInfoReader : RoosterTypeParser<CityInfo> {

		public override string TypeDisplayName => "#CityInfo_TypeDisplayName";

		protected async override ValueTask<TypeParserResult<CityInfo>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			CityService cities = context.ServiceProvider.GetService<CityService>();
			CityInfo? cityResult = await cities.Lookup(input);

			if (cityResult == null) {
				return TypeParserResult<CityInfo>.Unsuccessful("#CityInfoReader_ParseFailed");
			} else {
				return TypeParserResult<CityInfo>.Successful(cityResult);
			}
		}
	}
}