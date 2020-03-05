using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qmmands;
using StrawpollNET;

namespace RoosterBot.Tools {
	public class StrawpollModule : RoosterModule {
		[Command("strawpoll")]
		public async Task<CommandResult> CreateStrawpoll(string title, params string[] answers) {
			var api = new StrawPoll();
			var poll = await api.CreatePollAsync(title, answers.ToList(), false, StrawpollNET.Data.DupCheck.DISABLED, true);
			return TextResult.Success(poll.PollUrl);
		}
	}
}
