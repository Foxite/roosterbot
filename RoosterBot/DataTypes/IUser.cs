using System.Threading.Tasks;

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
		/// Gets an <see cref="IChannel"/> that can be used to communicate with this user in private.
		/// This can return null if it is not possible to create a private channel with this user.
		/// </summary>
		Task<IChannel?> GetPrivateChannel();

		bool IsBotAdmin { get; }
		bool IsChannelAdmin(IChannel channel);
	}
}
