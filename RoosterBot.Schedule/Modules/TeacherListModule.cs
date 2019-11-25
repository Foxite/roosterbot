using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;

namespace RoosterBot.Schedule {
	[Name("#TeacherListModule_Name"), LocalizedModule("nl-NL", "en-US")]
	public class TeacherListModule : RoosterModuleBase {
		public TeacherNameService Teachers { get; set; } = null!;

		[Command("#TeacherListModule_CommandName"), Description("#TeacherListModule_TeacherListCommand_Summary")]
		public Task TeacherListCommand([Remainder, Name("#TeacherListModule_ListCommand_NameParameterName")] string name = "") {
			IEnumerable<TeacherInfo> records;
			if (string.IsNullOrWhiteSpace(name)) {
				records = Teachers.GetAllRecords(Context.GuildConfig.GuildId);
			} else {
				records = Teachers.Lookup(Context.GuildConfig.GuildId, name).Select(match => match.Teacher);
			}

			if (!records.Any()) {
				ReplyDeferred(GetString("TeacherListModule_TeacherListCommand_NoTeachersFound"));
			} else if (records.Count() > 25) {
				ReplyDeferred(GetString("TeacherListModule_TeacherListCommand_TooManyResults"));
			} else {
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
				string response = StringUtil.FormatTextTable(cells);

				ReplyDeferred(response);
			}

			return Task.CompletedTask;
		}
	}
}
