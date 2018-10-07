using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using RoosterBot.Services;

namespace RoosterBot.Modules {
	public class ScheduleModuleBase : EditableCmdModuleBase {
		public ScheduleModuleBase() : base() {
			LogTag = "SMB";
		}

		[Command("daarna", RunMode = RunMode.Async), Summary("Kijk wat er gebeurt na het laatste wat je hebt bekeken")]
		public async Task GetAfterCommand() {
			if (!(Context.User is IGuildUser user))
				return;

			if (!await CheckCooldown())
				return;

			ScheduleCommandInfo query = LSCService.GetLastCommandFromUser(user);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError("Na wat?");
			} else {
				ScheduleRecord record = query.Record;
				string response;
				bool nullRecord = record == null;
				try {
					if (nullRecord) {
						record = Schedules.GetNextRecord(query.SourceSchedule, query.Identifier);
					} else {
						record = Schedules.GetRecordAfter(query.SourceSchedule, query.Record);
					}
				} catch (ScheduleNotFoundException) {
					await MinorError("Dat item staat niet op mijn rooster.");
					return;
				} catch (RecordsOutdatedException) {
					await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
					return;
				} catch (Exception ex) {
					await FatalError(ex.GetType().Name);
					throw;
				}

				if (query.SourceSchedule == "StudentSets") {
					if (nullRecord) {
						response = $"{record.StudentSets}: Hierna\n";
					} else {
						response = $"{record.StudentSets}: Na de vorige les\n";
					}
				} else if (query.SourceSchedule == "StaffMember") {
					if (nullRecord) {
						response = $"{Util.GetTeacherNameFromAbbr(record.StaffMember)}: Hierna\n";
					} else {
						response = $"{Util.GetTeacherNameFromAbbr(record.StaffMember)}: Na de vorige les\n";
					}
				} else if (query.SourceSchedule == "Room") {
					if (nullRecord) {
						response = $"{record.Room}: Hierna\n";
					} else {
						response = $"{record.Room}: Na de vorige les\n";
					}
				} else {
					await FatalError("query.SourceSchedule is not recognized");
					return;
				}
				response += $":notepad_spiral: {Util.GetActivityFromAbbr(record.Activity)}\n";

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						string teachers = Util.GetTeacherNameFromAbbr(record.StaffMember);
						if (query.SourceSchedule != "StaffMember" && !string.IsNullOrWhiteSpace(teachers)) {
							if (record.StaffMember == "JWO" && Util.RNG.NextDouble() < 0.1) {
								response += $"<:VRjoram:392762653367336960> {teachers}\n";
							} else {
								response += $":bust_in_silhouette: {teachers}\n";
							}
						}
						if (query.SourceSchedule != "StudentSets" && !string.IsNullOrWhiteSpace(record.StudentSets)) {
							response += $":busts_in_silhouette: {record.StudentSets}\n";
						}
						if (query.SourceSchedule != "Room" && !string.IsNullOrWhiteSpace(record.Room)) {
							response += $":round_pushpin: {record.Room}\n";
						}
					}
					response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
					response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
					response += $":stopwatch: {record.Duration}\n";
				}
				await ReplyAsync(response, query.SourceSchedule, query.Identifier, record);
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(bool next, string schedule, string name) {
			if (name == "") {
				await MinorError("Dat item staat niet op mijn rooster (of eigenlijk wel, maar niet op een zinvolle manier).");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}
			if (schedule == "Room" && name.Length != 4) {
				await MinorError("Dat is geen lokaal.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}

			name = name.ToUpper();
			ScheduleRecord record = null;
			try {
				record = next ? Schedules.GetNextRecord(schedule, name) : Schedules.GetCurrentRecord(schedule, name);
				return new ReturnValue<ScheduleRecord>() {
					Success = true,
					Value = record
				};
			} catch (ScheduleNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError(ex.GetType().Name);
				throw;
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetFirstRecord(DayOfWeek day, string schedule, string name) {
			if (name == "") {
				await MinorError("Dat item staat niet op mijn rooster (of eigenlijk wel, maar niet op een zinvolle manier).");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}
			if (schedule == "Room" && name.Length != 4) {
				await MinorError("Dat is geen lokaal.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}

			name = name.ToUpper();
			ScheduleRecord record = null;
			try {
				record = Schedules.GetFirstRecordForDay(schedule, name, day);
				return new ReturnValue<ScheduleRecord>() {
					Success = true,
					Value = record
				};
			} catch (ScheduleNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError(ex.GetType().Name);
				throw;
			}
		}
		
		/// <summary>
		/// Given two command arguments, this determines which is a DayOfWeek and which is not.
		/// </summary>
		/// <returns>bool: Success, DayOfWeek: One of the arguments as DOW, string: the other argument as received</returns>
		protected async Task<Tuple<bool, DayOfWeek, string>> GetValuesFromArguments(string arguments) {
			DayOfWeek day;
			string entry;
			string[] argumentWords = arguments.Split(' ');

			if (argumentWords.Length < 2) {
				await MinorError("Ik minstens twee woorden nodig.");
				return new Tuple<bool, DayOfWeek, string>(false, default, "");
			}

			try {
				day = Util.GetDayOfWeekFromString(argumentWords[0]);
				entry = string.Join(" ", argumentWords, 1, argumentWords.Length - 1); // get everything except first
			} catch (ArgumentException) {
				try {
					day = Util.GetDayOfWeekFromString(argumentWords[argumentWords.Length - 1]);
					entry = string.Join(" ", argumentWords, 0, argumentWords.Length - 1); // get everything except last
				} catch (ArgumentException) {
					await MinorError($"Ik weet niet welk deel van \"" + arguments + "\" een dag is.");
					return new Tuple<bool, DayOfWeek, string>(false, default, "");
				}
			}
			return new Tuple<bool, DayOfWeek, string>(true, day, entry);
		}

		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		public async Task<IUserMessage> ReplyAsync(string message, string schedule, string identifier, ScheduleRecord record, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret = await base.ReplyAsync(message, isTTS, embed, options);
			if (!(Context.User is IGuildUser user))
				return ret;
			
			LSCService.OnRequestByUser(user, schedule, identifier, record);
			return ret;
		}
	}
}
