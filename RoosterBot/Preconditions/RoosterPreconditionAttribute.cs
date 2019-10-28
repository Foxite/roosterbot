using Discord.Commands;

namespace RoosterBot {
	public abstract class RoosterPreconditionAttribute : PreconditionAttribute {
		public abstract string Summary { get; }
	}
}
