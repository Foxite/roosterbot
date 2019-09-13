using System;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace RoosterBot.Schedule {
	public class DayOfWeekReader : RoosterTypeReaderBase {
		protected override Task<TypeReaderResult> ReadAsync(RoosterCommandContext context, string input, IServiceProvider services) {
			try {
				return Task.FromResult(TypeReaderResult.FromSuccess(ScheduleUtil.GetDayOfWeekFromInput(context, services.GetService<GuildCultureService>(), input)));
			} catch (ArgumentException) {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, Resources.DayOfWeekReader_CheckFailed));
			}
		}
	}
}
