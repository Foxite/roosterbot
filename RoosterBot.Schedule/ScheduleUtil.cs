using System;

namespace RoosterBot.Schedule {
	public static class ScheduleUtil {
		#region User classes
		public static event EventHandler<UserChangedIdentifierEventArgs>? UserChangedIdentifier;

		public static IdentifierInfo? GetIdentifier(this UserConfig config) {
			config.TryGetData("schedule.userClass", out IdentifierInfoWrapper? ssi);
			return ssi?.WrappedIdentifier;
		}

		public static IdentifierInfo? SetIdentifier(this UserConfig config, IdentifierInfo info) {
			IdentifierInfo? old = GetIdentifier(config);
			config.SetData("schedule.userClass", (IdentifierInfoWrapper) info);
			if (old != info) {
				UserChangedIdentifier?.Invoke(null, new UserChangedIdentifierEventArgs(config.UserReference, old, info));
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
