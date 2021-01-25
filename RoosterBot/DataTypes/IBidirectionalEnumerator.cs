using System;
using System.Collections;
using System.Collections.Generic;

namespace RoosterBot {
	/// <summary>
	/// An <see cref="IEnumerator{T}"/> that can move backwards as well as forwards.
	/// </summary>
	public interface IBidirectionalEnumerator<out T> : IEnumerator<T> {
		/// <summary>
		/// Moves the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the enumerator was successfully moved to the previous element; <see langword="false"/> if the enumerator has passed the beginning of the enumeration.
		/// </returns>
		/// <exception cref="InvalidOperationException">
		/// The collection was modified after the enumerator was created.
		/// </exception>
		/// <remarks>
		/// It may move the enumerator past its initial value. Use <see cref="System.Collections.IEnumerator.Reset"/> to set the enumerator to its initial value.
		/// </remarks>
		bool MovePrevious();
	}

	/// <summary>
	/// A <see cref="IBidirectionalEnumerator{T}"/> over an <see cref="IReadOnlyList{T}"/>.
	/// </summary>
	public sealed class BidirectionalListEnumerator<T> : IBidirectionalEnumerator<T> {
		private readonly IReadOnlyList<T> m_List;
		private readonly int m_InitialPosition;
		private int m_Position;
		private bool m_Initialized = false;

		///
		public BidirectionalListEnumerator(IReadOnlyList<T> list, int position = 0) {
			m_List = list;
			m_Position = position;
			m_InitialPosition = position;
		}

		/// <inheritdoc/>
		public T Current => m_Initialized ? m_List[m_Position] : throw new InvalidOperationException($"Enumerator is not initialized. Call {nameof(MoveNext)} or {nameof(MovePrevious)} first.");
		object? IEnumerator.Current => Current;

		/// <inheritdoc/>
		public bool MoveNext() => Move(1);
		
		/// <inheritdoc/>
		public bool MovePrevious() => Move(-1);

		private bool Move(int direction) {
			if (m_Initialized) {
				m_Position += direction;
			} else {
				m_Position = m_InitialPosition;
				m_Initialized = true;
			}
			return m_Position >= 0 && m_Position < m_List.Count;
		}
		
		/// <inheritdoc/>
		public void Reset() {
			m_Initialized = false;
		}
		
		/// <inheritdoc/>
		public void Dispose() { }
	}
}
