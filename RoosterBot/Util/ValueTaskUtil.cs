using System.Threading.Tasks;

namespace RoosterBot {
	/// <summary>
	/// A static class containing some helper functions for dealing with <see cref="ValueTask"/>s.
	/// </summary>
	public static class ValueTaskUtil {
		/// <summary>
		/// Shortcut for:
		/// <code>
		/// new <see cref="ValueTask"/>&lt;<typeparamref name="T"/>&gt;(<paramref name="result"/>)
		/// </code>
		/// </summary>
		/// <typeparam name="T">The type of result. This can be inferred, making it shorter than the standard approach.</typeparam>
		/// <param name="result">The result of the completed task.</param>
		public static ValueTask<T> FromResult<T>(T result) => new ValueTask<T>(result);
	}
}
