using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	[Group("leraar")]
	public class TeacherScheduleModule : ScheduleModuleBase {
		private string LogTag { get; }

		public TeacherScheduleModule() : base() {
			LogTag = "TSM";
		}

		[Command("nu", RunMode = RunMode.Async), Summary("Waar een leraar nu mee bezig is")]
		public async Task TeacherCurrentCommand([Remainder] string leraar) {
			if (!await CheckCooldown())
				return;

			string teacher = GetTeacherAbbrFromName(leraar);

			if (teacher == null) {
				await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher.Contains(", ")) { // There are multiple
					await ReplyAsync(await RespondTeacherMultiple(false, leraar, teacher.Split(new[] { ", " }, StringSplitOptions.None)));
					ARS.RemoveLastQuery(Context.User);
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(false, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						await ReplyAsync(RespondTeacherCurrent(GetTeacherNameFromAbbr(teacher), record), "StaffMember", teacher, record);
					}
				}
			}
		}
		
		[Command("hierna", RunMode = RunMode.Async), Summary("Waar een leraar hierna mee bezig is")]
		public async Task TeacherNextCommand([Remainder] string leraar) {
			if (!await CheckCooldown())
				return;

			string teacher = GetTeacherAbbrFromName(leraar);
			if (teacher == null) {
				await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher.Contains(", ")) { // There are multiple
					await ReplyAsync(await RespondTeacherMultiple(true, leraar, teacher.Split(new[] { ", " }, StringSplitOptions.None)));
					ARS.RemoveLastQuery(Context.User);
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(true, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						if (record != null) {
							await ReplyAsync(RespondTeacherNext(GetTeacherNameFromAbbr(teacher), record).FirstCharToUpper(), "StaffMember", teacher, record);
						} else {
							await FatalError("GetRecord(TS1)==null)");
						}
					}
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Summary("Welke les een leraar als eerste heeft op een dag")]
		public async Task TeacherWeekdayCommand([Remainder] string leraar_en_weekdag) {
			if (!await CheckCooldown())
				return;
			
			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(leraar_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string teacherGiven = arguments.Item3;
				string teacher = GetTeacherAbbrFromName(arguments.Item3);

				if (teacher == null) {
					await MinorError("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
				} else {
					if (teacher.Contains(", ")) {
						await ReplyAsync(await RespondTeacherWeekdayMultiple(day, teacherGiven, teacher.Split(new[] { ", " }, StringSplitOptions.None)));
						ARS.RemoveLastQuery(Context.User);
					} else {
						ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "StaffMember", teacher);
						if (result.Success) {
							ScheduleRecord record = result.Value;
							await ReplyAsync(RespondTeacherWeekday(GetTeacherNameFromAbbr(teacher), day, record), "StaffMember", teacher, record);
						}
					}
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("Welke les een leraar morgen als eerste heeft")]
		public async Task StudentTomorrowCommand([Remainder] string leraar) {
			await TeacherWeekdayCommand(leraar + " " + GetStringFromDayOfWeek(DateTime.Today.AddDays(1).DayOfWeek));
		}

		// This is a seperate function because two teachers have the same name. We would have to write this function three times in TeacherCurrentCommand().
		private string RespondTeacherCurrent(string teacher, ScheduleRecord record) {
			string response;
			if (record == null) {
				response = $"Het lijkt erop dat {teacher} nu niets heeft.";
			} else {
				response = $"{teacher}: Nu\n";
				response += $":notepad_spiral: {GetActivityFromAbbr(record.Activity)}\n";

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						if (!string.IsNullOrWhiteSpace(record.Room)) {
							response += $":round_pushpin: {record.Room}\n";
						}
						if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
							response += $":busts_in_silhouette: {record.StudentSets}\n";
						}
					}
					response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
					TimeSpan timeLeft = record.End - DateTime.Now;
					response += $":stopwatch: {record.Duration} - nog {timeLeft.Hours}:{timeLeft.Minutes}\n";
				}
			}
			return response;
		}

		private string RespondTeacherNext(string teacher, ScheduleRecord record) {
			string response = $"{teacher}: Hierna\n";
			response += $":notepad_spiral: {GetActivityFromAbbr(record.Activity)}\n";

			if (record.Activity != "stdag doc") {
				if (record.Activity != "pauze") {
					if (!string.IsNullOrWhiteSpace(record.Room)) {
						response += $":round_pushpin: {record.Room}\n";
					}
					if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
						response += $":busts_in_silhouette: {record.StudentSets}\n";
					}
					if (record.Start.Date != DateTime.Today) {
						response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
					}
				}
				response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
				response += $":stopwatch: {record.Duration}\n";
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
				response += $":notepad_spiral: {GetActivityFromAbbr(record.Activity)}";
				if (record.Activity == "pauze")
					response += " :thinking:";
				response += "\n";

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						if (!string.IsNullOrWhiteSpace(record.Room)) {
							response += $":round_pushpin: {record.Room}\n";
						}
						if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
							response += $":busts_in_silhouette: {record.StudentSets}\n";
						}
					}
					response += $":calendar_spiral: {record.Start.ToShortDateString()}\n";
					response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
					response += $":stopwatch: {record.Duration}";
				}
			}
			return response;
		}

		private async Task<string> RespondTeacherMultiple(bool next, string ambiguousPart, params string[] teacherAbbrs) {
			string response = "";

			string[] teachers = new string[teacherAbbrs.Length];
			for (int i = 0; i < teacherAbbrs.Length; i++) {
				teachers[i] = GetTeacherNameFromAbbr(teacherAbbrs[i]);
			}

			if (teachers.Contains(null)) {
				await FatalError($"RespondTeacherMultiple: GetTeacherName({string.Join(",", teacherAbbrs)}) == null");
			}
			
			ReturnValue<ScheduleRecord>[] results = new ReturnValue<ScheduleRecord>[teachers.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = await GetRecord(next, "StaffMember", teacherAbbrs[i]);
			}

			Func<string, ScheduleRecord, string> respondFunction = next ? (Func<string, ScheduleRecord, string>) RespondTeacherNext : RespondTeacherCurrent;
			
			for (int i = 0; i < results.Length; i++) {
				if (results[i].Success) {
					response += respondFunction(teachers[i], results[i].Value) + "\n\n";
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
				teachers[i] = GetTeacherNameFromAbbr(teacherAbbrs[i]);
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
