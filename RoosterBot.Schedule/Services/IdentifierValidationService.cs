using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public class IdentifierValidationService {
		private readonly ConcurrentBag<IdentifierValidatorAsync> m_RegisteredValidators;

		public IdentifierValidationService() {
			m_RegisteredValidators = new ConcurrentBag<IdentifierValidatorAsync>();
		}

		public void RegisterValidator(IdentifierValidatorAsync validator) {
			m_RegisteredValidators.Add(validator);
		}

		public async Task<T?> ValidateAsync<T>(RoosterCommandContext context, string input) where T : IdentifierInfo {
			foreach (IdentifierValidatorAsync validator in m_RegisteredValidators) {
				IdentifierInfo? result = await validator(context, input);
				if (result is T resultT) {
					return resultT;
				}
			}
			return null;
		}
	}

	/// <summary>
	/// Given an ICommandContext and an input string, this will return a valid IdentifierInfo instance. If the input is invalid, it will return null.
	/// </summary>
	public delegate Task<IdentifierInfo?> IdentifierValidatorAsync(RoosterCommandContext context, string input);
}
