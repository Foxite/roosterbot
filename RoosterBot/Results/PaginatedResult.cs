using System;
using System.Collections;

namespace RoosterBot {
	/// <summary>
	/// A result that consists of multiple pages that can be viewed independently.
	/// </summary>
	// TODO (refactor) this should use IBidirectionalAsyncEnumerator
	public sealed class PaginatedResult : RoosterCommandResult, IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly IBidirectionalEnumerator<RoosterCommandResult> m_Enumerator;

		/// <summary>
		/// A string to display with every page of the result.
		/// </summary>
		public string? Caption { get; }

		/// <summary>
		/// The amount of pages, if known. The number of pages that can be enumerated can be unlimited. In such a case, or if the amount of pages is not known, this is null.
		/// </summary>
		public int? PageCount { get; }

		/// <inheritdoc/>
		public RoosterCommandResult Current => m_Enumerator.Current;

		object? IEnumerator.Current => m_Enumerator.Current;

		///
		public PaginatedResult(IBidirectionalEnumerator<RoosterCommandResult> pageEnumerator, string? caption, int? pageCount = null) {
			m_Enumerator = pageEnumerator;
			Caption = caption;
			PageCount = pageCount;
		}

		/// <inheritdoc/>
		public void Dispose() => m_Enumerator.Dispose();
		/// <inheritdoc/>
		public bool MoveNext() => m_Enumerator.MoveNext();
		/// <inheritdoc/>
		public bool MovePrevious() => m_Enumerator.MovePrevious();
		/// <inheritdoc/>
		public void Reset() => m_Enumerator.Reset();
	}

	/// <summary>
	/// An enumerator for <see cref="PaginatedResult"/> that enumerates pages from a <see cref="TableResult"/>
	/// </summary>
	public sealed class PaginatedTableEnumerator : IBidirectionalEnumerator<RoosterCommandResult> {
		private readonly string m_Caption;
		private readonly string[] m_Header;
		private readonly string[][] m_Cells;
		private readonly int m_RowsPerPage;
		private readonly int? m_MaxColumnWidth;
		private readonly int m_PageCount;
		private int m_PageIndex;
		
		/// <inheritdoc/>
		public RoosterCommandResult Current {
			get {
				int pageSize = Math.Min(m_Cells.Length - m_PageIndex * m_RowsPerPage, m_RowsPerPage);

				var ret = new string[pageSize + 1][];
				ret[0] = m_Header;
				m_Cells.AsSpan(m_PageIndex * m_RowsPerPage, pageSize).CopyTo(ret.AsSpan(1));
				return new TableResult(m_Caption, ret, m_MaxColumnWidth);
			}
		}

		object? IEnumerator.Current => Current;

		///
		public PaginatedTableEnumerator(string caption, string[] header, string[][] cells, int rowsPerPage = 10, int? maxColumnWidth = null) {
			m_Caption = caption;
			m_Header = header;
			m_Cells = cells;
			m_RowsPerPage = rowsPerPage;
			m_MaxColumnWidth = maxColumnWidth;
			m_PageIndex = -1;
			m_PageCount = m_Cells.Length / m_RowsPerPage + Math.Sign(m_Cells.Length % rowsPerPage); // Integer division, rounding up
		}

		/// <inheritdoc/>
		public bool MoveNext() {
			if (m_PageIndex < m_PageCount) {
				m_PageIndex++;
				return true;
			} else {
				m_PageIndex = m_PageCount - 1;
				return false;
			}
		}

		/// <inheritdoc/>
		public bool MovePrevious() {
			if (m_PageIndex > 0) {
				m_PageIndex--;
				return true;
			} else {
				m_PageIndex = 0;
				return false;
			}
		}

		/// <inheritdoc/>
		public void Reset() {
			m_PageIndex = -1;
		}

		/// <inheritdoc/>
		public void Dispose() { }
	}
}
