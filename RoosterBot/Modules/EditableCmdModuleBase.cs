using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public abstract class EditableCmdModuleBase : ModuleBase<EditedCommandContext> {
		public EditedCommandService EditService { get; set; }
		
		private bool m_ResponseWasModified;
		
		protected async override Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			if (Context.OriginalResponse == null) {
				IUserMessage response = await base.ReplyAsync(message, isTTS, embed, options);
				EditService.AddResponse(Context.Message, response);
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
