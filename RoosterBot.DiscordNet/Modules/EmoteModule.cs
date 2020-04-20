using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot.DiscordNet {
	[Group("emote"), HiddenFromList]
	public class EmoteModule : RoosterModule<DiscordCommandContext> {
		private IEnumerable<IGuild>? m_StorageGuilds;

		#region Helper stuff
		private IGuild GetStorageGuild(bool isAnimated) {
			if (m_StorageGuilds == null) {
				m_StorageGuilds = DiscordNetComponent.Instance.EmoteStorageGuilds.Select(id => Context.Client.GetGuild(id));
			}

			bool canStoreStaticEmote  (IGuild guild) => guild.Emotes.Count(emote => !isAnimated) < 50;
			bool canStoreAnimatedEmote(IGuild guild) => guild.Emotes.Count(emote =>  isAnimated) < 50;

			return m_StorageGuilds.FirstOrDefault(isAnimated ? (Func<IGuild, bool>) canStoreAnimatedEmote : canStoreStaticEmote);
		}

		private async Task<IUserMessage?> GetMessageBeforeCommand() {
			// Get last message before command
			IEnumerable<IUserMessage> messages = (await Context.Channel.GetMessagesAsync(5).ToListAsync()).SelectMany(c => c).OfType<IUserMessage>();
			if (messages.Any()) {
				bool sawCommand = false;
				foreach (IUserMessage message in messages) {
					if (sawCommand) {
						return message;
					} else if (message.Id == Context.Message.Id) {
						sawCommand = true;
					}
				}
			}
			return null;
		}

		private static List<Emote> GetEmotesFromText(string text) {
			return Regex.Matches(text, @"((?<!\\)\<a?:[A-z0-9\-_]+?:[0-9]+?\>)")
				.Select(match => Emote.TryParse(match.Captures[0].Value, out Emote ret) ? ret : null)
				.WhereNotNull()
				.ToList();
		}

		private async Task<CommandResult> CreateEmotes(IEnumerable<EmoteCreationData> emotes) {
			var successfulEmotes = new List<Emote>();
			int animatedFails = 0;
			int staticFails = 0;
			using (var webClient = new WebClient()) {
				// Download an emote image while we're uploading the previous one.
				Task<GuildEmote>? createEmote = null;
				GuildEmote stolenEmote;
				IGuild? guild = null;
				foreach (EmoteCreationData emote in emotes) {
					guild = GetStorageGuild(emote.IsAnimated);
					if (guild == null) {
						(emote.IsAnimated ? ref animatedFails : ref staticFails)++;
						continue;
					}

					byte[] emoteBytes = await webClient.DownloadDataTaskAsync(emote.Url);
					if (createEmote != null) {
						stolenEmote = await createEmote;
						successfulEmotes.Add(stolenEmote);
					}
					createEmote = guild.CreateEmoteAsync(emote.Name, new Image(new MemoryStream(emoteBytes)));
				}
				if (createEmote != null) {
					stolenEmote = await createEmote;
					successfulEmotes.Add(stolenEmote);
				}
			}
			var result = new CompoundResult("\n");

			if (successfulEmotes.Count > 0) {
				result.AddResult(TextResult.Success(string.Join(' ', successfulEmotes.Select(emote => emote.ToString()))));
			}

			if (staticFails > 0 || animatedFails > 0) {
				string response = $"Unable to create {animatedFails} animated and {staticFails} static emotes because we're out of space.";
				result.AddResult(successfulEmotes.Count > 0 ? TextResult.Warning(response) : TextResult.Error(response));
			}

			return result;
		}

		private struct EmoteCreationData {
			public string Name { get; }
			public string Url { get; }
			public bool IsAnimated { get; }

			public EmoteCreationData(string name, string url, bool isAnimated) {
				Name = name;
				Url = url;
				IsAnimated = isAnimated;
			}
		}
		#endregion

		#region Commands
		[Command("steal"), RequireBotManager]
		public async Task<CommandResult> StealEmoteCommand() {
			IUserMessage? message = await GetMessageBeforeCommand();
			if (message != null) {
				return await StealEmoteCommand();
			} else {
				return TextResult.Error("Could not get message before your command.");
			}
		}

		[Command("steal"), Priority(1), RequireBotManager]
		public async Task<CommandResult> StealEmoteCommand(IUserMessage message) {
			IEnumerable<EmoteCreationData> emotes = GetEmotesFromText(message.Content).Select(emote => new EmoteCreationData(emote.Name, emote.Url, emote.Animated));

			if (emotes.Any()) {
				return await CreateEmotes(emotes);
			} else {
				return TextResult.Error("Did not find any emotes");
			}
		}
		#endregion
	}
}
