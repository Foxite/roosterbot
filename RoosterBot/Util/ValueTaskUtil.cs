using System.Threading.Tasks;

namespace RoosterBot {
	public static class ValueTaskUtil {
		public static ValueTask<T> FromResult<T>(T result) => new ValueTask<T>(result);
		public static ValueTask CompletedTask() => default;
	}
}
