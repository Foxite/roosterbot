using System;
using System.Collections.Generic;

namespace RoosterBot {
	public partial class DependencyResult {
		private string? m_ErrorMessage;

		public bool OK => m_ErrorMessage == null;
		public string ErrorMessage => m_ErrorMessage ?? throw new InvalidOperationException("Can't get the error message of a successful operation");

		public static Builder Build(IEnumerable<ComponentBase> otherComponents) => new Builder(otherComponents);
	}
}
