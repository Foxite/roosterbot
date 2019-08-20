using System;
using System.Text;
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

		protected internal StringBuilder m_Response;
		protected internal string m_Reaction;

		protected override void BeforeExecute(CommandInfo command) {
			LogTag = null;
			foreach (Attribute attr in command.Module.Attributes) {
				if (attr is LogTagAttribute logTagAttribute) {
					LogTag = logTagAttribute.LogTag;
				}
			}

			if (LogTag == null) {
				LogTag = GetType().Name;
				Logger.Warning(LogTag, $"{GetType().Name} did not have a LogTag attribute and its tag has been set to its class name.");
			}

			Log = new ModuleLoggerInternal(LogTag);

			if (Context.Guild == null) {
				Log.Info($"Executing `{Context.Message.Content}` for `{Context.User.Username}#{Context.User.Discriminator}` in PM channel {Context.Channel.Name}");
			} else {
				Log.Info($"Executing `{Context.Message.Content}` for `{Context.User.Username}#{Context.User.Discriminator}` in {Context.Guild.Name} channel {Context.Channel.Name}");
			}

			m_Response = new StringBuilder();
		}

		protected override void AfterExecute(CommandInfo command) {
			if (m_Reaction != null) {
				Util.AddReaction(Context.Message, m_Reaction).GetAwaiter().GetResult();
			}

			if (m_Response.Length != 0) {
				string message = m_Response.ToString();
				m_Response.Clear();
				ReplyAsync(message).GetAwaiter().GetResult();
			}
		}

		/// <summary>
		/// Queues a message to be sent after the command has finished executing, as well as a reaction to be added.
		/// </summary>
		protected virtual void ReplyDeferred(string message, string reactionUnicode) {
			lock (m_Response) {
				m_Response.AppendLine(message);
			}
			SetReactionDeferred(reactionUnicode);
		}

		/// <summary>
		/// Queues a message to be sent after the command has finished executing.
		/// </summary>
		protected virtual void ReplyDeferred(string message) {
			lock (m_Response) {
				m_Response.AppendLine(message);
			}
		}

		/// <summary>
		/// Sets a reaction to be added after the command has finished executing. Set null to not add a reaction.
		/// </summary>
		protected virtual void SetReactionDeferred(string unicode) {
			m_Reaction = unicode;
		}

		protected async virtual Task<IUserMessage> SendDeferredResponseAsync() {
			if (m_Response.Length != 0) {
				string message = m_Response.ToString();
				m_Response.Clear();
				return await ReplyAsync(message);
			} else {
				return null;
			}
		}

		protected async virtual Task SendDeferredReactionsAsync() {
			if (m_Reaction != null) {
				await Util.AddReaction(Context.Message, m_Reaction);
				m_Reaction = null;
			}
		}

		/// <summary>
		/// Sends a response and reaction immediately.
		/// </summary>
		protected virtual async Task<IUserMessage> ReplyAsync(string message, string reactionUnicode, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			await AddReactionAsync(reactionUnicode);
			return await ReplyAsync(message, isTTS, embed, options);
		}

		protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			if (m_Response.Length != 0) {
				message = m_Response
					.AppendLine(message)
					.ToString();
				m_Response.Clear();
			}
			return await base.ReplyAsync(message);
		}

		/// <summary>
		/// Sends a reaction immediately.
		/// </summary>
		protected virtual async Task<bool> AddReactionAsync(string unicode) {
			return await Util.AddReaction(Context.Message, unicode);
		}

		/// <summary>
		/// Removes a reaction immediately.
		/// </summary>
		protected virtual async Task<bool> RemoveReactionAsync(string unicode) {
			return await Util.RemoveReaction(Context.Message, unicode, Context.Client.CurrentUser);
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
				report += $"\nAttached exception: {Util.EscapeString(exception.ToString())}\n";
			}

			Log.Error(report);
			await SNSService.SendCriticalErrorNotificationAsync(report);
			if (Config.BotOwner != null) {
				await Config.BotOwner.SendMessageAsync(report);
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
