using System;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using RoosterBot.Schedule;

namespace RoosterBot.GLU {
	internal class GLUStaffMemberReader {
		private readonly string m_Path;

		public GLUStaffMemberReader(string path) {
			m_Path = path;
		}

		public IEnumerable<StaffMemberInfo> ReadCSV() {
			Logger.Info("GLUStaffMemberReader", $"Loading staff CSV file {Path.GetFileName(m_Path)}");

			using StreamReader reader = File.OpenText(m_Path);
			using var csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," });
			csv.Read();
			csv.ReadHeader();

			while (csv.Read()) {
				string altSpellingsString = csv["AltSpellings"];
				string[]? altSpellings = null;

				if (!string.IsNullOrEmpty(altSpellingsString)) {
					altSpellings = altSpellingsString.Split(',');
				};

				var record = new StaffMemberInfo(
					code: csv["Abbreviation"],
					displayText: csv["FullName"],
					isUnknown: false,
					noLookup: bool.Parse(csv["NoLookup"]),
					discordUser: csv["DiscordUser"],
					altSpellings: altSpellings ?? Array.Empty<string>()
				);

				yield return record;
			}
			Logger.Info("GLUStaffMemberReader", $"Successfully loaded staff CSV file {Path.GetFileName(m_Path)}");
		}
	}
}
