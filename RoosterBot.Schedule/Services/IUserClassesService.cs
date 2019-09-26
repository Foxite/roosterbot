using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public interface IUserClassesService {
		/// <summary>
		/// User, old SSI, new SSE
		/// </summary>
		event Action<IUser, StudentSetInfo, StudentSetInfo> UserChangedClass;

		Task<StudentSetInfo> GetClassForDiscordUserAsync(ICommandContext context, IUser user);
		/// <returns>The old StudentSetInfo, or null if none was assigned</returns>
		Task<StudentSetInfo> SetClassForDiscordUserAsync(ICommandContext context, IUser user, StudentSetInfo ssi);
	}
}