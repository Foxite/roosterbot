using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("leraar"), RoosterBot.Attributes.LogTag("PTM")]
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
							await FatalError("GetRecord(TS1)==null)");
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
				response = $"Het lijkt er op dat {teacher} op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets heeft.";
			} else {
				if (DateTime.Today.DayOfWeek == day) {
					response = $"{teacher}: Als eerste op volgende week {DateTimeFormatInfo.CurrentInfo.GetDayName(day)}\n";
				} else {
					response = $"{teacher}: Als eerste op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)}\n";
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

			if (teachers.Contains(null)) {
				await FatalError($"RespondTeacherMultiple: GetTeacherName({string.Join(",", teacherAbbrs)}) == null");
			}
			
			ReturnValue<ScheduleRecord>[] results = new ReturnValue<ScheduleRecord>[teachers.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = await GetRecord(next, "StaffMember", teacherAbbrs[i]);
			}

			Func<string, string, ScheduleRecord, string> respondFunction = next ? (Func<string, string, ScheduleRecord, string>) RespondTeacherNext : RespondTeacherCurrent;
			
			for (int i = 0; i < results.Length; i++) {
				if (results[i].Success) {
					response += respondFunction(results[i].Value.StaffMember, teachers[i], results[i].Value) + "\n\n";
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

			if (teachers.Contains(null)) {
				await FatalError($"RespondTeacherWeekdayMultiple: GetTeacherName({string.Join(",", teacherAbbrs)}) == null");
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
