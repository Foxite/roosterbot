using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public abstract class EditableCmdModuleBase : ModuleBase<EditedCommandContext> {
		// These should actually be protected, but they're here because Discord.NET injects these services when a command is called.
		// There's a few other ways to get the services with the injection system, but this is the easiest way.
		public EditedCommandService CmdService { get; set; }

		internal ConfigService Config { get; set; }
		internal SNSService SNSService { get; set; }

		public string LogTag { get; protected set; }

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

		protected async virtual Task<bool> AddReaction(string unicode) {
			return await Util.AddReaction(Context.Message, unicode);
		}

		protected async virtual Task<bool> CheckCooldown() {
			Tuple<bool, bool> result = Config.CheckCooldown(Context.User.Id);
			if (result.Item1) {
				return true;
			} else {
				if (!result.Item2) {
					if (Config.ErrorReactions) {
						await AddReaction("⚠");
					}
					await ReplyAsync(Context.User.Mention + ", je gaat een beetje te snel.");
				}
				return false;
			}
		}

		protected async virtual Task MinorError(string message) {
			if (Config.ErrorReactions) {
				await AddReaction("❌");
			}
			await ReplyAsync(message);
		}

		protected async virtual Task FatalError(string message) {
			Logger.Log(LogSeverity.Error, LogTag, message);
			await SNSService.SendCriticalErrorNotificationAsync("Critical error: " + message);
			if (Config.ErrorReactions) {
				await AddReaction("🚫");
			}
			await ReplyAsync("Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.\n");
		}
	}
}
