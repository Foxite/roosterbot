using System;
using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// Represents multiple <see cref="RoosterCommandResult"/>s that are displayed together.
	/// </summary>
	[Obsolete("Needing this is symptomatic of bad UI. Find a more semantic way to express your information than chaining results")]
	public class CompoundResult : RoosterCommandResult {
		/// <summary>
		/// The individual results of this CompoundResult.
		/// </summary>
		public IEnumerable<RoosterCommandResult> IndividualResults => m_IndividualResults;

		/// <summary>
		/// The string added between the individual results.
		/// </summary>
		public string Separator { get; }

		private readonly List<RoosterCommandResult> m_IndividualResults;

		///
		public CompoundResult(string separator, params RoosterCommandResult[] results) {
			m_IndividualResults = new List<RoosterCommandResult>(results);
			Separator = separator;
		}

		/// <summary>
		/// Add a result to the CompoundResult.
		/// </summary>
		/// <param name="result"></param>
		public void AddResult(RoosterCommandResult result) {
			m_IndividualResults.Add(result);
		}

		/*
		/// <inheritdoc/>
		public override string ToString(RoosterCommandContext rcc) {
			return string.Join(Separator, IndividualResults.Select(result => result.ToString(rcc)));
		}//*/
	}
}
