using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace RoosterBot {
	public abstract class RoosterModuleBase<T> : ModuleBase<T>, IRoosterModuleBase where T : RoosterCommandContext {
		public ConfigService Config { get; set; } // TODO a lot of these dependencies aren't supposed to be used by actual modules.
		public GuildCultureService Cultures { get; set; } // We could/should use constructor DI instead of public settable properties.
		public ResourceService ResourcesService { get; set; }
		public RoosterCommandService CmdService { get; set; }
		public CommandResponseService CommandResponses { get; set; }
		public new T Context { get; internal set; }

		protected ModuleLogger Log { get; private set; }
		protected CultureInfo Culture {
			get {
				if (m_Culture == null) {
					m_Culture = Cultures.GetCultureForGuild(Context.Guild);
				}
				return m_Culture;
			}
		}

		private CultureInfo m_Culture = null;
		private StringBuilder m_Response;
		private bool m_Replied = false;
		
		void IRoosterModuleBase.BeforeExecuteInternal(CommandInfo command) => BeforeExecute(command);
		void IRoosterModuleBase.AfterExecuteInternal(CommandInfo command) => AfterExecute(command);

		protected override void BeforeExecute(CommandInfo command) {
			if (Context == null) {
				Context = base.Context;
			}
			
			Log = new ModuleLoggerInternal(GetType().Name);

			string logMessage = $"Executing `{Context.Message.Content}` for `{Context.User.Username}#{Context.User.Discriminator}` in ";
			if (Context.IsPrivate) {
				if (Context.Channel is IDMChannel) {
					logMessage += "DM";
				} else {
					logMessage = $"group {Context.Channel.Name}";
				}
			} else {
				logMessage += $"{Context.Guild.Name} channel {Context.Channel.Name}";
			}
			Log.Debug(logMessage);

			m_Response = new StringBuilder();
		}

		protected override void AfterExecute(CommandInfo command) {
			if (m_Response.Length != 0) {
				string message = m_Response.ToString();
				m_Response.Clear();
				ReplyAsync(message).GetAwaiter().GetResult();
			}
		}

		/// <summary>
		/// Queues a message to be sent after the command has finished executing.
		/// </summary>
		protected virtual void ReplyDeferred(string message) {
			lock (m_Response) {
				m_Response.AppendLine(message);
			}
		}

		protected virtual Task<IUserMessage> SendDeferredResponseAsync() {
			if (m_Response.Length != 0) {
				string message = m_Response.ToString();
				m_Response.Clear();
				return ReplyAsync(message);
			} else {
				return null;
			}
		}

		protected override async Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			if (m_Response.Length != 0) {
				message = m_Response
					.AppendLine(message)
					.ToString();
				m_Response.Clear();
			}

			IUserMessage ret;
			if (Context.Responses == null) {
				// The command was not edited, or the command somehow did not invoke a reply.
				IUserMessage response = await Context.Channel.SendMessageAsync(message, isTTS, embed, options);
				CommandResponses.AddResponse(Context.Message, response);
				ret = response;
			} else {
				// The command was edited.
				ret = await Util.ModifyResponsesIntoSingle(message, Context.Responses, m_Replied);

				CommandResponses.ModifyResponse(Context.Message, new[] { ret });
			}

			m_Replied = true;

			CommandResponses.AddResponse(Context.Message, ret);
			return ret;
		}

		// Discord.NET offers a command result system (IResult), we may be able to use that instead of MinorError and FatalError
		protected virtual Task MinorError(string message) {
			return ReplyAsync(Util.ErrorPrefix + message);
		}

		protected virtual async Task FatalError(string message, Exception exception = null) {
			string report = $"Fatal error executing `{Context.Message.Content}` for `{Context.User.Mention}` in {Context.Guild?.Name ?? "DM"} channel {Context.Channel.Name}: {message}";

			Log.Error(report, exception);

			if (exception != null) {
				report += $"\nAttached exception: {Util.EscapeString(exception.ToStringDemystified())}\n";
			}
			
			if (Config.BotOwner != null) {
				await Config.BotOwner.SendMessageAsync(report);
			}
			
			string response = Util.ErrorPrefix + GetString("RoosterBot_FatalError");
			await ReplyAsync(response);
		}

		protected string GetString(string name) {
			return ResourcesService.GetString(Assembly.GetCallingAssembly(), Culture, name);
		}

		protected string GetString(string name, params object[] args) {
			return string.Format(ResourcesService.GetString(Assembly.GetCallingAssembly(), Culture, name), args);
		}

		public abstract class ModuleLogger {
			protected internal string m_Tag;

			public void Verbose(string message, Exception e = null) {
				Logger.Verbose(m_Tag, message, e);
			}

			public void Debug(string message, Exception e = null) {
				Logger.Debug(m_Tag, message, e);
			}

			public void Info(string message, Exception e = null) {
				Logger.Info(m_Tag, message, e);
			}

			public void Warning(string message, Exception e = null) {
				Logger.Warning(m_Tag, message, e);
			}

			public void Error(string message, Exception e = null) {
				Logger.Error(m_Tag, message, e);
			}

			public void Critical(string message, Exception e = null) {
				Logger.Critical(m_Tag, message, e);
			}
		}

		private sealed class ModuleLoggerInternal : ModuleLogger {
			public ModuleLoggerInternal(string tag) {
				m_Tag = tag;
			}
		}
	}

	public abstract class RoosterModuleBase : RoosterModuleBase<RoosterCommandContext> { }

	public class RoosterCommandContext : ICommandContext {
		public IDiscordClient Client { get; }
		public IUserMessage Message { get; }
		public IUser User { get; }
		public IMessageChannel Channel { get; }
		public IGuild Guild { get; }
		public bool IsPrivate { get; }
		// If this is null, we should make a new message.
		public IReadOnlyList<IUserMessage> Responses { get; }
		
		public RoosterCommandContext(IDiscordClient client, IUserMessage message, IUserMessage[] originalResponses) {
			Client = client;
			Message = message;
			User = message.Author;
			Channel = message.Channel;
			IsPrivate = Channel is IPrivateChannel;
			Guild = IsPrivate ? (User as SocketUser)?.MutualGuilds.First() : (Channel as IGuildChannel).Guild;
			Responses = originalResponses;
		}
	}

	internal interface IRoosterModuleBase {
		void BeforeExecuteInternal(CommandInfo command);
		void AfterExecuteInternal(CommandInfo command);
	}
}
