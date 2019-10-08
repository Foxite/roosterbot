using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot {
	[AttributeUsage(AttributeTargets.Parameter)]
	public sealed class CountAttribute : ParameterPreconditionAttribute {
		public int Min { get; }
		public int Max { get; }

		public CountAttribute(int min, int max) {
			Min = min;
			Max = max;
		}

		public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, ParameterInfo parameter, object value, IServiceProvider services) {
			if (!parameter.Type.IsArray) {
				throw new InvalidOperationException("CountAttribute can only be used on array parameters.");
			} else {
				int length = ((Array) value).Length;
				if (length >= Min && length <= Max) {
					return Task.FromResult(PreconditionResult.FromSuccess());
				} else {
					return Task.FromResult(PreconditionResult.FromError(services.GetService<ResourceService>().GetString(context, "Program_OnCommandExecuted_BadArgCount")));
				}
			}
		}
	}
}
