using System.Threading.Tasks;
using Discord;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public abstract class EditableCmdModuleBase : RoosterModuleBase<EditedCommandContext> {
		// These should actually be protected, but they're here because Discord.NET injects these services when a command is called.
		// There's a few other ways to get the services with the injection system, but this is the easiest way.
		public EditedCommandService CmdService { get; set; }

		private bool m_ResponseWasModified;
		
		protected async override Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			if (Context.OriginalResponse == null) {
				IUserMessage response = await base.ReplyAsync(message, isTTS, embed, options);
				CmdService.AddResponse(Context.Message, response);
				return response;
			} else {
				await Context.OriginalResponse.ModifyAsync((msgProps) => {
					if (m_ResponseWasModified) {
						msgProps.Content += "\n\n" + message;
					} else {
						m_ResponseWasModified = true;
						msgProps.Content = message;
					}
				});
				return Context.OriginalResponse;
			}
		}
	}
}
