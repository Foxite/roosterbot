using Discord;
using System;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public static class ScheduleUtil {
		#region Last schedule command
		public static LastScheduleCommandInfo? GetLastScheduleCommand(this UserConfig userConfig, IChannel channel) {
			userConfig.TryGetData("schedule.lastCommand." + channel.Id, out LastScheduleCommandInfo? lsci);
			return lsci;
		}

		public static async Task OnScheduleRequestByUserAsync(this UserConfig userConfig, IChannel channel, IdentifierInfo identifier, DateTime recordEndTime) {
			LastScheduleCommandInfo lsci = new LastScheduleCommandInfo(identifier, recordEndTime);
			userConfig.SetData("schedule.lastCommand." + channel.Id, lsci);
			await userConfig.UpdateAsync();
		}

		public static async Task RemoveLastScheduleCommandAsync(this UserConfig userConfig, IChannel channel) {
			userConfig.SetData<LastScheduleCommandInfo?>("schedule.lastCommand." + channel.Id, null);
			await userConfig.UpdateAsync();
		}
		#endregion

		#region User classes
		public static event Func<ulong, StudentSetInfo?, StudentSetInfo, Task>? UserChangedClass;

		public static StudentSetInfo? GetStudentSet(this UserConfig config) {
			config.TryGetData("schedule.userClass", out StudentSetInfo? ssi);
			return ssi;
		}

		/// <returns>The old StudentSetInfo, or null if none was assigned</returns>
		public static async Task<StudentSetInfo?> SetStudentSetAsync(this UserConfig config, StudentSetInfo ssi) {
			StudentSetInfo? old = GetStudentSet(config);
			config.SetData("schedule.userClass", ssi);
			await config.UpdateAsync();
			if (old != ssi && UserChangedClass != null) {
				await DelegateUtil.InvokeAsyncEventSequential(UserChangedClass, config.UserId, old, ssi);
			}
			return old;
		}
		#endregion
	}
}
