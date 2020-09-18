using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Meta {
	public class BoolParser : RoosterTypeParser<bool> {
		public override string TypeDisplayName => "#BoolParser_Name";

		public override ValueTask<RoosterTypeParserResult<bool>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			ResourceService resourceService = context.ServiceProvider.GetRequiredService<ResourceService>();
			bool matches(string key) {
				return context.GetString(key).Split("|").Contains(input);
			}
			if (matches("BoolParser_True")) {
				return ValueTaskUtil.FromResult(Successful(true));
			} else if (matches("BoolParser_False")) {
				return ValueTaskUtil.FromResult(Successful(false));
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, "#BoolParser_Invalid"));
			}
		}
	}
}
