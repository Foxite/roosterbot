using System;
using System.Diagnostics;
using System.Linq;
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
			// IResult in an interface (obviously) and its actual object may be one of several types: https://github.com/discord-net/Discord.Net/tree/dev/src/Discord.Net.Commands/Results
			// I'd love to extend these into RoosterParseResult etc and add localization stuff, and turn this function into a more object-oriented handling system,
			//  but there's one critical problem: for no apparent reason[1], 4 of these are structs. This makes them impossible to extend.
			// Qmmands does not do this, so in 3.0 this won't be a problem.
			// 
			//     [1]: https://github.com/discord-net/Discord.Net/tree/dev/src/Discord.Net.Commands/Results
			if (context is RoosterCommandContext rcc) {
				if (!result.IsSuccess) {
					string response;
					bool bad = true;

					if (result.Error.HasValue) {
						switch (result.Error.Value) {
							case CommandError.UnknownCommand:
								response = string.Format(m_ResourcesService.GetString(rcc.Culture, "Program_OnCommandExecuted_UnknownCommand"), rcc.GuildConfig.CommandPrefix);
								bad = false;
								break;
							case CommandError.BadArgCount:
								response = m_ResourcesService.GetString(rcc.Culture, "Program_OnCommandExecuted_BadArgCount");
								if (command.IsSpecified) {
									response += "\n" + string.Format(m_ResourcesService.GetString(rcc.Culture, "PostCommandHandler_UsageHint"), command.Value.GetSignature(m_ResourcesService, rcc.Culture));
								}
								bad = false;
								break;
							case CommandError.UnmetPrecondition:
								if (result is RoosterPreconditionResult rpr) {
									response = string.Format(m_ResourcesService.ResolveString(rcc.Culture, rpr.ErrorReasonComponent, result.ErrorReason), rpr.ErrorReasonObjects.ToArray());
								} else {
									response = m_ResourcesService.ResolveString(rcc.Culture, Program.Instance.Components.GetComponentForModule(command.Value.Module), result.ErrorReason);
								}
								bad = false;
								break;
							case CommandError.ParseFailed:
								// This has the same problem described above, there is no way for me to know what TypeReader this was.
								// I need to only display the ErrorReason if it was one of my TypeReaders, otherwise I don't want to display the english-only reason from the built in readers.
								//response = result.ErrorReason;
								response = m_ResourcesService.GetString(rcc.Culture, "PostCommandHandler_ParseFailed");
								bad = false;
								break;
							case CommandError.MultipleMatches:
								if (result is SearchResult searchResult) {
									response = string.Join("\n", searchResult.Commands.Select(command => command.Command.GetSignature(m_ResourcesService, rcc.Culture)));
									bad = false;
								} else {
									response = "MultipleMatches\n";
									response += result.ErrorReason + "\n";
									response += result.ToString();
								}
								break;
							case CommandError.ObjectNotFound:
								response = "ObjectNotFoundn\n";
								response += result.ErrorReason + "\n";
								response += result.ToString();
								break;
							case CommandError.Exception:
								response = "Exception\n";
								if (result is ExecuteResult executeResult) {
									response += executeResult.Exception.ToStringDemystified();
								} else {
									// In any other case the actual exception is lost, ErrorReason contains the Message that was carried by the exception.
									response += result.ErrorReason;
								}
								response += "\n" + result.ToString();
								break;
							case CommandError.Unsuccessful:
								response = "Unsuccessful\n";
								response += result.ErrorReason + "\n";
								response += result.ToString();
								break;
							default:
								response = "Unknown error reason: " + result.Error.Value.ToString();
								break;
						}
					} else {
						response = "No error reason";
					}

					if (bad) {
						string report = "Error occurred while executing: " + context.ToString() + "\n" + response;
						Logger.Error("Program", report);
						if (m_Config.BotOwner != null) {
							await m_Config.BotOwner.SendMessageAsync(report);
						}

						response = Util.Error + m_ResourcesService.GetString(rcc.Culture, "RoosterBot_FatalError");
					} else {
						response = Util.Error + response;
					}

					await CommandResponseUtil.RespondAsync(rcc, response);
				}
				await rcc.UserConfig.UpdateAsync();
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
