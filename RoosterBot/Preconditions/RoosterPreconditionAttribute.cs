using Discord.Commands;

namespace RoosterBot {
	// TODO (feature) Child classes need a RoosterCommandContext, do a similar thing to RoosterTypeReader
	public abstract class RoosterPreconditionAttribute : PreconditionAttribute {
		public abstract string Summary { get; }
	}
}
