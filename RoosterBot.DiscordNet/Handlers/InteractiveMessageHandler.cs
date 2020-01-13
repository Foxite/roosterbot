using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Discord;

namespace RoosterBot.DiscordNet {
	/// <summary>
	/// A handler that watches for added reactions on an <see cref="IUserMessage"/> and executes callbacks when the message author adds them to the message.
	/// When the message is deleted, or after a timer expires (which resets when a callback is triggered), the handler disposes itself and no more reactions will be handled.
	/// </summary>
	public sealed class InteractiveMessageHandler {
		private readonly IUserMessage m_Message;
		private readonly Dictionary<Discord.IEmote, Action> m_Callbacks;
		private readonly TimeSpan m_Expiration;
		private Timer? m_ExpiryTimer;

		/// <param name="expires">5 minutes, if null.</param>
		public InteractiveMessageHandler(IUserMessage message, Dictionary<Discord.IEmote, Action> callbacks, TimeSpan? expires = null) {
			m_Message = message;
			m_Callbacks = callbacks;
			m_Expiration = expires ?? TimeSpan.FromMinutes(5);

			ResetTimer();

			DiscordNetComponent.Instance.Client.ReactionAdded += OnReactionAdded;
			DiscordNetComponent.Instance.Client.MessageDeleted += OnMessageDeleted;

			_ = Task.Run(async () => {
				foreach (KeyValuePair<Discord.IEmote, Action> kvp in callbacks) {
					await message.AddReactionAsync(kvp.Key);
				}
			});
		}

		private void OnExpired(object sender, ElapsedEventArgs e) => Dispose();

		private Task OnMessageDeleted(Cacheable<Discord.IMessage, ulong> message, IMessageChannel channel) {
			if (message.Id == m_Message.Id) {
				Dispose();
			}
			return Task.CompletedTask;
		}

		private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, IMessageChannel channel, IReaction reaction) {
			if (message.Id == m_Message.Id) {
				await foreach (IReadOnlyCollection<Discord.IUser> item in message.Value.GetReactionUsersAsync(reaction.Emote, message.Value.Reactions[reaction.Emote].ReactionCount + 1)) {
					if (item.Any(user => user.Id == m_Message.Author.Id) && m_Callbacks.TryGetValue(reaction.Emote, out Action? callback)) {
						ResetTimer();
						await message.Value.RemoveReactionAsync(reaction.Emote, m_Message.Author);
						callback();
						break;
					}
				}
			}
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
		}
	}
}
