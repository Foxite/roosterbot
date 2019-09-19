using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot.Schedule {
	[LogTag("TeacherListModule"), Name("#TeacherListModule_Name")]
	public class TeacherListModule : EditableCmdModuleBase {
		public TeacherNameService Teachers { get; set; }
		
		[Command("leraren", RunMode = RunMode.Async), Alias("docenten", "docent"), Summary("#TeacherListModule_TeacherListCommand_Summary")]
		public Task TeacherListCommand([Remainder, Name("#TeacherListModule_ListCommand_NameParameterName")] string name = "") {
			IEnumerable<TeacherInfo> records;
			if (string.IsNullOrWhiteSpace(name)) {
				records = Teachers.GetAllRecords(Context.Guild.Id);
			} else {
				records = Teachers.Lookup(Context.Guild.Id, name).Select(match => match.Teacher);
			}

			if (records.FirstOrDefault() == null) { // Faster than .Count() == 0 because otherwise it would have to actually count it, but we don't care about the count beyond it being 0
													// Although I would appreciate an .IsEmpty() method
				ReplyDeferred(GetString("TeacherListModule_TeacherListCommand_NoTeachersFound"));
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
					cells[recordIndex][0] = record.FullName;
					cells[recordIndex][1] = record.Abbreviation;
					cells[recordIndex][2] = record.DiscordUser;

					recordIndex++;
				}
				string response = Util.FormatTextTable(cells);

				ReplyDeferred(response);
			}

			return Task.CompletedTask;
		}
	}
}
