using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	internal sealed class PostCommandHandler {
		private readonly ConfigService m_Config;
		private readonly ResourceService m_ResourcesService;

		internal PostCommandHandler(RoosterCommandService commands, ConfigService config, ResourceService resources) {
			m_Config = config;
			m_ResourcesService = resources;

			commands.CommandExecuted += OnCommandExecuted;
		}

		public async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result) {
			if (context is RoosterCommandContext rcc) {
				if (!result.IsSuccess) {
					string response = "";
					bool bad = true;
					string badReport = $"\"{context.Message}\": ";

					if (result.Error.HasValue) {
						switch (result.Error.Value) {
							case CommandError.UnknownCommand:
								response = string.Format(m_ResourcesService.GetString(rcc.GuildConfig.Culture, "Program_OnCommandExecuted_UnknownCommand"), rcc.GuildConfig.CommandPrefix);
								bad = false;
								break;
							case CommandError.BadArgCount:
								response = m_ResourcesService.GetString(rcc.GuildConfig.Culture, "Program_OnCommandExecuted_BadArgCount");
								bad = false;
								break;
							case CommandError.UnmetPrecondition:
								response = m_ResourcesService.ResolveString(rcc.GuildConfig.Culture, Program.Instance.Components.GetComponentForModule(command.Value.Module), result.ErrorReason);
								// TODO (feature) Preconditions should use IResult and pass information about their component, this doesn't work with external components
								bad = false;
								break;
							case CommandError.ParseFailed:
								response = result.ErrorReason;
								bad = false;
								break;
							case CommandError.ObjectNotFound:
								badReport += "ObjectNotFound";
								break;
							case CommandError.MultipleMatches:
								badReport += "MultipleMatches";
								break;
							case CommandError.Exception:
								badReport += "Exception\n";
								badReport += result.ErrorReason;
								break;
							case CommandError.Unsuccessful:
								badReport += "Unsuccessful\n";
								badReport += result.ErrorReason;
								break;
							default:
								badReport += "Unknown error: " + result.Error.Value.ToString();
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

						response = Util.Error + m_ResourcesService.GetString(rcc.GuildConfig.Culture, "RoosterBot_FatalError");
					} else {
						response = Util.Error + response;
					}

					IUserMessage? initialResponse = rcc.Response;
					if (initialResponse == null) {
						await rcc.UserConfig.SetResponseAsync(context.Message, await context.Channel.SendMessageAsync(response));
					} else {
						await initialResponse.ModifyAsync(props => {
							props.Content += "\n\n" + response;
						});
					}
				}
			} else {
				var nse = new NotSupportedException($"A command has been executed that used a context of type {context.GetType().Name}. RoosterBot does not support this as of version 2.1. " +
					"All command context objects must be derived from RoosterCommandContext. " +
					$"Starting in RoosterBot 3.0, it will no longer be possible to add modules that are not derived from {nameof(RoosterModuleBase)}. " +
					"This exception object contains useful information in its Data property; use a debugger to see where this error came from.");
				nse.Data.Add("command", command);
				nse.Data.Add("context", context);
				nse.Data.Add("result", result);
				throw nse;
			}
		}
	}
}
