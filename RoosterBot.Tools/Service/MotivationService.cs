using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RoosterBot.Tools {
	public sealed class MotivationService : IDisposable {
		private readonly List<MotivationProvider> m_Providers;
		private readonly Random m_RNG;

		internal MotivationService(Random rng) {
			m_Providers = new List<MotivationProvider>();
			m_RNG = rng;
		}

		public void AddProvider<T>() where T : MotivationProvider, new() => AddProvider(new T());
		public void AddProvider(MotivationProvider provider) {
			m_Providers.Add(provider);
		}

		public string GetQuote(CultureInfo culture) {
			if (m_Providers.Count == 0) {
				Logger.Error("Motivation", "Tried to get a motivational quote, but there are no providers installed! In lieu of a quote, you will be mocked.");
				return "Here's a quote for you: the bot administrator is an idiot who does not read instructions prior to installing arbitrary components into his bot.";
			} else {
				return m_Providers[m_RNG.Next(0, m_Providers.Count)].GetQuote(culture);
			}
		}

		#region IDisposable Support
		private bool m_DisposedValue = false;

		public void Dispose() {
			if (!m_DisposedValue) {
				foreach (IDisposable disposable in m_Providers.OfType<IDisposable>()) {
					disposable.Dispose();
				}

				m_DisposedValue = true;
			}
		}
		#endregion
	}
}