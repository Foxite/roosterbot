using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public class IdentifierValidationService {
		private ConcurrentBag<IdentifierValidatorAsync> m_RegisteredValidators;

		public IdentifierValidationService() {
			m_RegisteredValidators = new ConcurrentBag<IdentifierValidatorAsync>();
		}

		public void RegisterValidator(IdentifierValidatorAsync validator) {
			m_RegisteredValidators.Add(validator);
		}

		public async Task<T> ValidateAsync<T>(ICommandContext context, string input) where T : IdentifierInfo {
			var guild = context.Guild;
			if (guild is null) {
				// Get common guilds with the user, select the first one
				// What to do if there's multiple?
				IReadOnlyCollection<IGuild> commonGuilds = await Util.GetCommonGuildsAsync(context.Client, context.User);
				if (commonGuilds.Count == 1) {
					guild = commonGuilds.First();
				} else {
					return null;
				}
			}

			foreach (IdentifierValidatorAsync validator in m_RegisteredValidators) {
				IdentifierInfo result = await validator(context, input, guild);
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
	/// <param name="contextGuild">Do not use context.Guild, but use this instead. This will provide a guild in a DM channel.</param>
	public delegate Task<IdentifierInfo> IdentifierValidatorAsync(ICommandContext context, string input, IGuild contextGuild);
}
