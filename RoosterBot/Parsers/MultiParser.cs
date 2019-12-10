using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public class MultiParser<T> : RoosterTypeParser<T> {
		private readonly List<IRoosterTypeParser> m_Readers;
		private readonly string m_ErrorMessage;

		public override string TypeDisplayName { get; }

		/// <summary>
		/// Allows multiple type parsers to be used for a single type.
		/// </summary>
		/// <param name="errorMessage">Error reason to be returned if all parsers return an unsuccessful result with InputValid == false.. If any parser returns with InputValid == true, then its result will be returned.</param>
		/// <param name="component">The component to be used when resolving the error message.</param>
		public MultiParser(string errorMessage, string typeDisplayName) {
			m_Readers = new List<IRoosterTypeParser>();
			TypeDisplayName = typeDisplayName;
			m_ErrorMessage = errorMessage;
		}

		public void AddReader<TParser>(RoosterTypeParser<TParser> reader) where TParser : T {
			m_Readers.Add(reader);
		}

		protected async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			foreach (IRoosterTypeParser reader in m_Readers) {
				var result = (IRoosterTypeParserResult) await ((dynamic) reader).ParseAsync(parameter, value, context);
				if (result.IsSuccessful) {
					return Successful((T) result.Value!); // should never throw -- see AddReader, there's no way to add an invalid parser
				} else if (result.InputValid) {
					return Unsuccessful(true, context, result.Reason);
				}
			}
			return Unsuccessful(false, context, m_ErrorMessage);
		}
	}
}
