using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	[Group("lokaal")]
	public class RoomScheduleModule : ScheduleModuleBase {
		private string LogTag { get; }

		public RoomScheduleModule() : base() {
			LogTag = "RSM";
		}

		[Command("nu", RunMode = RunMode.Async), Summary("Wat er nu in een lokaal plaatsvindt")]
		private async Task RoomCurrentCommand(string lokaal) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "Room", lokaal);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string response;
				if (record == null) {
					response = "Het ziet ernaar uit dat daar nu niets is.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
				} else {
					response = $"{record.Room}: Nu\n";
					response += $":notepad_spiral: {GetActivityFromAbbr(record.Activity)}\n";

					if (record.Activity != "stdag doc") {
						string teachers = GetTeacherNameFromAbbr(record.StaffMember);
						if (!string.IsNullOrWhiteSpace(teachers)) {
							response += $":bust_in_silhouette: {teachers}\n";
						}
						if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
							response += $":busts_in_silhouette: {record.StudentSets}\n";
						}
						response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
						TimeSpan timeLeft = record.End - DateTime.Now;
						response += $":stopwatch: {record.Duration} - nog {timeLeft.Hours}:{timeLeft.Minutes}\n";
					}
				}
				await ReplyAsync(response, "Room", (record?.Room) ?? lokaal.ToUpper(), record);
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Summary("Wat er hierna in een lokaal plaatsvindt")]
		private async Task RoomNextCommand(string lokaal) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "Room", lokaal);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError("GetRecord(RS1)==null");
				} else {
					string response = $"{record.Room}: Hierna\n";
					response += $":notepad_spiral: {GetActivityFromAbbr(record.Activity)}\n";

					if (record.Activity != "stdag doc") {
						string teachers = GetTeacherNameFromAbbr(record.StaffMember);
						if (!string.IsNullOrWhiteSpace(teachers)) {
							response += $":bust_in_silhouette: {teachers}\n";
						}
						if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
							response += $":busts_in_silhouette: {record.StudentSets}\n";
						}

						if (record.Start.Date != DateTime.Today) {
							response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
						}
						response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
						response += $":stopwatch: {record.Duration}\n";
					}
					await ReplyAsync(response, "Room", record.Room, record);
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Summary("Welke les er als eerste in een lokaal op een dag")]
		public async Task RoomWeekdayCommand([Remainder] string lokaal_en_weekdag) {
			if (!await CheckCooldown())
				return;


			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(lokaal_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string room = arguments.Item3.ToUpper();

				ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "Room", room);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					string response;
					if (record == null) {
						response = $"Het lijkt er op dat er in {room} op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets is.";
						if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
							response += "\nDat is dan ook in het weekend.";
						}
					} else {
						response = $"{record.Room}: Als eerste op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)}\n";
						response += $":notepad_spiral: {GetActivityFromAbbr(record.Activity)}\n";

						if (record.Activity != "stdag doc") {
							string teachers = GetTeacherNameFromAbbr(record.StaffMember);
							if (!string.IsNullOrWhiteSpace(teachers)) {
								response += $":bust_in_silhouette: {teachers}\n";
							}
							if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
								response += $":busts_in_silhouette: {record.StudentSets}\n";
							}

							if (record.Start.Date != DateTime.Today) {
								response += $":calendar_spiral: {record.Start.ToShortDateString()}\n";
							}
							response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
							response += $":stopwatch: {record.Duration}\n";
						}
					}
					await ReplyAsync(response, "Room", room, record);
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("Welke les er morgen als eerste in een lokaal is")]
		public async Task RoomTomorrowCommand(string lokaal) {
			await RoomWeekdayCommand(lokaal + " " + GetStringFromDayOfWeek(DateTime.Today.AddDays(1).DayOfWeek));
		}
	}
}
