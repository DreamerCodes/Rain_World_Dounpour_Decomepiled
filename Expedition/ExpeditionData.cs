using System.Collections.Generic;
using MoreSlugcats;

namespace Expedition;

public static class ExpeditionData
{
	public static Dictionary<SlugcatStats.Name, List<Challenge>> allChallengeLists = new Dictionary<SlugcatStats.Name, List<Challenge>>();

	public static List<Challenge> completedChallengeList = new List<Challenge>();

	public static Dictionary<string, string> allActiveMissions = new Dictionary<string, string>();

	public static Dictionary<string, List<string>> requiredExpeditionContent = new Dictionary<string, List<string>>();

	public static float challengeDifficulty = 0.5f;

	public static string startingDen;

	public static bool newGame = false;

	public static bool validateQuests = true;

	public static bool hasViewedManual = false;

	public static Dictionary<string, int> allEarnedPassages = new Dictionary<string, int>();

	public static bool devMode = false;

	public static int saveSlot;

	public static int[] ints;

	public static SlugcatStats.Name slugcatPlayer = SlugcatStats.Name.White;

	public static List<string> completedQuests = new List<string>();

	public static List<string> completedMissions = new List<string>();

	public static Dictionary<string, int> missionBestTimes = new Dictionary<string, int>();

	public static List<string> unlockables = new List<string>();

	public static List<string> newSongs = new List<string>();

	public static int level;

	public static int currentPoints;

	public static int perkLimit = 1;

	public static int totalPoints;

	public static int totalChallengesCompleted;

	public static int totalHiddenChallengesCompleted;

	public static int totalWins;

	public static Dictionary<string, int> slugcatWins = new Dictionary<string, int>();

	public static Dictionary<string, int> challengeTypes = new Dictionary<string, int>();

	public static string menuSong;

	public static List<Challenge> challengeList
	{
		get
		{
			if (!allChallengeLists.ContainsKey(slugcatPlayer))
			{
				allChallengeLists[slugcatPlayer] = new List<Challenge>();
			}
			return allChallengeLists[slugcatPlayer];
		}
	}

	public static string activeMission
	{
		get
		{
			if (allActiveMissions.ContainsKey(slugcatPlayer.value))
			{
				return allActiveMissions[slugcatPlayer.value];
			}
			return "";
		}
		set
		{
			if (!allActiveMissions.ContainsKey(slugcatPlayer.value))
			{
				allActiveMissions.Add(slugcatPlayer.value, value);
			}
			else
			{
				allActiveMissions[slugcatPlayer.value] = value;
			}
			ExpLog.Log(slugcatPlayer.value + " mission set: " + value);
		}
	}

	public static int earnedPassages
	{
		get
		{
			if (!allEarnedPassages.ContainsKey(slugcatPlayer.value))
			{
				allEarnedPassages.Add(slugcatPlayer.value, 0);
			}
			return allEarnedPassages[slugcatPlayer.value];
		}
		set
		{
			if (!allEarnedPassages.ContainsKey(slugcatPlayer.value))
			{
				allEarnedPassages.Add(slugcatPlayer.value, 0);
			}
			allEarnedPassages[slugcatPlayer.value] = value;
		}
	}

	public static void AddExpeditionRequirements(SlugcatStats.Name slugcat, bool ended)
	{
		if (requiredExpeditionContent == null)
		{
			requiredExpeditionContent = new Dictionary<string, List<string>>();
		}
		if (ended)
		{
			if (requiredExpeditionContent.ContainsKey(slugcatPlayer.value))
			{
				requiredExpeditionContent.Remove(slugcatPlayer.value);
			}
			return;
		}
		List<string> list = new List<string>();
		foreach (ModManager.Mod activeMod in ModManager.ActiveMods)
		{
			list.Add(activeMod.id);
		}
		if (!requiredExpeditionContent.ContainsKey(slugcat.value))
		{
			requiredExpeditionContent.Add(slugcat.value, list);
		}
		else
		{
			requiredExpeditionContent[slugcat.value] = list;
		}
	}

	public static bool MissingRequirements(SlugcatStats.Name slugcat)
	{
		if (requiredExpeditionContent != null && requiredExpeditionContent.ContainsKey(slugcat.value) && !ModManager.MSC && requiredExpeditionContent[slugcat.value].Contains("moreslugcats"))
		{
			return true;
		}
		return false;
	}

	public static List<SlugcatStats.Name> GetPlayableCharacters()
	{
		List<SlugcatStats.Name> list = new List<SlugcatStats.Name>
		{
			SlugcatStats.Name.Yellow,
			SlugcatStats.Name.White,
			SlugcatStats.Name.Red
		};
		if (ModManager.MSC)
		{
			List<SlugcatStats.Name> collection = new List<SlugcatStats.Name>
			{
				MoreSlugcatsEnums.SlugcatStatsName.Gourmand,
				MoreSlugcatsEnums.SlugcatStatsName.Artificer,
				MoreSlugcatsEnums.SlugcatStatsName.Spear,
				MoreSlugcatsEnums.SlugcatStatsName.Rivulet,
				MoreSlugcatsEnums.SlugcatStatsName.Saint
			};
			list.AddRange(collection);
		}
		return list;
	}

	public static void ClearActiveChallengeList()
	{
		if (allChallengeLists.ContainsKey(slugcatPlayer))
		{
			allChallengeLists[slugcatPlayer].Clear();
		}
	}
}
