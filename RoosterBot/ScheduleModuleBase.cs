using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;

namespace RoosterBot {
	public class ScheduleModuleBase : EditableCmdModuleBase {
		public ScheduleService Service { get; set; }
		public ConfigService Config { get; set; }
		public SNSService SNS { get; set; }
		public LastScheduleCommandService ARS { get; set; }

		private readonly string LogTag;

		public ScheduleModuleBase() : base() {
			LogTag = "SMB";
		}

		[Command("daarna", RunMode = RunMode.Async), Summary("Kijk wat er gebeurt na het laatste wat je hebt bekeken")]
		public async Task GetAfterCommand() {
			if (!(Context.User is IGuildUser user))
				return;
			ScheduleCommandInfo query = ARS.GetLastCommandFromUser(user);
			if (query.Equals(default(ScheduleCommandInfo))) {
				await MinorError("Na wat?");
			} else {
				ScheduleRecord record = query.Record;
				string response;
				bool nullRecord = record == null;
				try {
					if (nullRecord) {
						record = Service.GetNextRecord(query.SourceSchedule, query.Identifier);
					} else {
						record = Service.GetRecordAfter(query.SourceSchedule, query.Record);
					}
				} catch (ScheduleNotFoundException) {
					await MinorError("Dat item staat niet op mijn rooster.");
					return;
				} catch (RecordsOutdatedException) {
					await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
					return;
				} catch (Exception ex) {
					await FatalError(ex.GetType().Name);
					throw;
				}

				if (query.SourceSchedule == "StudentSets") {
					if (nullRecord) {
						response = $"{record.StudentSets}: Hierna\n";
					} else {
						response = $"{record.StudentSets}: Na de vorige les\n";
					}
				} else if (query.SourceSchedule == "StaffMember") {
					if (nullRecord) {
						response = $"{GetTeacherNameFromAbbr(record.StaffMember)}: Hierna\n";
					} else {
						response = $"{GetTeacherNameFromAbbr(record.StaffMember)}: Na de vorige les\n";
					}
				} else if (query.SourceSchedule == "Room") {
					if (nullRecord) {
						response = $"{record.Room}: Hierna\n";
					} else {
						response = $"{record.Room}: Na de vorige les\n";
					}
				} else {
					await FatalError("query.SourceSchedule is not recognized");
					return;
				}
				response += $":notepad_spiral: {GetActivityFromAbbr(record.Activity)}\n";

				if (record.Activity != "stdag doc") {
					if (record.Activity != "pauze") {
						string teachers = GetTeacherNameFromAbbr(record.StaffMember);
						if (query.SourceSchedule != "StaffMember" && !string.IsNullOrWhiteSpace(teachers)) {
							if (record.StaffMember == "JWO" && Util.RNG.NextDouble() < 0.1) {
								response += $"<:test_emoji:496301498234437656> {teachers}\n";
							} else {
								response += $":bust_in_silhouette: {teachers}\n";
							}
						}
						if (query.SourceSchedule != "StudentSets" && !string.IsNullOrWhiteSpace(record.StudentSets)) {
							response += $":busts_in_silhouette: {record.StudentSets}\n";
						}
						if (query.SourceSchedule != "Room" && !string.IsNullOrWhiteSpace(record.Room)) {
							response += $":round_pushpin: {record.Room}\n";
						}
					}
					response += $":calendar_spiral: {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} {record.Start.ToShortDateString()}\n";
					response += $":clock5: {record.Start.ToShortTimeString()} - {record.End.ToShortTimeString()}\n";
					response += $":stopwatch: {record.Duration}\n";
				}
				await ReplyAsync(response, query.SourceSchedule, query.Identifier, record);
			}
		}

		protected string GetTeacherNameFromAbbr(string teacherString) {
			string[] abbrs = teacherString.Split(new[] { ", " }, StringSplitOptions.None);
			string ret = "";
			bool anyTeacherAdded = false;
			for (int i = 0; i < abbrs.Length; i++) {
				string thisTeacher = GetSingleTeacherNameFromAbbr(abbrs[i]);

				// The schedule occasionally contains teachers that don't actually exist (like XGVAC2). Skip those.
				if (thisTeacher != null) {
					// This prevents it from adding a comma if this is the first item, or if we've only had nonexistent teachers so far.
					if (anyTeacherAdded) {
						ret += ", ";
					}
					ret += thisTeacher;
					anyTeacherAdded = true;
				}
			}
			return ret;
		}

		public static string GetSingleTeacherNameFromAbbr(string abbr) {
			switch (abbr) {
			case "ATE":
				return "Arnoud Telkamp";
			case "BHN":
				return "Bram den Hond";
			case "CPE":
				return "Chris-Jan Peterse";
			case "CSP":
				return "Cynthia Spier";
			case "DBU":
				return "David Buzzi";
			case "DWO":
				return "Dick Wories";
			case "HAL":
				return "Hyltsje Altenburg";
			case "HBE":
				return "Hsin Chi Berenst";
			case "JBO":
				return "Jaap van Boggelen";
			case "JWO":
				return "Joram Wolters";
			case "LCA":
				return "Laurence Candel";
			case "LEN":
				return "Laura Endert";
			case "LKR":
				return "Lance Krasniqi";
			case "LMU":
				return "Liselotte Mulder";
			case "MJA":
				return "Martijn Jacobs";
			case "MKU":
				return "Martijn Kunstman";
			case "MME":
				return "Marijn Moerbeek";
			case "MRE":
				return "Miriam Reutelingsperger";
			case "MVE":
				return "Maart Veldman";
			case "RBA":
				return "René Balkenende";
			case "RBR":
				return "Rubin de Bruin";
			case "SSC":
				return "Sander Scholl";
			case "SLO":
				return "Suus Looijen";
			case "SRI":
				return "Suzanne Ringeling";
			case "VV-GAGD":
				return "een vervangende docent";
			case "WSC":
				return "Willemijn Schmitz";
			case "YWI":
				return "Yelena de Wit";
			default:
				return null;
			}
		}

		public static string GetTeacherAbbrFromName(string name) {
			if (name.Length < 3)
				return null;

			if (name.ToLower() == "martijn kunstman")
				return "MKU";
			if (name.ToLower() == "martijn jacobs")
				return "MJA";

			switch (name.Split(' ')[0].ToLower()) {
			case "ate":
			case "arnoud":
				return "ATE";
			case "bhn":
			case "bram":
				return "BHN";
			case "cpe":
			case "chris-jan":
			case "chrisjan":
				return "CPE";
			case "csp":
			case "cynthia":
				return "CSP";
			case "dbu":
			case "david":
				return "DBU";
			case "dwo":
			case "dick":
				return "DWO";
			case "hal":
			case "hyltsje":
				return "HAL";
			case "hbe":
			case "hsin":
			case "chi":
				return "HBE";
			case "jbo":
			case "jaap":
				return "JBO";
			case "jwo":
			case "joram":
				return "JWO";
			case "len":
			case "laura":
				return "LEN";
			case "lca":
			case "laurence":
			case "laurens":
				return "LCA";
			case "lkr":
			case "lance":
				return "LKR";
			case "lmu":
			case "liselotte":
				return "LMU";
			case "mja":
				return "MJA";
			case "mku":
				return "MKU";
			case "martijn":
				return "MJA, MKU";
			case "mme":
			case "marijn":
				return "MME";
			case "mre":
			case "miriam":
				return "MRE";
			case "mve":
			case "maart":
				return "MVE";
			case "rba":
			case "rené":
			case "rene":
				return "RBA";
			case "rbr":
			case "rubin":
				return "RBR";
			case "slo":
			case "suus":
				return "SLO";
			case "sri":
			case "suzanne":
				return "SRI";
			case "ssc":
			case "sander":
				return "Sander Scholl";
			case "ywi":
			case "yelena":
				return "YWI";
			case "wsc":
			case "willemijn":
				return "Willemijn Schmitz";
			}
			return null;
		}

		public static string GetActivityFromAbbr(string abbr) {
			switch (abbr) {
			case "ned":
				return "Nederlands";
			case "eng":
				return "Engels";
			case "program":
				return "Programmeren";
			case "gamedes":
				return "Gamedesign";
			case "ond":
				return "Onderneming";
			case "k0072":
				return "Keuzedeel (k0072)";
			case "k0901":
				return "Keuzedeel (k0901)";
			case "burger":
				return "Burgerschap";
			case "rek":
				return "Rekenen";
			case "vormg":
				return "Vormgeving";
			case "engine":
				return "Engineering";
			case "stdag doc":
				return "Studiedag :tada:";
			
			case "3d":
			case "2d":
			case "bpv":
			case "vb bpv":
			case "2d/3d":
			case "slb":
				return abbr.ToUpper();
			
			case "pauze":
			case "gameaudio":
			case "keuzedeel":
			case "gametech":
			case "project":
			case "rapid":
			case "gameplay":
			case "taken":
			case "stage":
			case "examen":
				return abbr.FirstCharToUpper();
			
			default:
				return $"\"{abbr}\" (ik weet de volledige naam niet)";
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(bool next, string schedule, string name) {
			if (name == "") {
				await MinorError("Dat item staat niet op mijn rooster (of eigenlijk wel, maar niet op een zinvolle manier).");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}
			if (schedule == "Room" && name.Length != 4) {
				await MinorError("Dat is geen lokaal.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}

			name = name.ToUpper();
			ScheduleRecord record = null;
			try {
				record = next ? Service.GetNextRecord(schedule, name) : Service.GetCurrentRecord(schedule, name);
				return new ReturnValue<ScheduleRecord>() {
					Success = true,
					Value = record
				};
			} catch (ScheduleNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError(ex.GetType().Name);
				throw;
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetFirstRecord(DayOfWeek day, string schedule, string name) {
			if (name == "") {
				await MinorError("Dat item staat niet op mijn rooster (of eigenlijk wel, maar niet op een zinvolle manier).");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}
			if (schedule == "Room" && name.Length != 4) {
				await MinorError("Dat is geen lokaal.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			}

			name = name.ToUpper();
			ScheduleRecord record = null;
			try {
				record = Service.GetFirstRecordForDay(schedule, name, day);
				return new ReturnValue<ScheduleRecord>() {
					Success = true,
					Value = record
				};
			} catch (ScheduleNotFoundException) {
				await MinorError("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await MinorError("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (Exception ex) {
				await FatalError(ex.GetType().Name);
				throw;
			}
		}

		protected async Task AddReaction(string unicode) {
			try {
				await Context.Message.AddReactionAsync(new Emoji(unicode));
			} catch (HttpException) { } // Permission denied
		}

		protected async Task MinorError(string message) {
			ARS.RemoveLastQuery(Context.User);
			if (Config.ErrorReactions) {
				await AddReaction("❌");
			}
			await ReplyAsync(message);
		}

		protected async Task FatalError(string message) {
			Logger.Log(LogSeverity.Error, LogTag, message);
			await SNS.SendCriticalErrorNotificationAsync("Critical error: " + message);
			if (Config.ErrorReactions) {
				await AddReaction("🚫");
			}
			await ReplyAsync("Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.\n");
			ARS.RemoveLastQuery(Context.User);
		}

		protected async Task<bool> CheckCooldown() {
			if (Context.User.Id == 152412662972678144)
				return false;

			Tuple<bool, bool> result = Config.CheckCooldown(Context.User.Id);
			if (result.Item1) {
				return true;
			} else {
				if (!result.Item2) {
					if (Config.ErrorReactions) {
						await AddReaction("⚠");
					}
					await ReplyAsync(Context.User.Mention + ", je gaat een beetje te snel.");
				}
				return false;
			}
		}

		public static DayOfWeek GetDayOfWeekFromString(string dayofweek) {
			switch (dayofweek) {
			case "ma":
			case "maandag":
				return DayOfWeek.Monday;
			case "di":
			case "dinsdag":
				return DayOfWeek.Tuesday;
			case "wo":
			case "woensdag":
				return DayOfWeek.Wednesday;
			case "do":
			case "donderdag":
				return DayOfWeek.Thursday;
			case "vr":
			case "vrijdag":
				return DayOfWeek.Friday;
			case "za":
			case "zaterdag":
				return DayOfWeek.Saturday;
			case "zo":
			case "zondag":
				return DayOfWeek.Sunday;
			default:
				throw new ArgumentException();
			}
		}

		public static string GetStringFromDayOfWeek(DayOfWeek day) {
			switch (day) {
			case DayOfWeek.Monday:
				return "maandag";
			case DayOfWeek.Tuesday:
				return "dinsdag";
			case DayOfWeek.Wednesday:
				return "woensdag";
			case DayOfWeek.Thursday:
				return "donderdag";
			case DayOfWeek.Friday:
				return "vrijdag";
			case DayOfWeek.Saturday:
				return "zaterdag";
			case DayOfWeek.Sunday:
				return "zondag";
			default:
				throw new ArgumentException();
			}
		}

		/// <summary>
		/// Given two command arguments, this determines which is a DayOfWeek and which is not.
		/// </summary>
		/// <returns>bool: Success, DayOfWeek: One of the arguments as DOW, string: the other argument as received</returns>
		protected async Task<Tuple<bool, DayOfWeek, string>> GetValuesFromArguments(string arguments) {
			DayOfWeek day;
			string entry;
			string[] argumentWords = arguments.Split(' ');
			try {
				day = GetDayOfWeekFromString(argumentWords[0]);
				entry = string.Join(" ", argumentWords, 1, argumentWords.Length - 1); // get everything except first
			} catch (ArgumentException) {
				try {
					day = GetDayOfWeekFromString(argumentWords[argumentWords.Length - 1]);
					entry = string.Join(" ", argumentWords, 0, argumentWords.Length - 2); // get everything except last
				} catch (ArgumentException) {
					await MinorError($"Ik weet niet welk deel van \"" + arguments + "\" een dag is.");
					return new Tuple<bool, DayOfWeek, string>(false, default, "");
				}
			}
			return new Tuple<bool, DayOfWeek, string>(true, day, entry);
		}

		public async Task<IUserMessage> ReplyAsync(string message, string schedule, string identifier, ScheduleRecord record, bool isTTS = false, Embed embed = null, RequestOptions options = null) {
			IUserMessage ret = await base.ReplyAsync(message, isTTS, embed, options);
			if (!(Context.User is IGuildUser user))
				return ret;
			
			ARS.OnRequestByUser(user, schedule, identifier, record);
			return ret;
		}
	}
}
