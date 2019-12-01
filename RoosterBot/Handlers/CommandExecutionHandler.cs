using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Qmmands;

namespace RoosterBot {
	internal sealed class CommandExecutionHandler : RoosterHandler {
		public DiscordSocketClient Client { get; set; } = null!;
		public RoosterCommandService Commands { get; set; } = null!;
		public ResourceService Resources { get; set; } = null!;
		public ConfigService Config { get; set; } = null!;

		public CommandExecutionHandler(IServiceProvider isp) : base(isp) { }

		public async Task ExecuteCommandAsync(string input, IUserMessage message, GuildConfig guildConfig, UserConfig userConfig) {
			var context = new RoosterCommandContext(Client, message, userConfig, guildConfig, Program.Instance.Components.Services);
			IResult result = await Commands.ExecuteAsync(input, context);

			if (!(result.IsSuccessful || result is ExecutionFailedResult)) { // These will be handled by CommandExecuted and CommandExecutionFailed events
				string response = "";

				switch (result) {
					case CommandDisabledResult _:
						response = Resources.GetString(context.Culture, "CommandHandling_Disabled");
						break;
					case CommandNotFoundResult _:
						response = string.Format(Resources.GetString(context.Culture, "CommandHandling_NotFound"), context.GuildConfig.CommandPrefix);
						break;
					case CommandOnCooldownResult cooldown:
						// TODO (feature) Enable cooldowns by setting the bucket generator delegate
						response = string.Format(Resources.GetString(context.Culture, "CommandHandling_Cooldown"), cooldown.Cooldowns.First().RetryAfter);
						break;
					case OverloadsFailedResult overloads:
						response = Resources.GetString(context.Culture, "CommandHandling_OverloadsFailed") + "\n";
						response += string.Join('\n', overloads.FailedOverloads.Select(kvp => "`" + context.GuildConfig.CommandPrefix + kvp.Key.GetSignature() + "`: " +
							Resources.ResolveString(context.Culture, Program.Instance.Components.GetComponentForModule(kvp.Key.Module), kvp.Value.Reason)));
						break;
					case ArgumentParseFailedResult argument:
						response = Resources.GetString(context.Culture, "RoosterBot_FatalError"); // Not actually sure what to do with this
						Logger.Error("PostHandler", "Executing " + context.ToString() + " resulted in ArgumentParseFailedResult: " + argument.Reason);
						break;
					case IRoosterTypeParserResult type:
						response = string.Format(Resources.ResolveString(context.Culture, type.ErrorReasonComponent, type.Reason), type.ErrorReasonObjects);
						break;
					case TypeParseFailedResult type:
						response = type.Reason;
						break;
					case ChecksFailedResult check:
						foreach ((CheckAttribute Check, CheckResult Result) in check.FailedChecks) {
							if (Result is RoosterCheckResult rcr) {
								Component? component = Program.Instance.Components.GetComponentFromAssembly(check.FailedChecks.First().Check.GetType().Assembly);
								response += Resources.ResolveString(context.Culture, component, check.Reason);
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
								response += string.Format(Resources.GetString(context.Culture, "CommandHandling_ParamCheckFailed"), paramCheck.Parameter.Name,
									Resources.ResolveString(context.Culture, component, Result.Reason));
							} else {
								response += Result.Reason;
							}
							response += "\n";
						}
						break;
					default:
						string report = $"PostCommandHandler got an unknown result: {result.GetType().FullName}. This is the ToString: {result.ToString()}";
						Logger.Warning("CommandHandler", report);
						await Config.BotOwner.SendMessageAsync(report);
						response = Resources.GetString(context.Culture, "RoosterBot_FatalError");
						break;
				}

				await context.RespondAsync(Util.Error + response);
			}
		}
	}
}
