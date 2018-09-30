using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	public class TeacherScheduleModule : ScheduleModuleBase {
		public TeacherScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "TSM") { }

		[Command("leraarnu", RunMode = RunMode.Async), Summary("Waar een leraar nu mee bezig is")]
		public async Task TeacherCurrentCommand([Remainder] string leraar) {
			if (!await CheckCooldown())
				return;

			string teacher = GetTeacherAbbrFromName(leraar);

			if (teacher == null) {
				await ReactMinorError();
				await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher.Contains(", ")) { // There are multiple
					await RespondTeacherMultiple(false, leraar, teacher.Split(new[] { ", " }, StringSplitOptions.None));
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(false, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						await ReplyAsync(RespondTeacherCurrent(GetTeacherNameFromAbbr(teacher), record));
					}
				}
			}
		}
		
		[Command("leraarhierna", RunMode = RunMode.Async), Summary("Waar een leraar hierna mee bezig is")]
		public async Task TeacherNextCommand([Remainder] string leraar) {
			if (!await CheckCooldown())
				return;

			string teacher = GetTeacherAbbrFromName(leraar);
			if (teacher == null) {
				await ReactMinorError();
				await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher.Contains(", ")) { // There are multiple
					await RespondTeacherMultiple(true, leraar, teacher.Split(new[] { ", " }, StringSplitOptions.None));
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(true, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						if (record != null) {
							await ReplyAsync(RespondTeacherNext(GetTeacherNameFromAbbr(teacher), record));
						} else {
							await FatalError("GetRecord(TS1)==null)");
						}
					}
				}
			}
		}

		[Command("leraardag", RunMode = RunMode.Async), Summary("Welke les een leraar als eerste heeft op een dag")]
		public async Task TeacherWeekdayCommand(string leraar, string weekdag) {
			if (!await CheckCooldown())
				return;

			string teacher;
			DayOfWeek day;
			bool argumentsSwapped = false;
			try {
				day = GetDayOfWeekFromString(weekdag);
				teacher = GetTeacherAbbrFromName(leraar);
			} catch (ArgumentException) {
				try {
					day = GetDayOfWeekFromString(leraar);
					teacher = GetTeacherAbbrFromName(weekdag);
					argumentsSwapped = true;
				} catch (ArgumentException) {
					await ReactMinorError();
					await ReplyAsync($"Ik weet niet welke dag je bedoelt met {weekdag} of {leraar}");
					return;
				}
			}
			
			if (teacher == null) {
				await ReactMinorError();
				await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher.Contains(", ")) {
					await RespondTeacherWeekdayMultiple(day, argumentsSwapped ? weekdag : leraar, teacher.Split(new[] { ", " }, StringSplitOptions.None));
				} else {
					ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						await ReplyAsync(RespondTeacherWeekday(GetTeacherNameFromAbbr(teacher), day, record));
					}
				}
			}
		}

		private string RespondTeacherWeekday(string teacher, DayOfWeek day, ScheduleRecord record) {
			if (record == null) {
				string response = $"Het lijkt er op dat {teacher} op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets heeft.";
				if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
					response += "\nDat is dan ook in het weekend.";
				}
				return response;
			} else {
				string response = $"{teacher} heeft op {(DateTime.Today.DayOfWeek == day ? "volgende week " : "")}{DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} als eerste";
				if (record.Activity == "pauze") {
					response += $" pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
				} else {
					response += $" {record.Activity}";

					if (!string.IsNullOrEmpty(record.StudentSets)) {
						response += $" aan {record.StudentSets}";
					}
					if (!string.IsNullOrEmpty(record.Room)) {
						response += $" in {record.Room}";
					} else {
						response += ", maar ik weet niet waar";
					}
					response += $".\n{GetTimeSpanResponse(record)}";
				}
				return response;
			}
		}

		private async Task RespondTeacherWeekdayMultiple(DayOfWeek day, string ambiguousPart, params string[] teacherAbbrs) {
			string response = "";

			string[] teachers = new string[teacherAbbrs.Length];
			for (int i = 0; i < teacherAbbrs.Length; i++) {
				teachers[i] = GetTeacherNameFromAbbr(teacherAbbrs[i]);
			}

			if (teachers.Contains(null)) {
				await FatalError($"RespondTeacherWeekdayMultiple: GetTeacherName({string.Join(",", teacherAbbrs)}) == null");
			}

			response += $"\"{ambiguousPart}\" kan {Util.FormatStringArray(teachers, ", of ")} zijn.\n\n";
			ReturnValue<ScheduleRecord>[] results = new ReturnValue<ScheduleRecord>[teachers.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = await GetFirstRecord(day, "StaffMember", teacherAbbrs[i]);
			}

			bool allSuccessful = true;
			string resultResponse = "";
			for (int i = 0; i < results.Length; i++) {
				if (results[i].Success) {
					resultResponse += RespondTeacherWeekday(teachers[i], day, results[i].Value) + "\n";
				} else {
					allSuccessful = false;
				}
			}

			if (allSuccessful) {
				response += resultResponse;
				response += "Hou er rekening mee dat ik niet weet of een leraar ziek is, of om een andere reden weg is.";
			} else if (string.IsNullOrEmpty(resultResponse)) {
				response += "Echter heb ik info over geen enkele leraar kunnen vinden.\n";
			} else {
				response += "Echter heb ik alleen over sommige leraren info kunnen vinden.\n";
				response += resultResponse;
				response += "Hou er rekening mee dat ik niet weet of een leraar ziek is, of om een andere reden weg is.";
			}

			await ReplyAsync(response);
		}

		// This is a seperate function because two teachers have the same name. We would have to write this function three times in TeacherCurrentCommand().
		private string RespondTeacherCurrent(string teacher, ScheduleRecord record) {
			if (record == null) {
				string response = $"Het ziet ernaar uit dat {teacher} nu niets heeft.";
				if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
					response += " Het is dan ook weekend.";
				}
				return response;
			} else {
				string response = "";
				if (record.Activity == "pauze") {
					response += $"{teacher} heeft nu pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
				} else {
					response += $"{teacher} geeft nu {record.Activity}";

					if (!string.IsNullOrEmpty(record.StudentSets)) {
						response += $" aan {record.StudentSets}";
					}
					if (!string.IsNullOrEmpty(record.Room)) {
						response += $" in {record.Room}";
					} else {
						response += ", en ik weet niet waar";
					}
					response += $".\n{GetTimeSpanResponse(record)}";
				}
				return response;
			}
		}

		private string RespondTeacherNext(string teacher, ScheduleRecord record) {
			string response = "";
			if (record.Activity == "pauze") {
				response += $"{teacher} heeft {GetNextTimeString(record)} pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
			} else {
				response += $"{teacher} geeft {GetNextTimeString(record)} {record.Activity}";

				if (!string.IsNullOrEmpty(record.StudentSets)) {
					response += $" aan {record.StudentSets}";
				}
				if (!string.IsNullOrEmpty(record.Room)) {
					response += $" in {record.Room}";
				} else {
					response += ", en ik weet niet waar";
				}
				response += $".\n{GetTimeSpanResponse(record)}";
			}
			return response;
		}

		private async Task RespondTeacherMultiple(bool next, string ambiguousPart, params string[] teacherAbbrs) {
			string response = "";

			string[] teachers = new string[teacherAbbrs.Length];
			for (int i = 0; i < teacherAbbrs.Length; i++) {
				teachers[i] = GetTeacherNameFromAbbr(teacherAbbrs[i]);
			}

			if (teachers.Contains(null)) {
				await FatalError($"RespondTeacherMultiple: GetTeacherName({string.Join(",", teacherAbbrs)}) == null");
			}

			response += $"\"{ambiguousPart}\" kan {Util.FormatStringArray(teachers, ", of ")} zijn.\n\n";
			ReturnValue<ScheduleRecord>[] results = new ReturnValue<ScheduleRecord>[teachers.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = await GetRecord(next, "StaffMember", teacherAbbrs[i]);
			}

			Func<string, ScheduleRecord, string> respondFunction = next ? (Func<string, ScheduleRecord, string>) RespondTeacherNext : RespondTeacherCurrent;

			bool allSuccessful = true;
			string resultResponse = "";
			for (int i = 0; i < results.Length; i++) {
				if (results[i].Success) {
					resultResponse += respondFunction(teachers[i], results[i].Value) + "\n";
				} else {
					allSuccessful = false;
				}
			}

			if (allSuccessful) {
				response += resultResponse;
				response += "Hou er rekening mee dat ik niet weet of een leraar ziek is, of om een andere reden weg is.";
			} else if (string.IsNullOrEmpty(resultResponse)) {
				response += "Echter heb ik info over geen enkele leraar kunnen vinden.\n";
			} else {
				response += "Echter heb ik alleen over sommige leraren info kunnen vinden.\n";
				response += resultResponse;
				response += "Hou er rekening mee dat ik niet weet of een leraar ziek is, of om een andere reden weg is.";
			}

			await ReplyAsync(response);
		}
	}
}
