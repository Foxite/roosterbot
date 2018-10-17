using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Modules.Preconditions {
	/// <summary>
	/// This will prevent commands from being executed if Program.BotState is NOT BotRunning.
	/// </summary>
	public class RequireBotOperationalAttribute : PreconditionAttribute {
		public override Task<PreconditionResult> CheckPermissions(ICommandContext context, CommandInfo command, IServiceProvider services) {
			if (Program.Instance.State == ProgramState.BotRunning) {
				return Task.FromResult(PreconditionResult.FromSuccess());
			} else {
				// Don't give an error message, as the bot may not be able to send messages.
				Console.WriteLine("aaa");
				return Task.FromResult(PreconditionResult.FromError(""));
			}
		}
	}
}
