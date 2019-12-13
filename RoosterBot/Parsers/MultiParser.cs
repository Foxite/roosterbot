using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot {
	public class MultiParser<T> : RoosterTypeParser<T> {
		private readonly List<IRoosterTypeParser> m_Readers;
		private readonly Component m_ResourceComponent;
		private readonly string m_ErrorMessage;
		public override string TypeDisplayName { get; }

		/// <summary>
		/// Allows multiple type parsers to be used for a single type.
		/// </summary>
		/// <param name="errorMessage">Error reason to be returned if all parsers return an unsuccessful result with InputValid == false.. If any parser returns with InputValid == true, then its result will be returned.</param>
		/// <param name="component">The component to be used when resolving the error message.</param>
		public MultiParser(Component resourceComponent, string errorMessage, string typeDisplayName) {
			m_Readers = new List<IRoosterTypeParser>();
			TypeDisplayName = typeDisplayName;
			m_ResourceComponent = resourceComponent;
			m_ErrorMessage = errorMessage;
		}

		public void AddReader<TParser>(RoosterTypeParser<TParser> reader) where TParser : T {
			m_Readers.Add(reader);
		}

		protected async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string value, RoosterCommandContext context) {
			ResourceService resources = context.ServiceProvider.GetService<ResourceService>();
			foreach (IRoosterTypeParser reader in m_Readers) {
				var result = (IRoosterTypeParserResult) await ((dynamic) reader).ParseAsync(parameter, value, context);
				if (result.IsSuccessful) {
					return Successful((T) result.Value!); // should never throw -- see AddReader, there's no way to add an invalid parser
				} else if (result.InputValid) {
					// TODO (fix) Combined MultiParsers/ArrayParsers; They need to be able to access each other's ResourceComponent
					// This should be possible for external parsers that outsource error messages as well, do it with an interface
					return RoosterTypeParserResult<T>.Unsuccessful(true, resources.ResolveString(context.Culture, Program.Instance.Components.GetComponentFromAssembly(result.GetType().Assembly), result.Reason));
				}
			}
			return RoosterTypeParserResult<T>.Unsuccessful(false, resources.ResolveString(context.Culture, m_ResourceComponent, m_ErrorMessage));
		}
	}
}
