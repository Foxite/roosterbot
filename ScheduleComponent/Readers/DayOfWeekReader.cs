using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace ScheduleComponent.Readers {
	public class DayOfWeekReader : TypeReader {
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			try {
				return Task.FromResult(TypeReaderResult.FromSuccess(ScheduleUtil.GetDayOfWeekFromString(input)));
			} catch (ArgumentException) {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Dat is geen weekdag."));
			}
		}
	}
}
