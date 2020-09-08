using System;
using System.Globalization;
using System.Threading.Tasks;
using Qommon.Events;

namespace RoosterBot.Schedule {
	public static class ScheduleUtil {
		#region Last schedule command
		public static LastScheduleCommandInfo? GetLastScheduleCommand(this UserConfig userConfig, IChannel channel) {
			userConfig.TryGetData("schedule.lastCommand." + channel.Id, out LastScheduleCommandInfo? lsci);
			return lsci;
		}

		public static void OnScheduleRequestByUser(this UserConfig userConfig, IChannel channel, LastScheduleCommandInfo lsci) {
			userConfig.SetData("schedule.lastCommand." + channel.Id, lsci);
		}

		public static void RemoveLastScheduleCommand(this UserConfig userConfig, IChannel channel) {
			userConfig.RemoveData("schedule.lastCommand." + channel.Id);
		}
		#endregion

		#region User classes
		private static readonly AsynchronousEvent<UserChangedStudentSetEventArgs> ClassChangedEvent = new AsynchronousEvent<UserChangedStudentSetEventArgs>();

		public static event AsynchronousEventHandler<UserChangedStudentSetEventArgs> UserChangedClass {
			add => ClassChangedEvent.Hook(value);
			remove => ClassChangedEvent.Unhook(value);
		}

		public static StudentSetInfo? GetStudentSet(this UserConfig config) {
			config.TryGetData("schedule.userClass", out StudentSetInfo? ssi);
			return ssi;
		}

		/// <returns>The old StudentSetInfo, or null if none was assigned</returns>
		public static async Task<StudentSetInfo?> SetStudentSetAsync(this UserConfig config, StudentSetInfo ssi) {
			StudentSetInfo? old = GetStudentSet(config);
			config.SetData("schedule.userClass", ssi);
			if (old != ssi) {
				await ClassChangedEvent.InvokeAsync(new UserChangedStudentSetEventArgs(config.UserReference, old, ssi));
			}
			return old;
		}
		#endregion
		
		internal static async Task<ReturnValue<T>> HandleScheduleProviderErrorAsync<T>(ResourceService resources, CultureInfo culture, Func<Task<T>> action) {
			try {
				return ReturnValue<T>.Successful(await action());
			} catch (Exception e) {
				return ReturnValue<T>.Unsuccessful(TextResult.Error(resources.GetString(culture, e switch {
					IdentifierNotFoundException _ => "ScheduleModule_HandleError_NotFound",
					RecordsOutdatedException    _ => "ScheduleModule_HandleError_RecordsOutdated",
					NoAllowedChannelsException  _ => "ScheduleModule_HandleError_NoSchedulesAvailableForServer",
					_ => throw e
				})));
			}
		}
	}

	public class UserChangedStudentSetEventArgs : EventArgs {
		public SnowflakeReference UserReference { get; }
		public StudentSetInfo? OldSet { get; }
		public StudentSetInfo NewSet { get; }

		public UserChangedStudentSetEventArgs(SnowflakeReference userReference, StudentSetInfo? oldSet, StudentSetInfo newSet) {
			UserReference = userReference;
			OldSet = oldSet;
			NewSet = newSet;
		}
	}

}
