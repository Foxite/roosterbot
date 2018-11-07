using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public abstract class RoosterModuleBase<T> : ModuleBase<T> where T : class, ICommandContext {
		public ConfigService Config { get; set; }
		public SNSService SNSService { get; set; }

		private string m_LogTag = null;
		protected string LogTag {
			get {
				if (m_LogTag == null) {
					m_LogTag = "UnknownModule";
					Logger.Log(LogSeverity.Warning, m_LogTag, GetType().Name + " did not have a LogTag attribute and its tag has been set to UnknownModule.");
				}
				return m_LogTag;
			}
			private set => m_LogTag = value;
		}

		protected override void BeforeExecute(CommandInfo command) {
			base.BeforeExecute(command);
			foreach (Attribute attr in command.Module.Attributes) {
				if (attr is LogTagAttribute logTagAttribute) {
					LogTag = logTagAttribute.LogTag;
				}
			}
		}

		protected virtual async Task<bool> AddReaction(string unicode) {
			return await Util.AddReaction(Context.Message, unicode);
		}

		protected virtual async Task<bool> CheckCooldown(float cooldown = 2f) {
			Tuple<bool, bool> result = Config.CheckCooldown(Context.User.Id, cooldown);
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

		protected virtual async Task MinorError(string message) {
			if (Config.ErrorReactions) {
				await AddReaction("❌");
			}
			await ReplyAsync(message);
		}

		protected virtual async Task FatalError(string message) {
			Logger.Log(LogSeverity.Error, LogTag, message);
			await SNSService.SendCriticalErrorNotificationAsync("Critical error: " + message);
			if (Config.LogChannel != null) {
				await Config.LogChannel.SendMessageAsync((await Context.Client.GetUserAsync(Config.BotOwnerId)).Mention + " fatal error: " + message);
			}
			if (Config.ErrorReactions) {
				await AddReaction("🚫");
			}
			await ReplyAsync("Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.\n");
		}
	}

	public abstract class RoosterModuleBase : RoosterModuleBase<CommandContext> { }
}
