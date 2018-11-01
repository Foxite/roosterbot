using System;
using System.IO;

namespace MiscStuffComponent.Services {
	public class CounterService {
		private string m_CounterPath;

		public CounterService(string counterPath) {
			m_CounterPath = counterPath;
		}

		public string GetCounterDescription(string counterName) {
			if (File.Exists(Path.Combine(m_CounterPath, counterName))) {
				return File.ReadAllText(Path.Combine(m_CounterPath, counterName));
			} else {
				throw new FileNotFoundException();
			}
		}

		public DateTime GetDateCounter(string counterName) {
			if (File.Exists(Path.Combine(m_CounterPath, counterName))) {
				return File.GetLastWriteTime(Path.Combine(m_CounterPath, counterName));
			} else {
				throw new FileNotFoundException();
			}
		}

		public void ResetDateCounter(string counterName) {
			if (File.Exists(Path.Combine(m_CounterPath, counterName))) {
				File.SetLastWriteTime(Path.Combine(m_CounterPath, counterName), DateTime.Now);
			} else {
				throw new FileNotFoundException();
			}
		}
	}
}
