using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class StudentScheduleModule : ScheduleModuleBase {
		public StudentScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "SSM") { }

		[Command("nu", RunMode = RunMode.Async), Summary("Welke les een klas nu heeft")]
		public async Task StudentCurrentCommand(string klas) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "StudentSets", klas);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = "";
					if (record.Activity == "pauze") {
						response += $" pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
					} else {
						response += $"Je hebt nu {record.Activity}";
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

		[Command("hierna", RunMode = RunMode.Async), Summary("Welke les een klas hierna heeft")]
		public async Task StudentNextCommand(string klas) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "StudentSets", klas);
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

		[Command("dag", RunMode = RunMode.Async), Summary("Welke les je als eerste hebt op een dag")]
		public async Task StudentWeekdayCommand(string klas, string weekdag) {
			if (!await CheckCooldown())
				return;

			DayOfWeek day;
			bool argumentsSwapped = false;
			try {
				day = GetDayOfWeekFromString(weekdag);
				klas = klas.ToUpper();
			} catch (ArgumentException) {
				try {
					day = GetDayOfWeekFromString(klas);
					klas = weekdag.ToUpper();
					argumentsSwapped = true;
				} catch (ArgumentException) {
					await ReactMinorError();
					await ReplyAsync($"Ik weet niet welke dag je bedoelt met {weekdag} of {klas}");
					return;
				}
			}

			ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "StudentSets", argumentsSwapped ? weekdag : klas);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = $"Het lijkt er op dat je op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets hebt.";
					if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
						response += "\nDat is dan ook in het weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = $"Je hebt op {(DateTime.Today.DayOfWeek == day ? "volgende week ": "")}{DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} als eerste";
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
