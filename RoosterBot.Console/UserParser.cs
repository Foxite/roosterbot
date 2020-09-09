using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Console {
	public class UserParser : RoosterTypeParser<ConsoleUser> {
		public override string TypeDisplayName => "Console user";

		public override ValueTask<RoosterTypeParserResult<ConsoleUser>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			if (value.ToLower() == "me") {
				return ValueTaskUtil.FromResult(Successful(ConsoleComponent.Instance.TheConsoleUser));
			} else if (value.ToLower() == "you") {
				return ValueTaskUtil.FromResult(Successful(ConsoleComponent.Instance.ConsoleBotUser));
			} else {
				return ValueTaskUtil.FromResult(Unsuccessful(false, "Only me or you"));
			}
		}
	}
}
