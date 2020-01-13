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
		private readonly int m_PageCount;
		private int m_PageIndex;

		public RoosterCommandResult Current => GetPage();

		object? IEnumerator.Current => Current;

		public PaginatedTableEnumerator(string caption, string[] header, string[][] cells, int rowsPerPage = 10, int? maxColumnWidth = null) {
			m_Caption = caption;
			m_Header = header;
			m_Cells = cells;
			m_RowsPerPage = rowsPerPage;
			m_MaxColumnWidth = maxColumnWidth;
			m_PageIndex = -1;
			m_PageCount = m_Cells.Length / m_RowsPerPage + Math.Sign(m_Cells.Length % rowsPerPage); // Integer division, rounding up
		}

		private TableResult GetPage() {
			int pageSize = Math.Min(m_Cells.Length - m_PageIndex * m_RowsPerPage, m_RowsPerPage);

			var ret = new string[pageSize + 1][];
			ret[0] = m_Header;
			m_Cells.AsSpan(m_PageIndex * m_RowsPerPage, pageSize).CopyTo(ret.AsSpan(1));
			return new TableResult(m_Caption, ret, m_MaxColumnWidth);
		}

		public bool MoveNext() {
			if (m_PageIndex < m_PageCount) {
				m_PageIndex++;
				return true;
			} else {
				m_PageIndex = m_PageCount - 1;
				return false;
			}
		}

		public bool MovePrevious() {
			if (m_PageIndex > 0) {
				m_PageIndex--;
				return true;
			} else {
				m_PageIndex = 0;
				return false;
			}
		}

		public void Reset() {
			m_PageIndex = -1;
		}

		public void Dispose() { }
	}
}
