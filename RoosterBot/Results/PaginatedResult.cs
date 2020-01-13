using System;
using System.Collections;

namespace RoosterBot {
	public sealed class PaginatedResult : RoosterCommandResult, IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly IBidirectionalEnumerator<RoosterCommandResult> m_Enumerator;

		public RoosterCommandResult Current => m_Enumerator.Current;

		object? IEnumerator.Current => m_Enumerator.Current;

		public PaginatedResult(IBidirectionalEnumerator<RoosterCommandResult> pageEnumerator) {
			m_Enumerator = pageEnumerator;
		}

		public void Dispose() => m_Enumerator.Dispose();
		public bool MoveNext() => m_Enumerator.MoveNext();
		public bool MovePrevious() => m_Enumerator.MovePrevious();
		public void Reset() => m_Enumerator.Reset();

		public override string ToString(RoosterCommandContext rcc) => Current.ToString(rcc);
	}

	public sealed class PaginatedTableEnumerator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly string m_Caption;
		private readonly string[] m_Header;
		private readonly string[][] m_Cells;
		private readonly int m_RowsPerPage;
		private readonly int? m_MaxColumnWidth;
		private int m_Position;

		public RoosterCommandResult Current => GetPage();

		object? IEnumerator.Current => Current;

		public PaginatedTableEnumerator(string caption, string[] header, string[][] cells, int rowsPerPage = 10, int? maxColumnWidth = null) {
			m_Caption = caption;
			m_Header = header;
			m_Cells = cells;
			m_RowsPerPage = rowsPerPage;
			m_MaxColumnWidth = maxColumnWidth;
			m_Position = -rowsPerPage;
		}

		private TableResult GetPage() {
			var ret = new string[Math.Min(m_RowsPerPage, m_Cells.Length - m_Position - m_RowsPerPage) + 1][];

			ret[0] = m_Header;
			for (int sourcePos = m_Position, targetPos = 1; sourcePos < (m_Cells.Length - m_Position - 1) && targetPos < ret.Length; sourcePos++, targetPos++) {
				ret[targetPos] = m_Cells[sourcePos];
			}
			return new TableResult(m_Caption, ret, m_MaxColumnWidth);
		}

		public bool MoveNext() {
			m_Position += m_RowsPerPage;
			return m_Position < m_Cells.Length - m_RowsPerPage;
		}

		public bool MovePrevious() {
			m_Position -= m_RowsPerPage;
			return m_Position > 0;
		}

		public void Reset() {
			m_Position = -m_RowsPerPage;
		}

		public void Dispose() { }
	}
}
