﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using ScheduleComponent.Services;
using RoosterBot.Modules;
using RoosterBot;
using RoosterBot.Attributes;

namespace ScheduleComponent.Modules {
	public class ScheduleModuleBase : EditableCmdModuleBase {
		public LastScheduleCommandService LSCService { get; set; }
		public TeacherNameService Teachers { get; set; }
		public ScheduleProvider AllSchedules { get; set; }
		public UserClassesService Classes { get; set; }
		
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

		protected async Task GetAfterCommand() {
			await Program.Instance.ExecuteSpecificCommand(null, "!daarna", Context.Message);
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(bool next, IdentifierInfo identifier) {
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

		protected async Task<ReturnValue<ScheduleRecord[]>> GetSchedulesForDay(IdentifierInfo identifier, DayOfWeek day, bool includeToday) {
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
	}
}
