using System;
using System.Collections.Generic;

namespace RoosterBot {
	/// <summary>
	/// A TableResult consists of a two-dimensional grid of strings that are formatted for easy reading, optionally preceded by some text.
	/// </summary>
	public class TableResult : RoosterCommandResult {
		public string Caption { get; }
		public IReadOnlyList<IReadOnlyList<string>> Cells => m_WritableCells ?? m_ReadonlyCells;
		public int? MaxColumnWidth { get; }

		private string[][]? m_WritableCells;
		private IReadOnlyList<IReadOnlyList<string>> m_ReadonlyCells;

		public TableResult(string caption, int maxColumnWidth, int rows, int columns) {
			Caption = caption;
			MaxColumnWidth = maxColumnWidth;

			m_ReadonlyCells = null!;
			
			m_WritableCells = new string[rows][];
			for (int row = 0; row < rows; row++) {
				m_WritableCells[row] = new string[columns];
				for (int col = 0; col < columns; col++) {
					m_WritableCells[row][col] = "";
				}
			}
		}

		public TableResult(string caption, IReadOnlyList<IReadOnlyList<string>> cells, int? maxColumnWidth = null) {
			Caption = caption;
			m_ReadonlyCells = cells;
			MaxColumnWidth = maxColumnWidth;
		}

		public void SetCell(int row, int column, string cell) {
			if (m_WritableCells != null) {
				m_WritableCells[row][column] = cell;
			} else {
				throw new InvalidOperationException("Writing to this TableResult is not allowed.");
			}
		}

		public string GetCell(int row, int column) {
			return (m_WritableCells ?? m_ReadonlyCells)[row][column];
		}

		public void MakeReadOnly() {
			if (m_WritableCells == null) {
				throw new InvalidOperationException("Writing to this TableResult is already disabled.");
			} else {
				m_ReadonlyCells = m_WritableCells;
				m_WritableCells = null;
			}
		}

		public override string ToString() => Caption + "\n" + StringUtil.FormatTextTable(Cells, MaxColumnWidth);
	}
}
