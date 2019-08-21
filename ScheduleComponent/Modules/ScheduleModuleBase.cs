using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using RoosterBot;
using RoosterBot.Modules;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	public class ScheduleModuleBase : EditableCmdModuleBase {
		public LastScheduleCommandService LSCService { get; set; }
		public TeacherNameService Teachers { get; set; }
		public ScheduleProvider Schedules { get; set; }
		public UserClassesService Classes { get; set; }
		
		protected string TableItemActivity(ScheduleRecord record, bool isFirstRecord) {
			string ret = $":notepad_spiral: {ScheduleUtil.GetActivityFromAbbr(record.Activity)}";
			if (isFirstRecord && record.Activity == "pauze") {
				ret += " :thinking:";
			}
			return ret + "\n";
		}

		protected string TableItemStaffMember(ScheduleRecord record) {
			if (record.StaffMember.Length == 1 && record.StaffMember[0].IsUnknown) {
				return $":bust_in_silhouette: Onbekende leraar met afkorting {record.StaffMember[0].Abbreviation}\n";
			}

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
				ret += $":calendar_spiral: {ScheduleUtil.GetStringFromDayOfWeek(record.Start.DayOfWeek)} {record.Start.ToString("dd-MM-yyyy")}\n" + ret;
			}

			ret += $":clock5: {record.Start.ToString("HH:mm")} - {record.End.ToString("HH:mm")}";
			if (record.Start.Date == DateTime.Today && record.Start > DateTime.Now) {
				TimeSpan timeTillStart = record.Start - DateTime.Now;
				ret += $" - nog {timeTillStart.Hours}:{timeTillStart.Minutes.ToString().PadLeft(2, '0')}";
			}

			return ret + "\n";
		}

		protected string TableItemDuration(ScheduleRecord record) {
			string ret = $":stopwatch: {(int)record.Duration.TotalHours}:{record.Duration.Minutes.ToString().PadLeft(2, '0')}";
			if (record.Start < DateTime.Now && record.End > DateTime.Now) {
				TimeSpan timeLeft = record.End - DateTime.Now;
				ret += $" - nog {timeLeft.Hours}:{timeLeft.Minutes.ToString().PadLeft(2, '0')}";
			}
			return ret + "\n";
		}

		protected string TableItemBreak(ScheduleRecord record) {
			if (record.BreakStart.HasValue) {
				return $":coffee: {record.BreakStart.Value.ToString("HH:mm")} - {record.BreakEnd.Value.ToString("HH:mm")}\n";
			} else {
				return "";
			}
		}
		
		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		protected async Task<IUserMessage> ReplyAsync(string message, IdentifierInfo identifier, ScheduleRecord record, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret = await base.ReplyAsync(message, isTTS, embed, options);
			LSCService.OnRequestByUser(Context, identifier, record);
			return ret;
		}

		/// <summary>
		/// Posts a message in Context.Channel with the given text, and adds given schedule, identifier, and record to the LastScheduleCommandService for use in the !daarna command.
		/// </summary>
		protected void ReplyDeferred(string message, IdentifierInfo identifier, ScheduleRecord record) {
			base.ReplyDeferred(message);
			LSCService.OnRequestByUser(Context, identifier, record);
		}

		protected async override Task MinorError(string message) {
			await base.MinorError(message);
			LSCService.RemoveLastQuery(Context);
		}

		protected async override Task FatalError(string message, Exception exception = null) {
			await base.FatalError(message, exception);
			LSCService.RemoveLastQuery(Context);
		}

		protected async Task GetAfterCommand() {
			await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "daarna", Context.Message, "SMB daarna");
		}
		
		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(IdentifierInfo identifier) {
			if (ScheduleUtil.IsSummerBreak()) {
				await MinorError("Het is vakantie, man. Ga naar huis.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}

			return await HandleError(() => Schedules.GetCurrentRecord(identifier, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetNextRecord(IdentifierInfo identifier) {
			return await HandleError(() => Schedules.GetNextRecord(identifier, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord[]>> GetSchedulesForDay(IdentifierInfo identifier, DateTime date) {
			if (ScheduleUtil.IsSummerBreak(date)) {
				await MinorError("Het is vakantie, man. Ga naar huis.");
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			}

			return await HandleError(() => Schedules.GetSchedulesForDate(identifier, date, Context));
		}

		protected async Task<ReturnValue<AvailabilityInfo[]>> GetWeekAvailabilityInfo(IdentifierInfo identifier, int weeksFromNow) {
			return await HandleError(() => Schedules.GetWeekAvailability(identifier, weeksFromNow, Context));
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan span) {
			return await HandleError(() => Schedules.GetRecordAfterTimeSpan(identifier, span, Context));
		}

		protected async Task RespondRecord(string pretext, IdentifierInfo info, ScheduleRecord record, bool callNextIfBreak = true) {
			string response = pretext + "\n";
			response += TableItemActivity(record, false);

			if (record.Activity != "stdag doc") {
				if (record.Activity != "pauze") {
					if (info.ScheduleField != "StaffMember") {
						response += TableItemStaffMember(record);
					}
					if (info.ScheduleField != "StudentSets") {
						response += TableItemStudentSets(record);
					}
					if (info.ScheduleField != "Room") {
						response += TableItemRoom(record);
					}
				}

				response += TableItemStartEndTime(record);
				response += TableItemDuration(record);
				response += TableItemBreak(record);
			}
			ReplyDeferred(response, info, record);

			if (callNextIfBreak && record.Activity == "pauze") {
				await GetAfterCommand();
			}
		}

		private async Task<ReturnValue<T>> HandleError<T>(Func<T> action) {
			try {
				return new ReturnValue<T>() {
					Success = true,
					Value = action()
				};
			} catch (IdentifierNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar er staat nog niets op het rooster op dat moment.");
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (NoAllowedGuildsException) {
				await MinorError("Er zijn geen roosters beschikbaar voor deze server.");
				return new ReturnValue<T>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				throw;
			}
		}
	}
}
