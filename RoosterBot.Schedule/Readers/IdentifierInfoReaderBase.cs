﻿using Discord.Commands;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace RoosterBot.Schedule {
	public abstract class IdentifierInfoReaderBase<T> : RoosterTypeParser where T : IdentifierInfo {
		public override Type Type => typeof(T);

		protected async override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			T? result = await services.GetService<IdentifierValidationService>().ValidateAsync<T>(context as RoosterCommandContext, input);
			if (result is null) {
				return TypeReaderResult.FromError(CommandError.ParseFailed, "#IdentifierInfoReaderBase_ErrorMessage");
			} else {
				return TypeReaderResult.FromSuccess(result);
			}
		}
	}
}
