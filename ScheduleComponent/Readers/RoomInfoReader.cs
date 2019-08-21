using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Readers {
	public class RoomInfoReader : TypeReader {
		private static readonly Regex s_RoomRegex = new Regex("[aAbBwW][012][0-9]{2}");

		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			if (s_RoomRegex.IsMatch(input)) {
				return Task.FromResult(TypeReaderResult.FromSuccess(new RoomInfo() {
					Room = input.ToUpper()
				}));
			} else {
				return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, Resources.RoomInfoReader_CheckFailed));
			}
		}
	}
}
