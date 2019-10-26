using Discord.Commands;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RoosterBot.Meta {
	[HiddenFromList]
	public class DiagnosticModule : RoosterModuleBase {
		[Command("test", RunMode = RunMode.Async), RequireBotManager]
		public async Task ParseCommand([Remainder] string input) {
			SearchResult searchResult = CmdService.Search(input);
			if (searchResult.IsSuccess) {
				foreach (CommandMatch match in searchResult.Commands) {
					PreconditionResult preconditionResult = await match.CheckPreconditionsAsync(Context, Program.Instance.Components.Services);
					ParseResult parseResult;
					if (preconditionResult.IsSuccess) {
						parseResult = await match.ParseAsync(Context, searchResult, preconditionResult, Program.Instance.Components.Services);
					}
					if (Debugger.IsAttached) {
						Debugger.Break();
					}
				}
			}
		}
	}
}
