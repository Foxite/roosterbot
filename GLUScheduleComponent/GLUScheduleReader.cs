using CsvHelper;
using RoosterBot;
using ScheduleComponent.DataTypes;
using ScheduleComponent.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GLUScheduleComponent {
	public class GLUScheduleReader : ScheduleReaderBase {
		private readonly string m_Path;
		private readonly TeacherNameService m_Teachers;
		private readonly ulong m_Guild;

		public GLUScheduleReader(string path, TeacherNameService teachers, ulong guild) {
			m_Path = path;
			m_Teachers = teachers;
			m_Guild = guild;
		}

		/// <summary>
		/// Loads a schedule into memory from a CSV file so that it can be accessed using this service.
		/// </summary>
		/// <param name="name">Should be the same as the property you're going to search from.</param>
		public async override Task<List<ScheduleRecord>> GetSchedule() {
			Logger.Log(Discord.LogSeverity.Info, "GLUScheduleReader", $"Loading CSV file from {m_Path}");

			int line = 1;
			try {
				List<ScheduleRecord> schedule;
				using (StreamReader reader = File.OpenText(m_Path))
				using (CsvReader csv = new CsvReader(reader, new CsvHelper.Configuration.Configuration() { Delimiter = "," })) {
					await csv.ReadAsync();
					csv.ReadHeader();

					Dictionary<CompoundIdentifier, ScheduleRecord> lastRecords = new Dictionary<CompoundIdentifier, ScheduleRecord>();

					DateTime lastMonday = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1); // + 1 because C# weeks start on Sunday (which is 0, and Monday is 1, etc. Saturday is 6)

					schedule = new List<ScheduleRecord>();
					CultureInfo culture = new CultureInfo("en-US");

					while (await csv.ReadAsync()) {
						DateTime date = DateTime.ParseExact(csv["StartDate"], @"yyyy\-MM\-dd", culture);

						if (date < lastMonday) { // Only store past records for this week
							continue;
						}

						DateTime start = date + TimeSpan.ParseExact(csv["StartTime"], @"hh\:mm", culture);
						DateTime end = date + TimeSpan.ParseExact(csv["EndTime"], @"hh\:mm", culture); // Under the assumption that nobody works overnight

						ScheduleRecord record = new ScheduleRecord() {
							Activity = csv["Activity"],
							StaffMember = m_Teachers.GetRecordsFromAbbrs(m_Guild, csv["StaffMember"].Split(new[] { ", " }, StringSplitOptions.None)),
							StudentSets = csv["StudentSets"].Split(new[] { ", " }, StringSplitOptions.None).Select(code => new StudentSetInfo() { ClassName = code }).ToArray(),
							// Rooms often have " (0)" behind them. unknown reason.
							// Just remove them for now. This is the simplest way. We can't trim from the end, because multiple rooms may be listed and they will all have this suffix.
							Room = csv["Room"].Replace(" (0)", "").Split(new[] { ", " }, StringSplitOptions.None).Select(code => new RoomInfo() { Room = code }).ToArray(),
							Start = start,
							End = end
						};

						// TODO make sure this is still working
						// If it's not just revert this attempt at removing the name of the schedule reader. I've done my best. It's a bit hard to test a schedule program
						//  when it's summer break and there are no schedules.
						CompoundIdentifier key = new CompoundIdentifier(record);
						if (lastRecords.TryGetValue(key, out ScheduleRecord lastRecord) &&
							record.Activity == lastRecord.Activity &&
							record.Start.Date == lastRecord.Start.Date &&
							record.StudentSetsString == lastRecord.StudentSetsString &&
							record.StaffMember == lastRecord.StaffMember &&
							record.RoomString == lastRecord.RoomString) {
							lastRecord.BreakStart = lastRecord.End;
							lastRecord.BreakEnd = record.Start;
							lastRecord.End = record.End;
						} else {
							lastRecords[key] = record;
							schedule.Add(record);
						}
					}
				}
				return schedule;
			} catch (Exception e) {
				Logger.Log(Discord.LogSeverity.Critical, "GLUScheduleReader", $"The following exception was thrown while loading the CSV at \"{m_Path}\" on line {line}", e);
				throw;
			}
		}

		// Used to identify consecutive schedule items with a break in between them
		private class CompoundIdentifier {
			public StudentSetInfo[] Students { get; }
			public TeacherInfo[] Teachers { get; }
			public RoomInfo[] Rooms { get; }

			public CompoundIdentifier(ScheduleRecord fromRecord) {
				Students = fromRecord.StudentSets;
				Teachers = fromRecord.StaffMember;
				Rooms = fromRecord.Room;
			}

			public override bool Equals(object obj) {
				return obj is CompoundIdentifier identifier
					&& Enumerable.SequenceEqual(Students, identifier.Students)
					&& Enumerable.SequenceEqual(Teachers, identifier.Teachers)
					&& Enumerable.SequenceEqual(Rooms, identifier.Rooms);
			}

			public override int GetHashCode() {
				// Slightly modified https://stackoverflow.com/a/7244729/3141917
				int arrayHash<T>(T[] array) where T : IdentifierInfo {
					unchecked {
						if (array == null) {
							return 0;
						}
						int hash = 17;
						foreach (T element in array) {
							hash = hash * 31 + element.GetHashCode();
						}
						return hash;
					}
				}

				// The rest is based on VS 2019's autogenerated GetHashCode
				// I have no idea why these constants are what they are, a lot of StackOverflow users tend to choose much smaller (2-digit) ones.
				// I'll leave this as is.
				var hashCode = -1313702539;
				hashCode = hashCode * -1521134295 + arrayHash(Students);
				hashCode = hashCode * -1521134295 + arrayHash(Teachers);
				hashCode = hashCode * -1521134295 + arrayHash(Rooms);
				return hashCode;
			}
		}
	}
}
