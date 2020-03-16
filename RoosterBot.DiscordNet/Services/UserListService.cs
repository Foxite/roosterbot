using System.Collections.Concurrent;
using System.Collections.Generic;

namespace RoosterBot.DiscordNet {
	public class UserListService {
		private readonly ConcurrentDictionary<IUser, IEnumerable<Discord.IGuildUser>> m_ContextLists;

		public UserListService() {
			m_ContextLists = new ConcurrentDictionary<IUser, IEnumerable<Discord.IGuildUser>>();
		}

		public void SetListUserUser(IUser user, IEnumerable<Discord.IGuildUser> enumerable) => m_ContextLists[user] = enumerable;
		public IEnumerable<Discord.IGuildUser>? GetLastListForUser(IUser user) => m_ContextLists.TryGetValue(user, out var result) ? result : null;
		public bool RemoveListForUser(IUser user) => m_ContextLists.TryRemove(user, out _);
	}
}