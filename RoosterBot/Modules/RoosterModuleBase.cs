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

		protected string LogTag { get; private set; }
		protected ModuleLogger Log { get; private set; }

		protected override void BeforeExecute(CommandInfo command) {
			base.BeforeExecute(command);

			LogTag = null;
			foreach (Attribute attr in command.Module.Attributes) {
				if (attr is LogTagAttribute logTagAttribute) {
					LogTag = logTagAttribute.LogTag;
				}
			}

			if (LogTag == null) {
				LogTag = "UnknownModule";
				Logger.Log(LogSeverity.Warning, LogTag, $"{GetType().Name} did not have a LogTag attribute and its tag has been set to UnknownModule.");
			}

			Log = new ModuleLoggerInternal(LogTag);

			Log.Info($"Executing `{Context.Message.Content}` for `{Context.User.Username}#{Context.User.Discriminator}` in {Context.Guild.Name} channel {Context.Channel.Name}");
		}

		protected virtual async Task<IUserMessage> ReplyAsync(string message, string reactionUnicode, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			await AddReaction(reactionUnicode);
			return await ReplyAsync(message, isTTS, embed, options);
		}

		protected virtual async Task<bool> AddReaction(string unicode) {
			return await Util.AddReaction(Context.Message, unicode);
		}

		protected virtual async Task<bool> RemoveReaction(string unicode) {
			return await Util.RemoveReaction(Context.Message, unicode, Context.Client.CurrentUser);
		}

		protected virtual async Task<bool> CheckCooldown(float cooldown = 2f) {
			Tuple<bool, bool> result = Config.CheckCooldown(Context.User.Id, cooldown);
			if (result.Item1) {
				return true;
			} else {
				Log.Info($"Did not execute command `{Context.Message.Content}` for `{Context.User.Mention}` as they are still in cooldown");
				if (!result.Item2) {
					if (Config.ErrorReactions) {
						await ReplyAsync($"{Context.User.Mention}, je gaat een beetje te snel.", "⚠");
					} else {
						await ReplyAsync($"{Context.User.Mention}, je gaat een beetje te snel.");
					}
				}
				return false;
			}
		}

		protected virtual async Task MinorError(string message) {
			Log.Info($"Command failed: {message}");
			if (Config.ErrorReactions) {
				await ReplyAsync(message, "❌");
			} else {
				await ReplyAsync(message);
			}
		}

		protected virtual async Task FatalError(string message, Exception exception = null) {
			string report = $"Critical error executing `{Context.Message.Content}` for `{Context.User.Mention}` in {Context.Guild.Name} channel {Context.Channel.Name}: {message}";

			if (exception != null) {
				report += $"\nAttached exception: {exception.GetType().Name}\n";
				report += exception.StackTrace;
			}

			Log.Error(report);
			await SNSService.SendCriticalErrorNotificationAsync(report);
			if (Config.LogChannel != null) {
				await Config.LogChannel.SendMessageAsync($"{(await Context.Client.GetUserAsync(Config.BotOwnerId)).Mention} {report}");
			}
			string response = "Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.\n";
			if (Config.ErrorReactions) {
				await ReplyAsync(response, "🚫");
			} else {
				await ReplyAsync(response);
			}
		}

		public abstract class ModuleLogger {
			protected string m_Tag;

			public void Verbose(string message) {
				Logger.Verbose(m_Tag, message);
			}

			public void Debug(string message) {
				Logger.Debug(m_Tag, message);
			}

			public void Info(string message) {
				Logger.Info(m_Tag, message);
			}

			public void Warning(string message) {
				Logger.Warning(m_Tag, message);
			}

			public void Error(string message) {
				Logger.Error(m_Tag, message);
			}

			public void Critical(string message) {
				Logger.Critical(m_Tag, message);
			}
		}

		private sealed class ModuleLoggerInternal : ModuleLogger {
			public ModuleLoggerInternal(string tag) {
				m_Tag = tag;
			}
		}
	}

	public abstract class RoosterModuleBase : RoosterModuleBase<CommandContext> { }
}
