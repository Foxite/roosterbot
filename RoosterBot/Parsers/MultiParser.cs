using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// A <see cref="RoosterTypeParser{T}"/> that consults multiple other <see cref="RoosterTypeParser{T}"/>s to get the result.
	/// </summary>
	public class MultiParser<T> : RoosterTypeParser<T>, IExternalResultStringParser {
		private readonly List<IRoosterTypeParser> m_Parsers;
		private readonly string m_ErrorMessage;

		/// <inheritdoc/>
		public override string TypeDisplayName { get; }

		/// <inheritdoc/>
		public Component? ErrorReasonComponent { get; }

		/// <summary>
		/// Allows multiple type parsers to be used for a single type.
		/// </summary>
		/// <param name="errorMessage">Error reason to be returned if all parsers return an unsuccessful result with InputValid == false.. If any parser returns with InputValid == true, then its result will be returned.</param>
		/// <param name="resourceComponent">The component to be used when resolving the error message.</param>
		/// <param name="typeDisplayName">The <see cref="TypeDisplayName"/>.</param>
		public MultiParser(Component resourceComponent, string errorMessage, string typeDisplayName) {
			m_Parsers = new List<IRoosterTypeParser>();
			TypeDisplayName = typeDisplayName;
			ErrorReasonComponent = resourceComponent;
			m_ErrorMessage = errorMessage;
		}

		/// <summary>
		/// Adds a parser to the list of parsers to consult.
		/// </summary>
		public void AddParser<TParser>(RoosterTypeParser<TParser> reader) where TParser : T {
			m_Parsers.Add(reader);
		}

		/// <inheritdoc/>
		public async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			foreach (IRoosterTypeParser parser in m_Parsers) {
				IRoosterTypeParserResult result = await parser.ParseAsync(parameter, value, context);
				if (result.IsSuccessful) {
					return Successful((T) result.Value!); // should never throw -- see AddReader, there's no way to add an invalid parser
				} else if (result.InputValid) {
					return RoosterTypeParserResult<T>.Unsuccessful(parser, true, result.Reason, result.ErrorReasonObjects);
				}
			}
			ResourceService resources = context.ServiceProvider.GetRequiredService<ResourceService>();
			return Unsuccessful(false, resources.ResolveString(context.Culture, ErrorReasonComponent, m_ErrorMessage));
		}
	}
}
