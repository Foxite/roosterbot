using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using ScheduleComponent.Services;
using RoosterBot.Modules;
using RoosterBot;

namespace ScheduleComponent.Modules {
	[RoosterBot.Attributes.LogTag("ScheduleModuleBase")]
	public class ScheduleModuleBase<T> : EditableCmdModuleBase where T : IdentifierInfo {
		public LastScheduleCommandService LSCService { get; set; }
		public TeacherNameService Teachers { get; set; }
		public ScheduleService<T> Schedules { get; set; }
		public ScheduleProvider AllSchedules { get; set; }
		public UserClassesService Classes { get; set; }

		[Command("daarna", RunMode = RunMode.Async), Summary("Kijk wat er gebeurt na het laatste wat je hebt bekeken")]
		public async Task GetAfterCommand([Remainder] string ignored = "") {
			if (!string.IsNullOrWhiteSpace(ignored)) {
				ReplyDeferred("Hint: om !daarna te gebruiken hoef je geen parameters mee te geven.");
			}
			// This allows us to call !daarna automatically in certain conditions, and prevents the recursion from causing problems.
			await GetAfterCommandFunction();
		}

		protected async Task GetAfterCommandFunction(int recursion = 0) {
			ScheduleCommandInfo query = LSCService.GetLastCommandFromUser(Context.User);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError("Na wat?");
			} else {
				ScheduleRecord record = query.Record;
				string response;
				bool nullRecord = record == null;
				try {
					if (nullRecord) {
						record = AllSchedules.GetNextRecord(query.Identifier);
					} else {
						record = AllSchedules.GetRecordAfter(query.Identifier, query.Record);
					}
				} catch (RecordsOutdatedException) {
					await MinorError("Daarna heb ik nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
					return;
				} catch (ScheduleNotFoundException) {
					string report = $"daarna failed for query {query.Identifier.ScheduleField}:{query.Identifier}";
					if (nullRecord) {
						report += " with no record";
					} else {
						report += $" with record: {query.Record.ToString()}";
					}

					await FatalError(report);
					return;
				} catch (Exception ex) {
					await FatalError("Uncaught exception", ex);
					throw;
				}

				if (nullRecord) {
					response = $"{query.Identifier.DisplayText}: Hierna\n";
				} else {
					response = $"{query.Identifier.DisplayText}: Na de vorige les\n";
				}

				response += TableItemActivity(record, false);

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						if (query.Identifier.ScheduleField != "StaffMember") {
							response += TableItemStaffMember(record);
						}
						if (query.Identifier.ScheduleField != "StudentSets") {
							response += TableItemStudentSets(record);
						}
						if (query.Identifier.ScheduleField != "Room") {
							response += TableItemRoom(record);
						}
					}
					response += TableItemStartEndTime(record);
					response += TableItemDuration(record);
					response += TableItemBreak(record);
				}
				ReplyDeferred(response, query.Identifier, record);

				if (record.Activity == "pauze" && recursion <= 5) {
					await GetAfterCommandFunction(recursion + 1);
				}
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
			string teachers = string.Join(", ", record.StaffMember.Select(teacher => teacher.DisplayText));
			if (string.IsNullOrWhiteSpace(teachers)) {
				return "";
			} else {
				if (record.StaffMember.Length == 1 && record.StaffMember[0].Abbreviation == "JWO") {
					return $"<:VRjoram:392762653367336960> {teachers}\n";
				} else {
					return $":bust_in_silhouette: {teachers}\n";
				}
			}
		}

		protected string TableItemStudentSets(ScheduleRecord record) {
			if (!string.IsNullOrWhiteSpace(record.StudentSetsString)) {
				return $":busts_in_silhouette: {record.StudentSetsString}\n";
			} else {
				return "";
			}
		}
		
		protected string TableItemRoom(ScheduleRecord record) {
			if (!string.IsNullOrWhiteSpace(record.RoomString)) {
				return $":round_pushpin: {record.RoomString}\n";
			} else {
				return "";
			}
		}

		protected string TableItemStartEndTime(ScheduleRecord record) {
			string ret = "";

			if (record.Start.Date != DateTime.Today) {
				ret += $":calendar_spiral: {Util.GetStringFromDayOfWeek(record.Start.DayOfWeek)} {record.Start.ToString("dd-MM-yyyy")}\n" + ret;
			}

			ret += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}";
			if (record.Start.Date == DateTime.Today && record.Start > DateTime.Now) {
				TimeSpan timeTillStart = record.Start - DateTime.Now;
				ret += $" - nog {timeTillStart.Hours}:{timeTillStart.Minutes.ToString().PadLeft(2, '0')}";
			}

			return ret + "\n";
		}

		protected string TableItemDuration(ScheduleRecord record) {
			string ret = $":stopwatch: {(int) record.Duration.TotalHours}:{record.Duration.Minutes.ToString().PadLeft(2, '0')}";
			if (record.Start < DateTime.Now && record.End > DateTime.Now) {
				TimeSpan timeLeft = record.End - DateTime.Now;
				ret += $" - nog {timeLeft.Hours}:{timeLeft.Minutes.ToString().PadLeft(2, '0')}";
			}
			return ret + "\n";
		}

		protected string TableItemBreak(ScheduleRecord record) {
			if (record.BreakStart.HasValue) {
				return $":coffee: {record.BreakStart.Value.ToShortTimeString()} - {record.BreakEnd.Value.ToShortTimeString()}\n";
			} else {
				return "";
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(bool next, T identifier) {
			ScheduleRecord record = null;
			try {
				record = next ? Schedules.GetNextRecord(identifier) : Schedules.GetCurrentRecord(identifier);
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
				await FatalError("Uncaught exception", ex);
				throw;
			}
		}

		protected async Task<ReturnValue<ScheduleRecord[]>> GetSchedulesForDay(T identifier, DayOfWeek day, bool includeToday) {
			try {
				ScheduleRecord[] records = Schedules.GetSchedulesForDay(identifier, day, includeToday);
				return new ReturnValue<ScheduleRecord[]>() {
					Success = true,
					Value = records
				};
			} catch (ScheduleNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				throw;
			}
		}

		/// <summary>
		/// Given two command arguments, this determines which is a DayOfWeek and which is not.
		/// </summary>
		/// <returns>bool: Success, DayOfWeek: One of the arguments as DOW, string: the other argument as received, bool: wether "vandaag" was used as weekday</returns>
		protected async Task<Tuple<bool, DayOfWeek, string, bool>> GetValuesFromArguments(string arguments) {
			DayOfWeek day;
			string entry;
			string[] argumentWords = arguments.Split(' ');

			if (argumentWords.Length < 2) {
				await MinorError("Ik minstens twee woorden nodig.");
				return new Tuple<bool, DayOfWeek, string, bool>(false, DayOfWeek.Monday, "", false);
			}

			bool today = false;
			try {
				day = Util.GetDayOfWeekFromString(argumentWords[0]);
				entry = string.Join(" ", argumentWords, 1, argumentWords.Length - 1); // get everything except first
				today = argumentWords[0].ToLower() == "vandaag";
			} catch (ArgumentException) {
				try {
					day = Util.GetDayOfWeekFromString(argumentWords[argumentWords.Length - 1]);
					entry = string.Join(" ", argumentWords, 0, argumentWords.Length - 1); // get everything except last
					today = argumentWords[argumentWords.Length - 1].ToLower() == "vandaag";
				} catch (ArgumentException) {
					await MinorError($"Ik weet niet welk deel van \"" + arguments + "\" een dag is.");
					return new Tuple<bool, DayOfWeek, string, bool>(false, DayOfWeek.Monday, "", false);
				}
			}
			return new Tuple<bool, DayOfWeek, string, bool>(true, day, entry, today);
		}

		protected async Task<T> ResolveMeQuery(string input) {
			if (input == "ik") {
				return (T) (IdentifierInfo) await Classes.GetClassForDiscordUser(Context.User); // Ugly as fuck but this should never happen if it won't work
			} else {
				return null;
			}
		}
		
		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		protected async Task<IUserMessage> ReplyAsync(string message, IdentifierInfo identifier, ScheduleRecord record, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret = await base.ReplyAsync(message, isTTS, embed, options);
			LSCService.OnRequestByUser(Context.User, identifier, record);
			return ret;
		}
		
		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		protected void ReplyDeferred(string message, IdentifierInfo identifier, ScheduleRecord record, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			base.ReplyDeferred(message);
			LSCService.OnRequestByUser(Context.User, identifier, record);
		}

		protected async override Task MinorError(string message) {
			await base.MinorError(message);
			LSCService.RemoveLastQuery(Context.User);
		}

		protected async override Task FatalError(string message, Exception exception = null) {
			await base.FatalError(message, exception);
			LSCService.RemoveLastQuery(Context.User);
		}
	}
}
