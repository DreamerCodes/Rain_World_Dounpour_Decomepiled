using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Menu;
using Menu.Remix;
using Modding.Expedition;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Expedition;

public static class ExpeditionProgression
{
	public class WinPackage
	{
		public SlugcatStats.Name slugcat;

		public List<Challenge> challenges;

		public int points;
	}

	public struct Quest
	{
		public string key;

		public string type;

		public Color color;

		public string[] conditions;

		public string[] reward;
	}

	public struct Mission
	{
		public string key;

		public string name;

		public List<Challenge> challenges;

		public List<string> requirements;

		public string slugcat;

		public string den;
	}

	private static readonly AGLog<FromStaticClass> Log = new AGLog<FromStaticClass>();

	public static int totalPerks;

	public static int currentPerks;

	public static int totalBurdens;

	public static int currentBurdens;

	public static int currentTracks;

	public static Dictionary<string, List<string>> perkGroups = new Dictionary<string, List<string>>();

	public static Dictionary<string, List<string>> burdenGroups = new Dictionary<string, List<string>>();

	public static List<Quest> questList = new List<Quest>();

	public static Dictionary<string, List<Quest>> customQuests = new Dictionary<string, List<Quest>>();

	public static List<Mission> missionList = new List<Mission>();

	public static Dictionary<string, List<Mission>> customMissions = new Dictionary<string, List<Mission>>();

	public static InGameTranslator IGT => Custom.rainWorld.inGameTranslator;

	public static void SetupPerkGroups()
	{
		perkGroups = new Dictionary<string, List<string>>();
		List<string> value = new List<string> { "unl-lantern", "unl-vulture", "unl-bomb", "unl-glow", "unl-backspear", "unl-karma", "unl-passage", "unl-slow" };
		perkGroups.Add("expedition", value);
		if (ModManager.MSC)
		{
			List<string> value2 = new List<string> { "unl-sing", "unl-electric", "unl-dualwield", "unl-explosionimmunity", "unl-explosivejump", "unl-crafting", "unl-agility", "unl-gun" };
			perkGroups.Add("moreslugcats", value2);
		}
		foreach (string groupName in CustomPerks.CustomPerkGroups)
		{
			if (perkGroups.TryGetValue(groupName, out var value3))
			{
				value3.AddRange(from x in CustomPerks.RegisteredPerks
					where x.Group == groupName
					select x.ID);
			}
			else
			{
				perkGroups.Add(groupName, (from x in CustomPerks.RegisteredPerks
					where x.Group == groupName
					select x.ID).ToList());
			}
		}
		ExpeditionManualDialog.topicKeys["perks"] = Mathf.CeilToInt((float)perkGroups.SelectMany((KeyValuePair<string, List<string>> x) => x.Value).Count() / 4f);
	}

	public static void SetupBurdenGroups()
	{
		burdenGroups = new Dictionary<string, List<string>>();
		List<string> value = new List<string> { "bur-blinded", "bur-doomed", "bur-hunted" };
		burdenGroups.Add("expedition", value);
		if (ModManager.MSC)
		{
			List<string> value2 = new List<string> { "bur-pursued" };
			burdenGroups.Add("moreslugcats", value2);
		}
		foreach (string groupName in CustomBurdens.CustomBurdenGroups)
		{
			if (burdenGroups.TryGetValue(groupName, out var value3))
			{
				value3.AddRange(from x in CustomBurdens.RegisteredBurdens
					where x.Group == groupName
					select x.ID);
			}
			else
			{
				burdenGroups.Add(groupName, (from x in CustomBurdens.RegisteredBurdens
					where x.Group == groupName
					select x.ID).ToList());
			}
		}
		ExpeditionManualDialog.topicKeys["burdens"] = Mathf.CeilToInt((float)burdenGroups.SelectMany((KeyValuePair<string, List<string>> x) => x.Value).Count() / 4f);
	}

	public static void CountUnlockables()
	{
		totalPerks = 0;
		currentPerks = 0;
		totalBurdens = 0;
		currentBurdens = 0;
		currentTracks = 0;
		totalPerks = (ModManager.MSC ? (perkGroups["expedition"].Count + perkGroups["moreslugcats"].Count) : perkGroups["expedition"].Count);
		totalBurdens = (ModManager.MSC ? (burdenGroups["expedition"].Count + burdenGroups["moreslugcats"].Count) : burdenGroups["expedition"].Count);
		totalPerks += CustomPerks.RegisteredPerks.Count;
		totalBurdens += CustomBurdens.RegisteredBurdens.Count;
		List<string> list = new List<string>();
		for (int i = 0; i < perkGroups.Keys.Count; i++)
		{
			list.AddRange(perkGroups.ElementAt(i).Value);
		}
		List<string> list2 = new List<string>();
		for (int j = 0; j < burdenGroups.Keys.Count; j++)
		{
			list2.AddRange(burdenGroups.ElementAt(j).Value);
		}
		Dictionary<string, string> unlockedSongs = GetUnlockedSongs();
		for (int k = 0; k < ExpeditionData.unlockables.Count; k++)
		{
			if (ExpeditionData.unlockables[k].StartsWith("unl-") && list.Contains(ExpeditionData.unlockables[k]))
			{
				currentPerks++;
			}
			if (ExpeditionData.unlockables[k].StartsWith("bur-") && list2.Contains(ExpeditionData.unlockables[k]))
			{
				currentBurdens++;
			}
			if (!ExpeditionData.unlockables[k].StartsWith("mus-"))
			{
				continue;
			}
			for (int l = 0; l < unlockedSongs.Values.Count; l++)
			{
				if (ExpeditionData.unlockables[k] == unlockedSongs.Keys.ElementAt(l))
				{
					currentTracks++;
				}
			}
		}
	}

	public static void EvaluateExpedition(WinPackage package)
	{
		int num = 0;
		int num2 = 0;
		ExpeditionGame.pendingCompletedQuests = new List<string>();
		ExpeditionData.totalPoints += package.points;
		ExpeditionData.totalWins++;
		if (ExpeditionData.slugcatWins.ContainsKey(package.slugcat.value))
		{
			ExpeditionData.slugcatWins[package.slugcat.value]++;
		}
		else
		{
			ExpeditionData.slugcatWins.Add(package.slugcat.value, 1);
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		for (int i = 0; i < package.challenges.Count; i++)
		{
			if (package.challenges[i].completed)
			{
				num++;
				ExpeditionData.totalChallengesCompleted++;
				if (package.challenges[i].hidden)
				{
					num2++;
					ExpeditionData.totalHiddenChallengesCompleted++;
				}
				string name = package.challenges[i].GetType().Name;
				if (dictionary.ContainsKey(name))
				{
					dictionary[name]++;
				}
				else
				{
					dictionary.Add(name, 1);
				}
				if (ExpeditionData.challengeTypes.ContainsKey(name))
				{
					ExpeditionData.challengeTypes[name]++;
				}
				else
				{
					ExpeditionData.challengeTypes.Add(name, 1);
				}
			}
		}
		List<Quest> list = new List<Quest>();
		list.AddRange(questList);
		if (customQuests.Count > 0)
		{
			foreach (KeyValuePair<string, List<Quest>> customQuest in customQuests)
			{
				if (customQuest.Value.Count > 0)
				{
					list.AddRange(customQuest.Value);
				}
			}
		}
		foreach (Quest item in list)
		{
			if (ExpeditionData.completedQuests.Contains(item.key))
			{
				continue;
			}
			int num3 = 0;
			for (int j = 0; j < item.conditions.Length; j++)
			{
				ExpLog.Log(item.key + " condition: " + item.conditions[j]);
				if (item.conditions[j].StartsWith("points-"))
				{
					int num4 = int.Parse(Regex.Split(item.conditions[j], "-")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (package.points >= num4)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("cha-"))
				{
					int num5 = int.Parse(Regex.Split(item.conditions[j], "-")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (num >= num5)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("hid-"))
				{
					int num6 = int.Parse(Regex.Split(item.conditions[j], "-")[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (num2 >= num6)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("typ-"))
				{
					string[] array = Regex.Split(item.conditions[j], "-");
					string key = array[1];
					int num7 = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (dictionary.ContainsKey(key) && dictionary[key] >= num7)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("bur-"))
				{
					if (ExpeditionGame.activeUnlocks.Contains(item.conditions[j]))
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("slug-"))
				{
					if (new SlugcatStats.Name(Regex.Split(item.conditions[j], "-")[1]) == package.slugcat)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("tpoints-"))
				{
					string[] array2 = Regex.Split(item.conditions[j], "-");
					if (ExpeditionData.totalPoints >= int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture))
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("wins-"))
				{
					string[] array3 = Regex.Split(item.conditions[j], "-");
					if (ExpeditionData.totalWins >= int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture))
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("tcha-"))
				{
					int num8 = 0 + (ExpeditionData.totalChallengesCompleted + ExpeditionData.totalHiddenChallengesCompleted);
					string[] array4 = Regex.Split(item.conditions[j], "-");
					if (num8 >= int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture))
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("thid-"))
				{
					string[] array5 = Regex.Split(item.conditions[j], "-");
					if (ExpeditionData.totalHiddenChallengesCompleted >= int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture))
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("ttyp-"))
				{
					string[] array6 = Regex.Split(item.conditions[j], "-");
					string key2 = array6[1];
					int num9 = int.Parse(array6[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (ExpeditionData.challengeTypes.ContainsKey(key2) && ExpeditionData.challengeTypes[key2] >= num9)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("swins-"))
				{
					string[] array7 = Regex.Split(item.conditions[j], "-");
					string key3 = array7[1];
					int num10 = int.Parse(array7[2], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (ExpeditionData.slugcatWins.ContainsKey(key3) && ExpeditionData.slugcatWins[key3] >= num10)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("lvl-"))
				{
					string[] array8 = Regex.Split(item.conditions[j], "-");
					if (ExpeditionData.level >= int.Parse(array8[1], NumberStyles.Any, CultureInfo.InvariantCulture))
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
				else if (item.conditions[j].StartsWith("mis-"))
				{
					string text = Regex.Split(item.conditions[j], "-")[1];
					if (ExpeditionData.activeMission == text)
					{
						num3++;
						ExpLog.Log("Condition met: " + item.conditions[j]);
					}
				}
			}
			if (num3 != item.conditions.Length)
			{
				continue;
			}
			if (!ExpeditionData.completedQuests.Contains(item.key))
			{
				ExpeditionGame.pendingCompletedQuests.Add(item.key);
				ExpeditionData.completedQuests.Add(item.key);
				for (int k = 0; k < item.reward.Length; k++)
				{
					if (!ExpeditionData.unlockables.Contains(item.reward[k]) || item.reward[k].StartsWith("per-"))
					{
						ExpeditionData.unlockables.Add(item.reward[k]);
						if (item.reward[k].StartsWith("mus-"))
						{
							ExpeditionData.newSongs.Add(item.reward[k]);
						}
					}
				}
			}
			ExpLog.Log("Quest " + item.key + " completed!");
		}
		foreach (Quest item2 in list)
		{
			if (ExpeditionData.completedQuests.Contains(item2.key))
			{
				continue;
			}
			int num11 = 0;
			for (int l = 0; l < item2.conditions.Length; l++)
			{
				if (item2.conditions[l].StartsWith("qst-"))
				{
					string[] array9 = Regex.Split(item2.conditions[l], "-");
					if (ExpeditionData.completedQuests.Count >= int.Parse(array9[1], NumberStyles.Any, CultureInfo.InvariantCulture))
					{
						num11++;
						ExpLog.Log("Condition met: " + item2.conditions[l]);
					}
				}
			}
			if (num11 != item2.conditions.Length)
			{
				continue;
			}
			if (!ExpeditionData.completedQuests.Contains(item2.key))
			{
				ExpeditionData.completedQuests.Add(item2.key);
				ExpeditionGame.pendingCompletedQuests.Add(item2.key);
				for (int m = 0; m < item2.reward.Length; m++)
				{
					if (!ExpeditionData.unlockables.Contains(item2.reward[m]) || item2.reward[m].StartsWith("per-"))
					{
						ExpeditionData.unlockables.Add(item2.reward[m]);
						if (item2.reward[m].StartsWith("mus-"))
						{
							ExpeditionData.newSongs.Add(item2.reward[m]);
						}
					}
				}
			}
			ExpLog.Log("Quest " + item2.key + " completed!");
		}
		if (ExpeditionData.completedQuests.Count >= 75)
		{
			Custom.rainWorld.processManager.CueAchievement(RainWorld.AchievementID.Quests, 5f);
		}
		Expedition.coreFile.Save(runEnded: false);
	}

	public static string BurdenName(string key)
	{
		switch (key)
		{
		case "bur-blinded":
			return IGT.Translate("BLINDED");
		case "bur-doomed":
			return IGT.Translate("DOOMED");
		case "bur-hunted":
			return IGT.Translate("HUNTED");
		case "bur-pursued":
			if (ModManager.MSC)
			{
				return IGT.Translate("PURSUED");
			}
			break;
		}
		return CustomBurdens.BurdenForID(key)?.DisplayName ?? null;
	}

	public static string BurdenDescription(string key)
	{
		if (!ExpeditionData.unlockables.Contains(key))
		{
			return "???";
		}
		return key switch
		{
			"bur-blinded" => IGT.Translate("Shrouds the entire world in an endless night, obscuring danger in every shadow. A light source will be vital for survival."), 
			"bur-doomed" => IGT.Translate("Removes the safety net that karma provides, triggering permadeath for any failure. Made only slightly less punishing with the use of the Karma Flower perk."), 
			"bur-hunted" => IGT.Translate("Makes the player public enemy number one. All hostile creatures in the region will know your location at all times, making standing still a bad choice."), 
			"bur-pursued" => IGT.Translate("Something stalks you through the world, endlessly searching for its target. Though not impossible to defeat, it will always return."), 
			_ => CustomBurdens.BurdenForID(key)?.Description ?? "", 
		};
	}

	public static string BurdenManualDescription(string key)
	{
		if (!ExpeditionData.unlockables.Contains(key))
		{
			return "???";
		}
		return key switch
		{
			"bur-blinded" => IGT.Translate("Shrouds the entire world in an endless night, obscuring danger in every shadow. A light source will be vital for survival."), 
			"bur-doomed" => IGT.Translate("Removes the safety net that karma provides, triggering permadeath for any failure. Made only slightly less punishing with the use of the Karma Flower perk."), 
			"bur-hunted" => IGT.Translate("Makes the player public enemy number one. All hostile creatures in the region will know your location at all times, making standing still a bad choice."), 
			"bur-pursued" => IGT.Translate("Something stalks you through the world, endlessly searching for its target. Though not impossible to defeat, it will always return."), 
			_ => CustomBurdens.BurdenForID(key)?.ManualDescription ?? "", 
		};
	}

	public static float BurdenScoreMultiplier(string key)
	{
		return key switch
		{
			"bur-blinded" => 20f, 
			"bur-doomed" => 75f, 
			"bur-hunted" => 50f, 
			"bur-pursued" => 35f, 
			_ => CustomBurdens.BurdenForID(key)?.ScoreMultiplier ?? 0f, 
		};
	}

	public static Color BurdenMenuColor(string key)
	{
		return key switch
		{
			"bur-blinded" => new Color(0.35f, 0.1f, 0.8f), 
			"bur-doomed" => new Color(0.75f, 0f, 0f), 
			"bur-hunted" => new Color(0.85f, 0.4f, 0f), 
			"bur-pursued" => new Color(0.75f, 0.55f, 0f), 
			_ => CustomBurdens.BurdenForID(key)?.Color ?? new HSLColor(UnityEngine.Random.value, 0.9f, 0.55f).rgb, 
		};
	}

	public static string UnlockName(string key)
	{
		if (ModManager.MSC && key == "unl-gun" && !ExpeditionData.unlockables.Contains(key))
		{
			return "???";
		}
		switch (key)
		{
		case "unl-glow":
			return IGT.Translate("Neuron Glow");
		case "unl-bomb":
			return IGT.Translate("Scavenger Bomb");
		case "unl-lantern":
			return IGT.Translate("Scavenger Lantern");
		case "unl-slow":
			return IGT.Translate("Slow Time");
		case "unl-passage":
			return IGT.Translate("Enable Passages");
		case "unl-backspear":
			return IGT.Translate("Back Spear");
		case "unl-vulture":
			return IGT.Translate("Vulture Mask");
		case "unl-karma":
			return IGT.Translate("Karma Flower");
		default:
			if (ModManager.MSC)
			{
				switch (key)
				{
				case "unl-electric":
					return IGT.Translate("Electric Spear");
				case "unl-dualwield":
					return IGT.Translate("Spear Dual-Wielding");
				case "unl-sing":
					return IGT.Translate("Singularity Bomb");
				case "unl-explosivejump":
					return IGT.Translate("Explosive Jump");
				case "unl-explosionimmunity":
					return IGT.Translate("Explosion Resistance");
				case "unl-crafting":
					return IGT.Translate("Item Crafting");
				case "unl-agility":
					return IGT.Translate("High Agility");
				case "unl-gun":
					return IGT.Translate("Joke Rifle");
				}
			}
			return CustomPerks.PerkForID(key)?.DisplayName ?? null;
		}
	}

	public static string UnlockDescription(string key)
	{
		if (!ExpeditionData.unlockables.Contains(key))
		{
			return "???";
		}
		switch (key)
		{
		case "unl-glow":
			return IGT.Translate("Enables a persistent glow around the player");
		case "unl-bomb":
			return IGT.Translate("Start the expedition with a Scavenger Bomb");
		case "unl-lantern":
			return IGT.Translate("Start the expedition with a Scavenger Lantern");
		case "unl-slow":
			return IGT.Translate("Slow time at will by holding PICK UP and pressing MAP, also temporarily increases movement abilities");
		case "unl-passage":
			return IGT.Translate("Enables the use of passages during expeditions which can be earned by completing challenges");
		case "unl-backspear":
			return IGT.Translate("Allows the player to stow an additional spear on their back by holding PICK UP");
		case "unl-vulture":
			return IGT.Translate("Start the expedition with a Vulture Mask");
		case "unl-karma":
			return IGT.Translate("Start the expedition with reinforced karma, a Karma Flower will spawn at the site of death");
		default:
			if (ModManager.MSC)
			{
				switch (key)
				{
				case "unl-electric":
					return IGT.Translate("Start the expedition with an Electric Spear");
				case "unl-dualwield":
					return IGT.Translate("Allows the player to wield a spear in each hand");
				case "unl-sing":
					return IGT.Translate("Start the expedition with a Singularity Bomb");
				case "unl-explosivejump":
					return IGT.Translate("Allows the player to jump a second time mid-air by pressing JUMP and PICKUP");
				case "unl-explosionimmunity":
					return IGT.Translate("Makes the player highly resistant to explosions");
				case "unl-crafting":
					return IGT.Translate("Craft different combinations of items together by holding UP and PICK UP");
				case "unl-agility":
					return IGT.Translate("Increases the players movement abilities, granting high speed and jump height");
				case "unl-gun":
					return IGT.Translate("Start the expedition with the Joke Rifle");
				}
			}
			return CustomPerks.PerkForID(key)?.Description ?? null;
		}
	}

	public static string UnlockSprite(string key, bool alwaysShow)
	{
		if (ModManager.MSC && ExpeditionData.unlockables.Contains(key) && key == "unl-gun")
		{
			return "Symbol_JokeRifle";
		}
		if (ExpeditionData.unlockables.Contains(key) || alwaysShow)
		{
			switch (key)
			{
			case "unl-lantern":
				return "Symbol_Lantern";
			case "unl-bomb":
				return "Symbol_StunBomb";
			case "unl-vulture":
				return "Kill_Vulture";
			case "unl-glow":
				return "Symbol_Neuron";
			case "unl-passage":
				return "SurvivorB";
			case "unl-backspear":
				return "Symbol_Spear";
			case "unl-slow":
				return "Multiplayer_Time";
			case "unl-karma":
				return "FlowerMarker";
			}
			if (ModManager.MSC)
			{
				switch (key)
				{
				case "unl-sing":
					return "Symbol_Singularity";
				case "unl-electric":
					return "Symbol_ElectricSpear";
				case "unl-dualwield":
					return "Kill_Slugcat";
				case "unl-explosivejump":
					return "Kill_Slugcat";
				case "unl-explosionimmunity":
					return "Kill_Slugcat";
				case "unl-crafting":
					return "Kill_Slugcat";
				case "unl-agility":
					return "Kill_Slugcat";
				}
			}
			CustomPerk customPerk = CustomPerks.PerkForID(key);
			if (customPerk != null)
			{
				return customPerk.SpriteName;
			}
		}
		return "Sandbox_SmallQuestionmark";
	}

	public static Color UnlockColor(string key)
	{
		switch (key)
		{
		case "unl-lantern":
			return new Color(1f, 0.5f, 0f);
		case "unl-bomb":
			return new Color(1f, 0f, 0f);
		case "unl-vulture":
			return new Color(0.9f, 0.8f, 0.7f);
		case "unl-glow":
			return new Color(0.3f, 1f, 1f);
		case "unl-passage":
			return new Color(1f, 1f, 1f);
		case "unl-backspear":
			return new Color(0.8f, 0.8f, 0.8f);
		case "unl-slow":
			return new Color(1f, 0.5f, 0.3f);
		case "unl-karma":
			return new Color(1f, 0.75f, 0.1f);
		default:
			if (ModManager.MSC)
			{
				switch (key)
				{
				case "unl-sing":
					return new Color(0.2f, 0.2f, 1f);
				case "unl-electric":
					return new Color(0.75f, 0.75f, 1f);
				case "unl-dualwield":
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Spear);
				case "unl-explosivejump":
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
				case "unl-explosionimmunity":
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Artificer);
				case "unl-crafting":
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Gourmand);
				case "unl-agility":
					return PlayerGraphics.DefaultSlugcatColor(MoreSlugcatsEnums.SlugcatStatsName.Rivulet);
				case "unl-gun":
					return new Color(0.6f, 0.5f, 0.6f);
				}
			}
			return CustomPerks.PerkForID(key)?.Color ?? new Color(1f, 1f, 1f);
		}
	}

	public static string GetMissionName(string key)
	{
		for (int i = 0; i < missionList.Count; i++)
		{
			if (missionList[i].key == key)
			{
				return ChallengeTools.IGT.Translate(missionList[i].name);
			}
		}
		return "Custom Mission";
	}

	public static string TooltipRewardDescription(string key)
	{
		if (key.StartsWith("bur-"))
		{
			string text = BurdenName(key);
			if (text == null)
			{
				return null;
			}
			return IGT.Translate("Burden: <burden>").Replace("<burden>", text);
		}
		if (key.StartsWith("mus-"))
		{
			Dictionary<string, string> unlockedSongs = GetUnlockedSongs();
			if (!unlockedSongs.ContainsKey(key))
			{
				return null;
			}
			return IGT.Translate("Music: <songName>").Replace("<songName>", TrackName(unlockedSongs[key]));
		}
		if (key.StartsWith("unl-"))
		{
			string text2 = UnlockName(key);
			if (text2 == null)
			{
				return null;
			}
			return IGT.Translate("Perk: <perk>").Replace("<perk>", text2);
		}
		if (key.StartsWith("per-"))
		{
			string newValue = Regex.Split(key, "-")[1];
			return IGT.Translate("Perk Capacity: +<num>").Replace("<num>", newValue);
		}
		return null;
	}

	public static string TooltipRequirementDescription(string key)
	{
		if (key.StartsWith("slug-"))
		{
			string[] array = Regex.Split(key, "-");
			if (!ExtEnum<SlugcatStats.Name>.values.entries.Contains(array[1]))
			{
				return null;
			}
			SlugcatStats.Name i = new SlugcatStats.Name(array[1]);
			string newValue = IGT.Translate(SlugcatStats.getSlugcatName(i));
			return IGT.Translate("Slugcat: <slug>").Replace("<slug>", newValue);
		}
		if (key.StartsWith("points-"))
		{
			string newValue2 = Regex.Split(key, "-")[1];
			return IGT.Translate("Points: <points>").Replace("<points>", newValue2);
		}
		if (key.StartsWith("cha-"))
		{
			string newValue3 = Regex.Split(key, "-")[1];
			return IGT.Translate("Challenges: <num>").Replace("<num>", newValue3);
		}
		if (key.StartsWith("hid-"))
		{
			string newValue4 = Regex.Split(key, "-")[1];
			return IGT.Translate("Hidden Challenges: <num>").Replace("<num>", newValue4);
		}
		if (key.StartsWith("mis"))
		{
			string missionName = GetMissionName(Regex.Split(key, "-")[1]);
			return IGT.Translate("Mission: <mission_name>").Replace("<mission_name>", missionName);
		}
		if (key.StartsWith("typ-"))
		{
			string[] array2 = Regex.Split(key, "-");
			string newValue5 = ExpeditionGame.challengeNames[array2[1]];
			string newValue6 = array2[2];
			return IGT.Translate("<challenge> Challenges: <num>").Replace("<num>", newValue6).Replace("<challenge>", newValue5);
		}
		if (key.StartsWith("bur-"))
		{
			string text = BurdenName(key);
			if (text == null)
			{
				return null;
			}
			return IGT.Translate("Burden: <burden>").Replace("<burden>", text);
		}
		if (key.StartsWith("tpoints-"))
		{
			string[] array3 = Regex.Split(key, "-");
			return IGT.Translate("Total Points Earned: <points>").Replace("<points>", array3[1]);
		}
		if (key.StartsWith("tcha-"))
		{
			string[] array4 = Regex.Split(key, "-");
			return IGT.Translate("Total Challenges Completed: <num>").Replace("<num>", array4[1]);
		}
		if (key.StartsWith("thid-"))
		{
			string[] array5 = Regex.Split(key, "-");
			return IGT.Translate("Total Hidden Challenges: <num>").Replace("<num>", array5[1]);
		}
		if (key.StartsWith("wins-"))
		{
			string[] array6 = Regex.Split(key, "-");
			return IGT.Translate("Total Wins: <num>").Replace("<num>", array6[1]);
		}
		if (key.StartsWith("swins-"))
		{
			string[] array7 = Regex.Split(key, "-");
			if (!ExtEnum<SlugcatStats.Name>.values.entries.Contains(array7[1]))
			{
				return null;
			}
			string newValue7 = IGT.Translate(SlugcatStats.getSlugcatName(new SlugcatStats.Name(array7[1])));
			return IGT.Translate("Total Wins as <slugcat>: <num>").Replace("<slugcat>", newValue7).Replace("<num>", array7[2]);
		}
		if (key.StartsWith("ttyp-"))
		{
			string[] array8 = Regex.Split(key, "-");
			string newValue8 = ExpeditionGame.challengeNames[array8[1]];
			return IGT.Translate("Total <type> Challenges: <num>").Replace("<type>", newValue8).Replace("<num>", array8[2]);
		}
		if (key.StartsWith("lvl-"))
		{
			string[] array9 = Regex.Split(key, "-");
			return IGT.Translate("Reach Level: <num>").Replace("<num>", array9[1]);
		}
		if (key.StartsWith("qst-"))
		{
			string[] array10 = Regex.Split(key, "-");
			return IGT.Translate("<num> Quests Completed").Replace("<num>", array10[1]);
		}
		return null;
	}

	public static void ParseQuestFiles()
	{
		questList = new List<Quest>();
		ExpLog.Log("LOADING QUESTS");
		for (int i = 0; i < ModManager.ActiveMods.Count; i++)
		{
			string[] directories = Directory.GetDirectories(ModManager.ActiveMods[i].path);
			for (int j = 0; j < directories.Length; j++)
			{
				if (!directories[j].EndsWith("quests"))
				{
					continue;
				}
				List<Quest> list = new List<Quest>();
				string path = directories[j];
				List<string> list2 = new List<string>();
				string[] files = Directory.GetFiles(path);
				for (int k = 0; k < files.Length; k++)
				{
					if (files[k].EndsWith(".json"))
					{
						list2.Add(files[k]);
					}
				}
				for (int l = 0; l < list2.Count; l++)
				{
					Quest item = QuestFromJson(list2[l]);
					ExpLog.Log("Added Quest: \"" + item.key + "\" from mod: " + ModManager.ActiveMods[i].name);
					questList.Add(item);
					list.Add(item);
				}
				if (!customQuests.ContainsKey(ModManager.ActiveMods[i].id) && ModManager.ActiveMods[i].id != "expedition")
				{
					customQuests.Add(ModManager.ActiveMods[i].id, list);
				}
				ExpLog.Log("----------------------------------");
			}
		}
	}

	public static void ParseMissionFiles()
	{
		missionList = new List<Mission>();
		string[] array = AssetManager.ListDirectory("missions", directories: false, includeAll: true).Select(Path.GetDirectoryName).Distinct()
			.ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			List<Mission> list = new List<Mission>();
			List<string> list2 = new List<string>();
			string[] files = Directory.GetFiles(array[i]);
			for (int j = 0; j < files.Length; j++)
			{
				if (files[j].EndsWith(".json"))
				{
					list2.Add(files[j]);
				}
			}
			if (list2.Count != 0)
			{
				string fileName = Path.GetFileName(Path.GetDirectoryName(array[i]));
				for (int k = 0; k < list2.Count; k++)
				{
					Mission item = MissionFromJson(list2[k]);
					ExpLog.Log("Added Mission: \"" + item.name + "\" from mod: " + fileName);
					missionList.Add(item);
					list.Add(item);
				}
				if (!customMissions.ContainsKey(fileName) && fileName != "expedition")
				{
					customMissions.Add(fileName, list);
				}
				ExpLog.Log("----------------------------------");
				continue;
			}
			break;
		}
	}

	public static Dictionary<string, string> GetUnlockedSongs()
	{
		List<string> list = new List<string>
		{
			"RW_Intro_Theme", "NA_01 - Proxima", "NA_02 - Dustcloud", "NA_03 - Wormpad", "NA_04 - Silicon", "NA_05 - Sparkles", "NA_06 - Past Echoes", "NA_07 - Phasing", "NA_08 - Dark Sus", "NA_09 - Interest Pad",
			"NA_10 - Qanda", "NA_11 - Digital Sundown", "NA_11 - Reminiscence", "NA_16 - Drastic FM", "NA_17 - Dripping Time", "NA_18 - Glass Arcs", "NA_19 - Halcyon Memories", "NA_20 - Crystalline", "NA_21 - New Terra", "NA_22 - They Say",
			"NA_23 - Speaking Systems", "NA_24 - Emotion Thread", "NA_25 - Demonic Riser", "NA_26 - Energy Circuit", "NA_27 - Silent Construct", "NA_28 - Stargazer", "NA_29 - Flutter", "NA_30 - Distance", "NA_31 - Pulse", "RW_Outro_Theme",
			"Passages", "NA_32 - Else1", "NA_33 - Else2", "NA_34 - Else3", "NA_35 - Else4", "NA_36 - Else5", "NA_37 - Else6", "NA_38 - Else7", "NA_39 - Cracked Earth", "NA_40 - Unseen Lands",
			"NA_41 - Random Gods", "RW_1 - Urban Jungle", "RW_7 - Rooftops", "RW_8 - Sundown", "RW_9 - Mud Pits", "RW_10 - Noisy", "RW_13 - Action Scene", "RW_14 - All Thats Left", "RW_15 - Old Growth", "RW_16 - Shoreline",
			"RW_18 - The Captain", "RW_19 - Stone Heads", "RW_20 - Polybius", "RW_26 - Black Moonlight", "RW_27 - Train Tunnels", "RW_28 - Ferrous Forest", "RW_29 - Lovely Arps", "RW_32 - Grey Cloud", "RW_33 - Weyuon", "RW_34 - Slaughter",
			"RW_37 - Garbage City Shuffle", "RW_38 - The Wet Moist", "RW_39 - Lack of Comfort", "RW_40 - Floes", "RW_41 - Grumblebum", "RW_42 - Kayava", "RW_43 - Albino", "RW_43 - Bio Engineering", "RW_45 - Deep Energy", "RW_46 - Lonesound",
			"RW_47 - Maze of Soil", "RW_48 - Wind Chimes", "RW_49 - Nest in Metal", "RW_50 - Mist Engine", "RW_51 - Swaying Fronds", "RW_52 - Garbage Worms", "RW_53 - Leviathan Cave", "RW_54 - Raindeer Ride", "RW_55 - Sky Sprite", "RW_55 - White Lizard",
			"RW_58 - Lantern Mice"
		};
		if (ModManager.MSC)
		{
			List<string> collection = new List<string>
			{
				"RW_59 - Lost City", "RW_60 - Bloom", "RW_61 - Rain", "RW_62 - Orange Lizard", "RW_63 - Wandering Cut", "RW_64 - Daze", "RW_65 - Garden", "RW_66 - Metal Canopy", "RW_67 - Random Fate", "RW_68 - Wired",
				"RW_69 - Ancient", "RW_70 - Scapeless Doubt", "RW_71 - Sparking Pendulum", "RW_72 - Satellite", "RW_73 - Flicker", "RW_74 - Aquaphobia", "RW_75 - Onto A New Dawn", "RW_76 - Not Your Rain", "RW_77 - Fragile", "RW_78 - Obverse of the Old Wind",
				"RW_79 - Overcast", "RW_80 - Frosted Festival", "RW_81 - Breathing Hyometer", "RW_82 - Trusted Component", "RW_83 - Accidented Condition", "RW_84 - Chilblain Grace", "RW_85 - Ascent", "RW_86 - The Cycle", "RW_87 - Fading Light", "RW_88 - Flux",
				"RW_89 - Vast Unlife", "RW_90 - Eyes of Iron", "RW_91 - Sheer Ice Torrent", "RW_93 - Veiled Northstar", "RW_94 - Weathered Steps", "RW_95 - Reflection of the Moon", "RW_96 - Fragments", "RW_97 - Open Skies", "RW_98 - Frost Reaper", "NA_42 - Else8",
				"RW_Outro_Theme_B", "NA_19x - Halcyon Memories", "RW_92 - Reclaiming Entropy"
			};
			list.AddRange(collection);
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		for (int i = 0; i < list.Count; i++)
		{
			dictionary["mus-" + ValueConverter.ConvertToString(i + 1)] = list[i];
		}
		return dictionary;
	}

	public static string TrackName(string filename)
	{
		string result = filename;
		if (filename.Contains(" - "))
		{
			string[] array = Regex.Split(filename, " - ");
			result = array[1];
			if (array[0] == "NA_19x")
			{
				result = "Halcyon Memories (Reprise)";
			}
			if (array[1] == "Shoreline")
			{
				result = "The Coast";
			}
			if (array[1] == "Else1")
			{
				result = "ELSE I";
			}
			if (array[1] == "Else2")
			{
				result = "ELSE II";
			}
			if (array[1] == "Else3")
			{
				result = "ELSE III";
			}
			if (array[1] == "Else4")
			{
				result = "ELSE IV";
			}
			if (array[1] == "Else5")
			{
				result = "ELSE V";
			}
			if (array[1] == "Else6")
			{
				result = "ELSE VI";
			}
			if (array[1] == "Else7")
			{
				result = "ELSE VII";
			}
			if (array[1] == "Else8")
			{
				result = "ELSE VIII";
			}
		}
		if (filename == "RW_Intro_Theme")
		{
			result = "Pictures of the Past";
		}
		if (filename == "RW_Outro_Theme")
		{
			result = "Deep Light";
		}
		if (filename == "RW_Outro_Theme_B")
		{
			result = "Another Ending";
		}
		return result;
	}

	public static int LevelCap(int currentLevel)
	{
		currentLevel++;
		return Mathf.RoundToInt(100f + (1.45f * Mathf.Pow(currentLevel, 2f) - 1.45f * (float)currentLevel));
	}

	public static void CheckLevelUp()
	{
		while (ExpeditionData.currentPoints > LevelCap(ExpeditionData.level))
		{
			ExpeditionData.currentPoints -= LevelCap(ExpeditionData.level);
			ExpeditionData.level++;
		}
	}

	public static int CalculateOverload(int points)
	{
		int num = 0;
		int num2 = LevelCap(ExpeditionData.level);
		while (points > num2)
		{
			num++;
			points -= num2;
		}
		return num;
	}

	public static bool CheckUnlocked(ProcessManager manager, SlugcatStats.Name slugcat)
	{
		if (ExpeditionData.devMode || (ModManager.MSC && global::MoreSlugcats.MoreSlugcats.chtUnlockCampaigns.Value))
		{
			return true;
		}
		if (slugcat == SlugcatStats.Name.White)
		{
			return true;
		}
		if (slugcat == SlugcatStats.Name.Yellow)
		{
			return true;
		}
		if (slugcat == SlugcatStats.Name.Red && manager.rainWorld.progression.miscProgressionData.redUnlocked)
		{
			return true;
		}
		if (ModManager.MSC)
		{
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && (manager.rainWorld.progression.miscProgressionData.beaten_Gourmand || manager.rainWorld.progression.miscProgressionData.beaten_Gourmand_Full))
			{
				return true;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer)
			{
				return manager.rainWorld.progression.miscProgressionData.beaten_Artificer;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear)
			{
				return manager.rainWorld.progression.miscProgressionData.beaten_SpearMaster;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
			{
				return manager.rainWorld.progression.miscProgressionData.beaten_Rivulet;
			}
			if (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				return manager.rainWorld.progression.miscProgressionData.beaten_Saint;
			}
		}
		return false;
	}

	public static Quest QuestFromJson(string jsonPath)
	{
		Quest quest = default(Quest);
		quest.key = "NULL";
		quest.type = "NULL";
		quest.color = new Color(1f, 1f, 1f);
		quest.conditions = new string[0];
		quest.reward = new string[0];
		Quest result = quest;
		Dictionary<string, object> dictionary = File.ReadAllText(jsonPath).dictionaryFromJson();
		if (dictionary != null)
		{
			if (dictionary.ContainsKey("key"))
			{
				result.key = dictionary["key"].ToString();
			}
			if (dictionary.ContainsKey("type"))
			{
				result.type = ChallengeTools.IGT.Translate(dictionary["type"].ToString());
			}
			if (dictionary.ContainsKey("color"))
			{
				result.color = RXUtils.GetColorFromHex(dictionary["color"].ToString());
				result.color.a = 1f;
			}
			if (dictionary.ContainsKey("conditions"))
			{
				result.conditions = ((List<object>)dictionary["conditions"]).ConvertAll((object x) => x.ToString()).ToArray();
				for (int i = 0; i < result.conditions.Length; i++)
				{
				}
			}
			if (dictionary.ContainsKey("reward"))
			{
				result.reward = ((List<object>)dictionary["reward"]).ConvertAll((object x) => x.ToString()).ToArray();
				for (int j = 0; j < result.reward.Length; j++)
				{
				}
			}
		}
		return result;
	}

	public static bool MissionAvailable(string key)
	{
		for (int i = 0; i < missionList.Count; i++)
		{
			if (!(missionList[i].key == key))
			{
				continue;
			}
			SlugcatStats.Name item = new SlugcatStats.Name(missionList[i].slugcat);
			if (!ExpeditionGame.unlockedExpeditionSlugcats.Contains(item))
			{
				return false;
			}
			for (int j = 0; j < missionList[i].requirements.Count; j++)
			{
				if (!ExpeditionData.unlockables.Contains(missionList[i].requirements[j]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static string MissionRequirements(string key)
	{
		string text = "";
		bool flag = true;
		for (int i = 0; i < missionList.Count; i++)
		{
			if (!(missionList[i].key == key))
			{
				continue;
			}
			SlugcatStats.Name name = new SlugcatStats.Name(missionList[i].slugcat);
			if (!ExpeditionGame.unlockedExpeditionSlugcats.Contains(name))
			{
				text += ChallengeTools.IGT.Translate("Slugcat Unlocked: <slugcat>").Replace("<slugcat>", SlugcatStats.getSlugcatName(name));
				flag = false;
			}
			for (int j = 0; j < missionList[i].requirements.Count; j++)
			{
				if (!ExpeditionData.unlockables.Contains(missionList[i].requirements[j]))
				{
					if (text != "")
					{
						text += " | ";
					}
					string text2 = missionList[i].requirements[j];
					if (text2.StartsWith("bur-"))
					{
						text += ChallengeTools.IGT.Translate("Burden: <burden>").Replace("<burden>", BurdenName(text2));
					}
					if (text2.StartsWith("unl-"))
					{
						text += ChallengeTools.IGT.Translate("Perk: <perk>").Replace("<perk>", UnlockName(text2));
					}
					flag = false;
				}
			}
		}
		if (!flag)
		{
			return ChallengeTools.IGT.Translate("Missing Requirements:      <req>").Replace("<req>", text);
		}
		return "";
	}

	public static Mission? ValidateCurrentExpedition()
	{
		bool flag = false;
		foreach (Mission mission in missionList)
		{
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < mission.challenges.Count; i++)
			{
				for (int j = 0; j < ExpeditionData.challengeList.Count; j++)
				{
					if (ExpeditionData.challengeList[j] == mission.challenges[i])
					{
						num++;
					}
				}
			}
			for (int k = 0; k < mission.requirements.Count; k++)
			{
				for (int l = 0; l < ExpeditionGame.activeUnlocks.Count; l++)
				{
					if (ExpeditionGame.activeUnlocks[l] == mission.requirements[k])
					{
						num2++;
					}
				}
			}
			if (mission.slugcat == ExpeditionData.slugcatPlayer.value || mission.slugcat.ToLower() == "any")
			{
				flag = true;
			}
			if (num == mission.challenges.Count && num2 == mission.requirements.Count && flag)
			{
				ExpLog.Log("C:" + num + " R:" + num2 + " S:" + mission.slugcat);
				ExpLog.Log("MISSION TRUE");
				return mission;
			}
		}
		ExpLog.Log("NO MISSION");
		return null;
	}

	public static Mission MissionFromJson(string jsonPath)
	{
		Mission mission = default(Mission);
		mission.key = "NULL";
		mission.name = "NULL";
		mission.slugcat = "White";
		mission.challenges = new List<Challenge>();
		mission.requirements = new List<string>();
		mission.den = "";
		Mission result = mission;
		Dictionary<string, object> dictionary = File.ReadAllText(jsonPath).dictionaryFromJson();
		if (dictionary != null)
		{
			if (dictionary.ContainsKey("key"))
			{
				result.key = dictionary["key"].ToString();
			}
			if (dictionary.ContainsKey("name"))
			{
				result.name = dictionary["name"].ToString();
			}
			if (dictionary.ContainsKey("slugcat"))
			{
				result.slugcat = dictionary["slugcat"].ToString();
			}
			if (dictionary.ContainsKey("challenges"))
			{
				new SlugcatStats.Name(result.slugcat);
				string[] array = ((List<object>)dictionary["challenges"]).ConvertAll((object x) => x.ToString()).ToArray();
				string text = "";
				string text2 = "";
				for (int i = 0; i < array.Length; i++)
				{
					string[] array2 = Regex.Split(array[i], "~");
					text = array2[0];
					text2 = array2[1];
					foreach (Type item in from TheType in Assembly.GetAssembly(typeof(Challenge)).GetTypes()
						where TheType.IsClass && !TheType.IsAbstract && TheType.IsSubclassOf(typeof(Challenge))
						select TheType)
					{
						if (item.Name == text)
						{
							try
							{
								Challenge challenge = (Challenge)FormatterServices.GetUninitializedObject(item);
								challenge.FromString(text2);
								result.challenges.Add(challenge);
							}
							catch (Exception ex)
							{
								ExpLog.Log("ERROR: Problem recreating challenge type with reflection: " + ex.Message);
							}
						}
					}
				}
			}
			if (dictionary.ContainsKey("requirements"))
			{
				result.requirements = ((List<object>)dictionary["requirements"]).ConvertAll((object x) => x.ToString()).ToArray().ToList();
			}
			if (dictionary.ContainsKey("den"))
			{
				result.den = dictionary["den"].ToString();
			}
		}
		return result;
	}
}
