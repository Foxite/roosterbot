using System.Collections.Generic;
using System.Linq;

namespace RoosterBot {
	/// <summary>
	/// Represents multiple <see cref="RoosterCommandResult"/>s that are displayed together.
	/// </summary>
	public class CompoundResult : RoosterCommandResult {
		public IEnumerable<RoosterCommandResult> IndividualResults => m_IndividualResults;

		/// <summary>
		/// The string added between the individual results.
		/// </summary>
		public string Separator { get; }

		private readonly List<RoosterCommandResult> m_IndividualResults;

		public CompoundResult(string separator, params RoosterCommandResult[] results) {
			m_IndividualResults = new List<RoosterCommandResult>(results);
			Separator = separator;
		}

		public void AddResult(RoosterCommandResult result) {
			m_IndividualResults.Add(result);
		}

		public override string ToString(RoosterCommandContext r) {
			return string.Join(Separator, IndividualResults.Select(result => result.ToString()));
		}
	}
}
