using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	public sealed class DayScheduleEnumerator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly RoosterCommandContext m_Context;
		private readonly IdentifierInfo m_Identifier;
		private readonly ResourceService m_Resources;
		private readonly ScheduleService m_Schedule;
		private readonly DateTime m_InitialDate;
		private DateTime m_CurrentDate;
		private bool m_Initialized;

		public RoosterCommandResult Current {
			get {
				if (m_Initialized) {
					ScheduleRecord[] result = m_Schedule.GetSchedulesForDate(m_Identifier, m_CurrentDate, m_Context).Result;
					var cells = new IReadOnlyList<string>[Math.Max(result.Length, 1) + 1];
					cells[0] = result[0].PresentRowHeadings(m_Resources, m_Context.Culture);

					if (result.Length == 0) {
						string[] onlyRow = Enumerable.Repeat("", cells[0].Count).ToArray();
						onlyRow[0] = "---";
						cells[1] = onlyRow;
					} else {
						for (int i = 0; i < result.Length; i++) {
							cells[i] = result[i].PresentRow(m_Resources, m_Context.Culture);
						}
					}
					return new TableResult(m_Identifier.DisplayText + ": " + DateTimeUtil.GetRelativeDateReference(m_CurrentDate, m_Context.Culture), cells);
				} else {
					throw new InvalidOperationException($"Enumerator was not initialized with {nameof(MoveNext)} or {nameof(MovePrevious)} before getting {nameof(Current)}.");
				}
			}
		}

		object? IEnumerator.Current => Current;

		public DayScheduleEnumerator(RoosterCommandContext context, IdentifierInfo info, DateTime initialDate) {
			m_Context = context;
			m_Identifier = info;
			m_InitialDate = initialDate;
			m_CurrentDate = initialDate;
			
			m_Resources = m_Context.ServiceProvider.GetRequiredService<ResourceService>();
			m_Schedule = m_Context.ServiceProvider.GetRequiredService<ScheduleService>();
		}

		public void Dispose() { }
		public bool MoveNext() {
			if (m_Initialized) {
				m_CurrentDate = m_CurrentDate.AddDays(1);
			}
			m_Initialized = true;
			return true;
		}

		public bool MovePrevious() {
			if (m_Initialized) {
				m_CurrentDate = m_CurrentDate.AddDays(-1);
			}
			m_Initialized = true;
			return true;
		}

		public void Reset() => m_CurrentDate = m_InitialDate;
	}
}
