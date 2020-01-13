using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	public partial class ScheduleModule {
		/*private PaginatedResult RespondDay(IdentifierInfo? info, DateTime date) {
			info = ResolveNullInfo(info);
			if (info != null) {
				ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, date);
				if (result.Success) {
					ScheduleRecord[] records = result.Value;

					string relativeDateReference = DateTimeUtil.GetRelativeDateReference(date, Culture);

					if (records.Length == 0) {
						string response = GetString("ScheduleModule_RespondDay_NoRecordAtRelative", info.DisplayText, relativeDateReference);
						if (DateTimeUtil.IsWeekend(date)) {
							if (DateTimeUtil.IsWithinSameWeekend(date, DateTime.Today)) {
								response += GetString("ScheduleModule_ItIsWeekend");
							} else {
								response += GetString("ScheduleModule_ThatIsWeekend");
							}
						}
						m_Result.AddResult(new TextResult(null, response));
						m_LookedUpData = new LastScheduleCommandInfo(info, date, ScheduleResultKind.Day);
					} else if (records.Length == 1) {
						string pretext = GetString("ScheduleModule_RespondDay_OnlyRecordForDay", info.DisplayText, relativeDateReference);
						await RespondRecord(pretext, info, records[0]);
						m_LookedUpData!.Kind = ScheduleResultKind.Day;
					} else {
						string pretext = GetString("ScheduleModule_ResondDay_ScheduleForRelative", info.DisplayText, relativeDateReference);

						var cells = new IReadOnlyList<string>[records.Length + 1];
						cells[0] = new string[] {
							GetString("ScheduleModule_RespondDay_ColumnActivity"),
							GetString("ScheduleModule_RespondDay_ColumnTime"),
							GetString("ScheduleModule_RespondDay_ColumnStudentSets"),
							GetString("ScheduleModule_RespondDay_ColumnTeacher"),
							GetString("ScheduleModule_RespondDay_ColumnRoom")
						};

						int recordIndex = 1;
						foreach (ScheduleRecord record in records) {
							cells[recordIndex] = record.PresentRow(Resources, Culture);
							recordIndex++;
						}
						m_LookedUpData = new LastScheduleCommandInfo(info, records.First().End.Date, ScheduleResultKind.Day);
						m_Result.AddResult(new TableResult(pretext, cells));
					}
				}
			}
		}

		protected PaginatedResult RespondWeek(IdentifierInfo? info, int weeksFromNow) {
			info = ResolveNullInfo(info);
			if (info != null) {
				ScheduleRecord[] weekRecords = await Schedules.GetWeekRecordsAsync(info, weeksFromNow, Context);
				if (weekRecords.Length > 0) {
					var caption = GetString(weeksFromNow switch {
						0 => "ScheduleModule_RespondWeek_ScheduleThisWeek",
						1 => "ScheduleModule_RespondWeek_ScheduleNextWeek",
						_ => "ScheduleModule_RespondWeek_ScheduleInXWeeks"
					}, info, weeksFromNow);
					var dayRecords = weekRecords.GroupBy(record => record.Start.DayOfWeek).ToDictionary(
						/* Key select */ /*group => group.Key,
						/* Val select */ /*group => group.ToArray()
					);
					int longestColumn = dayRecords.Max(kvp => kvp.Value.Length);

					// Header
					string[][] cells = new string[longestColumn + 2][];
					cells[0] = Enumerable.Range(1, 5).Select(dow => ((DayOfWeek) dow).GetName(Culture)).ToArray(); // Outputs day names of Monday through Friday

					// Initialize cells to empty strings
					for (int i = 1; i < cells.Length; i++) {
						cells[i] = new string[5];
						for (int j = 0; j < cells[i].Length; j++) {
							cells[i][j] = string.Empty;
						}
					}

					foreach (KeyValuePair<DayOfWeek, ScheduleRecord[]> kvp in dayRecords) {
						for (int i = 0; i < kvp.Value.Length; i++) {
							cells[i + 2][(int) kvp.Key - 1] = kvp.Value[i].Activity.DisplayText;
						}
					}

					AvailabilityInfo[] availabilities;
					availabilities = new AvailabilityInfo[5];
					foreach (KeyValuePair<DayOfWeek, ScheduleRecord[]> kvp in dayRecords) {
						availabilities[(int) kvp.Key - 1] = new AvailabilityInfo(kvp.Value.First().Start, kvp.Value.Last().End);
					}

					// Time of day start/end, and set to "---" if empty
					for (DayOfWeek dow = DayOfWeek.Monday; dow <= DayOfWeek.Friday; dow++) {
						if (!dayRecords.ContainsKey(dow)) {
							cells[2][(int) dow - 1] = "---"; // dow - 1 because 0 is Sunday
						} else {
							AvailabilityInfo dayAvailability = availabilities[(int) dow - 1];
							cells[1][(int) dow - 1] = dayAvailability.StartOfAvailability.ToString("HH:mm") + " - " + dayAvailability.EndOfAvailability.ToString("HH:mm");
						}
					}

					m_Result.AddResult(new TableResult(caption, cells));
				} else {
					var response = GetString(weeksFromNow switch {
						0 => "ScheduleModule_RespondWorkingDays_NotOnScheduleThisWeek",
						1 => "ScheduleModule_RespondWorkingDays_NotOnScheduleNextWeek",
						_ => "ScheduleModule_RespondWorkingDays_NotOnScheduleInXWeeks"
					}, info, weeksFromNow);
					m_Result.AddResult(new TextResult(null, response));
				}
				m_LookedUpData = new LastScheduleCommandInfo(info, DateTime.Today.AddDays(7 * weeksFromNow), ScheduleResultKind.Week);
			}
		}*/
	}
}
