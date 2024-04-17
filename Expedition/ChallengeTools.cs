using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Modding.Passages;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Expedition;

public static class ChallengeTools
{
	public class ExpeditionCreature
	{
		public CreatureTemplate.Type creature;

		public int points;

		public int spawns;
	}

	public static string[] creatureNames;

	public static Dictionary<string, Dictionary<string, Vector2>> VistaLocations;

	public static List<AbstractPhysicalObject.AbstractObjectType> ObjectTypes;

	public static int[] echoScores = new int[ExtEnum<GhostWorldPresence.GhostID>.values.Count];

	public static Dictionary<WinState.EndgameID, int> achievementScores;

	public static Dictionary<string, int> creatureScores;

	public static Dictionary<string, List<ExpeditionCreature>> creatureSpawns;

	public static List<string> regionBlacklist = new List<string> { "HR", "MS" };

	public static List<string> creatureBlacklist = new List<string> { "TerrorLongLegs" };

	public static InGameTranslator IGT => Custom.rainWorld.inGameTranslator;

	public static void CreatureName(ref string[] creatureNames)
	{
		creatureNames = new string[ExtEnum<CreatureTemplate.Type>.values.Count];
		creatureNames[(int)CreatureTemplate.Type.Slugcat] = IGT.Translate("Slugcats");
		creatureNames[(int)CreatureTemplate.Type.GreenLizard] = IGT.Translate("Green Lizards");
		creatureNames[(int)CreatureTemplate.Type.PinkLizard] = IGT.Translate("Pink Lizards");
		creatureNames[(int)CreatureTemplate.Type.BlueLizard] = IGT.Translate("Blue Lizards");
		creatureNames[(int)CreatureTemplate.Type.WhiteLizard] = IGT.Translate("White Lizards");
		creatureNames[(int)CreatureTemplate.Type.BlackLizard] = IGT.Translate("Black Lizards");
		creatureNames[(int)CreatureTemplate.Type.YellowLizard] = IGT.Translate("Yellow Lizards");
		creatureNames[(int)CreatureTemplate.Type.CyanLizard] = IGT.Translate("Cyan Lizards");
		creatureNames[(int)CreatureTemplate.Type.RedLizard] = IGT.Translate("Red Lizards");
		creatureNames[(int)CreatureTemplate.Type.Salamander] = IGT.Translate("Salamander");
		creatureNames[(int)CreatureTemplate.Type.CicadaA] = IGT.Translate("White Cicadas");
		creatureNames[(int)CreatureTemplate.Type.CicadaB] = IGT.Translate("Black Cicadas");
		creatureNames[(int)CreatureTemplate.Type.Snail] = IGT.Translate("Snails");
		creatureNames[(int)CreatureTemplate.Type.PoleMimic] = IGT.Translate("Pole Mimics");
		creatureNames[(int)CreatureTemplate.Type.TentaclePlant] = IGT.Translate("Monster Kelp");
		creatureNames[(int)CreatureTemplate.Type.Scavenger] = IGT.Translate("Scavengers");
		creatureNames[(int)CreatureTemplate.Type.Vulture] = IGT.Translate("Vultures");
		creatureNames[(int)CreatureTemplate.Type.KingVulture] = IGT.Translate("King Vultures");
		creatureNames[(int)CreatureTemplate.Type.SmallCentipede] = IGT.Translate("Small Centipedes");
		creatureNames[(int)CreatureTemplate.Type.Centipede] = IGT.Translate("Large Centipedes");
		creatureNames[(int)CreatureTemplate.Type.RedCentipede] = IGT.Translate("Red Centipedes");
		creatureNames[(int)CreatureTemplate.Type.Centiwing] = IGT.Translate("Centiwings");
		creatureNames[(int)CreatureTemplate.Type.LanternMouse] = IGT.Translate("Lantern Mice");
		creatureNames[(int)CreatureTemplate.Type.BigSpider] = IGT.Translate("Large Spiders");
		creatureNames[(int)CreatureTemplate.Type.SpitterSpider] = IGT.Translate("Spitter Spiders");
		creatureNames[(int)CreatureTemplate.Type.MirosBird] = IGT.Translate("Miros Birds");
		creatureNames[(int)CreatureTemplate.Type.BrotherLongLegs] = IGT.Translate("Brother Long Legs");
		creatureNames[(int)CreatureTemplate.Type.DaddyLongLegs] = IGT.Translate("Daddy Long Legs");
		creatureNames[(int)CreatureTemplate.Type.TubeWorm] = IGT.Translate("Tube Worms");
		creatureNames[(int)CreatureTemplate.Type.EggBug] = IGT.Translate("Egg Bugs");
		creatureNames[(int)CreatureTemplate.Type.DropBug] = IGT.Translate("Dropwigs");
		creatureNames[(int)CreatureTemplate.Type.BigNeedleWorm] = IGT.Translate("Large Noodleflies");
		creatureNames[(int)CreatureTemplate.Type.JetFish] = IGT.Translate("Jetfish");
		creatureNames[(int)CreatureTemplate.Type.BigEel] = IGT.Translate("Leviathans");
		creatureNames[(int)CreatureTemplate.Type.Deer] = IGT.Translate("Rain Deer");
		creatureNames[(int)CreatureTemplate.Type.Fly] = IGT.Translate("Batflies");
		if (ModManager.MSC)
		{
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.MirosVulture] = IGT.Translate("Miros Vultures");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.MotherSpider] = IGT.Translate("Mother Spiders");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.EelLizard] = IGT.Translate("Eel Lizards");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.SpitLizard] = IGT.Translate("Caramel Lizards");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs] = IGT.Translate("Terror Long Legs");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.AquaCenti] = IGT.Translate("Aquapedes");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.FireBug] = IGT.Translate("Firebugs");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.Inspector] = IGT.Translate("Inspectors");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.Yeek] = IGT.Translate("Yeek");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.BigJelly] = IGT.Translate("Large Jellyfish");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.StowawayBug] = IGT.Translate("Stowaway Bugs");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard] = IGT.Translate("Strawberry Lizards");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite] = IGT.Translate("Elite Scavengers");
			creatureNames[(int)MoreSlugcatsEnums.CreatureTemplateType.SlugNPC] = IGT.Translate("Slugcats");
		}
	}

	public static List<string> PearlRegionBlackList()
	{
		return (from x in Custom.rainWorld.regionDataPearls
			where x.Value.Count == 0
			select x.Key.ToUpper()).ToList();
	}

	public static bool ValidRegionPearl(string region, DataPearl.AbstractDataPearl.DataPearlType type)
	{
		if (Custom.rainWorld.regionDataPearls.TryGetValue(region.ToLower(), out var value))
		{
			return value.Contains(type);
		}
		return false;
	}

	public static void EchoScore(ref int[] echoScores)
	{
		echoScores = new int[ExtEnum<GhostWorldPresence.GhostID>.values.Count];
		echoScores[(int)GhostWorldPresence.GhostID.CC] = 50;
		echoScores[(int)GhostWorldPresence.GhostID.LF] = 50;
		echoScores[(int)GhostWorldPresence.GhostID.SB] = 35;
		echoScores[(int)GhostWorldPresence.GhostID.SH] = 50;
		echoScores[(int)GhostWorldPresence.GhostID.SI] = 50;
		echoScores[(int)GhostWorldPresence.GhostID.UW] = 35;
		if (ModManager.MSC)
		{
			echoScores[(int)MoreSlugcatsEnums.GhostID.CL] = 50;
			echoScores[(int)MoreSlugcatsEnums.GhostID.LC] = 50;
			echoScores[(int)MoreSlugcatsEnums.GhostID.MS] = 50;
			echoScores[(int)MoreSlugcatsEnums.GhostID.SL] = 50;
			echoScores[(int)MoreSlugcatsEnums.GhostID.UG] = 50;
		}
	}

	public static void GenerateAchievementScores()
	{
		achievementScores = new Dictionary<WinState.EndgameID, int>
		{
			{
				WinState.EndgameID.Chieftain,
				80
			},
			{
				WinState.EndgameID.DragonSlayer,
				60
			},
			{
				WinState.EndgameID.Friend,
				50
			},
			{
				WinState.EndgameID.Hunter,
				40
			},
			{
				WinState.EndgameID.Monk,
				30
			},
			{
				WinState.EndgameID.Outlaw,
				25
			},
			{
				WinState.EndgameID.Saint,
				30
			},
			{
				WinState.EndgameID.Scholar,
				70
			},
			{
				WinState.EndgameID.Traveller,
				80
			}
		};
		if (ModManager.MSC)
		{
			achievementScores.Add(MoreSlugcatsEnums.EndgameID.Gourmand, 80);
			achievementScores.Add(MoreSlugcatsEnums.EndgameID.Martyr, 75);
			achievementScores.Add(MoreSlugcatsEnums.EndgameID.Nomad, 60);
			achievementScores.Add(MoreSlugcatsEnums.EndgameID.Pilgrim, 80);
		}
		foreach (CustomPassage registeredPassage in CustomPassages.RegisteredPassages)
		{
			achievementScores.Add(registeredPassage.ID, registeredPassage.ExpeditionScore);
		}
	}

	public static void GenerateObjectTypes()
	{
		ObjectTypes = new List<AbstractPhysicalObject.AbstractObjectType>
		{
			AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant,
			AbstractPhysicalObject.AbstractObjectType.FlareBomb,
			AbstractPhysicalObject.AbstractObjectType.FlyLure,
			AbstractPhysicalObject.AbstractObjectType.JellyFish,
			AbstractPhysicalObject.AbstractObjectType.Lantern,
			AbstractPhysicalObject.AbstractObjectType.Mushroom,
			AbstractPhysicalObject.AbstractObjectType.PuffBall,
			AbstractPhysicalObject.AbstractObjectType.ScavengerBomb,
			AbstractPhysicalObject.AbstractObjectType.VultureMask
		};
	}

	public static string ItemName(AbstractPhysicalObject.AbstractObjectType type)
	{
		if (type == AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant)
		{
			return IGT.Translate("Firecracker Plants");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
		{
			return IGT.Translate("Flare Bombs");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.FlyLure)
		{
			return IGT.Translate("Fly Lures");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.JellyFish)
		{
			return IGT.Translate("Jellyfish");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.Lantern)
		{
			return IGT.Translate("Scavenger Lanterns");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.Mushroom)
		{
			return IGT.Translate("Mushrooms");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.PuffBall)
		{
			return IGT.Translate("Puff Balls");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
		{
			return IGT.Translate("Scavenger Bombs");
		}
		if (type == AbstractPhysicalObject.AbstractObjectType.VultureMask)
		{
			return IGT.Translate("Vulture Masks");
		}
		return type.value;
	}

	public static void GenerateVistaLocations()
	{
		VistaLocations = new Dictionary<string, Dictionary<string, Vector2>>();
		string[] regionNames = Custom.rainWorld.progression.regionNames;
		foreach (string text in regionNames)
		{
			VistaLocations[text] = new Dictionary<string, Vector2>();
			string path = AssetManager.ResolveFilePath(Path.Combine("world", text, "vistas.txt"));
			if (!File.Exists(path))
			{
				continue;
			}
			string[] array = File.ReadAllLines(path);
			foreach (string text2 in array)
			{
				if (string.IsNullOrEmpty(text2.Trim()))
				{
					continue;
				}
				string[] array2 = text2.Split(',');
				if (array2.Length >= 3)
				{
					string text3 = array2[0];
					if (string.IsNullOrEmpty(text3) || !int.TryParse(array2[1], out var result) || !int.TryParse(array2[2], out var result2))
					{
						Custom.LogWarning("Failed to parse vista " + text2);
					}
					else
					{
						VistaLocations[text][text3] = new Vector2(result, result2);
					}
				}
			}
		}
	}

	public static ExpeditionCreature GetExpeditionCreature(SlugcatStats.Name slugcat, float difficulty)
	{
		int num = (int)(25.0 * Math.Pow(difficulty, 2.22));
		List<ExpeditionCreature> list = new List<ExpeditionCreature>();
		foreach (ExpeditionCreature item in creatureSpawns[slugcat.value])
		{
			if ((float)Math.Abs(num - item.points) <= Mathf.Lerp(5f, 17f, (float)Math.Pow(difficulty, 2.7)))
			{
				list.Add(item);
			}
		}
		return list[UnityEngine.Random.Range(0, list.Count - 1)];
	}

	public static void ParseCreatureSpawns()
	{
		if (creatureSpawns == null)
		{
			creatureSpawns = new Dictionary<string, List<ExpeditionCreature>>();
		}
		foreach (SlugcatStats.Name unlockedExpeditionSlugcat in ExpeditionGame.unlockedExpeditionSlugcats)
		{
			List<string> list = SlugcatStats.SlugcatStoryRegions(unlockedExpeditionSlugcat);
			list.RemoveAll((string x) => regionBlacklist.Contains(x));
			if (ModManager.MSC && (unlockedExpeditionSlugcat == SlugcatStats.Name.White || unlockedExpeditionSlugcat == SlugcatStats.Name.Yellow) && ExpeditionGame.unlockedExpeditionSlugcats.Contains(MoreSlugcatsEnums.SlugcatStatsName.Gourmand))
			{
				list.Add("OE");
			}
			List<ExpeditionCreature> list2 = new List<ExpeditionCreature>();
			Dictionary<string, int> dictionary = new Dictionary<string, int>();
			foreach (string item in list)
			{
				string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("world/" + item + "/world_" + item + ".txt"));
				bool flag = false;
				for (int i = 0; i < array.Length && !(array[i] == "END CREATURES"); i++)
				{
					if (flag)
					{
						if (array[i].Contains("LINEAGE"))
						{
							continue;
						}
						Match match = Regex.Match(array[i], "\\(([^\\(\\)]*)\\)");
						if (match.Success || array[i].Contains(" : "))
						{
							string[] array2 = match.Groups[1].Value.Split(',');
							if (Regex.IsMatch(array2[0], "X-") && array2.Contains(unlockedExpeditionSlugcat.value))
							{
								continue;
							}
							if (array[i].StartsWith("(" + unlockedExpeditionSlugcat.value) || !array[i].StartsWith("(") || (Regex.IsMatch(array2[0], "X-") && !array2[0].Contains(unlockedExpeditionSlugcat.value) && !array2.Contains(unlockedExpeditionSlugcat.value)))
							{
								string[] array3 = Regex.Split(Regex.Split(array[i], " : ")[1], ", ");
								foreach (string text in array3)
								{
									if (text.Contains("PreCycle"))
									{
										continue;
									}
									string[] array4 = Regex.Split(text, "-");
									string text2 = "";
									int result = 0;
									if (array4.Length == 2)
									{
										text2 = array4[1];
										result = 1;
									}
									else if (array4.Length == 3)
									{
										text2 = array4[1];
										if (!int.TryParse(array4[2], out result))
										{
											result = 1;
										}
									}
									else if (array4.Length == 4)
									{
										text2 = array4[1];
										if (!int.TryParse(array4[2], out result) && !int.TryParse(array4[3], out result))
										{
											result = 1;
											ExpLog.Log($"FAIL: {text} | Line {i} in {item}");
										}
									}
									if (string.IsNullOrEmpty(text2))
									{
										break;
									}
									try
									{
										text2 = WorldLoader.CreatureTypeFromString(text2).value;
									}
									catch
									{
										ExpLog.Log("FAILED TO PARSE: " + text2);
									}
									if (!dictionary.ContainsKey(text2) && text2 != "" && result > 0)
									{
										dictionary.Add(text2, result);
									}
									else if (dictionary.ContainsKey(text2) && text2 != "" && result > 0)
									{
										dictionary[text2] += result;
									}
								}
							}
						}
					}
					if (array[i] == "CREATURES")
					{
						flag = true;
					}
				}
			}
			foreach (KeyValuePair<string, int> item2 in dictionary)
			{
				int value = (creatureScores.TryGetValue(item2.Key, out value) ? value : 0);
				ExpeditionCreature expeditionCreature = new ExpeditionCreature
				{
					creature = new CreatureTemplate.Type(item2.Key),
					spawns = item2.Value,
					points = value
				};
				if (expeditionCreature.points > 0 && !creatureBlacklist.Contains(item2.Key))
				{
					list2.Add(expeditionCreature);
				}
			}
			if (!creatureSpawns.ContainsKey(unlockedExpeditionSlugcat.value))
			{
				creatureSpawns.Add(unlockedExpeditionSlugcat.value, list2);
			}
			else
			{
				creatureSpawns[unlockedExpeditionSlugcat.value] = list2;
			}
		}
		AppendAdditionalCreatureSpawns();
	}

	public static void AppendAdditionalCreatureSpawns()
	{
		if (ModManager.MSC)
		{
			int value;
			ExpeditionCreature item = new ExpeditionCreature
			{
				creature = MoreSlugcatsEnums.CreatureTemplateType.BigJelly,
				points = (creatureScores.TryGetValue(MoreSlugcatsEnums.CreatureTemplateType.BigJelly.value, out value) ? value : 0),
				spawns = 2
			};
			if (creatureSpawns.ContainsKey(MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value))
			{
				creatureSpawns[MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value].Add(item);
			}
			if (creatureSpawns.ContainsKey(MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value))
			{
				creatureSpawns[MoreSlugcatsEnums.SlugcatStatsName.Rivulet.value].Add(item);
			}
			if (creatureSpawns.ContainsKey(MoreSlugcatsEnums.SlugcatStatsName.Saint.value))
			{
				creatureSpawns[MoreSlugcatsEnums.SlugcatStatsName.Saint.value].Add(item);
			}
			int value2;
			ExpeditionCreature item2 = new ExpeditionCreature
			{
				creature = MoreSlugcatsEnums.CreatureTemplateType.StowawayBug,
				points = (creatureScores.TryGetValue(MoreSlugcatsEnums.CreatureTemplateType.StowawayBug.value, out value2) ? value2 : 0),
				spawns = 1
			};
			if (creatureSpawns.ContainsKey(MoreSlugcatsEnums.SlugcatStatsName.Saint.value))
			{
				creatureSpawns[MoreSlugcatsEnums.SlugcatStatsName.Saint.value].Add(item2);
			}
			if (creatureSpawns.ContainsKey(MoreSlugcatsEnums.SlugcatStatsName.Artificer.value))
			{
				creatureSpawns[MoreSlugcatsEnums.SlugcatStatsName.Artificer.value].Add(item2);
			}
		}
	}

	public static void GenerateCreatureScores(ref Dictionary<string, int> dict)
	{
		dict = new Dictionary<string, int>
		{
			{ "GreenLizard", 10 },
			{ "PinkLizard", 7 },
			{ "BlueLizard", 6 },
			{ "WhiteLizard", 8 },
			{ "BlackLizard", 7 },
			{ "YellowLizard", 6 },
			{ "CyanLizard", 9 },
			{ "RedLizard", 25 },
			{ "Salamander", 7 },
			{ "CicadaA", 2 },
			{ "CicadaB", 2 },
			{ "Snail", 1 },
			{ "PoleMimic", 2 },
			{ "TentaclePlant", 7 },
			{ "Scavenger", 6 },
			{ "Vulture", 15 },
			{ "KingVulture", 25 },
			{ "SmallCentipede", 4 },
			{ "Centipede", 4 },
			{ "RedCentipede", 25 },
			{ "Centiwing", 5 },
			{ "LanternMouse", 2 },
			{ "BigSpider", 4 },
			{ "SpitterSpider", 5 },
			{ "MirosBird", 16 },
			{ "BrotherLongLegs", 14 },
			{ "DaddyLongLegs", 25 },
			{ "TubeWorm", 1 },
			{ "EggBug", 2 },
			{ "DropBug", 5 },
			{ "BigNeedleWorm", 5 },
			{ "JetFish", 4 }
		};
		if (!ModManager.MSC)
		{
			return;
		}
		foreach (KeyValuePair<string, int> item in new Dictionary<string, int>
		{
			{ "MirosVulture", 25 },
			{ "MotherSpider", 4 },
			{ "EelLizard", 6 },
			{ "SpitLizard", 11 },
			{ "TerrorLongLegs", 25 },
			{ "AquaCenti", 10 },
			{ "FireBug", 5 },
			{ "Inspector", 12 },
			{ "Yeek", 2 },
			{ "BigJelly", 20 },
			{ "StowawayBug", 8 },
			{ "ZoopLizard", 6 },
			{ "ScavengerElite", 12 }
		})
		{
			if (!dict.ContainsKey(item.Key))
			{
				dict.Add(item.Key, item.Value);
			}
			else
			{
				dict[item.Key] = item.Value;
			}
		}
	}
}
