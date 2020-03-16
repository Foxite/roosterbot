using System.Collections.Concurrent;

namespace RoosterBot.DiscordNet {
	public class UserListService {
		private readonly ConcurrentDictionary<Discord.IGuildUser, ConcurrentBag<Discord.IGuildUser>> m_ContextLists;

		public UserListService() {
			m_ContextLists = new ConcurrentDictionary<Discord.IGuildUser, ConcurrentBag<Discord.IGuildUser>>();
		}

		public ConcurrentBag<Discord.IGuildUser>? GetLastListForUser(Discord.IGuildUser user) => m_ContextLists.TryGetValue(user, out var result) ? result : null;
		public bool RemoveListForUser(Discord.IGuildUser user) => m_ContextLists.TryRemove(user, out _);
	}
}