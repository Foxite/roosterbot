﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.MiscStuff {
	internal sealed class PropagationHandler {
		private readonly Dictionary<IGuildUser, DateTime> m_LastPropagations;
		private readonly PropagationService m_Service;

		public PropagationHandler(DiscordSocketClient client, PropagationService service) {
			m_LastPropagations = new Dictionary<IGuildUser, DateTime>();

			client.MessageReceived += DarkSidePropagation;
			m_Service = service;
		}

		private async Task DarkSidePropagation(SocketMessage message) {
			if (message is SocketUserMessage sum && message.Author is IGuildUser sendingUser && !message.Author.IsBot) {
				ulong? propagatedRoleId = m_Service.GetPropagatedRoleId(sendingUser.Guild);
				if (propagatedRoleId.HasValue && sendingUser.RoleIds.Any(roleId => roleId == propagatedRoleId) && message.MentionedUsers.Any()) {
					IGuildUser propagatedUser = (IGuildUser) message.MentionedUsers.First();

					if (propagatedUser.RoleIds.Any(roleId => roleId == propagatedRoleId)) {
						// Do not attempt to propagate to already infected users, and don't reset the propagator's cooldown
						return;
					}

					if (m_LastPropagations.TryGetValue(sendingUser, out DateTime lastProp)) {
						if ((DateTime.Now - lastProp).TotalMinutes < 1) {
							return;
						} else {
							m_LastPropagations[sendingUser] = DateTime.Now;
						}
					} else {
						m_LastPropagations.Add(sendingUser, DateTime.Now);
					}

					Logger.Info("Propagation", $"Propagating role from {sendingUser.Username}#{sendingUser.Discriminator} to {propagatedUser.Username}#{propagatedUser.Discriminator}");
					await propagatedUser.AddRoleAsync(sendingUser.Guild.GetRole(propagatedRoleId.Value));
				}
			}

		}
	}
}
