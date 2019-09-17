using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public abstract class EditableCmdModuleBase : RoosterModuleBase<EditedCommandContext> {
		// These should actually be protected, but they're here because Discord.NET injects these services when a command is called.
		// There's a few other ways to get the services with the injection system, but this is the easiest way.
		public EditedCommandService CmdService { get; set; }

		private bool m_Replied;

		protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			return await ReplyAsync(message, null, isTTS, embed, options);
		}

		protected override async Task<IUserMessage> ReplyAsync(string message, string reactionUnicode = null, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret;
			if (Context.Responses == null) {
				// The command was not edited, or the command somehow did not invoke a reply.
				if (reactionUnicode != null) {
					await AddReactionAsync(reactionUnicode);
				}
				IUserMessage response = await base.ReplyAsync(message, isTTS, embed, options);
				CmdService.AddResponse(Context.Message, response, reactionUnicode);
				ret = response;
			} else {
				// The command was edited.
				if (!m_Replied) {
					// This module has not yet replied to the command.
					// Delete any extra messages that may have been sent by another command.
					IEnumerable<IUserMessage> extraMessages = Context.Responses.Skip(1);
					if (extraMessages.Any()) {
						await Util.DeleteAll(Context.Channel, extraMessages);
					}
				}

				ret = Context.Responses.First();
				await ret.ModifyAsync((msgProps) => {
					// Edit the command:
					if (m_Replied) {
						// If we have replied already, make sure there remains only one reply message.
						msgProps.Content += "\n\n" + message;
					} else {
						m_Replied = true;
						msgProps.Content = message;
					}
				});

				CommandResponsePair crp = CmdService.GetResponse(Context.Message);
				if (crp.ReactionUnicode != reactionUnicode) {
					if (crp.ReactionUnicode != null) {
						await RemoveReactionAsync(crp.ReactionUnicode);
					}
					if (reactionUnicode != null) {
						await AddReactionAsync(reactionUnicode);
					}
					crp.ReactionUnicode = reactionUnicode;
				}
			}

			return ret;
		}
	}

	public class EditedCommandContext : RoosterCommandContext {
		// If this is null, we should make a new message.
		public IUserMessage[] Responses { get; }

		public EditedCommandContext(IDiscordClient client, IUserMessage command, IUserMessage[] originalResponses, string calltag) : base(client, command, calltag) {
			Responses = originalResponses;
		}
	}
}
