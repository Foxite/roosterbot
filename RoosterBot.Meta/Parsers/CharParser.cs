using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	public class CharParser : RoosterTypeParser<char> {
		public override string TypeDisplayName => "#CharParser_Name";

		public override ValueTask<RoosterTypeParserResult<char>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			if (value.Length == 1) {
				return ValueTaskUtil.FromResult(Successful(value[0]));
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, "#CharParser_TooLong"));
			}
		}
	}
}
