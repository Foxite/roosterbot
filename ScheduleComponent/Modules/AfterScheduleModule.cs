using System;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Attributes;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("AfterScheduleModule"), HiddenFromList]
	public class AfterScheduleModule : ScheduleModuleBase {
		[Command("daarna", RunMode = RunMode.Async)]
		public async Task GetAfterCommand([Remainder] string ignored = "") {
			if (!string.IsNullOrWhiteSpace(ignored)) {
				ReplyDeferred("Hint: om !daarna te gebruiken hoef je geen parameters mee te geven.");
			}
			// This allows us to call !daarna automatically in certain conditions, and prevents the recursion from causing problems.
			await GetAfterCommandInternal();
		}

		protected async Task GetAfterCommandInternal(int recursion = 0) {
			ScheduleCommandInfo query = LSCService.GetLastCommandFromUser(Context.User);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError("Na wat?");
			} else {
				ScheduleRecord record = query.Record;
				string response;
				bool nullRecord = record == null;
				try {
					if (nullRecord) {
						record = Schedules.GetNextRecord(query.Identifier);
					} else {
						record = Schedules.GetRecordAfter(query.Identifier, query.Record);
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
					await GetAfterCommandInternal(recursion + 1);
				}
			}
		}
	}
}
