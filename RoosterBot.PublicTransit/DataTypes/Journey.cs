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
			switch (input) {
			case "GEWIJZIGD":
				return JourneyStatus.Changed;
			case "VERTRAAGD":
				return JourneyStatus.Delayed;
			case "NIEUW":
				return JourneyStatus.New;
			case "NIET-OPTIMAAL":
				return JourneyStatus.NotOptimal;
			case "NIET-MOGELIJK":
				return JourneyStatus.NotPossible;
			case "PLAN-GEWIJZIGD":
				return JourneyStatus.PlanChanged;
			case "VOLGENS-PLAN":
			default:
				return JourneyStatus.OnSchedule;
			}
		}

		public static JourneyComponentStatus JCStatusFromString(string input) {
			switch (input) {
			case "GEANNULEERD":
				return JourneyComponentStatus.Cancelled;
			case "GEWIJZIGD":
				return JourneyComponentStatus.Changed;
			case "OVERSTAP-NIET-MOGELIJK":
				return JourneyComponentStatus.TransferNotPossible;
			case "VERTRAAGD":
				return JourneyComponentStatus.Delayed;
			case "NIEUW":
				return JourneyComponentStatus.New;
			case "VOLGENS-PLAN":
			default:
				return JourneyComponentStatus.OnSchedule;
			}
		}

		public static string HumanStringFromJStatus(JourneyStatus status) {
			switch (status) {
			case JourneyStatus.OnSchedule:
				return "Op tijd";
			case JourneyStatus.Changed:
				return "Gewijzigd";
			case JourneyStatus.Delayed:
				return "Vertraagd";
			case JourneyStatus.New:
				return "Nieuw";
			case JourneyStatus.NotOptimal:
				return "Niet optimaal";
			case JourneyStatus.NotPossible:
				return "Niet mogelijk";
			case JourneyStatus.PlanChanged:
				return "Plan gewijzigd";
			default:
				return "ERROR";
			}
		}

		public static string HummanStringFromJCStatus(JourneyComponentStatus status) {
			switch (status) {
			case JourneyComponentStatus.OnSchedule:
				return "Op tijd";
			case JourneyComponentStatus.Cancelled:
				return "Geannulleerd";
			case JourneyComponentStatus.Changed:
				return "Gewijzigd";
			case JourneyComponentStatus.TransferNotPossible:
				return "Overstap niet mogelijk";
			case JourneyComponentStatus.Delayed:
				return "Vertraagd";
			case JourneyComponentStatus.New:
				return "Nieuw";
			default:
				return "ERROR";
			}
		}
	}
}
