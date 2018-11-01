using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public class RoosterModuleBase<T> : ModuleBase<T> where T : class, ICommandContext {
		public ConfigService Config { get; set; }
		public SNSService SNSService { get; set; }

		public string LogTag { get; protected set; }

		protected async virtual Task<bool> AddReaction(string unicode) {
			return await Util.AddReaction(Context.Message, unicode);
		}

		protected async virtual Task<bool> CheckCooldown() {
			Tuple<bool, bool> result = Config.CheckCooldown(Context.User.Id, 2f); // TODO set cooldowns per command
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

	public class RoosterModuleBase : RoosterModuleBase<CommandContext> { }
}
