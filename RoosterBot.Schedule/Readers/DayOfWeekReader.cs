using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	public class DayOfWeekReader : TypeReader {
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			try {
				return Task.FromResult(TypeReaderResult.FromSuccess(ScheduleUtil.GetDayOfWeekFromString(input)));
			} catch (ArgumentException) {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, Resources.DayOfWeekReader_CheckFailed));
			}
		}
	}
}
