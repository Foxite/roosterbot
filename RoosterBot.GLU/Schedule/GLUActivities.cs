namespace RoosterBot.GLU {
	public static class GLUActivities {
		public static string GetActivityFromAbbr(string abbr) {
			return abbr switch {
				// Specific expansions
				"ned" => "Nederlands",
				"eng" => "Engels",
				"program" => "Programmeren",
				"gamedes" => "Gamedesign",
				"ond" => "Onderneming",
				"k0072" => "Keuzedeel (k0072)",
				"k0821" => "Keuzedeel (k0821)",
				"k0901" => "Keuzedeel (k0901)",
				"burger" => "Burgerschap",
				"rek" => "Rekenen",
				"vormg" => "Vormgeving",
				"engine" => "Engineering",
				"to" => "Teamoverleg",
				"skc" => "Studiekeuzecheck",
				"soll" => "Solliciteren",
				"mastercl" => "Masterclass",

				// Day off
				"studiedag" or
				"stdag doc" => "Studiedag :tada:",

				// Display as abbreviation
				"3d" or
				"2d" or
				"bpv" or
				"vb bpv" or
				"vb pvb" or
				"2d/3d" or
				"slb" or
				"avo" => abbr.ToUpper(),

				// Display literally
				"pauze" or
				"gameaudio" or
				"keuzedeel" or
				"gametech" or
				"project" or
				"rapid" or
				"gameplay" or
				"taken" or
				"stage" or
				"examen" or
				"animatie" or
				"werkveld" or
				"afstudeer" or
				"rozosho" or
				"rozosho-i" or
				"twinstick" => abbr[0].ToString().ToUpper() + abbr[1..],

				// Special
				"Sinterklaas" => abbr,

				// Not found
				_ => $"\"{abbr}\"",
			};
		}
	}
}
