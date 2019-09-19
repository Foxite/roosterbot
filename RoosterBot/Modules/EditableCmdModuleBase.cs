﻿using System.Threading.Tasks;
using Discord;

namespace RoosterBot {
	public abstract class EditableCmdModuleBase : RoosterModuleBase<EditedCommandContext> {
		// These should actually be protected, but they're here because Discord.NET injects these services when a command is called.
		// There's a few other ways to get the services with the injection system, but this is the easiest way.
		public RoosterCommandService CmdService { get; set; }

		private bool m_Replied;

		protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret;
			if (Context.Responses == null) {
				// The command was not edited, or the command somehow did not invoke a reply.
				IUserMessage response = await base.ReplyAsync(message, isTTS, embed, options);
				CmdService.AddResponse(Context.Message, response);
				ret = response;
			} else {
				// The command was edited.
				ret = await Util.ModifyResponsesIntoSingle(message, Context.Responses, m_Replied);

				CmdService.ModifyResponse(Context.Message, new[] { ret });
			}

			m_Replied = true;

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
