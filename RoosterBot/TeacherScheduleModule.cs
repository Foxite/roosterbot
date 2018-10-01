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
					await ReplyAsync(await RespondTeacherMultiple(false, leraar, teacher.Split(new[] { ", " }, StringSplitOptions.None)));
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
					await ReplyAsync(await RespondTeacherMultiple(true, leraar, teacher.Split(new[] { ", " }, StringSplitOptions.None)));
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


			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(leraar, weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string teacherGiven = arguments.Item3;
				string teacher = GetTeacherAbbrFromName(arguments.Item3);

				if (teacher == null) {
					await ReactMinorError();
					await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
				} else {
					if (teacher.Contains(", ")) {
						await ReplyAsync(await RespondTeacherWeekdayMultiple(day, teacherGiven, teacher.Split(new[] { ", " }, StringSplitOptions.None)));
					} else {
						ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "StaffMember", teacher);
						if (result.Success) {
							ScheduleRecord record = result.Value;
							await ReplyAsync(RespondTeacherWeekday(GetTeacherNameFromAbbr(teacher), day, record));
						}
					}
				}
			}
		}
		
		// This is a seperate function because two teachers have the same name. We would have to write this function three times in TeacherCurrentCommand().
		private string RespondTeacherCurrent(string teacher, ScheduleRecord record) {
			string response;
			if (record == null) {
				response = $"Het lijkt erop dat {teacher} nu niets heeft.";
				if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
					response += " Het is dan ook weekend.";
				}
			} else {
				response = $"{teacher}: Nu\n";
				response += $":notepad_spiral: {record.Activity}\n";

				if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
					response += $":busts_in_silhouette: {record.StudentSets}\n";
				}
				response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
				response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
				response += $":stopwatch: {record.Duration}\n";
			}
			return response;
		}

		private string RespondTeacherNext(string teacher, ScheduleRecord record) {
			string response = $"{teacher}: Hierna\n";
			response += $":notepad_spiral: {record.Activity}\n";

			if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
				response += $":busts_in_silhouette: {record.StudentSets}\n";
			}
			response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
			response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
			response += $":stopwatch: {record.Duration}\n";
			return response;
		}

		private string RespondTeacherWeekday(string teacher, DayOfWeek day, ScheduleRecord record) {
			string response;
			if (record == null) {
				response = $"Het lijkt er op dat {teacher} op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets heeft.";
				if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
					response += "\nDat is dan ook in het weekend.";
				}
			} else {
				response = $"{teacher}: Als eerste op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)}\n";
				response += $":notepad_spiral: {record.Activity}\n";

				if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
					response += $":busts_in_silhouette: {record.StudentSets}\n";
				}
				response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
				response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
				response += $":stopwatch: {record.Duration}";
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
