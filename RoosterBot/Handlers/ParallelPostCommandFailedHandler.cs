using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot {
	internal sealed class ParallelPostCommandFailedHandler : ParallelPostCommandHandler {
		private readonly ConfigService m_Config;
		private readonly ResourceService m_ResourcesService;

		internal ParallelPostCommandFailedHandler(RoosterCommandService commands, ConfigService config, ResourceService resources) {
			m_Config = config;
			m_ResourcesService = resources;

			commands.CommandExecutionFailed += OnCommandFailed;
		}

		private async Task OnCommandFailed(CommandExecutionFailedEventArgs args) {
			if (args.Context is RoosterCommandContext rcc) {
				string response;
				bool bad = true;

				switch (args.Result.CommandExecutionStep) {
					case CommandExecutionStep.Checks:
					case CommandExecutionStep.ArgumentParsing: // Still not sure the difference between this,
					case CommandExecutionStep.TypeParsing:     //  and this.
						if (args.Result.Exception == null) {
							bad = false;
							response = args.Result.Reason;
						} else {
							response = args.Result.CommandExecutionStep.ToString() + ": Exception\n" + args.Result.Exception.ToStringDemystified();
						}
						break;
					default: // CooldownBucketGeneration, BeforeExecute, Command -- all are bad
						response = args.Result.CommandExecutionStep.ToString() + ": ";
						if (args.Result.Exception == null) {
							response += args.Result.Reason;
						} else {
							response += "Exception\n" + args.Result.Exception.ToStringDemystified();
						}
						break;
				}

				/* Old DNET handling
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
				case CommandError.Exception:
					
					break;
					*/

				if (bad) {
					string report = "Error occurred while executing: " + rcc.ToString() + "\n" + response;
					Logger.Error("Program", report);
					if (m_Config.BotOwner != null) {
						await m_Config.BotOwner.SendMessageAsync(report);
					}

					response = Util.Error + m_ResourcesService.GetString(rcc.Culture, "RoosterBot_FatalError");
				} else {
					response = Util.Error + response;
				}

				await CommandResponseUtil.RespondAsync(rcc, response);
				await rcc.UserConfig.UpdateAsync();
			} else {
				ForeignContext(args.Result.Command, args.Context, args.Result);
			}
		}
	}
}
