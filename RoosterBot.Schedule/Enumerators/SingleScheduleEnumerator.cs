using System.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public sealed class SingleScheduleEnumerator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly ScheduleService m_ScheduleService;
		private readonly RoosterCommandContext m_Context;
		private readonly ScheduleRecord m_Initial;
		private readonly IdentifierInfo m_Identifier;

		private ScheduleRecord m_CurrentRecord;

		object? IEnumerator.Current => Current;
		public RoosterCommandResult Current { get; private set; } = null!;
		
		public SingleScheduleEnumerator(RoosterCommandContext context, ScheduleRecord initial, IdentifierInfo identifier) {
			m_ScheduleService = context.ServiceProvider.GetRequiredService<ScheduleService>();
			m_Context = context;
			m_Initial = initial;
			m_Identifier = identifier;
			m_CurrentRecord = null!;
		}

		public bool MoveNext() {
			if (m_CurrentRecord == null) {
				m_CurrentRecord = m_Initial;
			}
			ReturnValue<ScheduleRecord> result =
				ScheduleUtil.HandleScheduleProviderErrorAsync(m_Context, () => m_ScheduleService.GetRecordAfterDateTime(m_Identifier, m_CurrentRecord.End, m_Context)).Result;

			if (result.Success) {
				m_CurrentRecord = result.Value;
				Current = new AspectListResult(m_Identifier.DisplayText, m_CurrentRecord.Present(m_Context), includeAspectNames: false);
				return true;
			} else {
				Current = result.ErrorResult;
				return m_CurrentRecord == m_Initial;
			}
		}

		public bool MovePrevious() {
			if (m_CurrentRecord == null) {
				m_CurrentRecord = m_Initial;
			}
			
			ReturnValue<ScheduleRecord> result =
				ScheduleUtil.HandleScheduleProviderErrorAsync(m_Context, () => m_ScheduleService.GetRecordBeforeDateTime(m_Identifier, m_CurrentRecord.Start, m_Context)).Result;

			if (result.Success) {
				m_CurrentRecord = result.Value;
				Current = new AspectListResult(m_Identifier.DisplayText, m_CurrentRecord.Present(m_Context), includeAspectNames: false);
				return true;
			} else {
				Current = result.ErrorResult;
				return false;
			}
		}

		public void Reset() => m_CurrentRecord = m_Initial;
		public void Dispose() { }
	}
}
