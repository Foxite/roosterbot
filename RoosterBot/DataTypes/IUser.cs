namespace RoosterBot {
	/// <summary>
	/// Represents a user that can interact with RoosterBot, and vice versa.
	/// </summary>
	public interface IUser : ISnowflake {
		/// <summary>
		/// Gets the user's system name.
		/// </summary>
		string UserName { get; }

		/// <summary>
		/// Gets the user's display name.
		/// </summary>
		string DisplayName { get; }

		/// <summary>
		/// Gets a special string used to explicitly mention this user.
		/// </summary>
		string Mention { get; }

		/// <summary>
		/// This user represents us.
		/// </summary>
		bool IsRoosterBot { get; }

		/// <summary>
		/// This user has administrative permissions over the bot, and can do things like shut it down.
		/// </summary>
		bool IsBotAdmin { get; }

		/// <summary>
		/// This user has administrative permissions within a given <see cref="IChannel"/>, and within the channel, can do things like delete other user's messages and remove users.
		/// </summary>
		bool IsChannelAdmin(IChannel channel);
	}
}
