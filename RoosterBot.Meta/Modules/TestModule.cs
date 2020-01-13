using System.Collections;
using Qmmands;

namespace RoosterBot.Meta {
	public class TestModule : RoosterModule {
		[Command("test")]
		public CommandResult Test(int head) {
			return new PaginatedResult(new TestBDE(head));
		}

		private class TestBDE : IBidirectionalEnumerator<RoosterCommandResult> {
			private int m_Position;

			public RoosterCommandResult Current => new TextResult(null, m_Position.ToString());

			object? IEnumerator.Current => Current;

			public TestBDE(int position) {
				m_Position = position - 1;
			}

			public void Dispose() { }
			public bool MoveNext() {
				m_Position++;
				return true;
			}

			public bool MovePrevious() {
				m_Position--;
				return true;
			}

			public void Reset() {
				m_Position = 0;
			}
		}
	}
}
