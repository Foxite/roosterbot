using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	public class TeacherScheduleModule : ScheduleModuleBase {
		public TeacherScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "TSM") { }

		[Command("leraarnu", RunMode = RunMode.Async)]
		public async Task TeacherCurrentCommand([Remainder] string teacherInput) {
			if (!await CheckCooldown())
				return;

			string teacher = GetTeacherAbbrFromName(teacherInput);

			if (teacher == null) {
				await Context.Message.AddReactionAsync(new Emoji("❌"));
				await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher.Contains(", ")) { // There are multiple
					await RespondTeacherMultiple(false, teacherInput, teacher.Split(new[] { ", " }, StringSplitOptions.None));
				} else {
					ReturnValue<ScheduleRecord> result = await GetRecord(false, "StaffMember", teacher);
					if (result.Success) {
						ScheduleRecord record = result.Value;
						await ReplyAsync(RespondTeacherCurrent(GetTeacherNameFromAbbr(teacher), record));
					}
				}
			}
		}
		
		[Command("leraarhierna", RunMode = RunMode.Async)]
		public async Task TeacherNextCommand([Remainder] string teacherInput) {
			if (!await CheckCooldown())
				return;

			string teacher = GetTeacherAbbrFromName(teacherInput);
			if (teacher == null) {
				await ReactMinorError();
				await ReplyAsync("Is dat wel een leraar? :thinking: Als hij of zij nieuw is, moet hij worden toegevoegd door de bot eigenaar.");
			} else {
				if (teacher.Contains(", ")) { // There are multiple
					await RespondTeacherMultiple(true, teacherInput, teacher.Split(new[] { ", " }, StringSplitOptions.None));
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

		// This is a seperate function because we two teachers have the same name. We would have to write this function three times in TeacherCurrentCommand().
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
				response += $"{teacher} heeft {GetTomorrowOrNext(record)} pauze van {record.Start.ToShortTimeString()} tot {record.End.ToShortTimeString()}.";
			} else {
				response += $"{teacher} geeft {GetTomorrowOrNext(record)} {record.Activity}";

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
}
