using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot {
	public class ArrayReader<T> : RoosterTypeReader<T[]> {
		private readonly RoosterTypeReader<T> m_IndivReader;

		public override string TypeDisplayName { get; }

		public ArrayReader(RoosterTypeReader<T> indivReader, string? typeDisplayName = null) {
			m_IndivReader = indivReader;
			TypeDisplayName = typeDisplayName ?? (m_IndivReader.TypeDisplayName + "[]");
		}

		protected async override ValueTask<TypeParserResult<T[]>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			string[] inputs = input.Split(',');
			T[] results = new T[inputs.Length];
			for (int i = 0; i < inputs.Length; i++) {
				TypeParserResult<T> indivResult = await m_IndivReader.ParseAsync(parameter, inputs[i].Trim(), context);
				if (indivResult.IsSuccessful) {
					results[i] = indivResult.Value;
				} else {
					return TypeParserResult<T[]>.Unsuccessful(indivResult.Reason);
				}
			}
			return TypeParserResult<T[]>.Successful(results);
		}
	}
}
