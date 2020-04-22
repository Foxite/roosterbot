using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	public sealed class MessageHasAttachmentAttribute : DiscordNetCheckAttribute {
		public override string Summary => "The command must have an attachment" + (RequiredTypes.Count > 0 ? (" of type " + string.Join(", ", RequiredTypes)) : "") + ".";

		public IReadOnlyList<string> RequiredTypes { get; }

		public MessageHasAttachmentAttribute(params string[] requiredTypes) {
			RequiredTypes = requiredTypes;
		}

		protected override ValueTask<RoosterCheckResult> CheckAsync(DiscordCommandContext context) {
			if (context.Message.Attachments.Any()) {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Successful);
			} else {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Unsuccessful(Summary, DiscordNetComponent.Instance));
			}
		}
	}
}
