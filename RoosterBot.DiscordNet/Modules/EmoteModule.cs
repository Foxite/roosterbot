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
	[Group("emote"), HiddenFromList, RequireUserPermissionInGuild(GuildPermission.ManageEmojis, 346682476149866497)]
	public class EmoteModule : RoosterModule<DiscordCommandContext> {
		#region Helper stuff
		private IEnumerable<IGuild>? m_StorageGuilds;

		private IGuild GetStorageGuild(bool isAnimated) {
			if (m_StorageGuilds == null) {
				m_StorageGuilds = DiscordNetComponent.Instance.EmoteStorageGuilds.Select(id => Context.Client.GetGuild(id));
			}

			bool canStoreStaticEmote(IGuild guild) => guild.Emotes.Count(emote => !isAnimated) < 50;
			bool canStoreAnimatedEmote(IGuild guild) => guild.Emotes.Count(emote => isAnimated) < 50;

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

		private enum FailureReason {
			OutOfSpace, InvalidFormat
		}

		private async Task<CommandResult> CreateEmotes(IEnumerable<EmoteCreationData> emotes) {
			var successfulEmotes = new List<Emote>();

			var fails = new Dictionary<(FailureReason reason, bool Animated), int>();

			using (var webClient = new WebClient()) {
				// Download an emote image while we're uploading the previous one.
				Task<GuildEmote>? createEmote = null;
				GuildEmote stolenEmote;
				IGuild? guild = null;
				foreach (EmoteCreationData emote in emotes) {
					string extension = Path.GetExtension(emote.Url);
					guild = GetStorageGuild(extension == "gif");
					if (guild == null) {
						(FailureReason OutOfSpace, bool) key = (FailureReason.OutOfSpace, extension == "gif");
						if (fails.ContainsKey(key)) {
							fails[key]++;
						} else {
							fails[key] = 1;
						}
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

			if (fails.Count > 0) {
				string response = "Unable to create";
				foreach (var info in fails) {
					response += $"\n- {info.Value} {(info.Key.Animated ? "animated" : "static")} emote(s) because " +
						info.Key.reason switch {
							FailureReason.InvalidFormat => "of an unsupported format",
							FailureReason.OutOfSpace => "because we're out of space",
							_ => info.Key.reason.ToString()
						};
				}
				result.AddResult(successfulEmotes.Count > 0 ? TextResult.Warning(response) : TextResult.Error(response));
			}

			return result;
		}

		private struct EmoteCreationData {
			public string Name { get; }
			public string Url { get; }

			public EmoteCreationData(string name, string url) {
				Name = name;
				Url = url;
			}
		}
		#endregion

		#region Steal (create from other message)
		[Command("steal")]
		public async Task<CommandResult> StealEmote() {
			IUserMessage? message = await GetMessageBeforeCommand();
			if (message != null) {
				return await StealEmote();
			} else {
				return TextResult.Error("Could not get message before your command.");
			}
		}

		[Command("steal"), Priority(0), RequireBotManager]
		public Task<CommandResult> StealEmote(IUserMessage message) {
			IEnumerable<EmoteCreationData> emotes = GetEmotesFromText(message.Content).Select(emote => new EmoteCreationData(emote.Name, emote.Url));

			if (emotes.Any()) {
				return CreateEmotes(emotes);
			} else {
				return Task.FromResult((CommandResult) TextResult.Error("Did not find any emotes"));
			}
		}

		[Command("steal from attachment"), Priority(1)]
		public async Task<CommandResult> StealEmoteFromAttachment(params string[] names) {
			IUserMessage? message = await GetMessageBeforeCommand();
			if (message != null) {
				return await CreateEmotes(message.Attachments.Zip(names, (att, name) => new EmoteCreationData(name, att.Url)));
			} else {
				return TextResult.Error("Could not get message before your command.");
			}
		}
		#endregion

		#region Create (create from user message)
		[Command("create from attachment"), MessageHasAttachment]
		public Task<CommandResult> CreateEmoteFromAttachment(params string[] names) {
			return CreateEmotes(Context.Message.Attachments.Zip(names, (att, name) => new EmoteCreationData(name, att.Url)));
		}

		[Command("create from url")]
		public Task<CommandResult> CreateEmoteFromUrl(Uri uri, [Remainder] string name) {
			return CreateEmotes(new[] { new EmoteCreationData(name, uri.ToString()) });
		}
		#endregion
	}
}
