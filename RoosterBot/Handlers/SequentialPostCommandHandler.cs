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
			if (result is FailedResult) {
				string response = "";
				bool unknownResult = false;

				switch (result) {
					case CommandDisabledResult _:
						response = m_Resources.GetString(context.Culture, "CommandHandling_Disabled");
						break;
					case CommandNotFoundResult _:
						response = m_Resources.GetString(context.Culture, "CommandHandling_NotFound");
						break;
					case CommandOnCooldownResult cooldown:
						// TODO (feature) Display cooldown time left
						// cooldown seems to contain the right data for this, but I haven't looked into how it works yet
						response = string.Format(m_Resources.GetString(context.Culture, "CommandHandling_Cooldown"), "`TODO`");
						break;
					case OverloadsFailedResult overloads:
						response = m_Resources.GetString(context.Culture, "CommandHandling_OverloadsFailed") + "\n";
						response += string.Join('\n', overloads.FailedOverloads.Select(kvp => "`" + context.GuildConfig.CommandPrefix + kvp.Key.GetSignature(m_Resources, context.Culture) + "`: " +
							m_Resources.ResolveString(context.Culture, Program.Instance.Components.GetComponentForModule(kvp.Key.Module), kvp.Value.Reason)));
						break;
					default:
						unknownResult = true;
						break;
				}

				if (unknownResult) {
					await m_ConfigService.BotOwner.SendMessageAsync("NewCommandHandler got an unknown result: " + result.GetType().FullName + ". This is the ToString: " + result.ToString());
				} else {
					await context.RespondAsync(response);
				}
			}
		}
	}
}
