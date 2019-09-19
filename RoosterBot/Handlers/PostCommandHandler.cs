using Discord;
using Discord.Commands;
using System.Globalization;
using System.Threading.Tasks;

namespace RoosterBot {
	internal sealed class PostCommandHandler {
		private readonly RoosterCommandService m_Commands;
		private readonly ConfigService m_Config;
		private readonly GuildCultureService m_GCS;
		private readonly ResourceService m_ResourcesService;

		internal PostCommandHandler(RoosterCommandService commands, ConfigService config, GuildCultureService gcs, ResourceService resources) {
			m_Commands = commands;
			m_Config = config;
			m_ResourcesService = resources;
			m_GCS = gcs;

			m_Commands.CommandExecuted += OnCommandExecuted;
		}

		private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result) {
			if (!result.IsSuccess) {
				string response = null;
				bool bad = false;
				string badReport = $"\"{context.Message}\": ";

				CultureInfo culture = m_GCS.GetCultureForGuild(context.Guild);

				if (result.Error.HasValue) {
					switch (result.Error.Value) {
						case CommandError.UnknownCommand:
							response = string.Format(m_ResourcesService.GetString(culture, "Program_OnCommandExecuted_UnknownCommand"), m_Config.CommandPrefix);
							break;
						case CommandError.BadArgCount:
							response = m_ResourcesService.GetString(culture, "Program_OnCommandExecuted_BadArgCount");
							break;
						case CommandError.UnmetPrecondition:
							response = m_ResourcesService.ResolveString(culture, Program.Instance.Components.GetComponentForModule(command.Value.Module), result.ErrorReason);
							break;
						case CommandError.ParseFailed:
							response = m_ResourcesService.GetString(culture, "Program_OnCommandExecuted_ParseFailed");
							break;
						case CommandError.ObjectNotFound:
							badReport += "ObjectNotFound";
							bad = true;
							break;
						case CommandError.MultipleMatches:
							badReport += "MultipleMatches";
							bad = true;
							break;
						case CommandError.Exception:
							badReport += "Exception\n";
							badReport += result.ErrorReason;
							bad = true;
							break;
						case CommandError.Unsuccessful:
							badReport += "Unsuccessful\n";
							badReport += result.ErrorReason;
							bad = true;
							break;
						default:
							badReport += "Unknown error: " + result.Error.Value;
							bad = true;
							break;
					}
				} else {
					badReport += "No error reason";
					bad = true;
				}

				if (bad) {
					Logger.Error("Program", "Error occurred while parsing command " + badReport);
					if (m_Config.BotOwner != null) {
						await m_Config.BotOwner.SendMessageAsync(badReport);
					}

					response = m_ResourcesService.GetString(culture, "RoosterBot_FatalError");
				} else {
					response = Util.ErrorPrefix + response;
				}

				IUserMessage[] initialResponses = (context as RoosterCommandContext)?.Responses;
				if (initialResponses == null) {
					m_Commands.AddResponse(context.Message, await context.Channel.SendMessageAsync(response));
				} else {
					await Util.ModifyResponsesIntoSingle(response, initialResponses, false);
				}
			}
		}
	}
}
