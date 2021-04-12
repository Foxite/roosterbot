using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Common;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using Qmmands;

namespace RoosterBot.Tools {
	[Group("rs"), Name("Runescape")]
	public class RunescapeModule : RoosterModule {
		public HttpClient Http { get; set; } = null!;

		[Command("vis"), Description("Today's Rune Goldberg combinations")]
		public async Task<CommandResult> Vis() {
			var vis = JObject.Parse(await Http.GetStringAsync("https://runeguide.info/alt1/viswax/api/getVisWaxCombo.php")).ToObject<IDictionary<string, VisData>>().First().Value;
			
			var sb = new StringBuilder("Runes for today:\n");
			sb.AppendLine($"- Slot 1: {string.Join(", ", vis.Slot1.Select(vso => $"{vso.Emote} {vso.ProfitFormat}"))}");
			for (int i = 0; i <= 2; i++) {
				sb.AppendLine($"- Slot 2/{i + 1}: {string.Join(", ", vis.Slot2[i].Select(vso => $"{vso.Emote} {vso.ProfitFormat}"))}");
			}

			return TextResult.Info(sb.ToString());
		}

		private class VisData {
			public string? Data { get; }
			public string Source { get; }
			public DateTime LastUpdated { get; }
			public IReadOnlyList<VisSlotOption> Slot1 { get; }
			public IReadOnlyList<IReadOnlyList<VisSlotOption>> Slot2 { get; }

			public VisData(string? data, string source, int lastupdated, int slot1_best, VisSlotOption[] slot1_other, int slot2_1_best, VisSlotOption[] slot2_1_other, int slot2_2_best, VisSlotOption[] slot2_2_other, int slot2_3_best, VisSlotOption[] slot2_3_other, string lastupdated_format) {
				Data = data;
				Source = source;
				LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(lastupdated);

				Slot1 = slot1_other.Append(new VisSlotOption(slot1_best, 30)).OrderByDescending(vso => vso.Profit).ToList();

				Slot2 = new List<List<VisSlotOption>>() {
					slot2_1_other.Append(new VisSlotOption(slot2_1_best, 30)).OrderByDescending(vso => vso.Profit).ToList(),
					slot2_2_other.Append(new VisSlotOption(slot2_2_best, 30)).OrderByDescending(vso => vso.Profit).ToList(),
					slot2_3_other.Append(new VisSlotOption(slot2_3_best, 30)).OrderByDescending(vso => vso.Profit).ToList()
				};
			}
			
			public class VisSlotOption {
				private static readonly Dictionary<int, (string Name, string Emote, int Price, int Amount)> s_NameLut = new Dictionary<int, (string Name, string Emote, int Price, int Amount)>() {
					{ 556,  ("Air",    "<:air_rune:831129078580641802>",    71,   1000) },
					{ 9075, ("Astral", "<:astral_rune:831129080136728586>", 435,  300 ) },
					{ 565,  ("Blood",  "<:blood_rune:831129074255659009>",  562,  350 ) },
					{ 559,  ("Body",   "<:body_rune:831129076977893416>",   24,   200 ) },
					{ 562,  ("Chaos",  "<:chaos_rune:831128676480843806>",  161,  500 ) },
					{ 564,  ("Cosmic", "<:cosmic_rune:831128677558910988>", 310,  400 ) },
					{ 560,  ("Death",  "<:death_rune:831128687008022588>",  253,  400 ) },
					{ 4696, ("Dust",   "<:dust_rune:831128687855665193>",   1222, 500 ) },
					{ 557,  ("Earth",  "<:earth_rune:831128685402521602>",  17,   1000) },
					{ 554,  ("Fire",   "<:fire_rune:831128683548639273>",   116,  1000) },
					{ 4699, ("Lava",   "<:lava_rune:831128680943583253>",   969,  500 ) },
					{ 563,  ("Law",    "<:law_rune:831128682369908736>",    381,  300 ) },
					{ 558,  ("Mind",   "<:mind_rune:831129087405064192>",   16,   2000) },
					{ 4695, ("Mist",   "<:mist_rune:831129088625606657>",   1092, 500 ) },
					{ 4698, ("Mud",    "<:mud_rune:831129090425094184>",    1075, 300 ) },
					{ 561,  ("Nature", "<:nature_rune:831129086038245416>", 297,  350 ) },
					{ 4697, ("Smoke",  "<:smoke_rune:831129081562267648>",  963,  500 ) },
					{ 566,  ("Soul",   "<:soul_rune:831129082934722600>",   2871, 300 ) },
					{ 4694, ("Steam",  "<:steam_rune:831129084347154472>",  920,  500 ) },
					{ 555,  ("Water",  "<:water_rune:831129091902144552>",  20,   1000) },
				};
				
				public int Id { get; }
				public string Name { get; }
				public string Emote { get; }
				public int Vis { get; }
				public int Price { get; }
				public int Amount { get; }
				public int Profit => 30 * 12358 - Price * Amount;

				public string ProfitFormat {
					get {
						if (Math.Abs(Profit) >= 1_000_000) {
							return (Profit / 1_000_000) + "m";
						} else if (Math.Abs(Profit) >= 10_000) {
							return (Profit / 1_000) + "k";
						} else {
							return Profit.ToString();
						}
					}
				}

				public VisSlotOption(int id, int vis) {
					Id = id;
					Name = s_NameLut[id].Name;
					Emote = s_NameLut[id].Emote;
					Vis = vis;
					Price = s_NameLut[id].Price;
					Amount = s_NameLut[id].Amount;
				}
			}
		}
	}
}