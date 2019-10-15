using Discord;
using Discord.WebSocket;
using MiscStuffComponent.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiscStuffComponent {
	internal sealed class PropagationHandler {
		private readonly Dictionary<IGuildUser, DateTime> m_LastPropagations;
		private readonly PropagationService m_Service;

		public PropagationHandler(DiscordSocketClient client, PropagationService service) {
			m_LastPropagations = new Dictionary<IGuildUser, DateTime>();

			client.MessageReceived += DarkSidePropagation;
			m_Service = service;
		}

		private async Task DarkSidePropagation(SocketMessage arg) {
			const long DarkSideRole = 633610934979395584;
			if (arg is SocketUserMessage sum && arg.Author is IGuildUser sendingUser && sendingUser.RoleIds.Any(id => id == DarkSideRole)) {
				if (MentionUtils.TryParseUser(arg.Content, out ulong mentionedUserId)) {
					if (m_LastPropagations.TryGetValue(sendingUser, out DateTime lastProp)) {
						if ((DateTime.Now - lastProp).TotalMinutes < 10) {
							return;
						} else {
							m_LastPropagations[sendingUser] = DateTime.Now;
						}
					} else {
						m_LastPropagations.Add(sendingUser, DateTime.Now);
					}

					ITextChannel textChannel = arg.Channel as ITextChannel;
					IGuildUser propagatedUser = await textChannel.GetUserAsync(mentionedUserId);
					Logger.Info("Propagation", $"Propagating role from {sendingUser.Username}#{sendingUser.Discriminator} to {propagatedUser.Username}#{propagatedUser.Discriminator}");
					await propagatedUser.AddRoleAsync(textChannel.Guild.GetRole(DarkSideRole));
				}
			}
		}
	}
}
