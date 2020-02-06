using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	/// <summary>
	/// Note to implementers:
	/// 
	/// To use a TypeParser deriving from this class you must have a resource with this key in all your locales:
	/// <code>IdentifierInfoReaderBase_ErrorMessage</code>
	/// This string will be used as the error reason key when the validator can't parse the input.
	/// </summary>
	// TODO fix problem described above
	public abstract class IdentifierInfoParserBase<T> : RoosterTypeParser<T> where T : IdentifierInfo {
		public async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			T? result = await context.ServiceProvider.GetRequiredService<IdentifierValidationService>().ValidateAsync<T>(context, input);
			if (result is null) {
				return Unsuccessful(false, context, "#IdentifierInfoReaderBase_ErrorMessage");
			} else {
				return Successful(result);
			}
		}
	}
}
