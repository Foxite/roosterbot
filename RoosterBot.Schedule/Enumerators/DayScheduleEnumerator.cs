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
		private bool m_PreInitial = true;
		
		object? IEnumerator.Current => Current;
		public RoosterCommandResult Current { get; private set; } = null!;

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
			if (m_PreInitial) {
				m_CurrentDate = m_InitialDate;
			} else {
				m_CurrentDate = m_CurrentDate.AddDays(1);
			}

			try {
				return Update();
			} finally {
				m_PreInitial = false;
			}
		}

		public bool MovePrevious() {
			if (m_PreInitial) {
				m_CurrentDate = m_InitialDate;
			} else {
				m_CurrentDate = m_CurrentDate.AddDays(-1);
			}

			try {
				return Update();
			} finally {
				m_PreInitial = false;
			}
		}

		public void Reset() {
			m_PreInitial = true;
			m_CurrentDate = m_InitialDate;
		}

		private bool Update() {
			ReturnValue<ScheduleRecord[]> scheduleResult =
				ScheduleUtil.HandleScheduleProviderErrorAsync(m_Resources, m_Context.Culture, () => m_Schedule.GetSchedulesForDate(m_Identifier, m_CurrentDate, m_Context)).Result;

			if (scheduleResult.Success) {
				ScheduleRecord[] result = scheduleResult.Value;
				
				if (result.Length == 0) {
					Current = TextResult.Info(m_Resources.GetString(m_Context.Culture, "ScheduleModule_NoRecordsForDay"));
				} else {
					var cells = new IReadOnlyList<string>[Math.Max(result.Length, 1) + 1];
					cells[0] = result[0].PresentRowHeadings(m_Resources, m_Context.Culture);

					for (int i = 0; i < result.Length; i++) {
						var record = result[i];
						cells[i + 1] = record.PresentRow(m_Resources, m_Context.Culture);
					}

					Current = new TableResult(m_Identifier.DisplayText + ": " + DateTimeUtil.GetRelativeDateReference(m_CurrentDate, m_Context.Culture), cells);
				}
				return true;
			} else {
				Current = scheduleResult.ErrorResult;
				return m_PreInitial;
			}
		}
	}
}
