#nullable disable
/* I used to be a big fan of this pattern:
 * 
 * var thing = new object() {
 *     Prop = value,
 *     Et = cetera
 * }
 * 
 * But this doesn't work with nullable reference types.
 * So I've abandoned it in favor of proper constructors almost everywhere, except here. The constructs for these classes would have the longest parameter lists in the entire solution,
 *  and I won't have that.
 */

using System;
using System.Collections.Generic;

namespace RoosterBot.PublicTransit {
	/// <summary>
	/// A trip from one location to another, 
	/// </summary>
	public class Journey {
		public IList<JourneyNotification> Notifications;

		/// <summary>
		/// The amount of transfers is not the same as Components.Count - 1 because walking is inherent to transfers.
		/// For example: taking a bus to a train station, walking to a platform, and taking a train, is 3 components, but only 1 transfer.
		/// </summary>
		public int Transfers;
		
		public TimeSpan PlannedDuration;
		public TimeSpan ActualDuration;

		public int DepartureDelayMinutes;
		public int ArrivalDelayMinutes;

		public DateTime PlannedDepartureTime;
		public DateTime ActualDepartureTime;
		public DateTime PlannedArrivalTime;
		public DateTime ActualArrivalTime;

		public JourneyStatus Status;

		public IList<JourneyComponent> Components;
	}
	
	public class JourneyNotification {
		public string Id;
		public bool Unplanned;
		public string Text;
	}

	/// <summary>
	/// A trip from one location to another without transferring.
	/// </summary>
	public class JourneyComponent {
		public string Carrier;
		public string TransportType;
		public JourneyComponentStatus Status;

		public JourneyStop Departure;
		public JourneyStop Arrival;
	}

	public class JourneyStop {
		public string Location;
		public string Platform;
		public bool PlatformModified;
		public DateTime Time;
		public int DelayMinutes;
	}

	public enum JourneyStatus {
		OnSchedule, Changed, Delayed, New, NotOptimal, NotPossible, PlanChanged
	}

	public enum JourneyComponentStatus {
		OnSchedule, Cancelled, Changed, TransferNotPossible, Delayed, New
	}

	public static class JourneyStatusFunctions {
		public static JourneyStatus JStatusFromString(string input) {
			return input switch {
				"GEWIJZIGD" => JourneyStatus.Changed,
				"VERTRAAGD" => JourneyStatus.Delayed,
				"NIEUW" => JourneyStatus.New,
				"NIET-OPTIMAAL" => JourneyStatus.NotOptimal,
				"NIET-MOGELIJK" => JourneyStatus.NotPossible,
				"PLAN-GEWIJZIGD" => JourneyStatus.PlanChanged,
				_ => JourneyStatus.OnSchedule,
			};
		}

		public static JourneyComponentStatus JCStatusFromString(string input) {
			return input switch {
				"GEANNULEERD" => JourneyComponentStatus.Cancelled,
				"GEWIJZIGD" => JourneyComponentStatus.Changed,
				"OVERSTAP-NIET-MOGELIJK" => JourneyComponentStatus.TransferNotPossible,
				"VERTRAAGD" => JourneyComponentStatus.Delayed,
				"NIEUW" => JourneyComponentStatus.New,
				_ => JourneyComponentStatus.OnSchedule,
			};
		}

		public static string HumanStringFromJStatus(JourneyStatus status) {
			return status switch {
				JourneyStatus.OnSchedule => "Op tijd",
				JourneyStatus.Changed => "Gewijzigd",
				JourneyStatus.Delayed => "Vertraagd",
				JourneyStatus.New => "Nieuw",
				JourneyStatus.NotOptimal => "Niet optimaal",
				JourneyStatus.NotPossible => "Niet mogelijk",
				JourneyStatus.PlanChanged => "Plan gewijzigd",
				_ => "ERROR",
			};
		}

		public static string HummanStringFromJCStatus(JourneyComponentStatus status) {
			return status switch {
				JourneyComponentStatus.OnSchedule => "Op tijd",
				JourneyComponentStatus.Cancelled => "Geannulleerd",
				JourneyComponentStatus.Changed => "Gewijzigd",
				JourneyComponentStatus.TransferNotPossible => "Overstap niet mogelijk",
				JourneyComponentStatus.Delayed => "Vertraagd",
				JourneyComponentStatus.New => "Nieuw",
				_ => "ERROR",
			};
		}
	}
}
