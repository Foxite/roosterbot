using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExecutionHandler {
		private readonly DiscordSocketClient m_Client;
		private readonly RoosterCommandService m_Commands;
		private readonly ResourceService m_Resources;
		private readonly ConfigService m_Config;

		public CommandExecutionHandler(DiscordSocketClient client, RoosterCommandService commands, ResourceService resources, ConfigService config) {
			m_Client = client;
			m_Commands = commands;
			m_Resources = resources;
			m_Config = config;
		}

		public async Task ExecuteCommandAsync(string input, IUserMessage message, GuildConfig guildConfig, UserConfig userConfig) {
			var context = new RoosterCommandContext(m_Client, message, userConfig, guildConfig, Program.Instance.Components.Services);
			IResult result = await m_Commands.ExecuteAsync(input, context);

			if (!(result is ExecutionFailedResult) && !(result is SuccessfulResult)) { // These will be handled by CommandExecuted and CommandExecutionFailed events
				string response = "";

				switch (result) {
					case CommandDisabledResult _:
						response = m_Resources.GetString(context.Culture, "CommandHandling_Disabled");
						break;
					case CommandNotFoundResult _:
						response = string.Format(m_Resources.GetString(context.Culture, "CommandHandling_NotFound"), context.GuildConfig.CommandPrefix);
						break;
					case CommandOnCooldownResult cooldown:
						// TODO (feature) Enable cooldowns by setting the bucket generator delegate
						response = string.Format(m_Resources.GetString(context.Culture, "CommandHandling_Cooldown"), cooldown.Cooldowns.First().RetryAfter);
						break;
					case OverloadsFailedResult overloads:
						response = m_Resources.GetString(context.Culture, "CommandHandling_OverloadsFailed") + "\n";
						response += string.Join('\n', overloads.FailedOverloads.Select(kvp => "`" + context.GuildConfig.CommandPrefix + kvp.Key.GetSignature() + "`: " +
							m_Resources.ResolveString(context.Culture, Program.Instance.Components.GetComponentForModule(kvp.Key.Module), kvp.Value.Reason)));
						break;
					case ArgumentParseFailedResult argument:
						response = m_Resources.GetString(context.Culture, "RoosterBot_FatalError"); // Not actually sure what to do with this
						Logger.Error("PostHandler", "Executing " + context.ToString() + " resulted in ArgumentParseFailedResult: " + argument.Reason);
						break;
					case IRoosterTypeParserResult type:
						response = string.Format(m_Resources.ResolveString(context.Culture, type.ErrorReasonComponent, type.Reason), type.ErrorReasonObjects);
						break;
					case TypeParseFailedResult type:
						response = type.Reason;
						break;
					case ChecksFailedResult check:
						foreach ((CheckAttribute Check, CheckResult Result) in check.FailedChecks) {
							if (Result is RoosterCheckResult rcr) {
								Component? component = Program.Instance.Components.GetComponentFromAssembly(check.FailedChecks.First().Check.GetType().Assembly);
								response += m_Resources.ResolveString(context.Culture, component, check.Reason);
							} else {
								response += Result.Reason;
							}
							response += "\n";
						}
						break;
					case ParameterChecksFailedResult paramCheck:
						foreach ((ParameterCheckAttribute Check, CheckResult Result) in paramCheck.FailedChecks) {
							if (Result is RoosterCheckResult rcr) {
								Component? component = Program.Instance.Components.GetComponentFromAssembly(Check.GetType().Assembly);
								response += string.Format(m_Resources.GetString(context.Culture, "CommandHandling_ParamCheckFailed"), paramCheck.Parameter.Name,
									m_Resources.ResolveString(context.Culture, component, Result.Reason));
							} else {
								response += Result.Reason;
							}
							response += "\n";
						}
						break;
					default:
						await m_Config.BotOwner.SendMessageAsync("PostCommandHandler got an unknown result: " + result.GetType().FullName + ". This is the ToString: " + result.ToString());
						response += result.ToString();
						break;
				}

				await context.RespondAsync(Util.Error + response);
			}
		}
	}
}
