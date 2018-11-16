using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ScheduleComponent.Services;
using RoosterBot.Modules;
using RoosterBot;

namespace ScheduleComponent.Modules {
	[RoosterBot.Attributes.LogTag("SMB")]
	public class ScheduleModuleBase : EditableCmdModuleBase {
		public LastScheduleCommandService LSCService { get; set; }
		public TeacherNameService Teachers { get; set; }
		public ScheduleService Schedules { get; set; }
		
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
						record = Schedules.GetRecordAfter(query.SourceSchedule, query.Identifier, query.Record);
					}
				} catch (ScheduleNotFoundException) {
					await MinorError("Dat item staat niet op mijn rooster.");
					return;
				} catch (RecordsOutdatedException) {
					await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
					return;
				} catch (Exception ex) {
					await FatalError($"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
					throw;
				}

				if (query.SourceSchedule == "StudentSets") {
					if (nullRecord) {
						response = $"{query.Identifier}: Hierna\n";
					} else {
						response = $"{query.Identifier}: Na de vorige les\n";
					}
				} else if (query.SourceSchedule == "StaffMember") {
					if (nullRecord) {
						response = $"{Teachers.GetFullNameFromAbbr(query.Identifier)}: Hierna\n";
					} else {
						response = $"{Teachers.GetFullNameFromAbbr(query.Identifier)}: Na de vorige les\n";
					}
				} else if (query.SourceSchedule == "Room") {
					if (nullRecord) {
						response = $"{query.Identifier}: Hierna\n";
					} else {
						response = $"{query.Identifier}: Na de vorige les\n";
					}
				} else {
					await FatalError("query.SourceSchedule is not recognized");
					return;
				}
				response += TableItemActivity(record, false);

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						string teachers = Teachers.GetFullNameFromAbbr(record.StaffMember);
						if (query.SourceSchedule != "StaffMember") {
							response += TableItemStaffMember(record);
						}
						if (query.SourceSchedule != "StudentSets") {
							response += TableItemStudentSets(record);
						}
						if (query.SourceSchedule != "Room") {
							response += TableItemRoom(record);
						}
					}
					bool isToday = record.Start.Date == DateTime.Today;
					response += TableItemStartEndTime(record);
					response += TableItemDuration(record);
				}
				await ReplyAsync(response, query.SourceSchedule, query.Identifier, record);
			}
		}

		protected string TableItemActivity(ScheduleRecord record, bool isFirstRecord) {
			string ret = $":notepad_spiral: {Util.GetActivityFromAbbr(record.Activity)}";
			if (isFirstRecord && record.Activity == "pauze") {
				ret += " :thinking:";
			}
			return ret + "\n";
		}
		
		protected string TableItemStaffMember(ScheduleRecord record) {
			string teachers = GetTeacherFullNamesFromAbbrs(record.StaffMember);
			if (string.IsNullOrWhiteSpace(teachers)) {
				return "";
			} else {
				if (record.StaffMember == "JWO") {
					return $"<:VRjoram:392762653367336960> {teachers}\n";
				} else {
					return $":bust_in_silhouette: {teachers}\n";
				}
			}
		}

		protected string TableItemStudentSets(ScheduleRecord record) {
			if (!string.IsNullOrWhiteSpace(record.StudentSets)) {
				return $":busts_in_silhouette: {record.StudentSets}\n";
			} else {
				return "";
			}
		}
		
		protected string TableItemRoom(ScheduleRecord record) {
			if (!string.IsNullOrWhiteSpace(record.Room)) {
				return $":round_pushpin: {record.Room}\n";
			} else {
				return "";
			}
		}

		protected string TableItemStartEndTime(ScheduleRecord record) {
			string ret = "";

			if (record.Start.Date != DateTime.Today) {
				ret += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n" + ret;
			}

			ret += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}";
			if (record.Start.Date == DateTime.Today && record.Start > DateTime.Now) {
				TimeSpan timeTillStart = record.Start - DateTime.Now;
				ret += $" - nog {timeTillStart.Hours}:{timeTillStart.Minutes.ToString().PadLeft(2, '0')}";
			}

			return ret + "\n";
		}

		protected string TableItemDuration(ScheduleRecord record) {
			string ret = $":stopwatch: {record.Duration}";
			if (record.Start < DateTime.Now && record.End > DateTime.Now) {
				TimeSpan timeLeft = record.End - DateTime.Now;
				ret += $" - nog {timeLeft.Hours}:{timeLeft.Minutes.ToString().PadLeft(2, '0')}";
			}
			return ret + "\n";
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
				await FatalError($"{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
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

		protected string GetTeacherFullNamesFromAbbrs(string abbrs) {
			return Util.FormatStringArray(Teachers.GetRecordsFromAbbrs(abbrs.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries)).Select(each => each.FullName).ToArray(), ", en ");
		}

		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		protected async Task<IUserMessage> ReplyAsync(string message, string schedule, string identifier, ScheduleRecord record, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret = await base.ReplyAsync(message, isTTS, embed, options);
			if (!(Context.User is IGuildUser user))
				return ret;
			
			LSCService.OnRequestByUser(user, schedule, identifier, record);
			return ret;
		}

		protected async override Task MinorError(string message) {
			await base.MinorError(message);
			LSCService.RemoveLastQuery(Context.User);
		}

		protected async override Task FatalError(string message) {
			await base.FatalError(message);
			LSCService.RemoveLastQuery(Context.User);
		}
	}
}
