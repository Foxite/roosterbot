using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class IdentifierInfoReaderBase<T> : TypeReader where T : IdentifierInfo {
		public async override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			T result = await services.GetService<IdentifierValidationService>().ValidateAsync<T>(context, input);
			if (result is null) {
				return TypeReaderResult.FromError(CommandError.ParseFailed, Resources.IdentifierInfoReaderBase_ErrorMessage);
			} else {
				return TypeReaderResult.FromSuccess(result);
			}
		}
	}
}
