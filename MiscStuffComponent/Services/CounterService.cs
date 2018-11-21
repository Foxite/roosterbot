using System;
using System.IO;

namespace MiscStuffComponent.Services {
	public class CounterService {
		private string m_CounterFolder;

		public CounterService(string counterPath) {
			m_CounterFolder = counterPath;
		}

		public string GetCounterDescription(string counterName) {
			if (File.Exists(Path.Combine(m_CounterFolder, counterName))) {
				return File.ReadAllLines(Path.Combine(m_CounterFolder, counterName))[0];
			} else {
				throw new FileNotFoundException();
			}
		}

		public CounterData GetDateCounter(string counterName) {
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
		}

		/// <summary>
		/// Reset a counter.
		/// </summary>
		/// <returns>True if this reset ended a highscore.</returns>
		public bool ResetDateCounter(string counterName) {
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
		}
	}

	public struct CounterData {
		public string Description;
		public DateTime LastResetDate;
		public TimeSpan HighScoreTimespan;

		public CounterData(string description, DateTime lastResetDate, TimeSpan highScoreTimespan) {
			this.Description = description;
			this.LastResetDate = lastResetDate;
			this.HighScoreTimespan = highScoreTimespan;
		}
	}
}
