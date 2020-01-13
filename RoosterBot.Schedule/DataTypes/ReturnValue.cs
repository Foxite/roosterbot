using System;

namespace RoosterBot.Schedule {
	public class ReturnValue<T> {
		private T m_Value = default!;
		private RoosterCommandResult m_ErrorResult = default!;

		public bool Success { get; private set; }
		public T Value => Success ? m_Value : throw new InvalidOperationException("Can't get the result of an unsuccessful operation");
		public RoosterCommandResult ErrorResult => Success ? throw new InvalidOperationException("Can't get the error result of a successful operation") : m_ErrorResult;

		public static ReturnValue<T> Unsuccessful(RoosterCommandResult errorResult) {
			return new ReturnValue<T>() {
				Success = false,
				m_Value = default!,
				m_ErrorResult = errorResult
			};
		}

		public static ReturnValue<T> Successful(T result) {
			return new ReturnValue<T>() {
				Success = true,
				m_Value = result,
				m_ErrorResult = default!
			};
		}
	}
}
