using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("klas"), RoosterBot.Attributes.LogTag("SSM")]
	public class StudentScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async), Summary("Welke les een klas nu heeft")]
		public async Task StudentCurrentCommand(string klas) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "StudentSets", klas);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string response;
				if (record == null) {
					response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response, "StudentSets", klas.ToUpper(), null);
				} else {
					response = $"{record.StudentSets}: Nu\n";
					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						if (record.Activity != "pauze") {
							response += TableItemStaffMember(record);
							response += TableItemRoom(record);
						}

						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
					}
					await ReplyAsync(response, "StudentSets", record.StudentSets, record);

					if (record.Activity == "pauze") {
						await GetAfterCommandFunction();
					}
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
					await FatalError($"`GetRecord(true, \"StudentSets\", {klas})` returned null");
				} else {
					bool isToday = record.Start.Date == DateTime.Today;
					string response;

					if (isToday) {
						response = $"{record.StudentSets}: Hierna\n";
					} else {
						response = $"{record.StudentSets}: Als eerste op {Util.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
					}
					
					response += TableItemActivity(record, false);

					if (record.Activity != "stdag doc") {
						if (record.Activity != "pauze") {
							response += TableItemStaffMember(record);
							response += TableItemRoom(record);
						}
						
						response += TableItemStartEndTime(record);
						response += TableItemDuration(record);
					}
					await ReplyAsync(response, "StudentSets", record.StudentSets, record);
					
					if (record.Activity == "pauze") {
						await GetAfterCommandFunction();
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Summary("Welke les je als eerste hebt op een dag")]
		public async Task StudentWeekdayCommand([Remainder] string klas_en_weekdag) {
			if (!await CheckCooldown())
				return;

			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(klas_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string clazz = arguments.Item3;
				ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "StudentSets", clazz);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					string response;
					if (record == null) {
						response = $"Het lijkt er op dat je op {Util.GetStringFromDayOfWeek(day)} niets hebt.";
						if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
							response += "\nDat is dan ook in het weekend.";
						}
					} else {
						if (DateTime.Today.DayOfWeek == day) {
							response = $"{record.StudentSets}: Als eerste op volgende week {Util.GetStringFromDayOfWeek(day)}\n";
						} else {
							response = $"{record.StudentSets}: Als eerste op {Util.GetStringFromDayOfWeek(day)}\n";
						}
						response += TableItemActivity(record, true);

						if (record.Activity != "stdag doc") {
							if (record.Activity != "pauze") {
								response += TableItemStaffMember(record);
								response += TableItemRoom(record);
							}

							response += TableItemStartEndTime(record);
							response += TableItemDuration(record);
						}
					}
					await ReplyAsync(response, "StudentSets", clazz, record);
					
					if (record?.Activity == "pauze") {
						await GetAfterCommandFunction();
					}
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("Welke les je morgen als eerste hebt")]
		public async Task StudentTomorrowCommand(string klas) {
			await StudentWeekdayCommand(klas + " " + Util.GetStringFromDayOfWeek(DateTime.Today.AddDays(1).DayOfWeek));
		}
	}
}
