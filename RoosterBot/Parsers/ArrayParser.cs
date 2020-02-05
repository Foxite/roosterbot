using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	/// <summary>
	/// Parse an array of objects according to another <see cref="RoosterTypeParser{T}"/>.
	/// </summary>
	public class ArrayParser<T> : RoosterTypeParser<T[]>, IExternalResultStringParser {
		private readonly RoosterTypeParser<T> m_IndivReader;

		/// <inheritdoc/>
		public override string TypeDisplayName { get; }

		/// <summary>
		/// The Component to be used when resolving the <see cref="TypeParserResult{T}.Reason"/>.
		/// </summary>
		public Component? ErrorReasonComponent {
			get {
				if (m_IndivReader is IExternalResultStringParser ersp) {
					return ersp.ErrorReasonComponent;
				} else {
					return Program.Instance.Components.GetComponentFromAssembly(m_IndivReader.GetType().Assembly);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="indivParser">The <see cref="RoosterTypeParser{T}"/> used when parsing individual items.</param>
		/// <param name="typeDisplayName">The <see cref="TypeDisplayName"/>.</param>
		public ArrayParser(RoosterTypeParser<T> indivParser, string? typeDisplayName = null) {
			m_IndivReader = indivParser;
			TypeDisplayName = typeDisplayName ?? (m_IndivReader.TypeDisplayName + "[]");
		}

		/// <inheritdoc/>
		public async override ValueTask<RoosterTypeParserResult<T[]>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			string[] inputs = input.Split(',');
			var results = new T[inputs.Length];
			for (int i = 0; i < inputs.Length; i++) {
				var indivResult = await m_IndivReader.ParseAsync(parameter, inputs[i].Trim(), context);
				if (indivResult.IsSuccessful) {
					results[i] = indivResult.Value;
				} else {
					return Unsuccessful(indivResult.InputValid, context, indivResult.Reason);
				}
			}
			return Successful(results);
		}
	}
}
