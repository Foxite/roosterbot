using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using AngleSharp.Common;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Qmmands;

namespace RoosterBot.Tools {
	[Group("rs"), Name("Runescape")]
	public class RunescapeModule : RoosterModule {
		public HttpClient Http { get; set; } = null!;

		[Command("vis"), Description("Today's Rune Goldberg combinations")]
		public async Task<CommandResult> Vis() {
			var vis = JObject.Parse(await Http.GetStringAsync("https://runeguide.info/alt1/viswax/api/getVisWaxCombo.php")).ToObject<IDictionary<string, VisData>>()!["75,76,378,66118165"];

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

			// ReSharper disable InconsistentNaming
			public VisData(string? data, string source, int lastupdated, int slot1_best, VisSlotOption?[] slot1_other, int slot2_1_best, VisSlotOption?[] slot2_1_other, int slot2_2_best, VisSlotOption[] slot2_2_other, int slot2_3_best, VisSlotOption?[] slot2_3_other, string lastupdated_format) {
				// ReSharper restore InconsistentNaming
				Data = data;
				Source = source;
				LastUpdated = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(lastupdated);

				Slot1 = GetVisSlotOptionList(new VisSlotOption(slot1_best, 30), slot1_other);
				//Slot1 = (slot1_other ?? Array.Empty<VisSlotOption>()).Append(new VisSlotOption(slot1_best, 30)).OrderByDescending(vso => vso.Profit).ToList();

				Slot2 = new List<List<VisSlotOption>>() {
					GetVisSlotOptionList(new VisSlotOption(slot2_1_best, 30), slot2_1_other),
					GetVisSlotOptionList(new VisSlotOption(slot2_2_best, 30), slot2_2_other),
					GetVisSlotOptionList(new VisSlotOption(slot2_3_best, 30), slot2_3_other),
					//(slot2_1_other ?? Array.Empty<VisSlotOption>()).Append(new VisSlotOption(slot2_1_best, 30)).OrderByDescending(vso => vso.Profit).ToList(),
					//(slot2_2_other ?? Array.Empty<VisSlotOption>()).Append(new VisSlotOption(slot2_2_best, 30)).OrderByDescending(vso => vso.Profit).ToList(),
					//(slot2_3_other ?? Array.Empty<VisSlotOption>()).Append(new VisSlotOption(slot2_3_best, 30)).OrderByDescending(vso => vso.Profit).ToList()
				};
			}

			private static List<VisSlotOption> GetVisSlotOptionList(VisSlotOption best, VisSlotOption?[] others) {
				var ret = new List<VisSlotOption>(1 + (others?.Length ?? 0));
				ret.Add(best);
				if (others != null) {
					ret.AddRange(others.WhereNotNull().Where(vso => vso.Id != best.Id));
				}
				ret.Sort((left, right) => left.Profit > right.Profit ? 1 : -1);
				return ret;
			}

			[JsonConverter(typeof(VisSlotOptionConverter))]
			public class VisSlotOption {
				private static readonly Dictionary<int, (string Name, string Emote, int Price, int Amount)> s_NameLut = new Dictionary<int, (string Name, string Emote, int Price, int Amount)>() {
					{ 556, ("Air", "<:air_rune:831129078580641802>", 71, 1000) },
					{ 9075, ("Astral", "<:astral_rune:831129080136728586>", 435, 300) },
					{ 565, ("Blood", "<:blood_rune:831129074255659009>", 562, 350) },
					{ 559, ("Body", "<:body_rune:831129076977893416>", 24, 200) },
					{ 562, ("Chaos", "<:chaos_rune:831128676480843806>", 161, 500) },
					{ 564, ("Cosmic", "<:cosmic_rune:831128677558910988>", 310, 400) },
					{ 560, ("Death", "<:death_rune:831128687008022588>", 253, 400) },
					{ 4696, ("Dust", "<:dust_rune:831128687855665193>", 1222, 500) },
					{ 557, ("Earth", "<:earth_rune:831128685402521602>", 17, 1000) },
					{ 554, ("Fire", "<:fire_rune:831128683548639273>", 116, 1000) },
					{ 4699, ("Lava", "<:lava_rune:831128680943583253>", 969, 500) },
					{ 563, ("Law", "<:law_rune:831128682369908736>", 381, 300) },
					{ 558, ("Mind", "<:mind_rune:831129087405064192>", 16, 2000) },
					{ 4695, ("Mist", "<:mist_rune:831129088625606657>", 1092, 500) },
					{ 4698, ("Mud", "<:mud_rune:831129090425094184>", 1075, 300) },
					{ 561, ("Nature", "<:nature_rune:831129086038245416>", 297, 350) },
					{ 4697, ("Smoke", "<:smoke_rune:831129081562267648>", 963, 500) },
					{ 566, ("Soul", "<:soul_rune:831129082934722600>", 2871, 300) },
					{ 4694, ("Steam", "<:steam_rune:831129084347154472>", 920, 500) },
					{ 555, ("Water", "<:water_rune:831129091902144552>", 20, 1000) },
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

				public class VisSlotOptionConverter : JsonConverter {
					public override bool CanWrite => false;
					public override bool CanRead => true;

					public override bool CanConvert(Type objectType) => typeof(VisSlotOption).IsAssignableFrom(objectType);
					
					public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotSupportedException();
					
					public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) {
						//reader.Read();
						Console.WriteLine(reader.TokenType);
						if (reader.TokenType == JsonToken.StartArray) {
							reader.Skip(); // reader.TokenType == JsonToken.EndArray
							return null;
						} else {
							var jo = serializer.Deserialize<JObject>(reader)!;
							return new VisSlotOption(jo["id"]!.ToObject<int>(), jo["vis"]!.ToObject<int>());
						}
					}
				}
			}
		}


		public enum Skill {
			Attack, Strength, Defence, Ranged, Prayer, Magic, Runecrafting, Construction, Dungeoneering, Archaeology,
			Constitution, Agility, Herblore, Thieving, Crafting, Fletching, Slayer, Hunter, Divination, Sailing,
			Mining, Smithing, Fishing, Cooking, Firemaking, Woodcutting, Farming, Summoning, Invention,
		}

		private static int GetSkillingPetModifier(Skill skill) => skill switch {
			Skill.Archaeology => 40,
			Skill.Attack => 48,
			Skill.Constitution => 16,
			Skill.Construction => 30,
			Skill.Cooking => 25,
			Skill.Crafting => 20,
			Skill.Defence => 48,
			Skill.Dungeoneering => 25,
			Skill.Farming => 12,
			Skill.Fletching => 20,
			Skill.Herblore => 45,
			Skill.Invention => 180,
			Skill.Magic => 48,
			Skill.Prayer => 40,
			Skill.Ranged => 48,
			Skill.Runecrafting => 28,
			Skill.Slayer => 42,
			Skill.Sailing => 420,
			Skill.Smithing => 20,
			Skill.Strength => 48,
			Skill.Summoning => 23,
			_ => 1
		};

		private static int GetVirtualLevel(Skill skill, int xp) {
			if (skill == Skill.Invention || skill == Skill.Sailing) {
				throw new NotImplementedException();
			} else {
				double fourTimesXpAtLevel = 0;
				for (double level = 1f; level < 120f; level += 1) {
					fourTimesXpAtLevel += Math.Floor(level + 300 * Math.Pow(2, level / 7));
					if (Math.Floor(fourTimesXpAtLevel / 4) > xp) {
						return (int) level;
					}
				}
				return 320;
			}
		}

		private static double GetSkillingPetChance(Skill skill, double xpOrTicks, int level) {
			return xpOrTicks * level / (50_000_000d * GetSkillingPetModifier(skill));
		}

		[Command("pet")]
		public CommandResult SkillingPetChance(Skill skill, double xpPerTry, double xpOrTicks, int xp, int itemsPerRound, int secondsPerRound) {
			// double xpPerTry = 198.5f;
			// double xpOrTicks = xpPerTry;
			// int xp = 2521912;
			// int itemsPerRound = 14;
			// int secondsPerRound = 24;
			// Skill skill = Skill.Herblore;


			if (skill == Skill.Invention || skill == Skill.Sailing) {
				return TextResult.Error("Not yet implemented");
			}

			int levelInitial = GetVirtualLevel(skill, xp);
			double chanceInitial = GetSkillingPetChance(skill, xpOrTicks, levelInitial);
			double triesInitial = Math.Log(1d - 0.5d, 1d - chanceInitial);
			TimeSpan timeInitial = TimeSpan.FromSeconds(triesInitial * secondsPerRound / itemsPerRound);

			double triesRefined = triesInitial;
			int levelRefined = levelInitial;
			int prevLevelRefined; // I thought you should be able to declare this one in the do/while block, but you can't

			int iterations = 0;
			do {
				prevLevelRefined = levelRefined;
				levelRefined = GetVirtualLevel(skill, (int) Math.Floor(xp + triesRefined * xpPerTry * 0.5d));
				double chanceRefined = GetSkillingPetChance(skill, xpOrTicks, levelRefined);
				triesRefined = Math.Log(1d - 0.5d, 1d - chanceRefined);
				iterations++;
			} while (levelRefined != prevLevelRefined && iterations < 20);

			if (iterations >= 19) {
				Console.WriteLine("Broke out of infinite loop");
			}

			TimeSpan timeRefined = TimeSpan.FromSeconds(triesRefined * secondsPerRound / itemsPerRound);
			int levelAtTries = GetVirtualLevel(skill, (int) Math.Floor(xp + xpPerTry * triesRefined));
			
			//return TextResult.Info(
			return TextResult.Info($@"Raw chance: {(chanceInitial * 100):N6}%
Tries for 50% chance (initial): {triesInitial:N1}
Time for that many tries (initial): {GetHumanTime(timeInitial)}

Tries for 50% chance (refined): {triesRefined:N1}
Time for that many tries (refined): {GetHumanTime(timeRefined)}
Level after that many tries: {levelAtTries}");
		}

		private static string GetHumanTime(TimeSpan ts) {
			if (Math.Abs(ts.TotalHours) > 24) {
				return $"{ts.TotalDays:N1} days";
			}
			if (Math.Abs(ts.TotalMinutes) > 60) {
				return $"{ts.TotalHours:N1} hours";
			}
			if (Math.Abs(ts.TotalSeconds) > 60) {
				return $"{ts.TotalMinutes:N1} minutes";
			}
			return $"{ts.TotalSeconds:N1} seconds";
		}
	}
}
