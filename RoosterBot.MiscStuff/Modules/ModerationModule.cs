﻿using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.MiscStuff {
	[HiddenFromList]
	public class ModerationModule : RoosterModuleBase {
		[Command("users unnamed"), UserIsModerator, RequireContext(ContextType.Guild)]
		public async Task GetUnnamedUsers() {
			IEnumerable<IGrouping<DateTime?, IGuildUser>> unnamedUsers =
				from user in await Context.Guild!.GetUsersAsync()
				where !user.IsBot && user.Nickname == null
				group user by user.JoinedAt?.Date into groups
				orderby groups.Key
				select groups;

			string response = "Users with no set nickname (excluding bots, grouped by join date):\n";
			foreach (IGrouping<DateTime?, IGuildUser> group in unnamedUsers) {
				if (group.Key.HasValue) {
					response += $"\n**{group.Key.Value.ToString("yyyy-MM-dd")}**\n";
				} else {
					response += "\n**Unknown**\n";
				}
				foreach (IGuildUser item in group) {
					response += $"@{item.Username}#{item.Discriminator}\n";
				}
			}
			await ReplyAsync(response);
		}
	}
}
