using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using RoosterBot.DateTimeUtils;

namespace RoosterBot.Schedule {
	public sealed class DayScheduleEnumerator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly RoosterCommandContext m_Context;
		private readonly IdentifierInfo m_Identifier;
		private readonly ResourceService m_Resources;
		private readonly ScheduleService m_Schedule;
		private readonly string[] m_Header;
		private readonly DateTime m_InitialDate;
		private DateTime m_CurrentDate;

		public RoosterCommandResult Current {
			get {
				ScheduleRecord[] result = m_Schedule.GetSchedulesForDate(m_Identifier, m_CurrentDate, m_Context).Result;
				var cells = new IReadOnlyList<string>[result.Length + 1];
				cells[0] = m_Header;

				int recordIndex = 1;
				foreach (ScheduleRecord record in result) {
					cells[recordIndex] = record.PresentRow(m_Resources, m_Context.Culture);
					recordIndex++;
				}
				return new TableResult(m_Identifier.DisplayText + ": " + DateTimeUtil.GetRelativeDateReference(m_CurrentDate, m_Context.Culture), cells);
			}
		}

		public DayScheduleEnumerator(RoosterCommandContext context, IdentifierInfo info, DateTime initialDate, string[] header) {
			m_Context = context;
			m_Identifier = info;
			m_Header = header;
			m_InitialDate = initialDate.AddDays(-1);
			m_CurrentDate = m_InitialDate;
			
			m_Resources = m_Context.ServiceProvider.GetRequiredService<ResourceService>();
			m_Schedule = m_Context.ServiceProvider.GetRequiredService<ScheduleService>();
		}

		object? IEnumerator.Current => Current;

		public void Dispose() { }
		public bool MoveNext() {
			m_CurrentDate = m_CurrentDate.AddDays(1);
			return true;
		}

		public bool MovePrevious() {
			m_CurrentDate = m_CurrentDate.AddDays(-1);
			return true;
		}

		public void Reset() => m_CurrentDate = m_InitialDate;
	}
}
