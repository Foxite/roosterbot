using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;
using Discord.WebSocket;

namespace RoosterBot.DiscordNet {
	/// <summary>
	/// A handler that watches for added reactions on an <see cref="IUserMessage"/> and executes callbacks when the message author adds them to the message.
	/// When the message is deleted, or after a timer expires (which resets when a callback is triggered), the handler disposes itself and no more reactions will be handled.
	/// </summary>
	public sealed class InteractiveMessageHandler {
		private readonly IUserMessage m_BotMessage;
		private readonly IUserMessage m_UserCommand;
		private readonly Discord.IUser m_User;
		private readonly Dictionary<Discord.IEmote, Func<Task>> m_Callbacks;
		private readonly TimeSpan m_Expiration;
		private Timer? m_ExpiryTimer;

		/// <param name="expires">5 minutes, if null.</param>
		public InteractiveMessageHandler(IUserMessage botMessage, IUserMessage userCommand, Discord.IUser user, Dictionary<Discord.IEmote, Func<Task>> callbacks, TimeSpan? expires = null) {
			m_BotMessage = botMessage;
			m_UserCommand = userCommand;
			m_User = user;
			m_Callbacks = callbacks;
			m_Expiration = expires ?? TimeSpan.FromMinutes(5);

			ResetTimer();

			DiscordNetComponent.Instance.Client.ReactionAdded += OnReactionAdded;
			DiscordNetComponent.Instance.Client.MessageDeleted += OnMessageDeleted;
			DiscordNetComponent.Instance.Client.MessageUpdated += OnMessageUpdated;

			_ = Task.Run(async () => {
				// Remove our reactions left over from previous InteractiveMessageHandlers
				foreach (Discord.IEmote emote in botMessage.Reactions.Keys.Except(callbacks.Keys)) {
					await botMessage.RemoveReactionAsync(emote, DiscordNetComponent.Instance.Client.CurrentUser);
				}

				foreach (KeyValuePair<Discord.IEmote, Func<Task>> kvp in callbacks) {
					// Don't bother adding the reaction if it's already there
					if (!(botMessage.Reactions.TryGetValue(kvp.Key, out var reactionData) && reactionData.IsMe)) {
						await botMessage.AddReactionAsync(kvp.Key);
					}
				}
			});
		}

		private void OnExpired(object sender, ElapsedEventArgs e) => Dispose();

		private Task OnMessageUpdated(Cacheable<Discord.IMessage, ulong> cachedMessage, SocketMessage message, ISocketMessageChannel channel) {
			_ = Task.Run(() => {
				if (cachedMessage.Id == m_UserCommand.Id) {
					Dispose();
				}
			});
			return Task.CompletedTask;
		}

		private Task OnMessageDeleted(Cacheable<Discord.IMessage, ulong> message, IMessageChannel channel) {
			_ = Task.Run(() => {
				if (message.Id == m_BotMessage.Id || message.Id == m_UserCommand.Id) {
					Dispose();
				}
			});
			return Task.CompletedTask;
		}

		private Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, IMessageChannel channel, IReaction reaction) {
			_ = Task.Run(async () => {
				if (message.Id == m_BotMessage.Id) {
					await foreach (IReadOnlyCollection<Discord.IUser> item in message.Value.GetReactionUsersAsync(reaction.Emote, message.Value.Reactions[reaction.Emote].ReactionCount + 1)) {
						if (item.Any(user => user.Id == m_User.Id) && m_Callbacks.TryGetValue(reaction.Emote, out Func<Task>? callback)) {
							ResetTimer();
							await message.Value.RemoveReactionAsync(reaction.Emote, m_User.Id);
							await callback();
							break;
						}
					}
				}
			});
			return Task.CompletedTask;
		}

		// There seems no way to actually reset a timer so we just create a new one.
		private void ResetTimer() {
			if (m_ExpiryTimer != null) {
				m_ExpiryTimer.Elapsed -= OnExpired;
				m_ExpiryTimer.Dispose();
			}
			m_ExpiryTimer = new Timer(m_Expiration.TotalMilliseconds);
			m_ExpiryTimer.Elapsed += OnExpired;
			m_ExpiryTimer.Start();
		}

		private void Dispose() {
			DiscordNetComponent.Instance.Client.ReactionAdded -= OnReactionAdded;
			DiscordNetComponent.Instance.Client.MessageDeleted -= OnMessageDeleted;
			DiscordNetComponent.Instance.Client.MessageUpdated -= OnMessageUpdated;
			
			if (m_ExpiryTimer != null) {
				m_ExpiryTimer.Elapsed -= OnExpired;
				m_ExpiryTimer.Dispose();
			}
		}
	}
}
