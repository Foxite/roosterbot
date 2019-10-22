using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class ArrayReader : TypeReader {
		private TypeReader m_IndivReader;

		public ArrayReader(TypeReader indivReader) {
			m_IndivReader = indivReader;
		}

		public async override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			string[] inputs = input.Split(',');
			object[] results = new object[inputs.Length];
			for (int i = 0; i < inputs.Length; i++) {
				TypeReaderResult indivResult = await m_IndivReader.ReadAsync(context, inputs[i].Trim(), services);
				if (indivResult.IsSuccess) {
					results[i] = indivResult.BestMatch; // Unsafe cast but if this ever goes wrong, then it sure as hell isn't this class' fault.
				} else {
					return indivResult;
				}
			}
			return TypeReaderResult.FromSuccess(results);
		}
	}
}
