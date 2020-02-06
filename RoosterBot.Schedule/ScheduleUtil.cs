using System;
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
			userConfig.SetData<LastScheduleCommandInfo?>("schedule.lastCommand." + channel.Id, null);
		}
		#endregion

		#region User classes
		private static readonly AsynchronousEvent<UserChangedStudentSetEventArgs> ClassChangedEvent = new AsynchronousEvent<UserChangedStudentSetEventArgs>();

		public static event AsynchronousEventHandler<UserChangedStudentSetEventArgs> UserChangedClass {
			add => ClassChangedEvent.Hook(value);
			remove => ClassChangedEvent.Unhook(value);
		}

		public static IdentifierInfo? GetIdentifier(this UserConfig config) {
			config.TryGetData("schedule.userClass", out IdentifierInfo? ssi);
			return ssi;
		}

		/// <returns>The old StudentSetInfo, or null if none was assigned</returns>
		public static async Task<IdentifierInfo?> SetStudentSetAsync(this UserConfig config, IdentifierInfo info) {
			IdentifierInfo? old = GetIdentifier(config);
			config.SetData("schedule.userClass", info);
			if (old != info) {
				await ClassChangedEvent.InvokeAsync(new UserChangedStudentSetEventArgs(config.UserReference, old, info));
			}
			return old;
		}
		#endregion
	}

	public class UserChangedStudentSetEventArgs : EventArgs {
		public SnowflakeReference UserReference { get; }
		public IdentifierInfo? OldSet { get; }
		public IdentifierInfo NewSet { get; }

		public UserChangedStudentSetEventArgs(SnowflakeReference userReference, IdentifierInfo? oldSet, IdentifierInfo newSet) {
			UserReference = userReference;
			OldSet = oldSet;
			NewSet = newSet;
		}
	}
}
