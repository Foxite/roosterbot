using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public abstract class IdentifierInfoReaderBase<T> : RoosterTypeParser<T> where T : IdentifierInfo {
		protected async override ValueTask<TypeParserResult<T>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			T? result = await context.ServiceProvider.GetService<IdentifierValidationService>().ValidateAsync<T>(context, input);
			if (result is null) {
				return TypeParserResult<T>.Unsuccessful("#IdentifierInfoReaderBase_ErrorMessage");
			} else {
				return TypeParserResult<T>.Successful(result);
			}
		}
	}
}
