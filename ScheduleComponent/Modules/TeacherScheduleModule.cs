using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using RoosterBot.Attributes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("leraar"), LogTag("TeacherSM"), HiddenFromList]
	public class TeacherScheduleModule : ScheduleModuleBase<TeacherInfo> {
		[Command("nu", RunMode = RunMode.Async), Priority(1), Summary("Waar een leraar nu mee bezig is")]
		public async Task TeacherCurrentCommand([Remainder] string leraar) {
			TeacherInfo[] teachers = Teachers.Lookup(leraar);

			if (teachers.Length == 0) {
				await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
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
		}
		
		[Command("hierna", RunMode = RunMode.Async), Alias("later", "straks", "zometeen"), Priority(1), Summary("Waar een leraar hierna mee bezig is")]
		public async Task TeacherNextCommand([Remainder] string leraar) {
			TeacherInfo[] teachers = Teachers.Lookup(leraar);

			if (teachers.Length == 0) {
				await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
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
		}

		[Command("dag", RunMode = RunMode.Async), Priority(1), Summary("Welke les een leraar als eerste heeft op een dag")]
		public async Task TeacherWeekdayCommand([Remainder] string leraar_en_weekdag) {
			Tuple<bool, DayOfWeek, string, bool> arguments = await GetValuesFromArguments(leraar_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string teacherGiven = arguments.Item3;
				TeacherInfo[] teachers = Teachers.Lookup(arguments.Item3);

				if (teachers.Length == 0) {
					await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
				} else {
					string response = "";
					foreach (TeacherInfo teacher in teachers) {
						ReturnValue<ScheduleRecord[]> result = await GetSchedulesForDay(teacher, day, arguments.Item4);
						if (result.Success) {
							ScheduleRecord[] records = result.Value;
							if (records.Length != 0) {
								response += $"{teacher.DisplayText}: Rooster ";
								if (DateTime.Today.DayOfWeek == day && arguments.Item4) {
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
								if (DateTime.Today.DayOfWeek == day && arguments.Item4) {
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
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Priority(1), Summary("Welke les een leraar morgen als eerste heeft")]
		public async Task TeacherTomorrowCommand([Remainder] string leraar) {
			await TeacherWeekdayCommand(leraar + " morgen");
		}

		[Command("vandaag", RunMode = RunMode.Async), Priority(1), Summary("Het rooster van vandaag van een leraar")]
		public async Task TeacherTodayCommand([Remainder] string leraarInput) {
			await TeacherWeekdayCommand(leraarInput + " vandaag");
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
						response += TableItemStudentSets(record);
						response += TableItemRoom(record);
					}

					response += TableItemStartEndTime(record);
					response += TableItemDuration(record);
					response += TableItemBreak(record);
				}
			}

			ReplyDeferred(response, teacher, record);

			if (record?.Activity == "pauze") {
				GetAfterCommandFunction().GetAwaiter().GetResult();
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
					response += TableItemStudentSets(record);
					response += TableItemRoom(record);
				}

				response += TableItemStartEndTime(record);
				response += TableItemDuration(record);
				response += TableItemBreak(record);
			}
			ReplyDeferred(response, teacher, record);
			
			if (record.Activity == "pauze") {
				GetAfterCommandFunction().GetAwaiter().GetResult();
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
