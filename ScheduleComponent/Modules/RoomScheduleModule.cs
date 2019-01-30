using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("lokaal"), RoosterBot.Attributes.LogTag("RoomSM")]
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
						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
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
					await FatalError($"`GetRecord(true, \"Room\", {lokaal})` returned null");
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
						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
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
						response = $"Het lijkt er op dat er in {room} op {Util.GetStringFromDayOfWeek(day)} niets is.";
						if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
							response += "\nDat is dan ook in het weekend.";
						}
					} else {
						response = $"{record.Room}: Als eerste op {Util.GetStringFromDayOfWeek(day)}\n";

						response += TableItemActivity(record, false);

						if (record.Activity != "stdag doc") {
							response += TableItemStaffMember(record);
							response += TableItemStudentSets(record);
							response += TableItemStartEndTime(record);
							response += TableItemDuration(record);
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

		[Command("vandaag", RunMode = RunMode.Async), Summary("Het rooster voor een lokaal voor vandaag")]
		public async Task StudentTodayCommand(string lokaal) {
			if (!await CheckCooldown())
				return;

			DayOfWeek day = DateTime.Today.DayOfWeek;

			ReturnValue<ScheduleRecord[]> result = await GetScheduleForToday("Room", lokaal);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = "Het ziet ernaar uit dat daar vandaag niets is.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response, "Room", lokaal.ToUpper(), null);
				} else {
					response = $"{lokaal.ToUpper()}: Rooster voor vandaag\n";

					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] { "Activiteit", "Tijd", "Klas", "Leraar" };
					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity;
						cells[recordIndex][1] = $"{record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}";
						cells[recordIndex][2] = record.StudentSets;
						cells[recordIndex][3] = string.IsNullOrEmpty(record.StaffMember) ? "" : GetTeacherFullNamesFromAbbrs(record.StaffMember);
						recordIndex++;
					}
					response += Util.FormatTextTable(cells, true);
					await ReplyAsync(response, "Room", lokaal.ToUpper(), records.Last());
				}
			}
		}
	}
}
