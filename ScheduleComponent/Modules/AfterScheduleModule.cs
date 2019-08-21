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
			ScheduleCommandInfo query = LSCService.GetLastCommandForContext(Context);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError("Na wat?");
			} else {
				ScheduleRecord nextRecord;
				try {
					if (query.Record == null) {
						nextRecord = Schedules.GetNextRecord(query.Identifier, Context);
					} else {
						nextRecord = Schedules.GetRecordAfter(query.Identifier, query.Record, Context);
					}
				} catch (RecordsOutdatedException) {
					await MinorError("Daarna heb ik nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
					return;
				} catch (IdentifierNotFoundException) {
					// This catch block scores 9 out of 10 on the "oh shit" scale
					// It should never happen and it indicates that something has really been messed up somewhere, but it's not quite bad enough for a 10.
					string report = $"daarna failed for query {query.Identifier.ScheduleField}:{query.Identifier}";
					if (query.Record == null) {
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

				string pretext;
				if (query.Record == null) {
					pretext = $"{query.Identifier.DisplayText}: Hierna";
				} else {
					pretext = $"{query.Identifier.DisplayText}: Na de vorige les";
				}

				// Avoid RespondRecord automatically calling this function again because we do it ourselves
				// We don't use RespondRecord's handling because we have our own recursion limit, which RespondRecord can't use
				await RespondRecord(pretext, query.Identifier, nextRecord, false);

				if (nextRecord.Activity == "pauze" && recursion <= 5) {
					await GetAfterCommandInternal(recursion + 1);
				}
			}
		}
	}
}
