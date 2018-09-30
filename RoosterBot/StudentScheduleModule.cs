using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class StudentScheduleModule : ScheduleModuleBase {
		public StudentScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "SSM") { }

		[Command("nu", RunMode = RunMode.Async)]
		public async Task StudentCurrentCommand(string clazz) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "StudentSets", clazz);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = "Je hebt nu ";
					if (record.Activity == "pauze") {
						response += $"pauze tot {record.End.ToShortTimeString()}.";
					}
					response += $"{record.Activity} van {GetTeacherNameFromAbbr(record.StaffMember)} in {record.Room}\n";
					response += GetTimeSpanResponse(record);
					await ReplyAsync(response);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async)]
		public async Task StudentNextCommand(string clazz) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "StudentSets", clazz);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError("GetRecord(SS1)==null)");
				} else {
					string response = $"Je hebt {GetNextTimeString(record)}";
					if (record.Activity == "pauze") {
						response += $" pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
					} else {
						response += $" {record.Activity}";

						if (!string.IsNullOrEmpty(record.StaffMember)) {
							response += $" van {GetTeacherNameFromAbbr(record.StaffMember)}";
						}
						if (!string.IsNullOrEmpty(record.Room)) {
							response += $" in {record.Room}";
						} else {
							response += ", maar ik weet niet waar";
						}
						response += $".\n{GetTimeSpanResponse(record)}";
					}
					await ReplyAsync(response);
				}
			}
		}
	}
}
