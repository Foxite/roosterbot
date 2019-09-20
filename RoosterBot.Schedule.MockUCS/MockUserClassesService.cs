using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule.MockUCS {
	/// <summary>
	/// Keeps all user StudentSetInfo in a dictionary. This information is lost on shutdown. This class is meant for testing.
	/// </summary>
	public class MockUserClassesService : IUserClassesService {
		private ConcurrentDictionary<IUser, StudentSetInfo> m_UserClasses;

		public MockUserClassesService() {
			m_UserClasses = new ConcurrentDictionary<IUser, StudentSetInfo>();
		}

		public event Action<IGuildUser, StudentSetInfo, StudentSetInfo> UserChangedClass;

		public Task<StudentSetInfo> GetClassForDiscordUserAsync(ICommandContext context, IUser user) {
			m_UserClasses.TryGetValue(user, out StudentSetInfo ssi);
			return Task.FromResult(ssi);
		}

		public Task<StudentSetInfo> SetClassForDiscordUserAsync(ICommandContext context, IUser user, StudentSetInfo ssi) {
			m_UserClasses.TryGetValue(user, out StudentSetInfo old);
			m_UserClasses[user] = ssi;
			if (user is IGuildUser guildUser) {
				UserChangedClass?.Invoke(guildUser, old, ssi);
			}
			return Task.FromResult(old);
		}
	}
}