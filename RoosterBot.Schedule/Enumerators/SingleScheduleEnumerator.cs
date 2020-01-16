﻿using System;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public sealed class SingleScheduleEnumerator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly ResourceService m_Resources;
		private readonly ScheduleService m_ScheduleService;
		private readonly RoosterCommandContext m_Context;
		private readonly ScheduleRecord m_Initial;
		private readonly IdentifierInfo m_Identifier;

		private ScheduleRecord m_CurrentRecord;

		public RoosterCommandResult Current => new AspectListResult(m_Identifier.DisplayText, m_CurrentRecord.Present(m_Resources, m_Context.Culture), false);

		object? IEnumerator.Current => throw new NotImplementedException();
		
		public SingleScheduleEnumerator(RoosterCommandContext context, ScheduleRecord initial, IdentifierInfo identifier) {
			m_Resources = context.ServiceProvider.GetRequiredService<ResourceService>();
			m_ScheduleService = context.ServiceProvider.GetRequiredService<ScheduleService>();
			m_Context = context;
			m_Initial = initial;
			m_Identifier = identifier;
			m_CurrentRecord = null!;
		}

		public bool MoveNext() {
			if (m_CurrentRecord == null) {
				m_CurrentRecord = m_Initial;
				return true;
			} else {
				try {
					m_CurrentRecord = m_ScheduleService.GetRecordAfterDateTime(m_Identifier, m_CurrentRecord.End, m_Context).Result;
					return true;
				} catch {
					return false;
				}
			}
		}

		public bool MovePrevious() {
			ScheduleRecord record;
			if (m_CurrentRecord == null) {
				record = m_CurrentRecord = m_Initial;
			} else {
				record = m_CurrentRecord;
			}
			try {
				m_CurrentRecord = m_ScheduleService.GetRecordBeforeDateTime(m_Identifier, record.Start, m_Context).Result;
				return true;
			} catch {
				return false;
			}
		}

		public void Reset() => m_CurrentRecord = m_Initial;
		public void Dispose() { }
	}
}