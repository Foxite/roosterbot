using Discord.Commands;

namespace RoosterBot.Preconditions {
	public abstract class RoosterPreconditionAttribute : PreconditionAttribute {
		public abstract string Summary { get; }
	}
}
