﻿using System;
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
			await Program.Instance.ExecuteSpecificCommand(Context.OriginalResponse, "daarna", Context.Message);
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(bool next, IdentifierInfo identifier) {
			if (!next && ScheduleUtil.IsSummerBreak()) {
				await MinorError("Het is vakantie, man. Ga naar huis.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}

			try {
				ScheduleRecord record = next ? Schedules.GetNextRecord(identifier, Context) : Schedules.GetCurrentRecord(identifier, Context);
				return new ReturnValue<ScheduleRecord>() {
					Success = true,
					Value = record
				};
			} catch (IdentifierNotFoundException) { // TODO I smell repetition, get it out of here.
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar er staat nog niets op het rooster op dat moment.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (NoSchedulesAvailableException) {
				await MinorError("Er zijn geen roosters beschikbaar voor deze server.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				throw;
			}
		}

		protected async Task<ReturnValue<ScheduleRecord[]>> GetSchedulesForDay(IdentifierInfo identifier, DateTime date) {
			if (ScheduleUtil.IsSummerBreak(date)) {
				await MinorError("Het is vakantie, man. Ga naar huis.");
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			}

			try {
				ScheduleRecord[] records = Schedules.GetSchedulesForDate(identifier, date, Context);
				return new ReturnValue<ScheduleRecord[]>() {
					Success = true,
					Value = records
				};
			} catch (IdentifierNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar er staat nog niets op het rooster op dat moment.");
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			} catch (NoSchedulesAvailableException) {
				await MinorError("Er zijn geen roosters beschikbaar voor deze server.");
				return new ReturnValue<ScheduleRecord[]>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				throw;
			}
		}

		protected async Task<ReturnValue<AvailabilityInfo[]>> GetWeekAvailabilityInfo(IdentifierInfo identifier, int weeksFromNow) {
			try {
				AvailabilityInfo[] records = Schedules.GetWeekAvailability(identifier, weeksFromNow, Context);
				return new ReturnValue<AvailabilityInfo[]>() {
					Success = true,
					Value = records
				};
			} catch (IdentifierNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<AvailabilityInfo[]>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar er staat nog niets op het rooster op dat moment.");
				return new ReturnValue<AvailabilityInfo[]>() {
					Success = false
				};
			} catch (NoSchedulesAvailableException) {
				await MinorError("Er zijn geen roosters beschikbaar voor deze server.");
				return new ReturnValue<AvailabilityInfo[]>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				throw;
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecordAfterTimeSpan(IdentifierInfo identifier, TimeSpan span) {
			try {
				ScheduleRecord record = Schedules.GetRecordAfterTimeSpan(identifier, span, Context);
				return new ReturnValue<ScheduleRecord>() {
					Success = true,
					Value = record
				};
			} catch (IdentifierNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar er staat nog niets op het rooster op dat moment.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (NoSchedulesAvailableException) {
				await MinorError("Er zijn geen roosters beschikbaar voor deze server.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError("Uncaught exception", ex);
				throw;
			}
		}

		protected async Task RespondRecord(IdentifierInfo info, ScheduleRecord record) {
			string response = "";
			response += TableItemActivity(record, false);

			if (record.Activity != "stdag doc") {
				if (record.Activity != "pauze") {
					response += TableItemStaffMember(record);
					response += TableItemRoom(record);
				}

				response += TableItemStartEndTime(record);
				response += TableItemDuration(record);
				response += TableItemBreak(record);
			}
			ReplyDeferred(response, info, record);

			if (record.Activity == "pauze") {
				await GetAfterCommand();
			}
		}
	}
}
