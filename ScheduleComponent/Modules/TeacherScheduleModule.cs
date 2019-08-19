using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using RoosterBot.Preconditions;
using ScheduleComponent.DataTypes;

namespace ScheduleComponent.Modules {
	[LogTag("TeacherSM"), HiddenFromList]
	public class TeacherScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherCurrentCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				ReturnValue<ScheduleRecord> result = await GetRecord(false, teachers[0]);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record == null) {
						string response = $"Het lijkt erop dat {teacher.DisplayText} nu niets heeft.";
						ReturnValue<ScheduleRecord> nextRecord = GetRecord(true, teacher).GetAwaiter().GetResult();

						if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
							response += "\nHij/zij staat vandaag ook niet (meer) op het rooster, en is dus waarschijnlijk afwezig.";
						}

						ReplyDeferred(response, teacher, record);
					} else {
						await RespondRecord($"{teacher.DisplayText}: Nu\n", teacher, record);
					}
				}
			}
		}
		
		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Priority(1)]
		public async Task TeacherNextCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				ReturnValue<ScheduleRecord> result = await GetRecord(true, teachers[0]);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					
					if (record == null) {
						string response = $"Het lijkt erop dat {teacher.DisplayText} nu niets heeft.";
						ReturnValue<ScheduleRecord> nextRecord = GetRecord(true, teacher).GetAwaiter().GetResult();

						if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
							response += "\nHij/zij staat vandaag ook niet (meer) op het rooster, en is dus waarschijnlijk afwezig.";
						}

						ReplyDeferred(response, teacher, record);
					} else {
						string pretext;
						if (record.Start.Date == DateTime.Today) {
							pretext = $"{teacher.DisplayText}: Hierna\n";
						} else {
							pretext = $"{teacher.DisplayText}: Als eerste op {ScheduleUtil.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
						}

						await RespondRecord(pretext, teacher, record);
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherWeekdayCommand(DayOfWeek day, [Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				await RespondDay(teacher, ScheduleUtil.NextDayOfWeek(day, false));
			}
		}

		[Command("vandaag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTodayCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				await RespondDay(teacher, DateTime.Today);
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTomorrowCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo teacher in teachers) {
				await RespondDay(teacher, DateTime.Today.AddDays(1));
			}
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public async Task ShowThisWeekWorkingDaysCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo info in teachers) {
				await RespondWeek(info, 0);
			}
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public async Task ShowNextWeekWorkingDaysCommand([Remainder] TeacherInfo[] teachers) {
			foreach (TeacherInfo info in teachers) {
				await RespondWeek(info, 1);
			}
		}

		[Command("over", RunMode = RunMode.Sync)]
		public async Task ShowNWeeksWorkingDaysCommand([Range(1, 52)] int weeks, TeacherInfo[] infos) {
			foreach (var info in infos) {
				await RespondWeek(info, weeks);
			}
		}

		public async Task ShowFutureCommand([Range(1, 52)] int amount, string unit, TeacherInfo[] teachers) {
			if (unit == "uur") {
				foreach (TeacherInfo teacher in teachers) {
					ReturnValue<ScheduleRecord> result = await GetRecordAfterTimeSpan(teacher, TimeSpan.FromHours(amount));
					if (result.Success) {
						await RespondRecord(string.Join(", ", teachers.Select(t => t.DisplayText)), teacher, result.Value);
					}
				}
			} else if (unit == "dag" || unit == "dagen") {
				foreach (TeacherInfo teacher in teachers) {
					await RespondDay(teacher, DateTime.Today.AddDays(amount));
				}
			} else if (unit == "week" || unit == "weken") {
				foreach (TeacherInfo teacher in teachers) {
					await RespondWeek(teacher, amount);
				}
			} else {
				await MinorError("Ik ondersteun alleen uren, dagen, en weken.");
			}
		}

		private async Task RespondDay(TeacherInfo teacher, DateTime date) {
			ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(teacher, date);
			string response = "";
			if (result.Success) {
				ScheduleRecord[] records = result.Value;
				if (records.Length != 0) {
					response += $"{teacher.DisplayText}: Rooster van {ScheduleUtil.GetRelativeDateReference(date)}\n";


					string[][] cells = new string[records.Length + 1][];
					cells[0] = new string[] { "Activiteit", "Tijd", "Klas", "Lokaal" };
					int recordIndex = 1;
					foreach (ScheduleRecord record in records) {
						cells[recordIndex] = new string[4];
						cells[recordIndex][0] = record.Activity;
						cells[recordIndex][1] = $"{record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
						cells[recordIndex][2] = record.StudentSetsString;

						string room = record.RoomString;
						if (room.Contains(',')) {
							room = Util.FormatStringArray(room.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries), " en ");
						}
						cells[recordIndex][3] = room;
						recordIndex++;
					}
					response += Util.FormatTextTable(cells, true);
				} else {
					response += $"Het lijkt er op dat {teacher.DisplayText} {ScheduleUtil.GetRelativeDateReference(date)} niets heeft.";
					if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday) {
						response += " Dat is dan ook weekend.";
					}
					response += "\n";
				}
			}
			ReplyDeferred(response, null, null);
		}

		private async Task RespondWeek(TeacherInfo info, int weeksFromNow) {
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
