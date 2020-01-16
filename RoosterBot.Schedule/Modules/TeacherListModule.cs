﻿using System.Collections.Generic;
using System.Linq;
using Qmmands;

namespace RoosterBot.Schedule {
	[Name("#TeacherListModule_Name")]
	public class TeacherListModule : RoosterModule {
		public TeacherNameService Teachers { get; set; } = null!;

		[Command("#TeacherListModule_CommandName"), Description("#TeacherListModule_TeacherListCommand_Summary")]
		public CommandResult TeacherListCommand([Remainder, Name("#TeacherListModule_ListCommand_NameParameterName")] string name = "") {
			IEnumerable<TeacherInfo> records;
			if (string.IsNullOrWhiteSpace(name)) {
				records = Teachers.GetAllRecords(Context.ChannelConfig.ChannelReference);
			} else {
				records = Teachers.Lookup(Context.ChannelConfig.ChannelReference, name).Select(match => match.Teacher);
			}

			if (!records.Any()) {
				return TextResult.Info(GetString("TeacherListModule_TeacherListCommand_NoTeachersFound"));
			} else {
				string[][] cells = new string[records.Count()][];
				string[] header = new string[] {
					GetString("TeacherListModule_TeacherListCommand_ColumnFullName"),
					GetString("TeacherListModule_TeacherListCommand_ColumnAbbreviation"),
					GetString("TeacherListModule_TeacherListCommand_DiscordName")
				};
				int recordIndex = 0;
				foreach (TeacherInfo record in records) {
					cells[recordIndex] = new string[3];
					cells[recordIndex][0] = record.DisplayText;
					cells[recordIndex][1] = record.ScheduleCode;
					cells[recordIndex][2] = record.DiscordUser ?? "";

					recordIndex++;
				}

				return new PaginatedResult(new PaginatedTableEnumerator("Table", header, cells), GetString("TeacherListModule_TeacherListCommand_ResultsFor", name));
			}
		}
	}
}
