using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;

namespace RoosterBot {
	public abstract class ScheduleModuleBase : ModuleBase {
		protected ScheduleService Service { get; }
		protected ConfigService Config { get; }
		protected SNSService SNS { get; }
		protected string LogTag { get; }

		public ScheduleModuleBase(ScheduleService serv, ConfigService config, SNSService sns, string logTag) {
			Service = serv;
			Config = config;
			LogTag = logTag;
			SNS = sns;
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

		protected string GetSingleTeacherNameFromAbbr(string abbr) {
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

		protected string GetTeacherAbbrFromName(string name) {
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

		protected string GetActivityFromAbbr(string abbr) {
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
				return abbr.FirstCharToUpper();
			
			default:
				return $"\"{abbr}\" (ik weet de volledige naam niet)";
			}
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(bool next, string schedule, string name) {
			if (name == "") {
				await ReactMinorError();
				await ReplyAsync("Dat item staat niet op mijn rooster (of eigenlijk wel, maar niet op een zinvolle manier).");
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
				await ReactMinorError();
				await ReplyAsync("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await ReactMinorError();
				await ReplyAsync("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
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
				await ReactMinorError();
				await ReplyAsync("Dat item staat niet op mijn rooster (of eigenlijk wel, maar niet op een zinvolle manier).");
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
				await ReactMinorError();
				await ReplyAsync("Dat item staat niet op mijn rooster.");
				return new ReturnValue<ScheduleRecord>() {
					Success = false
				};
			} catch (RecordsOutdatedException) {
				await ReactMinorError();
				await ReplyAsync("Ik heb dat item gevonden in mijn rooster, maar ik heb nog geen toegang tot de laatste roostertabellen, dus ik kan niets zien.");
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

		protected async Task ReactMinorError() {
			if (Config.ErrorReactions) {
				await AddReaction("❌");
			}
		}

		protected async Task FatalError(string message) {
			Logger.Log(LogSeverity.Error, LogTag, message);
			await SNS.SendCriticalErrorNotificationAsync("Critical error: " + message);
			if (Config.ErrorReactions) {
				await AddReaction("🚫");
			}
			string response = "Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.\n";
			await ReplyAsync(response);
		}

		protected async Task<bool> CheckCooldown() {
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
		
		protected DayOfWeek GetDayOfWeekFromString(string dayofweek) {
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

		protected string GetStringFromDayOfWeek(DayOfWeek day) {
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
		protected async Task<Tuple<bool, DayOfWeek, string>> GetValuesFromArguments(string argument1, string argument2) {
			DayOfWeek day;
			string entry;
			try {
				day = GetDayOfWeekFromString(argument1);
				entry = argument2;
			} catch (ArgumentException) {
				try {
					day = GetDayOfWeekFromString(argument2);
					entry = argument1;
				} catch (ArgumentException) {
					await ReactMinorError();
					await ReplyAsync($"Ik weet niet welke dag je bedoelt met {argument1} of {argument2}.");
					return new Tuple<bool, DayOfWeek, string>(false, default(DayOfWeek), "");
				}
			}
			return new Tuple<bool, DayOfWeek, string>(true, day, entry);
		}
	}
}
