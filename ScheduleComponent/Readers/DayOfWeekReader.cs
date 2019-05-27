using System;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Readers {
	public class DayOfWeekReader : TypeReader {
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			try {
				return Task.FromResult(TypeReaderResult.FromSuccess(Util.GetDayOfWeekFromString(input)));
			} catch (ArgumentException) {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Dat is geen weekdag."));
			}
		}
	}
}
