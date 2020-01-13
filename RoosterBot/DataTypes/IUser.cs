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
		bool IsBotAdmin { get; }
		bool IsChannelAdmin(IChannel channel);
	}
}
