using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public class ArrayParser<T> : RoosterTypeParser<T[]>, IExternalResultStringParser {
		private readonly RoosterTypeParser<T> m_IndivReader;

		public override string TypeDisplayName { get; }
		public Component ErrorReasonComponent {
			get {
				if (m_IndivReader is IExternalResultStringParser ersp) {
					return ersp.ErrorReasonComponent;
				} else {
					return Program.Instance.Components.GetComponentFromAssembly(m_IndivReader.GetType().Assembly);
				}
			}
		}


		public ArrayParser(RoosterTypeParser<T> indivReader, string? typeDisplayName = null) {
			m_IndivReader = indivReader;
			TypeDisplayName = typeDisplayName ?? (m_IndivReader.TypeDisplayName + "[]");
		}

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
