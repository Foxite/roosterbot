using System;
using System.Globalization;
using System.Threading.Tasks;

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
