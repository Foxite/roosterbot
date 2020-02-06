using System.Collections.Generic;
using System.Linq;
using Qmmands;

namespace RoosterBot.Schedule {
	[Name("#StaffMemberListModule_Name")]
	public class StaffMemberModule : RoosterModule {
		/* TODO see item in StaffMemberService
		public StaffMemberService Staff { get; set; } = null!;

		[Command("#StaffMemberListModule_CommandName"), Description("#StaffMemberListModule_StaffMemberListCommand_Summary")]
		public CommandResult StaffMemberListCommand([Remainder, Name("#StaffMemberListModule_ListCommand_NameParameterName")] string name = "") {
			IEnumerable<StaffMemberInfo> records;
			if (string.IsNullOrWhiteSpace(name)) {
				records = Staff.GetAllRecords(Context.ChannelConfig.ChannelReference);
			} else {
				records = Staff.Lookup(Context.ChannelConfig.ChannelReference, name).Select(match => match.StaffMember);
			}

			if (!records.Any()) {
				return TextResult.Info(GetString("StaffMemberListModule_StaffMemberListCommand_NoStaffMembersFound"));
			} else {
				string[][] cells = new string[records.Count()][];
				string[] header = new string[] {
					GetString("StaffMemberListModule_StaffMemberListCommand_ColumnFullName"),
					GetString("StaffMemberListModule_StaffMemberListCommand_ColumnAbbreviation"),
					GetString("StaffMemberListModule_StaffMemberListCommand_DiscordName")
				};
				int recordIndex = 0;
				foreach (StaffMemberInfo record in records) {
					cells[recordIndex] = new string[3];
					cells[recordIndex][0] = record.DisplayText;
					cells[recordIndex][1] = record.ScheduleCode;
					cells[recordIndex][2] = record.DiscordUser ?? "";

					recordIndex++;
				}

				return new PaginatedResult(new PaginatedTableEnumerator("Table", header, cells), GetString("StaffMemberListModule_StaffMemberListCommand_ResultsFor", name));
			}
		}*/
	}
}
