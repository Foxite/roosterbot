using System;
using System.Threading.Tasks;

namespace RoosterBot.DiscordNet {
	/// <summary>
	/// The parent to all checks defined in RoosterBot.DiscordNet. This class enforces the usage of <see cref="DiscordCommandContext"/>.
	/// </summary>
	public abstract class DiscordNetCheckAttribute : RoosterCheckAttribute {
		protected override ValueTask<RoosterCheckResult> CheckAsync(RoosterCommandContext context) {
			if (context is DiscordCommandContext dcc) {
				return CheckAsync(dcc);
			} else if (ThrowOnInvalidContext) {
				throw new InvalidOperationException($"{nameof(RoosterCheckAttribute)} requires a ICommandContext instance that derives from {nameof(RoosterCommandContext)}.");
			} else {
				return ValueTaskUtil.FromResult(RoosterCheckResult.Unsuccessful("If you see this, then you may slap the programmer.", DiscordNetComponent.Instance));
			}
		}

		protected abstract ValueTask<RoosterCheckResult> CheckAsync(DiscordCommandContext context);
	}
}
