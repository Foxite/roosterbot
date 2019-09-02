using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class IdentifierInfoReaderBase<T> : TypeReader where T : IdentifierInfo {
		// TODO We can probably move all the matching code out of the typereaders into IdentifierValidationService delegates
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			T result = services.GetService<IdentifierValidationService>().Validate<T>(context, input);
			if (result is null) {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "That does not match anything.")); // TODO localize
			} else {
				return Task.FromResult(TypeReaderResult.FromSuccess(result));
			}
		}
	}
}
