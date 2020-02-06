using System;
using System.Threading.Tasks;
using Qommon.Events;

namespace RoosterBot.Schedule {
	public static class ScheduleUtil {
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
		public static async Task<IdentifierInfo?> SetIdentifierAsync(this UserConfig config, IdentifierInfo info) {
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
