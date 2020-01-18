using System.Collections.Generic;

namespace RoosterBot {
	/// <summary>
	/// An <see cref="IEnumerator{T}"/> that can move backwards as well as forwards.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IBidirectionalEnumerator<T> : IEnumerator<T> {
		/// <summary>
		/// Moves the enumerator to the previous element of the collection.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the enumerator was successfully moved to the previous element; <see langword="false"/> if the enumerator has passed the beginning of the enumeration.
		/// </returns>
		/// <exception cref="System.InvalidOperationException">
		/// The collection was modified after the enumerator was created.
		/// </exception>
		/// <remarks>
		/// It may move the enumerator past its initial value. Use <see cref="System.Collections.IEnumerator.Reset"/> to set the enumerator to its initial value.
		/// </remarks>
		bool MovePrevious();
	}
}
