using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class RoomScheduleModule : ScheduleModuleBase {
		public RoomScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "RSM") { }

		[Command("lokaalnu", RunMode = RunMode.Async), Summary("Wat er nu in een lokaal plaatsvindt")]
		private async Task RoomCurrentCommand(string lokaal) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "Room", lokaal);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = "Het ziet ernaar uit dat daar nu niets is.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = $"In {lokaal} is nu {record.Activity}";

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

		[Command("lokaalhierna", RunMode = RunMode.Async), Summary("Wat er hierna in een lokaal plaatsvindt")]
		private async Task RoomNextCommand(string lokaal) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "Room", lokaal);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError("GetRecord(RS1)==null");
				} else {
					string response = $"In {lokaal} is {GetNextTimeString(record)} {record.Activity}";

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

		[Command("lokaaldag", RunMode = RunMode.Async), Summary("Welke les er als eerste in een lokaal op een dag")]
		public async Task RoomWeekdayCommand(string lokaal, string weekdag) {
			if (!await CheckCooldown())
				return;

			DayOfWeek day;
			bool argumentsSwapped = false;
			try {
				day = GetDayOfWeekFromString(weekdag);
				lokaal = lokaal.ToUpper();
			} catch (ArgumentException) {
				try {
					day = GetDayOfWeekFromString(lokaal);
					lokaal = weekdag.ToUpper();
					argumentsSwapped = true;
				} catch (ArgumentException) {
					await ReactMinorError();
					await ReplyAsync($"Ik weet niet welke dag je bedoelt met {weekdag} of {lokaal}");
					return;
				}
			}

			ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "Room", argumentsSwapped ? weekdag : lokaal);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = $"Het lijkt er op dat er in {lokaal} op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets hebt.";
					if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
						response += "\nDat is dan ook in het weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = $"In {lokaal} is er op {(DateTime.Today.DayOfWeek == day ? "volgende week " : "")}{DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} als eerste";
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
