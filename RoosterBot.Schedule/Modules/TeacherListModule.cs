using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[Name("#TeacherListModule_Name")]
	public class TeacherListModule : RoosterModuleBase {
		private TeacherNameService Teachers { get; }

		public TeacherListModule(TeacherNameService teachers) {
			Teachers = teachers;
		}

		[Command("leraren"), Alias("docenten", "docent"), Summary("#TeacherListModule_TeacherListCommand_Summary")]
		public Task TeacherListCommand([Remainder, Name("#TeacherListModule_ListCommand_NameParameterName")] string name = "") {
			IEnumerable<TeacherInfo> records;
			if (string.IsNullOrWhiteSpace(name)) {
				records = Teachers.GetAllRecords((Context.Guild ?? Context.UserGuild).Id);
			} else {
				records = Teachers.Lookup((Context.Guild ?? Context.UserGuild).Id, name).Select(match => match.Teacher);
			}

			if (!records.Any()) {
				ReplyDeferred(GetString("TeacherListModule_TeacherListCommand_NoTeachersFound"));
			} else if (records.Count() > 25) {
				ReplyDeferred(GetString("TeacherListModule_TeacherListCommand_TooManyResults"));
			} else {
				// A foreach loop is faster than a for loop if you have to use the item more than once.
				// https://www.dotnetperls.com/for-foreach
				// Because of NoLookup, we don't actually know how many times we use the item, but in practice, we almost always have to use it twice and not once.

				string[][] cells = new string[records.Count() + 1][];
				cells[0] = new string[] {
					GetString("TeacherListModule_TeacherListCommand_ColumnFullName"),
					GetString("TeacherListModule_TeacherListCommand_ColumnAbbreviation"),
					GetString("TeacherListModule_TeacherListCommand_DiscordName")
				};
				int recordIndex = 1;
				foreach (TeacherInfo record in records) {
					cells[recordIndex] = new string[3];
					cells[recordIndex][0] = record.DisplayText;
					cells[recordIndex][1] = record.ScheduleCode;
					cells[recordIndex][2] = record.DiscordUser ?? "";

					recordIndex++;
				}
				string response = Util.FormatTextTable(cells);

				ReplyDeferred(response);
			}

			return Task.CompletedTask;
		}
	}
}
