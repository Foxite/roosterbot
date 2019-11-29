using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;

namespace RoosterBot {
	internal sealed class SequentialPostCommandHandler {
		private readonly ResourceService m_Resources;
		private readonly ConfigService m_ConfigService;

		public SequentialPostCommandHandler(ResourceService resources, ConfigService configService) {
			m_Resources = resources;
			m_ConfigService = configService;
		}

		public async Task HandleResultAsync(IResult result, RoosterCommandContext context) {
			if (!(result is SuccessfulResult)) { // Result for async commands that is returned from ExecuteAsync
				if (result.IsSuccessful) {
					if (result is RoosterCommandResult rcr) {
						await rcr.PresentAsync(context);
					}
					await context.UserConfig.UpdateAsync();
				} else {
					string response = "";
					bool bad = false;
					ComponentBase? component;

					switch (result) {
						case CommandDisabledResult _:
							response = m_Resources.GetString(context.Culture, "CommandHandling_Disabled");
							break;
						case CommandNotFoundResult _:
							response = string.Format(m_Resources.GetString(context.Culture, "CommandHandling_NotFound"), context.GuildConfig.CommandPrefix);
							break;
						case CommandOnCooldownResult cooldown:
							// Cooldowns don't actually work because we never set the cooldown bucket generator delegate,
							//  (not like I actually understand how that works),
							//  but just for completeness' sake we have this case for cooldowns.
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
							response += string.Format(m_Resources.ResolveString(context.Culture, type.ErrorReasonComponent, type.Reason), type.ErrorReasonObjects);
							break;
						case TypeParseFailedResult type:
							response += type.Reason;
							break;
						case ExecutionFailedResult execution:
							if (execution.Exception == null) {
								response = m_Resources.GetString(context.Culture, "RoosterBot_FatalError");
							} else {
								bad = true;
								response = "Exception\n" + execution.Exception.ToStringDemystified();
							}
							break;
						case ChecksFailedResult check:
							foreach ((CheckAttribute Check, CheckResult Result) in check.FailedChecks) {
								if (Result is RoosterCheckResult rcr) {
									component = Program.Instance.Components.GetComponentFromAssembly(check.FailedChecks.First().Check.GetType().Assembly);
									response = m_Resources.ResolveString(context.Culture, component, check.Reason) + "\n";
								} else {
									response = Result.Reason;
								}
							}
							break;
						case ParameterChecksFailedResult paramCheck:
							foreach ((ParameterCheckAttribute Check, CheckResult Result) in paramCheck.FailedChecks) {
								if (Result is RoosterCheckResult rcr) {
									component = Program.Instance.Components.GetComponentFromAssembly(Check.GetType().Assembly);
									response += string.Format(m_Resources.GetString(context.Culture, "CommandHandling_ParamCheckFailed"), paramCheck.Parameter.Name,
										m_Resources.ResolveString(context.Culture, component, Result.Reason)) + "\n";
								} else {
									response = Result.Reason;
								}
							}
							break;
						default:
							bad = true;
							response = "PostCommandHandler got an unknown result: " + result.GetType().FullName + ". This is the ToString: " + result.ToString();
							break;
					}

					if (bad) {
						await m_ConfigService.BotOwner.SendMessageAsync(response);
						await context.RespondAsync(Util.Error + m_Resources.GetString(context.Culture, "RoosterBot_FatalError"));
					} else {
						await context.RespondAsync(Util.Error + response);
					}
				}
			}
		}
	}
}
