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

		public static void OnScheduleRequestByUser(this UserConfig userConfig, IChannel channel, IdentifierInfo identifier, DateTime recordEndTime) {
			userConfig.SetData("schedule.lastCommand." + channel.Id, new LastScheduleCommandInfo(identifier, recordEndTime));
		}

		public static void RemoveLastScheduleCommand(this UserConfig userConfig, IChannel channel) {
			userConfig.SetData<LastScheduleCommandInfo?>("schedule.lastCommand." + channel.Id, null);
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
