using System.Collections.Concurrent;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public class IdentifierValidationService {
		private ConcurrentBag<IdentifierValidator> m_RegisteredValidators;

		public IdentifierValidationService() {
			m_RegisteredValidators = new ConcurrentBag<IdentifierValidator>();
		}

		public void RegisterValidator(IdentifierValidator validator) {
			m_RegisteredValidators.Add(validator);
		}

		public T Validate<T>(ICommandContext context, string input) where T : IdentifierInfo {
			foreach (IdentifierValidator validator in m_RegisteredValidators) {
				IdentifierInfo result = validator(context, input);
				if (result != null && result is T) {
					return result as T;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// Given an ICommandContext and an input string, this will return a valid IdentifierInfo instance. If the input is invalid, it will return null.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="input"></param>
	/// <returns></returns>
	public delegate IdentifierInfo IdentifierValidator(ICommandContext context, string input);
}
