using System.Threading.Tasks;
using Discord;

namespace RoosterBot.DiscordNet {
	public class TableResultAdapter : DiscordResultAdapter<TableResult> {
		protected override Task<IUserMessage> HandleResult(DiscordCommandContext context, TableResult result) {
			string text = result.Caption + "\n```" + Foxite.Common.StringUtil.FormatTextTable(result.Cells, result.MaxColumnWidth) + "```";

			return SendMessage(context, text);
		}
	}
}
