using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public abstract class IdentifierInfoParserBase<T> : RoosterTypeParser<T> where T : IdentifierInfo {
		public async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			T? result = await context.ServiceProvider.GetService<IdentifierValidationService>().ValidateAsync<T>(context, input);
			if (result is null) {
				return Unsuccessful(false, context, "#IdentifierInfoReaderBase_ErrorMessage");
			} else {
				return Successful(result);
			}
		}
	}
}
