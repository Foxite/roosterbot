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
}
