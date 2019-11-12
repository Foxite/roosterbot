﻿using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public static class ScheduleUtil {
		#region Last schedule command
		public static LastScheduleCommandInfo? GetLastScheduleCommand(this UserConfig userConfig, IChannel channel) {
			userConfig.TryGetData("schedule.lastCommand." + channel.Id, out LastScheduleCommandInfo? lsci);
			return lsci;
		}

		public static void OnScheduleRequestByUser(this UserConfig userConfig, IChannel channel, IdentifierInfo identifier, ScheduleRecord? record) {
			LastScheduleCommandInfo lsci = new LastScheduleCommandInfo(identifier, record);
			userConfig.SetData("schedule.lastCommand." + channel.Id, lsci);
		}

		public static void RemoveLastScheduleCommand(this UserConfig userConfig, IChannel channel) {
			userConfig.SetData<LastScheduleCommandInfo?>("schedule.lastCommand." + channel.Id, null);
		}
		#endregion

		#region User classes
		public static event Action<ulong, StudentSetInfo?, StudentSetInfo>? UserChangedClass;

		public static StudentSetInfo? GetStudentSet(this UserConfig config, IGuild guild) {
			config.TryGetData("schedule.lastCommand." + guild.Id, out StudentSetInfo? ssi);
			return ssi;
		}

		/// <returns>The old StudentSetInfo, or null if none was assigned</returns>
		public static StudentSetInfo? SetStudentSet(this UserConfig config, IGuild guild, StudentSetInfo ssi) {
			StudentSetInfo? old = GetStudentSet(config, guild);
			config.SetData("schedule.lastCommand." + guild.Id, ssi);
			if (old != ssi) {
				UserChangedClass?.Invoke(config.UserId, old, ssi);
			}
			return old;
		}
		#endregion
	}

	public class LastScheduleCommandInfo {
		public IdentifierInfo Identifier { get; set; }
		public ScheduleRecord? Record { get; set; }

		internal LastScheduleCommandInfo(IdentifierInfo identifier, ScheduleRecord? record) {
			Identifier = identifier;
			Record = record;
		}
	}
}