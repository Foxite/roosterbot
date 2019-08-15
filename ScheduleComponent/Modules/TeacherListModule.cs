using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot.Attributes;
using RoosterBot.Modules;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[LogTag("TeacherListModule"), Name("Leraren")]
	public class TeacherListModule : EditableCmdModuleBase {
		public TeacherNameService Teachers { get; set; }
		
		[Command("leraren", RunMode = RunMode.Async), Alias("docenten", "docent"), Summary("Een lijst van alle leraren, hun afkortingen, en hun Discord namen (als die bekend is). Je kan filteren op naam.")]
		public async Task TeacherListCommand([Remainder, Name("naam")] string name = "") {
			IEnumerable<TeacherInfo> records;

			if (string.IsNullOrWhiteSpace(name)) {
				records = Teachers.GetAllRecords(Context.Guild.Id);
			} else {
				records = Teachers.Lookup(Context.Guild.Id, name);
			}

			if (records.FirstOrDefault() == null) { // Faster than .Count() == 0 because otherwise it would have to actually count it, but we don't care about the count beyond it being 0
													// Although I would appreciate an .IsEmpty() method
				await ReplyAsync("Geen leraren gevonden.");
			} else {
				// A foreach loop is faster than a for loop if you have to use the item more than once.
				// https://www.dotnetperls.com/for-foreach
				// Because of NoLookup, we don't actually know how many times we use the item, but in practice, we almost always have to use it twice and not once.
				string fullNameHeader = "Volledige naam";
				int maxNameLength = fullNameHeader.Length;
				bool discordNamesPresent = false;
				foreach (TeacherInfo record in records) {
					if (record.NoLookup) {
						continue;
					}
					maxNameLength = Math.Max(maxNameLength, record.FullName.Length);

					if (!string.IsNullOrEmpty(record.DiscordUser)) {
						discordNamesPresent = true;
					}
				}

				string response = $"`{fullNameHeader.PadRight(maxNameLength)}  Afk.";

				if (discordNamesPresent) {
					response += " Discord naam";
				}

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
