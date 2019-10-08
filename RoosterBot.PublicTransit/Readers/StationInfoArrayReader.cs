using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.PublicTransit {
	// TODO generalize to work with any TypeReader, move to RoosterBot
	public class StationInfoArrayReader : RoosterTypeReaderBase {
		private StationInfoReader m_IndivReader;

		public StationInfoArrayReader(StationInfoReader indivReader) {
			m_IndivReader = indivReader;
		}

		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			string[] inputs = input.Split(',');
			StationInfo[] results = new StationInfo[inputs.Length];
			for (int i = 0; i < inputs.Length; i++) {
				TypeReaderResult indivResult = await m_IndivReader.ReadAsync(context, inputs[i].Trim(), services);
				if (indivResult.IsSuccess) {
					results[i] = (StationInfo) indivResult.BestMatch; // Unsafe cast but if this ever goes wrong, then it sure as hell isn't this class' fault.
				} else {
					return indivResult;
				}
			}
			return TypeReaderResult.FromSuccess(results);
		}
	}
}
