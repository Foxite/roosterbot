using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("TeacherListModule"), Name("Leraren")]
	public class TeacherListModule : EditableCmdModuleBase {
		public TeacherNameService Teachers { get; set; }
		
		[Command("leraren", RunMode = RunMode.Async), Alias("docenten", "docent"), Summary("Een lijst van alle leraren, hun afkortingen, en hun Discord namen (als die bekend is)")]
		public async Task TeacherListCommand([Remainder, Name("naam")] string name = "") {
			IReadOnlyList<TeacherInfo> records;

			if (string.IsNullOrWhiteSpace(name)) {
				records = Teachers.GetAllRecords();
			} else {
				records = Teachers.Lookup(name);
			}

			if (records.Count == 0) {
				await ReplyAsync("Geen leraren gevonden.");
			} else {
				// A foreach loop is faster than a for loop if you have to use the item more than once.
				// https://www.dotnetperls.com/for-foreach
				// Because of NoLookup, we don't actually know how many times we use the item, but in practice, we almost always have to use it twice and not once.
				string fullNameHeader = "Volledige naam";
				int maxNameLength = fullNameHeader.Length;
				foreach (TeacherInfo record in records) {
					if (record.NoLookup) {
						continue;
					}
					maxNameLength = Math.Max(maxNameLength, record.FullName.Length);
				}

				string response = $"`{fullNameHeader.PadRight(maxNameLength)}  Afk. Discord naam";
				foreach (TeacherInfo record in records) {
					if (record.NoLookup) {
						continue;
					}
					response += $"\n{record.DisplayText.PadRight(maxNameLength)}  {record.Abbreviation}  {record.DiscordUser}";
				}
				response += "`";

				await ReplyAsync(response);
			}
		}
	}
}
