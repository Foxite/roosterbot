using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class RoomScheduleModule : ScheduleModuleBase {
		public RoomScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "RSM") { }

		[Command("lokaalnu", RunMode = RunMode.Async)]
		private async Task RoomCurrentCommand(string room) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "Room", room);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = "Het ziet ernaar uit dat daar nu niets is.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = $"In {room} is nu {record.Activity}";

					if (!string.IsNullOrEmpty(record.StaffMember)) {
						response += $" van {GetTeacherNameFromAbbr(record.StaffMember)}";
					}
					if (!string.IsNullOrEmpty(record.StudentSets)) {
						response += $" met de klas {record.StudentSets}";
					}
					response += $".\n{GetTimeSpanResponse(record)}";

					await ReplyAsync(response);
				}
			}
		}

		[Command("lokaalhierna", RunMode = RunMode.Async)]
		private async Task RoomNextCommand(string room) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "Room", room);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError("GetRecord(RS1)==null");
				} else {
					string response = $"In {room} is {GetTomorrowOrNext(record)} {record.Activity}";

					if (!string.IsNullOrEmpty(record.StaffMember)) {
						response += $" van {GetTeacherNameFromAbbr(record.StaffMember)}";
					}
					if (!string.IsNullOrEmpty(record.StudentSets)) {
						response += $" met de klas {record.StudentSets}";
					}
					response += $".\n{GetTimeSpanResponse(record)}";
					await ReplyAsync(response);
				}
			}
		}
	}
}
