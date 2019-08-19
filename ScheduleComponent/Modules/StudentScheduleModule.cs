using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using RoosterBot.Preconditions;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Modules {
	[LogTag("StudentSM"), HiddenFromList]
	public class StudentScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async)]
		public async Task StudentCurrentCommand(StudentSetInfo info) {
			ReturnValue<ScheduleRecord> result = await GetRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response, info, null);
				} else {
					await RespondRecord($"{record.StudentSetsString}: Nu\n", info, record);
				}
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen")]
		public async Task StudentNextCommand(StudentSetInfo info) {
			ReturnValue<ScheduleRecord> result = await GetNextRecord(info);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError($"`GetNextRecord(\"StudentSets\", {info.DisplayText})` returned null");
				} else {
					string pretext;
					if (record.Start.Date == DateTime.Today) {
						pretext = $"{record.StudentSetsString}: Hierna\n";
					} else {
						pretext = $"{record.StudentSetsString}: Als eerste op {ScheduleUtil.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
					}
					await RespondRecord(pretext, info, record);
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async)]
		public async Task StudentWeekdayCommand(DayOfWeek day, StudentSetInfo info) {
			await RespondDay(info, ScheduleUtil.NextDayOfWeek(day, false));
		}

		[Command("vandaag", RunMode = RunMode.Async)]
		public async Task StudentTodayCommand(StudentSetInfo info) {
			await RespondDay(info, DateTime.Today);
		}

		[Command("morgen", RunMode = RunMode.Async)]
		public async Task StudentTomorrowCommand(StudentSetInfo info) {
			await RespondDay(info, DateTime.Today.AddDays(1));
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public async Task ShowThisWeekWorkingDaysCommand(StudentSetInfo info) {
			await RespondWorkingDays(info, 0);
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public async Task ShowNextWeekWorkingDaysCommand(StudentSetInfo info) {
			await RespondWorkingDays(info, 1);
		}

		[Command("over", RunMode = RunMode.Sync)]
		public async Task ShowFutureCommand([Range(1, 52)] int amount, string unit, StudentSetInfo info) {
			if (unit == "uur") {
				ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(info, TimeSpan.FromHours(amount));
				if (result.Success) {
					ScheduleRecord record = result.Value;
					await RespondRecord($"{record.StudentSetsString}: Over {amount} uur", info, record);
				}
			} else if (unit == "dag" || unit == "dagen") {
				await RespondDay(info, DateTime.Today.AddDays(amount));
			} else if (unit == "week" || unit == "weken") {
				await RespondWorkingDays(info, amount);
			} else {
				await MinorError("Ik ondersteun alleen uren, dagen, en weken.");
			}
		}

		private async Task RespondDay(StudentSetInfo info, DateTime date) {
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(info, date);
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				string response;
				if (records.Length == 0) {
					response = $"Het ziet ernaar uit dat je {ScheduleUtil.GetRelativeDateReference(date)} niets hebt.";
					if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response, info, null);
				} else {
					response = $"{info.DisplayText}: Rooster van {ScheduleUtil.GetRelativeDateReference(date)}\n";

					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] { "Activiteit", "Tijd", "Leraar", "Lokaal" };
					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity;
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
						cells[recordIndex][2] = record.StaffMember.Length == 0 ? "---" : string.Join(", ", record.StaffMember.Select(t => t.DisplayText));

						string room = record.RoomString;
						if (room.Contains(',')) {
							room = Util.FormatStringArray(room.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries), " en ");
						}
						cells[recordIndex][3] = room;
						recordIndex++;
					}
					response += Util.FormatTextTable(cells, true);
					ReplyDeferred(response, info, records.Last());
				}
			}
		}

		private async Task RespondWorkingDays(StudentSetInfo info, int weeksFromNow) {
			ReturnValue<AvailabilityInfo[]> result = await GetWeekAvailabilityInfo(info, weeksFromNow);
			if (result.Success) {
				AvailabilityInfo[] availability = result.Value;

				string response = info.DisplayText + ": ";

				if (availability.Length > 0) {
					if (weeksFromNow == 0) {
						response += "Deze week";
					} else if (weeksFromNow == 1) {
						response += "Volgende week";
					} else {
						response += $"Over {weeksFromNow} weken";
					}
					response += " op school op \n";

					string[][] cells = new string[availability.Length + 1][];
					cells[0] = new[] { "Dag", "Van", "Tot" };

					int i = 1;
					foreach (AvailabilityInfo item in availability) {
						cells[i] = new[] { ScheduleUtil.GetStringFromDayOfWeek(item.StartOfAvailability.DayOfWeek).FirstCharToUpper(), item.StartOfAvailability.ToShortTimeString(), item.EndOfAvailability.ToShortTimeString() };
						i++;
					}
					response += Util.FormatTextTable(cells, false);
				} else {
					response += "Niet op school ";
					if (weeksFromNow == 0) {
						response += "deze week";
					} else if (weeksFromNow == 1) {
						response += "volgende week";
					} else {
						response += $"over {weeksFromNow} weken";
					}
				}

				await ReplyAsync(response);
			}
		}
	}
}
