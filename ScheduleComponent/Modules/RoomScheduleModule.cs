using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("lokaal"), RoosterBot.Attributes.LogTag("PTM")]
	public class RoomScheduleModule : ScheduleModuleBase {
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
					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						response += TableItemStaffMember(record);
						response += TableItemStudentSets(record);
						response += TableItemStartEndTime(record, false, false);
						response += TableItemDuration(record, true);
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
					bool isToday = record.Start.Date == DateTime.Today;
					string response;

					if (isToday) {
						response = $"{record.Room}: Hierna\n";
					} else {
						response = $"{record.Room}: Als eerste op {Util.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
					}

					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						response += TableItemStaffMember(record);
						response += TableItemStudentSets(record);
						response += TableItemStartEndTime(record, isToday, !isToday);
						response += TableItemDuration(record, false);
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

						response += TableItemActivity(record, false);

						if (record.Activity != "stdag doc") {
							response += TableItemStaffMember(record);
							response += TableItemStudentSets(record);
							response += TableItemStartEndTime(record, false, true);
							response += TableItemDuration(record, false);
						}
					}
					await ReplyAsync(response, "Room", room, record);
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("Welke les er morgen als eerste in een lokaal is")]
		public async Task RoomTomorrowCommand(string lokaal) {
			await RoomWeekdayCommand(lokaal + " " + Util.GetStringFromDayOfWeek(DateTime.Today.AddDays(1).DayOfWeek));
		}
	}
}
