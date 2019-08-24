using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public interface IUserClassesService {
		Task<StudentSetInfo> GetClassForDiscordUserAsync(ICommandContext context, IUser user);
		Task SetClassForDiscordUserAsync(ICommandContext context, IUser user, string clazz);
	}
}