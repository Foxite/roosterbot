using System.Diagnostics;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExecutionFailedHandler : PostCommandHandler {
		private readonly ConfigService m_Config;
		private readonly ResourceService m_ResourcesService;

		internal CommandExecutionFailedHandler(RoosterCommandService commands, ConfigService config, ResourceService resources) {
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
