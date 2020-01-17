using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	public class MultiParser<T> : RoosterTypeParser<T>, IExternalResultStringParser {
		private readonly List<IRoosterTypeParser> m_Readers;
		private readonly string m_ErrorMessage;
		public override string TypeDisplayName { get; }

		public Component ErrorReasonComponent { get; }

		/// <summary>
		/// Allows multiple type parsers to be used for a single type.
		/// </summary>
		/// <param name="errorMessage">Error reason to be returned if all parsers return an unsuccessful result with InputValid == false.. If any parser returns with InputValid == true, then its result will be returned.</param>
		/// <param name="component">The component to be used when resolving the error message.</param>
		public MultiParser(Component resourceComponent, string errorMessage, string typeDisplayName) {
			m_Readers = new List<IRoosterTypeParser>();
			TypeDisplayName = typeDisplayName;
			ErrorReasonComponent = resourceComponent;
			m_ErrorMessage = errorMessage;
		}

		public void AddReader<TParser>(RoosterTypeParser<TParser> reader) where TParser : T {
			m_Readers.Add(reader);
		}

		public async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			ResourceService resources = context.ServiceProvider.GetRequiredService<ResourceService>();
			foreach (IRoosterTypeParser reader in m_Readers) {
				IRoosterTypeParserResult result = await reader.ParseAsync(parameter, value, context);
				if (result.IsSuccessful) {
					return Successful((T) result.Value!); // should never throw -- see AddReader, there's no way to add an invalid parser
				} else if (result.InputValid) {
					Component resourceComponent;
					if (reader is IExternalResultStringParser ersp) {
						resourceComponent = ersp.ErrorReasonComponent;
					} else {
						resourceComponent = Program.Instance.Components.GetComponentFromAssembly(reader.GetType().Assembly);
					}
					return RoosterTypeParserResult<T>.Unsuccessful(true, resources.ResolveString(context.Culture, resourceComponent, result.Reason));
				}
			}
			return RoosterTypeParserResult<T>.Unsuccessful(false, resources.ResolveString(context.Culture, ErrorReasonComponent, m_ErrorMessage));
		}
	}
}
