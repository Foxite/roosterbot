using System;
using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public sealed class ScheduleResultPaginator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly ResourceService m_Resources;
		private readonly ScheduleService m_ScheduleService;
		private readonly RoosterCommandContext m_Context;
		private readonly ScheduleRecord m_Initial;
		private readonly IdentifierInfo m_Identifier;
		private readonly string m_Caption;

		private ScheduleRecord m_CurrentRecord;

		public RoosterCommandResult Current => new AspectListResult(m_Caption, m_CurrentRecord.Present(m_Resources, m_Context.Culture), false);

		object? IEnumerator.Current => throw new NotImplementedException();
		
		public ScheduleResultPaginator(RoosterCommandContext context, ScheduleRecord initial, IdentifierInfo identifier, string caption) {
			m_Resources = context.ServiceProvider.GetService<ResourceService>();
			m_ScheduleService = context.ServiceProvider.GetService<ScheduleService>();
			m_Context = context;
			m_Initial = initial;
			m_Identifier = identifier;
			m_Caption = caption;
			m_CurrentRecord = null!;
		}

		public bool MoveNext() {
			if (m_CurrentRecord == null) {
				m_CurrentRecord = m_Initial;
			} else {
				// TODO error handling
				m_CurrentRecord = m_ScheduleService.GetRecordAfterDateTime(m_Identifier, m_CurrentRecord.End, m_Context).Result;
			}
			return true;
		}

		public bool MovePrevious() {
			ScheduleRecord record;
			if (m_CurrentRecord == null) {
				record = m_CurrentRecord = m_Initial;
			} else {
				record = m_CurrentRecord;
			}
			m_CurrentRecord = m_ScheduleService.GetRecordBeforeDateTime(m_Identifier, record.Start, m_Context).Result;

			return true;
		}

		public void Reset() => m_CurrentRecord = m_Initial;
		public void Dispose() { }
	}
}
