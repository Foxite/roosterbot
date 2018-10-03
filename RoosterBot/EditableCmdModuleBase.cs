using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	public abstract class EditableCmdModuleBase : ModuleBase<EditedCommandContext> {
		private bool m_ResponseWasModified;
		private EditedCommandService m_EditService;

		protected EditableCmdModuleBase(EditedCommandService editService) {
			m_EditService = editService;
		}

		protected async override Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			if (Context.OriginalResponse == null) {
				IUserMessage response = await base.ReplyAsync(message, isTTS, embed, options);
				m_EditService.AddResponse(Context.Message, response);
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
