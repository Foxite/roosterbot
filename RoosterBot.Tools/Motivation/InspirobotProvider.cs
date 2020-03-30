using System;
using System.Globalization;
using System.Net;

namespace RoosterBot.Tools {
	internal sealed class InspirobotProvider : MotivationProvider, IDisposable {
		private readonly WebClient m_Web;

		public InspirobotProvider() {
			m_Web = new WebClient();
		}

		public override string GetQuote(CultureInfo culture) {
			return m_Web.DownloadString(GetUrl());
		}

		private string GetUrl() {
			if (DateTime.Today.Month == 12 && DateTime.Today.Day >= 21 && DateTime.Today.Day <= 25) {
				return "http://inspirobot.me/api?generate=true&season=xmas";
			} else {
				return "http://inspirobot.me/api?generate=true";
			}
		}

		#region IDisposable Support
		private bool m_DisposedValue = false;

		public void Dispose() {
			if (!m_DisposedValue) {
				m_Web.Dispose();

				m_DisposedValue = true;
			}
		}
		#endregion
	}
}
