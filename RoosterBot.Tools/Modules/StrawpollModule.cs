using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using StrawPollNET;

namespace RoosterBot.Tools {
	public class StrawpollModule : RoosterModule {
		[Command("strawpoll")]
		public async Task<CommandResult> CreateStrawpoll(string title, params string[] answers) {
			var poll = await API.Create.CreatePollAsync(title, answers.ToList(), false, StrawPollNET.Enums.DupCheck.Disabled, false);
			return TextResult.Success(poll.PollUrl);
		}
	}
}
