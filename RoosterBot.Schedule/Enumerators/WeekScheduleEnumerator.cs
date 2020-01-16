using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	public sealed class WeekScheduleEnumerator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly RoosterCommandContext m_Context;
		private readonly IdentifierInfo m_Identifier;
		private readonly ResourceService m_Resources;
		private readonly ScheduleService m_Schedule;
		private readonly int m_InitialOffset;
		private int m_Offset;

		public RoosterCommandResult Current {
			get {
				ScheduleRecord[] result = m_Schedule.GetWeekRecordsAsync(m_Identifier, m_Offset, m_Context).Result;

				var dayRecords = result.GroupBy(record => record.Start.DayOfWeek).ToDictionary(
					/* Key select */ group => group.Key,
					/* Val select */ group => group.ToArray()
				);
				int longestColumn = dayRecords.Max(kvp => kvp.Value.Length);

				// Header
				string[][] cells = new string[longestColumn + 2][];
				cells[0] = Enumerable.Range(1, 5).Select(dow => ((DayOfWeek) dow).GetName(m_Context.Culture)).ToArray(); // Outputs day names of Monday through Friday

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

				return new TableResult(string.Format(m_Resources.GetString(m_Context.Culture, m_Offset switch
				{
					0 => "ScheduleModule_RespondWeek_ScheduleThisWeek",
					1 => "ScheduleModule_RespondWeek_ScheduleNextWeek",
					_ => "ScheduleModule_RespondWeek_ScheduleInXWeeks"
				}), m_Identifier.DisplayText, m_Offset), cells);
			}
		}

		public WeekScheduleEnumerator(RoosterCommandContext context, IdentifierInfo info, int initialWeekOffset) {
			m_Context = context;
			m_Identifier = info;
			m_InitialOffset = initialWeekOffset - 1;
			m_Offset = m_InitialOffset;

			m_Resources = m_Context.ServiceProvider.GetRequiredService<ResourceService>();
			m_Schedule = m_Context.ServiceProvider.GetRequiredService<ScheduleService>();
		}

		object? IEnumerator.Current => Current;

		public void Dispose() { }
		public bool MoveNext() {
			m_Offset++;
			return true;
		}

		public bool MovePrevious() {
			m_Offset--;
			return true;
		}

		public void Reset() => m_Offset = m_InitialOffset;
	}
}
