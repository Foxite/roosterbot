using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord.Commands;
using RoosterBot;
using ScheduleComponent.Services;

namespace ScheduleComponent.Modules {
	[Group("klas")]
	public class StudentScheduleModule : ScheduleModuleBase {
		public StudentScheduleModule() : base() {
			LogTag = "SSM";
		}

		[Command("nu", RunMode = RunMode.Async), Summary("Welke les een klas nu heeft")]
		public async Task StudentCurrentCommand(string klas) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(false, "StudentSets", klas);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				string response;
				if (record == null) {
					response = "Het ziet ernaar uit dat je nu niets hebt.";
					if (DateTime.Today.DayOfWeek == DayOfWeek.Saturday || DateTime.Today.DayOfWeek == DayOfWeek.Sunday) {
						response += " Het is dan ook weekend.";
					}
				} else {
					response = $"{record.StudentSets}: Nu\n";
					response += $":notepad_spiral: {Util.GetActivityFromAbbr(record.Activity)}\n";

					if (record.Activity != "stdag doc") {
						if (record.Activity != "pauze") {
							string teachers = GetTeacherFullNamesFromAbbrs(record.StaffMember);
							if (record.StaffMember == "JWO" && Util.RNG.NextDouble() < 0.1) {
								response += $"<:VRjoram:392762653367336960> {teachers}\n";
							} else {
								response += $":bust_in_silhouette: {teachers}\n";
							}
							if (!string.IsNullOrWhiteSpace(record.Room)) {
								response += $":round_pushpin: {record.Room}\n";
							}
						}

						response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
						TimeSpan timeLeft = record.End - DateTime.Now;
						response += $":stopwatch: {record.Duration} - nog {timeLeft.Hours}:{timeLeft.Minutes.ToString().PadLeft(2, '0')}\n";
					}
				}
				await ReplyAsync(response, "StudentSets", (record?.StudentSets) ?? klas.ToUpper(), record);
			}
		}

		[Command("hierna", RunMode = RunMode.Async), Summary("Welke les een klas hierna heeft")]
		public async Task StudentNextCommand(string klas) {
			if (!await CheckCooldown())
				return;

			ReturnValue<ScheduleRecord> result = await GetRecord(true, "StudentSets", klas);
			if (result.Success) {
				ScheduleRecord record = result.Value;
				if (record == null) {
					await FatalError("GetRecord(SS1)==null)");
				} else {
					string response = $"{record.StudentSets}: Hierna\n";
					response += $":notepad_spiral: {Util.GetActivityFromAbbr(record.Activity)}\n";


					if (record.Activity != "stdag doc") {
						if (record.Activity != "pauze") {
							string teachers = GetTeacherFullNamesFromAbbrs(record.StaffMember);
							if (!string.IsNullOrWhiteSpace(teachers)) {
								if (record.StaffMember == "JWO" && Util.RNG.NextDouble() < 0.1) {
									response += $"<:VRjoram:392762653367336960> {teachers}\n";
								} else {
									response += $":bust_in_silhouette: {teachers}\n";
								}
							}
							if (!string.IsNullOrWhiteSpace(record.Room)) {
								response += $":round_pushpin: {record.Room}\n";
							}
						}
						
						if (record.Start.Date == DateTime.Today) {
							TimeSpan timeTillStart = record.Start - DateTime.Now;
							response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}" +
								$" - nog {timeTillStart.Hours}:{timeTillStart.Minutes.ToString().PadLeft(2, '0')}\n";
						} else {
							response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
							response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
						}
						response += $":stopwatch: {record.Duration}\n";
					}
					await ReplyAsync(response, "StudentSets", record.StudentSets, record);
				}
			}
		}

		[Command("dag", RunMode = RunMode.Async), Summary("Welke les je als eerste hebt op een dag")]
		public async Task StudentWeekdayCommand([Remainder] string klas_en_weekdag) {
			if (!await CheckCooldown())
				return;

			Tuple<bool, DayOfWeek, string> arguments = await GetValuesFromArguments(klas_en_weekdag);

			if (arguments.Item1) {
				DayOfWeek day = arguments.Item2;
				string clazz = arguments.Item3;
				ReturnValue<ScheduleRecord> result = await GetFirstRecord(day, "StudentSets", clazz);
				if (result.Success) {
					ScheduleRecord record = result.Value;
					string response;
					if (record == null) {
						response = $"Het lijkt er op dat je op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)} niets hebt.";
						if (day == DayOfWeek.Saturday || day == DayOfWeek.Sunday) {
							response += "\nDat is dan ook in het weekend.";
						}
					} else {
						if (DateTime.Today.DayOfWeek == day) {
							response = $"{record.StudentSets}: Als eerste op volgende week {DateTimeFormatInfo.CurrentInfo.GetDayName(day)}\n";
						} else {
							response = $"{record.StudentSets}: Als eerste op {DateTimeFormatInfo.CurrentInfo.GetDayName(day)}\n";
						}
						response += $":notepad_spiral: {Util.GetActivityFromAbbr(record.Activity)}";
						if (record.Activity == "pauze") {
							response += " :thinking:";
						}
						response += "\n";

						if (record.Activity != "stdag doc") {
							if (record.Activity != "pauze") {
								string teachers = GetTeacherFullNamesFromAbbrs(record.StaffMember);
								if (!string.IsNullOrWhiteSpace(teachers)) {
									if (record.StaffMember == "JWO" && Util.RNG.NextDouble() < 0.1) {
										response += $"<:VRjoram:392762653367336960> {teachers}\n";
									} else {
										response += $":bust_in_silhouette: {teachers}\n";
									}
								}
								if (!string.IsNullOrWhiteSpace(record.Room)) {
									response += $":round_pushpin: {record.Room}\n";
								}
							}
							response += $":calendar_spiral: {record.Start.ToShortDateString()}\n";
							response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
							response += $":stopwatch: {record.Duration}\n";
						}
					}
					await ReplyAsync(response, "StudentSets", clazz, record);
				}
			}
		}

		[Command("morgen", RunMode = RunMode.Async), Summary("Welke les je morgen als eerste hebt")]
		public async Task StudentTomorrowCommand(string klas) {
			await StudentWeekdayCommand(klas + " " + Util.GetStringFromDayOfWeek(DateTime.Today.AddDays(1).DayOfWeek));
		}
	}
}
