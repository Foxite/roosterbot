﻿using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Qmmands;

namespace RoosterBot.Schedule {
	public abstract class IdentifierInfoParserBase<T> : RoosterTypeParser<T> where T : IdentifierInfo {
		protected virtual string ErrorMessage => "#IdentifierInfoReaderBase_ErrorMessage";

		public async override ValueTask<RoosterTypeParserResult<T>> ParseAsync(Parameter parameter, string input, RoosterCommandContext context) {
			T? result = await context.ServiceProvider.GetRequiredService<IdentifierValidationService>().ValidateAsync<T>(context, input);
			if (result is null) {
				return Unsuccessful(false, ErrorMessage);
			} else {
				return Successful(result);
			}
		}
	}
}
