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


			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(lokaal, weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string room = arguments.Item3.ToUpper();

				ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "Room", room);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record == null) {
						string response = $"Het lijkt er op dat er in {room} op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets is.";
						if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
							response += "\nDat is dan ook in het weekend.";
						}
						await ReplyAsync(response);
					} else {
						string response = $"In {room} is er op {(DateTime.Today.DayOfWeek == day ? "volgende week " : "")}{DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} als eerste";
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
}
