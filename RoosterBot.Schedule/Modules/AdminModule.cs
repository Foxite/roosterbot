using System.Collections.Generic;
using System.Linq;
using Qmmands;

namespace RoosterBot.Schedule {
	[HiddenFromList, RequireBotManager, Group("admin schedule")]
	public class AdminModule : RoosterModule {
		public ScheduleService Schedules { get; set; } = null!;

		[Command("provider show")]
		public CommandResult ShowProvider(string? identifierType = null) {
			IEnumerable<KeyValuePair<System.Type, IReadOnlyList<ScheduleProvider>>> providers = Schedules.Providers;
			if (identifierType != null) {
				string lowercaseFilter = identifierType.ToLower();
				providers = providers.Where(kvp => kvp.Key.Name.ToLower() == lowercaseFilter);
			}
			var cells = providers.Select(kvp => (new[] {
				(kvp.Key.IsGenericTypeDefinition ? kvp.Key.GetGenericTypeDefinition().FullName : kvp.Key.FullName)!, // Column 1: type name of identifier
				string.Join(", ", kvp.Value.SelectMany(provider => provider.AllowedChannels).Select(sr => sr.ToString())), // Column 2: list of allowed guild IDs
				string.Join(", ", kvp.Value.Select(provider => provider is MemoryScheduleProvider msp ? msp.End.ToLongDateString() : "N/A")) // Column 3: expiration date, if it's an MSP
			})).ToArray();

			return new PaginatedResult(new PaginatedTableEnumerator("All providers", new string[] { "Type", "Allowed channels", "Last record date" }, cells), "All providers");
		}
		// TODO: commands for adding/reloading providers using attached files
	}
}
