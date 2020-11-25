using System;
using System.Collections.Generic;

namespace RoosterBot.PublicTransit {
	/// <summary>
	/// A trip from one location to another, consisting of zero or more transfers.
	/// </summary>
	public record Journey(
		IList<JourneyNotification> Notifications,
		
		/// <summary>
		/// The amount of transfers is not the same as Components.Count - 1 because walking is inherent to transfers.
		/// For example: taking a bus to a train station, walking to a platform, and taking a train, is 3 components, but only 1 transfer.
		/// </summary>
		int Transfers,
		TimeSpan PlannedDuration,
		TimeSpan ActualDuration,
		int DepartureDelayMinutes,
		int ArrivalDelayMinutes,
		DateTime PlannedDepartureTime,
		DateTime ActualDepartureTime,
		DateTime PlannedArrivalTime,
		DateTime ActualArrivalTime,
		JourneyStatus Status,
		IList<JourneyComponent> Components
	);
	
	public record JourneyNotification(
		string Id,
		bool Unplanned,
		string Text
	);

	/// <summary>
	/// A trip from one location to another without transferring.
	/// </summary>
	public record JourneyComponent(
		string Carrier,
		string TransportType,
		JourneyComponentStatus Status,
		JourneyStop Departure,
		JourneyStop Arrival
	);

	public record JourneyStop(
		string Location,
		string Platform,
		bool PlatformModified,
		DateTime Time,
		int DelayMinutes
	);

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
