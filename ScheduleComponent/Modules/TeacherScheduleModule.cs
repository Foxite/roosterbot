using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("leraar"), RoosterBot.Attributes.LogTag("TeacherSM")]
	public class TeacherScheduleModule : ScheduleModuleBase {
		[Command("nu", RunMode = RunMode.Async), Priority(1), Summary("Waar een leraar nu mee bezig is")]
		public async Task TeacherCurrentCommand([Remainder] string leraar) {
			if (!await CheckCooldown())
				return;

			string[] teachers = Teachers.GetAbbrsFromNameInput(leraar);

			if (teachers.Length == 0) {
				await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teachers.Length > 1) {
					await ReplyAsync(await RespondTeacherMultiple(false, leraar, teachers));
					LSCService.RemoveLastQuery(Context.User);
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(false, "StaffMember", teachers[0]);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						await ReplyAsync(RespondTeacherCurrent(teachers[0], Teachers.GetFullNameFromAbbr(teachers[0]), record), "StaffMember", teachers[0], record);
					}
				}
			}
		}
		
		[Command("hierna", RunMode = RunMode.Async), Priority(1), Summary("Waar een leraar hierna mee bezig is")]
		public async Task TeacherNextCommand([Remainder] string leraar) {
			if (!await CheckCooldown())
				return;

			string[] teachers = Teachers.GetAbbrsFromNameInput(leraar);
			if (teachers.Length == 0) {
				await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teachers.Length > 1) { // There are multiple
					await ReplyAsync(await RespondTeacherMultiple(true, leraar, teachers));
					LSCService.RemoveLastQuery(Context.User);
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(true, "StaffMember", teachers[0]);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						if (record != null) {
							await ReplyAsync(RespondTeacherNext(record.StaffMember, Teachers.GetFullNameFromAbbr(teachers[0]), record).FirstCharToUpper(), "StaffMember", teachers[0], record);
						} else {
							await FatalError($"`GetRecord(true, \"StaffMember\", {teachers[0]})` returned null");
						}
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Priority(1), Summary("Welke les een leraar als eerste heeft op een dag")]
		public async Task TeacherWeekdayCommand([Remainder] string leraar_en_weekdag) {
			if (!await CheckCooldown())
				return;
			
			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(leraar_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string teacherGiven = arguments.Item3;
				string[] teachers = Teachers.GetAbbrsFromNameInput(arguments.Item3);

				if (teachers.Length == 0) {
					await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
				} else {
					if (teachers.Length > 1) {
						await ReplyAsync(await RespondTeacherWeekdayMultiple(day, teacherGiven, teachers));
						LSCService.RemoveLastQuery(Context.User);
					} else {
						ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "StaffMember", teachers[0]);
						if (result.Success) {
							ScheduleRecord record = result.Value;
							await ReplyAsync(RespondTeacherWeekday(Teachers.GetFullNameFromAbbr(teachers[0]), day, record), "StaffMember", teachers[0], record);
						}
					}
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Priority(1), Summary("Welke les een leraar morgen als eerste heeft")]
		public async Task TeacherTomorrowCommand([Remainder] string leraar) {
			await TeacherWeekdayCommand(leraar + " " + Util.GetStringFromDayOfWeek(DateTime.Today.AddDays(1).DayOfWeek));
		}

		[Command("vandaag", RunMode = RunMode.Async), Priority(1), Summary("Het rooster van vandaag van een leraar")]
		public async Task StudentTodayCommand([Remainder] string leraarInput) {
			if (!await CheckCooldown())
				return;

			DayOfWeek day = DateTime.Today.DayOfWeek;

			string[] teachers = Teachers.GetAbbrsFromNameInput(leraarInput);

			if (teachers.Length == 0) {
				await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				string response = "";
				foreach (string teacher in teachers) {
					ReturnValue<ScheduleRecord[]> result = await GetScheduleForToday("StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord[] records = result.Value;
						if (records.Length != 0) {
							response += $"{Teachers.GetFullNameFromAbbr(teacher)}: Rooster voor vandaag\n";

							string[][] cells = new string[records.Length + 1][];
							cells[0] = new string[] { "Activiteit", "Tijd", "Klas", "Lokaal" };
							int recordIndex = 1;
							foreach (ScheduleRecord record in records) {
								cells[recordIndex] = new string[4];
								cells[recordIndex][0] = record.Activity;
								cells[recordIndex][1] = $"{record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}";
								cells[recordIndex][2] = record.StudentSets;

								string room = record.Room;
								if (room.Contains(',')) {
									room = Util.FormatStringArray(room.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries), " en ");
								}
								cells[recordIndex][3] = room;
								recordIndex++;
							}
							response += Util.FormatTextTable(cells, true);
						} else {
							response += $"Het lijkt er op dat {Teachers.GetFullNameFromAbbr(teacher)} vandaag niets heeft.\n";
						}
					}
				}
				await ReplyAsync(response, "StaffMember", Util.FormatStringArray(Teachers.GetRecordsFromAbbrs(teachers).Select(r => r.Abbreviation).ToArray()), null);
			}
		}

		// This is a seperate function because two teachers have the same name. We would have to write this function three times in TeacherCurrentCommand().
		private string RespondTeacherCurrent(string teacherAbbr, string teacher, ScheduleRecord record) {
			string response;
			if (record == null) {
				response = $"Het lijkt erop dat {teacher} nu niets heeft.";
				ReturnValue<ScheduleRecord> nextRecord = GetRecord(true, "StaffMember", teacherAbbr).GetAwaiter().GetResult();

				if (nextRecord.Success && nextRecord.Value.Start.Date != DateTime.Today) {
					response += "\nHij/zij staat vandaag ook niet op het rooster, en is dus waarschijnlijk afwezig.";
				}
			} else {
				response = $"{teacher}: Nu\n";
				response += TableItemActivity(record, false);

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						response += TableItemRoom(record);
						response += TableItemStudentSets(record);
					}

					response += TableItemStartEndTime(record);
					response += TableItemDuration(record);
				}
			}
			return response;
		}

		private string RespondTeacherNext(string teacherAbbr, string teacher, ScheduleRecord record) {
			bool isToday = record.Start.Date == DateTime.Today;
			string response;

			if (isToday) {
				response = $"{teacher}: Hierna\n";
			} else {
				response = $"{teacher}: Als eerste op {Util.GetStringFromDayOfWeek(record.Start.DayOfWeek)}\n";
			}

			response += TableItemActivity(record, !isToday);

			if (record.Activity != "stdag doc") {
				if (record.Activity != "pauze") {
					response += TableItemRoom(record);
					response += TableItemStudentSets(record);
				}

				response += TableItemStartEndTime(record);
				response += TableItemDuration(record);
			}
			return response;
		}
		
		private string RespondTeacherWeekday(string teacher, DayOfWeek day, ScheduleRecord record) {
			string response;
			if (record == null) {
				response = $"Het lijkt er op dat {teacher} op {Util.GetStringFromDayOfWeek(day)} niets heeft.";
			} else {
				if (DateTime.Today.DayOfWeek == day) {
					response = $"{teacher}: Als eerste op volgende week {Util.GetStringFromDayOfWeek(day)}\n";
				} else {
					response = $"{teacher}: Als eerste op {Util.GetStringFromDayOfWeek(day)}\n";
				}
				response += TableItemActivity(record, true);

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						response += TableItemRoom(record);
						response += TableItemStudentSets(record);
					}

					response += TableItemStartEndTime(record);
					response += TableItemDuration(record);
				}
			}
			return response;
		}

		private async Task<string> RespondTeacherMultiple(bool next, string ambiguousPart, params string[] teacherAbbrs) {
			string response = "";

			string[] teachers = new string[teacherAbbrs.Length];
			for (int i = 0; i < teacherAbbrs.Length; i++) {
				teachers[i] = Teachers.GetFullNameFromAbbr(teacherAbbrs[i]);
			}
			
			ReturnValue<ScheduleRecord>[] results = new ReturnValue<ScheduleRecord>[teachers.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = await GetRecord(next, "StaffMember", teacherAbbrs[i]);
			}

			Func<string, string, ScheduleRecord, string> respondFunction = next ? (Func<string, string, ScheduleRecord, string>) RespondTeacherNext : RespondTeacherCurrent;
			
			for (int i = 0; i < results.Length; i++) {
				if (results[i].Success) {
					Console.WriteLine(results[i].Value == null);
					response += respondFunction(teacherAbbrs[i], teachers[i], results[i].Value) + "\n\n";
				} else {
					response += $"{teachers[i]}: Geen info.";
				}
			}

			return response;
		}

		private async Task<string> RespondTeacherWeekdayMultiple(DayOfWeek day, string ambiguousPart, params string[] teacherAbbrs) {
			string response = "";

			string[] teachers = new string[teacherAbbrs.Length];
			for (int i = 0; i < teacherAbbrs.Length; i++) {
				teachers[i] = Teachers.GetFullNameFromAbbr(teacherAbbrs[i]);
			}
			
			ReturnValue<ScheduleRecord>[] results = new ReturnValue<ScheduleRecord>[teachers.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = await GetFirstRecord(day, "StaffMember", teacherAbbrs[i]);
			}
			
			for (int i = 0; i < results.Length; i++) {
				if (results[i].Success) {
					response += RespondTeacherWeekday(teachers[i], day, results[i].Value) + "\n\n";
				} else {
					response += $"{teachers[i]}: Geen info.";
				}
			}

			return response;
		}
	}
}
