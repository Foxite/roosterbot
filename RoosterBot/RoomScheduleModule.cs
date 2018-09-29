using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace RoosterBot {
	public class RoomScheduleModule : ScheduleModuleBase {
		public RoomScheduleModule(ScheduleService serv, ConfigService config) : base(serv, config, "RSM") { }

		[Command("lokaalnu", RunMode = RunMode.Async)]
		private async Task RoomCurrentCommand(string room) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "Room", room);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					string response = "Het ziet ernaar uit dat daar nu niets is.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
					await ReplyAsync(response);
				} else {
					string response = $"In {room} is nu {record.Activity}";

					if (!string.IsNullOrEmpty(record.StaffMember)) {
						response += $" van {GetTeacherNameFromAbbr(record.StaffMember)}";
					}
					if (!string.IsNullOrEmpty(record.StudentSets)) {
						response += $" met de klas {record.StudentSets}";
					}
					response += ".\n";

					TimeSpan actualDuration = record.End - record.Start;
					string[] givenDuration = record.Duration.Split(':');
					response += $"Dit is begonnen om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";

					if (!(actualDuration.Hours == int.Parse(givenDuration[0]) && actualDuration.Minutes == int.Parse(givenDuration[1]))) {
						response += $"Tenminste, dat staat er, maar volgens mijn berekeningen is dat complete onzin en duurt dat eigenlijk {actualDuration.Hours}:{actualDuration.Minutes}.\n";
					}
					await ReplyAsync(response);
				}
			}
		}

		[Command("lokaalhierna", RunMode = RunMode.Async)]
		private async Task RoomNextCommand(string room) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "Room", room);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record != null) {
					string response = $"In {room} is hierna {record.Activity}";

					if (!string.IsNullOrEmpty(record.StaffMember)) {
						response += $" van {GetTeacherNameFromAbbr(record.StaffMember)}";
					}
					if (!string.IsNullOrEmpty(record.StudentSets)) {
						response += $" met de klas {record.StudentSets}";
					}
					response += ".\n";

					TimeSpan actualDuration = record.End - record.Start;
					string[] givenDuration = record.Duration.Split(':');
					response += $"Dit is begonnen om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";

					if (!(actualDuration.Hours == int.Parse(givenDuration[0]) && actualDuration.Minutes == int.Parse(givenDuration[1]))) {
						response += $"Tenminste, dat staat er, maar volgens mijn berekeningen is dat complete onzin en duurt dat eigenlijk {actualDuration.Hours}:{actualDuration.Minutes}.\n";
					}
					await ReplyAsync(response);
				} else {
					await FatalError("GetRecord(RS1)==null");
				}
			}
		}
	}
}
