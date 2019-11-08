using System;
using System.IO;

namespace RoosterBot.MiscStuff {
	public class CounterService {
		private string m_CounterFolder;

		public CounterService(string counterPath) {
			m_CounterFolder = counterPath;
		}

		public string GetCounterDescription(string counterName) {
			string path;
			try {
				path = Path.Combine(m_CounterFolder, counterName);
			} catch (ArgumentException e) {
				throw new FileNotFoundException("Counter is invalid", e);
			}

			if (File.Exists(path)) {
				return File.ReadAllLines(Path.Combine(m_CounterFolder, counterName))[0];
			} else {
				throw new FileNotFoundException();
			}

		}

		public CounterData GetDateCounter(string counterName) {
			try {
				string counterPath = Path.Combine(m_CounterFolder, counterName);
				if (File.Exists(counterPath)) {
					string[] contents = File.ReadAllLines(counterPath);
					return new CounterData() {
						Description = contents[0],
						LastResetDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(contents[1])).UtcDateTime,
						HighScoreTimespan = TimeSpan.FromSeconds(long.Parse(contents[2]))
					};
				} else {
					throw new FileNotFoundException();
				}
			} catch (ArgumentException e) {
				throw new ArgumentException("Counter is invalid", e);
			}
		}

		/// <summary>
		/// Reset a counter.
		/// </summary>
		/// <returns>True if this reset ended a highscore.</returns>
		public bool ResetDateCounter(string counterName) {
			try {
				string counterPath = Path.Combine(m_CounterFolder, counterName);
				if (File.Exists(counterPath)) {
					string[] contents = File.ReadAllLines(counterPath);
					if (contents.Length != 3) {
						throw new InvalidDataException($"File {counterName} does not have 3 lines.");
					}
					long oldHighScore = long.Parse(contents[2]);
					long previousTimespan = (long) (DateTimeOffset.UtcNow - DateTimeOffset.FromUnixTimeSeconds(long.Parse(contents[1]))).TotalSeconds;
					bool newHighScore = oldHighScore < previousTimespan;

					File.WriteAllLines(counterPath, new string[] {
						contents[0],
						DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
						(newHighScore ? previousTimespan : oldHighScore).ToString()
					});
					return newHighScore;
				} else {
					throw new FileNotFoundException();
				}
			} catch (ArgumentException e) {
				throw new ArgumentException("Counter is invalid", e);
			}
		}
	}

	public struct CounterData {
		public string Description;
		public DateTime LastResetDate;
		public TimeSpan HighScoreTimespan;

		public CounterData(string description, DateTime lastResetDate, TimeSpan highScoreTimespan) {
			Description = description;
			LastResetDate = lastResetDate;
			HighScoreTimespan = highScoreTimespan;
		}

		public override bool Equals(object? obj) {
			return obj is CounterData data
				&& Description == data.Description
				&& LastResetDate == data.LastResetDate
				&& HighScoreTimespan.Equals(data.HighScoreTimespan);
		}

		public override int GetHashCode() => HashCode.Combine(Description, LastResetDate, HighScoreTimespan);

		public static bool operator ==(CounterData left, CounterData right) {
			return left.Equals(right);
		}

		public static bool operator !=(CounterData left, CounterData right) {
			return !(left == right);
		}
	}
}
