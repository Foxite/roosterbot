using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using StrawPollNET;

namespace RoosterBot.Tools {
	[Name("#StrawpollModule_Name")]
	public class StrawpollModule : RoosterModule {
		[Command("#StrawpollModule_Command_Name"), Description("#StrawpollModule_Command_Description")]
		public async Task<CommandResult> CreateStrawpoll(string title, [Count(2, 30)] params string[] answers) {
			var poll = await API.Create.CreatePollAsync(title, answers.ToList(), false, StrawPollNET.Enums.DupCheck.Normal, false);
			return TextResult.Success(poll.PollUrl);
		}
	}
}
