using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("TeacherSM"), HiddenFromList]
	public class TeacherScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherCurrentCommand(TeacherInfo[] teachers) {
			if (teachers.Length > 1) {
				await RespondTeacherMultiple(false, teachers);
				LSCService.RemoveLastQuery(Context.User);
			} else {
				ReturnValue<ScheduleRecord> result = await GetRecord(false, teachers[0]);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					RespondTeacherCurrent(teachers[0], record);
				}
			}
		}
		
		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Priority(1)]
		public async Task TeacherNextCommand(TeacherInfo[] teachers) {
			if (teachers.Length > 1) { // There are multiple
				await RespondTeacherMultiple(true, teachers);
				LSCService.RemoveLastQuery(Context.User);
			} else {
				ReturnValue<ScheduleRecord> result = await GetRecord(true, teachers[0]);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					if (record != null) {
						RespondTeacherNext(teachers[0], record);
					} else {
						await FatalError($"`GetRecord(true, \"StaffMember\", {teachers[0]})` returned null");
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherWeekdayCommand(TeacherInfo[] teachers, DayOfWeek day) {
			await RespondTeacherDay(teachers, day, false);
		}

		[Command("dag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherWeekdayCommand(DayOfWeek day, TeacherInfo[] teachers) {
			await RespondTeacherDay(teachers, day, false);
		}

		[Command("vandaag", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTodayCommand(TeacherInfo[] teachers) {
			await RespondTeacherDay(teachers, Util.GetDayOfWeekFromString("vandaag"), true);
		}

		[Command("morgen", RunMode = RunMode.Async), Priority(1)]
		public async Task TeacherTomorrowCommand(TeacherInfo[] teachers) {
			await RespondTeacherDay(teachers, Util.GetDayOfWeekFromString("morgen"), false);
		}

		[Command("deze week", RunMode = RunMode.Sync)]
		public Task ShowThisWeekWorkingDaysCommand(TeacherInfo[] teachers) {
			foreach (TeacherInfo info in teachers) {
				AvailabilityInfo[] days = Schedules.GetWeekAvailability(info, 0);
				RespondWorkingDays(info, days, 0);
			}
			return Task.CompletedTask;
		}

		[Command("volgende week", RunMode = RunMode.Sync)]
		public Task ShowNextWeekWorkingDaysCommand(TeacherInfo[] teachers) {
			foreach (TeacherInfo info in teachers) {
				AvailabilityInfo[] days = Schedules.GetWeekAvailability(info, 1);
				RespondWorkingDays(info, days, 1);
			}
			return Task.CompletedTask;
		}

		private void RespondWorkingDays(TeacherInfo info, AvailabilityInfo[] availability, int weeksFromNow) {
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

				foreach (AvailabilityInfo item in availability) {
					response += $" - {Util.GetStringFromDayOfWeek(item.StartOfAvailability.DayOfWeek).FirstCharToUpper()}: {item.StartOfAvailability.ToShortTimeString()} - {item.EndOfAvailability.ToShortTimeString()}\n";
				}
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

			ReplyDeferred(response);
		}

		private async Task RespondTeacherDay(TeacherInfo[] teachers, DayOfWeek day, bool includeToday) {
			string response = "";
			foreach (TeacherInfo teacher in teachers) {
				ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(teacher, day, includeToday);
				if (result.Success) {
					ScheduleRecord[] records = result.Value;
					if (records.Length != 0) {
						response += $"{teacher.DisplayText}: Rooster ";
						if (DateTime.Today.DayOfWeek == day && includeToday) {
							response += "voor vandaag";
						} else if (DateTime.Today.AddDays(1).DayOfWeek == day) {
							response += "voor morgen";
						} else {
							response += "op " + Util.GetStringFromDayOfWeek(day);
						}
						response += "\n";


						string[][] cells = new string[records.Length + 1][];
						cells[0] = new string[] { "Activiteit", "Tijd", "Klas", "Lokaal" };
						int recordIndex = 1;
						foreach (ScheduleRecord record in records) {
							cells[recordIndex] = new string[4];
							cells[recordIndex][0] = record.Activity;
							cells[recordIndex][1] = $"{record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}";
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
						response += $"Het lijkt er op dat {teacher.DisplayText} ";
						if (DateTime.Today.DayOfWeek == day && includeToday) {
							response += "vandaag";
						} else if (DateTime.Today.AddDays(1).DayOfWeek == day) {
							response += "morgen";
						} else {
							response += "op " + Util.GetStringFromDayOfWeek(day);
						}
						response += " niets heeft.";
						if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
							response += " Het is dan ook weekend.";
						}
						response += "\n";
					}
				}
			}
			await ReplyAsync(response, null, null);
		}

		// This is a seperate function because two teachers have the same name. We would have to write this function three times in TeacherCurrentCommand().
		private void RespondTeacherCurrent(TeacherInfo teacher, ScheduleRecord record) {
			string response;
			if (record == null) {
				response = $"Het lijkt erop dat {teacher.DisplayText} nu niets heeft.";
				ReturnValue<ScheduleRecord> nextRecord = GetRecord(true, teacher).GetAwaiter().GetResult();

				if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
					response += "\nHij/zij staat vandaag ook niet op het rooster, en is dus waarschijnlijk afwezig.";
				}
			} else {
				response = $"{teacher.DisplayText}: Nu\n";
				response += TableItemActivity(record, false);

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						response += TableItemRoom(record);
						response += TableItemStudentSets(record);
					}

					response += TableItemStartEndTime(record);
					response += TableItemDuration(record);
					response += TableItemBreak(record);
				}
			}

			ReplyDeferred(response, teacher, record);

			if (record?.Activity == "pauze") {
				GetAfterCommand().GetAwaiter().GetResult();
			}
		}

		private void RespondTeacherNext(TeacherInfo teacher, ScheduleRecord record) {
			bool isToday = record.Start.Date == DateTime.Today;
			string response;

			if (isToday) {
				response = $"{teacher.DisplayText}: Hierna\n";
			} else {
				response = $"{teacher.DisplayText}: Als eerste op {Util.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
			}

			response += TableItemActivity(record, !isToday);

			if (record.Activity != "stdag doc") {
				if (record.Activity != "pauze") {
					response += TableItemRoom(record);
					response += TableItemStudentSets(record);
				}

				response += TableItemStartEndTime(record);
				response += TableItemDuration(record);
				response += TableItemBreak(record);
			}
			ReplyDeferred(response, teacher, record);
			
			if (record.Activity == "pauze") {
				GetAfterCommand().GetAwaiter().GetResult();
			}
		}

		private async Task RespondTeacherMultiple(bool next, params TeacherInfo[] teachers) {
			ReturnValue<ScheduleRecord>[] results = new ReturnValue<ScheduleRecord>[teachers.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = await GetRecord(next, teachers[i]);
			}

			Action<TeacherInfo, ScheduleRecord> respondFunction = next ? (Action<TeacherInfo, ScheduleRecord>) RespondTeacherNext : RespondTeacherCurrent;
			
			for (int i = 0; i < results.Length; i++) {
				if (results[i].Success) {
					respondFunction(teachers[i], results[i].Value);
				} else {
					ReplyDeferred($"{teachers[i].DisplayText}: Geen info.");
				}
			}
		}
	}
}
