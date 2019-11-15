using System;

namespace RoosterBot {
	public class ReturnValue<T> {
		private readonly T m_Value;

		public bool Success { get; }
		public T Value => Success ? m_Value : throw new InvalidOperationException("Can't get the result of an unsuccessful operation");

		public ReturnValue() {
			Success = false;
			m_Value = default!;
		}

		public ReturnValue(T value) {
			Success = true;
			m_Value = value;
		}
	}
}
