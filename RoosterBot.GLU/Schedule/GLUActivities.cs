namespace RoosterBot.Schedule.GLU {
	public static class GLUActivities {
		public static string GetActivityFromAbbr(string abbr) {
			switch (abbr) {
				// Specific expansions
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
				case "k0821":
					return "Keuzedeel (k0821)";
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
				case "to":
					return "Teamoverleg";
				case "skc":
					return "Studiekeuzecheck";
				case "soll":
					return "Solliciteren";
				case "mastercl":
					return "Masterclass";

				// Day off
				case "studiedag":
				case "stdag doc":
					return "Studiedag :tada:";

				// Display as abbreviation
				case "3d":
				case "2d":
				case "bpv":
				case "vb bpv":
				case "vb pvb":
				case "2d/3d":
				case "slb":
				case "avo":
					return abbr.ToUpper();

				// Display literally
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
				case "animatie":
				case "werkveld":
				case "afstudeer":
				case "rozosho":
				case "rozosho-i":
				case "twinstick":
					return abbr.FirstCharToUpper();

				// Special
				case "Sinterklaas":
					return abbr;

				// Not found
				default:
					return $"\"{abbr}\"";
			}
		}
	}
}
