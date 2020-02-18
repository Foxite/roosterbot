using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RoosterBot.Schedule {
	public class IdentifierValidationService {
		private readonly ConcurrentBag<IdentifierValidator> m_RegisteredValidators;

		public IdentifierValidationService() {
			m_RegisteredValidators = new ConcurrentBag<IdentifierValidator>();
		}

		public void RegisterValidator(IdentifierValidator validator) {
			m_RegisteredValidators.Add(validator);
		}

		public async Task<T?> ValidateAsync<T>(RoosterCommandContext context, string input) where T : IdentifierInfo {
			foreach (IdentifierValidator validator in m_RegisteredValidators.Where(validator => validator.IsChannelAllowed(context.ChannelConfig.ChannelReference))) {
				IdentifierInfo? result = await validator.ValidateAsync(context, input);
				if (result is T resultT) {
					return resultT;
				}
			}
			return null;
		}
	}

	public abstract class IdentifierValidator : ChannelSpecificInfo {
		protected IdentifierValidator(IReadOnlyCollection<SnowflakeReference> allowedChannels) : base(allowedChannels) { }

		/// <summary>
		/// Given an ICommandContext and an input string, this will return a valid IdentifierInfo instance. If the input is invalid, it will return null.
		/// </summary>
		public abstract Task<IdentifierInfo?> ValidateAsync(RoosterCommandContext context, string input);
	}
}
