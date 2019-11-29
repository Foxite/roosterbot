using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.Tools {
	[Group("emote"), HiddenFromList]
	public class EmoteTheftModule : RoosterModule {
		[Command("steal"), RequireBotManager]
		public async Task<CommandResult> StealEmoteCommand() {
			// Get last message before command
			IEnumerable<IUserMessage> messages = (await Context.Channel.GetMessagesAsync(5).ToList()).SelectMany(c => c).OfType<IUserMessage>();
			if (messages.Any()) {
				bool sawCommand = false;
				foreach (IUserMessage message in messages) {
					if (sawCommand) {
						return await StealEmoteCommand(message);
					} else if (message.Id == Context.Message.Id) {
						sawCommand = true;
					}
				}
				return TextResult.Error("Ninja'd");
			} else {
				return TextResult.Error("Cache is disabled");
			}
		}
		
		[Command("steal"), Priority(1), RequireBotManager]
		public async Task<CommandResult> StealEmoteCommand(IUserMessage message) {
			MatchCollection matches = Regex.Matches(message.Content, @"((?<!\\)\<:[A-z0-9\-_]+?:[0-9]+?\>)");

			static bool canStoreStaticEmote  (IGuild guild) => guild.Emotes.Count(emote => !emote.Animated) < 50;
			static bool canStoreAnimatedEmote(IGuild guild) => guild.Emotes.Count(emote =>  emote.Animated) < 50;

			if (matches.Count > 0) {
				IEnumerable<IGuild> storageGuilds = await Task.WhenAll(new ulong[] {
					346682476149866497, 649728161281736704
				}.Select(id => Context.Client.GetGuildAsync(id)));
				
				string stolenEmotes = "";
				int animatedStealFails = 0;
				int staticStealFails = 0;
				bool anySuccessfulSteals = false;
				using (var webClient = new WebClient()) {
					IGuild getStorageGuild(Emote emote) => storageGuilds.FirstOrDefault(emote.Animated ? (Func<IGuild, bool>) canStoreAnimatedEmote : canStoreStaticEmote);

					// Download an emote image while we're uploading the previous one.
					Task<GuildEmote>? createEmote = null;
					GuildEmote stolenEmote;
					IGuild? guild = null;
					for (int i = 0; i < matches.Count; i++) {
						Match match = matches[i];

						// Download an emote image while we're uploading the previous one.
						var emote = Emote.Parse(match.Captures[0].Value);
						guild = getStorageGuild(emote);
						if (guild == null) {
							(emote.Animated ? ref animatedStealFails : ref staticStealFails)++;
							continue;
						} else {
							anySuccessfulSteals = true;
						}

						byte[] emoteBytes = await webClient.DownloadDataTaskAsync(emote.Url);
						if (createEmote != null) {
							stolenEmote = await createEmote;
							stolenEmotes += stolenEmote.ToString();
						}
						createEmote = guild.CreateEmoteAsync(emote.Name, new Image(new MemoryStream(emoteBytes)));
					}
					if (createEmote != null) {
						stolenEmote = await createEmote;
						stolenEmotes += stolenEmote.ToString();
					}
				}
				var result = new CompoundResult("\n");
				
				if (anySuccessfulSteals) {
					result.AddResult(TextResult.Success(stolenEmotes));
				}

				if (staticStealFails > 0 || animatedStealFails > 0) {
					string response = $"Unable to steal {animatedStealFails} animated and {staticStealFails} static emotes because we're out of space.";
					result.AddResult(anySuccessfulSteals ? TextResult.Warning(response) : TextResult.Error(response));
				}

				return result;
			} else {
				return TextResult.Error("Did not find any emotes");
			}
		}
	}
}
