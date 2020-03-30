using System.Globalization;

namespace RoosterBot.Tools {
	public abstract class MotivationProvider {
		public abstract string GetQuote(CultureInfo culture);
	}
}