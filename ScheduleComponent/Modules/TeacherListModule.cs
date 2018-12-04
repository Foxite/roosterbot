using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Modules;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[RoosterBot.Attributes.LogTag("TeacherListModule")]
	public class TeacherListModule : EditableCmdModuleBase {
		public TeacherNameService Teachers { get; set; }
		
		[Command("docenten", RunMode = RunMode.Async), Alias("leraren", "docent", "leraar")]
		public async Task TeacherListCommand([Remainder] string name = "") {
			if (!await CheckCooldown())
				return;

			IReadOnlyList<TeacherRecord> records;

			if (string.IsNullOrWhiteSpace(name)) {
				records = Teachers.GetAllRecords();
			} else {
				records = Teachers.GetRecordsFromNameInput(name);
			}

			if (records.Count == 0) {
				await ReplyAsync("Geen leraren gevonden.");
			} else {
				// A foreach loop is faster than a for loop if you have to use the item more than once.
				// https://www.dotnetperls.com/for-foreach
				// Because of NoLookup, we don't actually know how many times we use the item, but in practice, we almost always have to use it twice and not once.
				string fullNameHeader = "Volledige naam";
				int maxNameLength = fullNameHeader.Length;
				foreach (TeacherRecord record in records) {
					if (record.NoLookup) {
						continue;
					}
					maxNameLength = Math.Max(maxNameLength, record.FullName.Length);
				}

				string response = $"`{fullNameHeader.PadRight(maxNameLength)}  Afk. Discord naam";
				foreach (TeacherRecord record in records) {
					if (record.NoLookup) {
						continue;
					}
					response += $"\n{record.FullName.PadRight(maxNameLength)}  {record.Abbreviation}  {record.DiscordUser}";
				}
				response += "`";

				await ReplyAsync(response);
			}
		}
	}
}
