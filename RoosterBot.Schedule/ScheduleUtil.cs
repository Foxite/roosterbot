using System;
using System.Threading.Tasks;
using Qommon.Events;

namespace RoosterBot.Schedule {
	public static class ScheduleUtil {
		#region User classes
		// TODO should be async?
		private static readonly AsynchronousEvent<UserChangedIdentifierEventArgs> IdentifierChangedEvent = new AsynchronousEvent<UserChangedIdentifierEventArgs>();

		public static event AsynchronousEventHandler<UserChangedIdentifierEventArgs> UserChangedIdentifier {
			add => IdentifierChangedEvent.Hook(value);
			remove => IdentifierChangedEvent.Unhook(value);
		}

		public static IdentifierInfo? GetIdentifier(this UserConfig config) {
			config.TryGetData("schedule.userClass", out IdentifierInfoWrapper? ssi);
			return ssi?.WrappedIdentifier;
		}

		public static async Task<IdentifierInfo?> SetIdentifierAsync(this UserConfig config, IdentifierInfo info) {
			IdentifierInfo? old = GetIdentifier(config);
			config.SetData("schedule.userClass", (IdentifierInfoWrapper) info);
			if (old != info) {
				await IdentifierChangedEvent.InvokeAsync(new UserChangedIdentifierEventArgs(config.UserReference, old, info));
			}
			return old;
		}
		#endregion
	}

	public class UserChangedIdentifierEventArgs : EventArgs {
		public SnowflakeReference UserReference { get; }
		public IdentifierInfo? OldIdentifier { get; }
		public IdentifierInfo NewIdentifier { get; }

		public UserChangedIdentifierEventArgs(SnowflakeReference userReference, IdentifierInfo? oldInfo, IdentifierInfo newInfo) {
			UserReference = userReference;
			OldIdentifier = oldInfo;
			NewIdentifier = newInfo;
		}
	}
}
