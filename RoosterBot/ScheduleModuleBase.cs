using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace RoosterBot {
	public abstract class ScheduleModuleBase : ModuleBase {
		protected ScheduleService Service { get; }
		protected ConfigService Config { get; }
		protected string LogTag { get; }

		public ScheduleModuleBase(ScheduleService serv, ConfigService config, string logTag) {
			Service = serv;
			Config = config;
			LogTag = logTag;
		}

		protected string GetTeacherNameFromAbbr(string teacherString) {
			string[] abbrs = teacherString.Split(new[] { ", " }, StringSplitOptions.None);
			string ret = "";
			for (int i = 0; i < abbrs.Length; i++) {
				ret += GetSingleTeacherNameFromAbbr(abbrs[i]);
				if (i != abbrs.Length - 1) {
					ret += ", ";
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
			case "SLO":
				return "Suus Looijen";
			case "SRI":
				return "Suzanne Ringeling";
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
			case "ywi":
			case "yelena":
				return "YWI";
			}
			return null;
		}

		protected async Task<ReturnValue<ScheduleRecord>> GetRecord(bool next, string schedule, string name) {
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

		protected async Task ReactMinorError() {
			if (Config.ErrorReactions) {
				await Context.Message.AddReactionAsync(new Emoji("❌"));
			}
		}

		protected async Task FatalError(string message) {
			Logger.Log(LogSeverity.Error, LogTag, message);
			if (Config.ErrorReactions) {
				await Context.Message.AddReactionAsync(new Emoji("⛔"));
			}
			string response = "Ik weet niet wat, maar er is iets gloeiend misgegaan. Probeer het later nog eens? Dat moet ik zeggen van mijn maker, maar volgens mij gaat het niet werken totdat hij het fixt. Sorry.\n";
			//response += $"{(await Context.Client.GetUserAsync(133798410024255488)).Mention} FIX IT! ({message})";
			// TODO alert via Amazon AWS
			await ReplyAsync(response);
		}

		protected async Task<bool> CheckCooldown() {
			var result = Config.CheckCooldown(Context.User.Id);
			if (result.Item1) {
				return true;
			} else {
				if (!result.Item2) {
					if (Config.ErrorReactions) {
						await Context.Message.AddReactionAsync(new Emoji("⚠️"));
					}
					await ReplyAsync(Context.User.Mention + ", je gaat een beetje te snel.");
				}
				return false;
			}
		}

		protected string GetTimeSpanResponse(ScheduleRecord record) {
			string ret = "";
			TimeSpan actualDuration = record.End - record.Start;
			string[] givenDuration = record.Duration.Split(':');
			if (record.Start.Day == DateTime.Today.Day) {
				ret += $"Dit begint om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";
			} else {
				ret += $"Dit begint morgen om {record.Start.ToShortTimeString()} en eindigd om {record.End.ToShortTimeString()}. Dit duurt dus {record.Duration}.\n";
			}

			if (!(actualDuration.Hours == int.Parse(givenDuration[0]) && actualDuration.Minutes == int.Parse(givenDuration[1]))) {
				ret += $"Tenminste, dat staat er, maar volgens mijn berekeningen is dat complete onzin en duurt de les eigenlijk {actualDuration.Hours}:{actualDuration.Minutes}.\n";
			}
			return ret;
		}

		protected string GetNextTimeString(ScheduleRecord record) {
			if (record.Start.Date > DateTime.Now.Date.AddDays(1)) { // More than 1 day from now
				return $"morgen niets, en op {DateTimeFormatInfo.CurrentInfo.GetDayName(record.Start.DayOfWeek)} als eerste";
			} else if (record.Start.Date > DateTime.Now.Date) { // 1 day from now
				return "morgen als eerste";
			} else { // Today
				return "hierna";
			}
		}
	}
}
