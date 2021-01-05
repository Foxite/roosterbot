using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// The singleton class that provides entry to the command handling pipeline.
	/// </summary>
	public sealed class CommandHandler {
		private RoosterCommandService Commands { get; }
		private ResourceService Resources { get; }
		private NotificationService Notifications { get; }

		internal CommandHandler(IServiceProvider isp) {
			Commands = isp.GetService<RoosterCommandService>();
			Resources = isp.GetService<ResourceService>();
			Notifications = isp.GetService<NotificationService>();
		}

		/// <summary>
		/// Execute a command asynchronously.
		/// </summary>
		/// <param name="input">Input to parse.</param>
		/// <param name="context">The <see cref="RoosterCommandContext"/> for this execution.</param>
		public async Task ExecuteCommandAsync(string input, RoosterCommandContext context) {
			IResult result = await Commands.ExecuteAsync(input, context);

			if (!(result.IsSuccessful || result is ExecutionFailedResult)) { // These will be handled by CommandExecuted and CommandExecutionFailed events
				string response;

				switch (result) {
					case CommandDisabledResult _:
						response = HandleDisabled(context);
						break;
					case CommandNotFoundResult _:
						response = HandleNotFound(context);
						break;
					case CommandOnCooldownResult cooldown:
						response = HandleCooldown(context, cooldown);
						break;
					case OverloadsFailedResult overloads:
						response = HandleOverloadsFailed(context, overloads);
						break;
					case ArgumentParseFailedResult argument:
						response = await HandleArgumentParseFailed(context, argument);
						break;
					case TypeParseFailedResult type:
						response = HandleTypeParseFailed(context, type);
						break;
					case ChecksFailedResult check:
						response = HandleCheckFailed(context, check);
						break;
					case ParameterChecksFailedResult paramCheck:
						response = HandleParameterCheckFailed(context, paramCheck);
						break;
					default:
						response = $"PostCommandHandler got an unknown result: {result.GetType().FullName}. This is the ToString: {result}";
						Logger.Warning("CommandHandler", response);
						await Notifications.AddNotificationAsync(response);
						response = context.GetString("CommandHandling_FatalError");
						break;
				}

				await context.RespondAsync(TextResult.Error(response));
			}
		}

		private string HandleDisabled(RoosterCommandContext context) => context.GetString("CommandHandling_Disabled");
		private string HandleNotFound(RoosterCommandContext context) => context.GetString("CommandHandling_NotFound", context.ChannelConfig.CommandPrefix);
		private string HandleCooldown(RoosterCommandContext context, CommandOnCooldownResult cooldown) => context.GetString("CommandHandling_Cooldown", cooldown.Cooldowns.First().RetryAfter.ToString("c", context.Culture));

		private string HandleOverloadsFailed(RoosterCommandContext context, OverloadsFailedResult overloads) {
			var rows = overloads.FailedOverloads.Where(kvp => !(kvp.Value is ArgumentParseFailedResult)).Select(kvp => {
				string reason = kvp.Value switch
				{
					ChecksFailedResult          check      => HandleCheckFailed(context, check),
					CommandDisabledResult       _          => HandleDisabled(context),
					TypeParseFailedResult       type       => HandleTypeParseFailed(context, type),
					CommandOnCooldownResult     cooldown   => HandleCooldown(context, cooldown),
					ParameterChecksFailedResult paramCheck => HandleParameterCheckFailed(context, paramCheck),
					_ => kvp.Value.Reason,
				};
				return new KeyValuePair<string, string>("`" + context.ChannelConfig.CommandPrefix + kvp.Key.GetSignature() + "`: ", Resources.ResolveString(context.Culture, Program.Instance.Components.GetComponentForModule(kvp.Key.Module), reason));
			}).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

			if (rows.Values.Distinct().CountEquals(1)) {
				return rows.Values.First();
			} else {
				return context.GetString("CommandHandling_OverloadsFailed") + "\n" + string.Join('\n', rows.Select(kvp => kvp.Key + kvp.Value));
			}
		}

		private async Task<string> HandleArgumentParseFailed(RoosterCommandContext context, ArgumentParseFailedResult argument) {
			string response;
			if (argument.ParserResult is DefaultArgumentParserResult parseResult) {
				response = context.GetString("CommandHandling_Arguments_" + parseResult.Failure.ToString()) + "\n";
				if (parseResult.FailurePosition != null) {
					response += "`" + context.Message.Content + "`\n`" + new string(' ', context.Message.Content.Length - context.RawArguments.Length + parseResult.FailurePosition.Value) + "^`";
				}
			} else {
				response = $"PostCommandHandler got ArgumentParseFailedResult but it has an unknown ParserResult: {argument.ParserResult.GetType().FullName}. This is the ToString: {argument.ParserResult}";
				Logger.Warning("CommandHandler", response);
				if (response.Length > 2000) {
					const string TooLong = "The error message was longer than 2000 characters. This is the first section:\n";
					response = TooLong + response.Substring(0, 1999 - TooLong.Length);
				}
				await Notifications.AddNotificationAsync(response);
				response = context.GetString("CommandHandling_FatalError");
			}

			return response;
		}

		private string HandleParameterCheckFailed(RoosterCommandContext context, ParameterChecksFailedResult paramCheck) {
			string response = "";
			bool first = true;
			foreach ((ParameterCheckAttribute Check, CheckResult Result) in paramCheck.FailedChecks) {
				if (first) {
					first = false;
				} else {
					response += "\n";
				}
				if (Result is RoosterCheckResult rcr) {
					Component? component = Program.Instance.Components.GetComponentFromAssembly(Check.GetType().Assembly);
					response += context.GetString(
						"CommandHandling_ParamCheckFailed",
						paramCheck.Parameter.Name,
						string.Format(
							Resources.ResolveString(context.Culture, component, Result.Reason),
							rcr.ErrorReasonObjects.ToArray()
						)
					);
				} else {
					response += Result.Reason;
				}
			}

			return response;
		}

		private string HandleCheckFailed(RoosterCommandContext context, ChecksFailedResult check) {
			string response = "";
			bool first = true;
			foreach ((CheckAttribute Check, CheckResult Result) in check.FailedChecks) {
				if (first) {
					first = false;
				} else {
					response += "\n";
				}

				if (Result is RoosterCheckResult rcr) {
					Component? component = Program.Instance.Components.GetComponentFromAssembly(Check.GetType().Assembly);
					response += string.Format(Resources.ResolveString(context.Culture, component, rcr.Reason), rcr.ErrorReasonObjects);
				} else {
					response += Result.Reason;
				}
			}

			return response;
		}

		private string HandleTypeParseFailed(RoosterCommandContext context, TypeParseFailedResult type) {
			string response = type.Reason;
			if (type.TypeParserResult is IRoosterTypeParserResult rtpr) {
				if (!rtpr.Parser.GetType().Assembly.Equals(Assembly.GetExecutingAssembly())) {
					Component? component;
					if (rtpr.Parser is IExternalResultStringParser ersp) {
						component = ersp.ErrorReasonComponent;
					} else {
						component = Program.Instance.Components.GetComponentFromAssembly(rtpr.Parser.GetType().Assembly);
					}
					response = Resources.ResolveString(context.Culture, component, response);
				}
			}

			return response;
		}
	}
}
