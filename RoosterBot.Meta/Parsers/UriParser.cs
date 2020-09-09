using System;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Meta {
	public class UriParser : RoosterTypeParser<Uri> {
		public override string TypeDisplayName => "URL";

		public override ValueTask<RoosterTypeParserResult<Uri>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			if (Uri.TryCreate(value, UriKind.Absolute, out Uri? result)) {
				return ValueTaskUtil.FromResult(Successful(result));
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, "#UriParser_Invalid"));
			}
		}
	}
}
