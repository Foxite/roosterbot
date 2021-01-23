using System;
using System.Collections.Generic;

namespace RoosterBot {
	/// <summary>
	/// A TableResult consists of a two-dimensional grid of strings that are formatted for easy reading, optionally preceded by some text.
	/// </summary>
	public class TableResult : RoosterCommandResult {
		/// <summary>
		/// The text to prefix to this TableResult.
		/// </summary>
		public string Caption { get; }

		/// <summary>
		/// The cells of the table.
		/// </summary>
		public IReadOnlyList<IReadOnlyList<string>> Cells => m_WritableCells ?? m_ReadonlyCells;

		/// <summary>
		/// The optional maximum width for each column of the table. If null, there is no maximum.
		/// </summary>
		public int? MaxColumnWidth { get; }

		private string[][]? m_WritableCells;
		private IReadOnlyList<IReadOnlyList<string>> m_ReadonlyCells;

		/// <summary>
		/// Construct a new writable, empty TableResult.
		/// </summary>
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

		/// <summary>
		/// Construct a new read-only TableResult with pre-filled cells.
		/// </summary>
		public TableResult(string caption, IReadOnlyList<IReadOnlyList<string>> cells, int? maxColumnWidth = null) {
			Caption = caption;
			m_ReadonlyCells = cells;
			MaxColumnWidth = maxColumnWidth;
		}

		/// <summary>
		/// Set the value of a cell in the table. <b>Can only be used if you used the empty constructor.</b>
		/// </summary>
		/// <seealso cref="TableResult(string, int, int, int)"/>
		/// <exception cref="InvalidOperationException">If writing has been disabled.</exception>
		public void SetCell(int row, int column, string cell) {
			if (m_WritableCells != null) {
				m_WritableCells[row][column] = cell;
			} else {
				throw new InvalidOperationException("Writing to this TableResult is not allowed.");
			}
		}

		/// <summary>
		/// Get the vaue of a cell in the table.
		/// </summary>
		/// <param name="row"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public string GetCell(int row, int column) {
			return (m_WritableCells ?? m_ReadonlyCells)[row][column];
		}

		/// <summary>
		/// Disable writing to this table. <b>Can only be used if you used the empty constructor.</b>
		/// </summary>
		/// <exception cref="InvalidOperationException">If writing has already been disabled.</exception>
		/// <seealso cref="TableResult(string, int, int, int)"/>
		public void MakeReadOnly() {
			if (m_WritableCells == null) {
				throw new InvalidOperationException("Writing to this TableResult is already disabled.");
			} else {
				m_ReadonlyCells = m_WritableCells;
				m_WritableCells = null;
			}
		}

		/// <inheritdoc/>
		//public override string ToString(RoosterCommandContext r) => Caption + "\n```" + Foxite.Common.StringUtil.FormatTextTable(Cells, MaxColumnWidth) + "```";
	}
}
