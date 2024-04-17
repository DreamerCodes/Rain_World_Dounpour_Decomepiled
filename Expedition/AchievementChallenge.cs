using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Menu.Remix;
using Modding.Passages;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class AchievementChallenge : Challenge
{
	public WinState.EndgameID ID;

	public override void UpdateDescription()
	{
		description = ChallengeTools.IGT.Translate("Earn <achievement_name> passage").Replace("<achievement_name>", ChallengeTools.IGT.Translate(WinState.PassageDisplayName(ID)));
		base.UpdateDescription();
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Passage");
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is AchievementChallenge)
		{
			if ((challenge as AchievementChallenge).ID.value == ID.value)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public override int Points()
	{
		int num = 0;
		try
		{
			num = ChallengeTools.achievementScores[ID];
			if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear && (ID == WinState.EndgameID.Saint || ID == WinState.EndgameID.Monk))
			{
				num += 40;
			}
			return num * (int)(hidden ? 2f : 1f);
		}
		catch
		{
			ExpLog.Log("Could not get achievement score for ID: " + ID.value);
			return 0;
		}
	}

	public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
	{
		if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint && (ID == WinState.EndgameID.Traveller || ID == WinState.EndgameID.Hunter || ID == WinState.EndgameID.Scholar))
		{
			return false;
		}
		if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer && ID == WinState.EndgameID.Chieftain)
		{
			return false;
		}
		return CustomPassages.PassageForID(ID)?.IsAvailableForSlugcat(slugcat) ?? true;
	}

	public override Challenge Generate()
	{
		List<WinState.EndgameID> list = new List<WinState.EndgameID>();
		for (int i = 0; i < ChallengeTools.achievementScores.Count; i++)
		{
			if (!ModManager.MSC || (!(ChallengeTools.achievementScores.ElementAt(i).Key == MoreSlugcatsEnums.EndgameID.Mother) && !(ChallengeTools.achievementScores.ElementAt(i).Key == MoreSlugcatsEnums.EndgameID.Gourmand)))
			{
				list.Add(ChallengeTools.achievementScores.ElementAt(i).Key);
			}
		}
		WinState.EndgameID iD = list[UnityEngine.Random.Range(0, list.Count)];
		return new AchievementChallenge
		{
			ID = iD
		};
	}

	public override bool CombatRequired()
	{
		if (ID == WinState.EndgameID.Chieftain)
		{
			return true;
		}
		if (ID == WinState.EndgameID.DragonSlayer)
		{
			return true;
		}
		if (ID == WinState.EndgameID.Friend)
		{
			return false;
		}
		if (ID == WinState.EndgameID.Hunter)
		{
			return true;
		}
		if (ID == WinState.EndgameID.Monk)
		{
			return false;
		}
		if (ID == WinState.EndgameID.Outlaw)
		{
			return true;
		}
		if (ID == WinState.EndgameID.Saint)
		{
			return false;
		}
		if (ID == WinState.EndgameID.Scholar)
		{
			return false;
		}
		if (ID == WinState.EndgameID.Survivor)
		{
			return false;
		}
		if (ID == WinState.EndgameID.Traveller)
		{
			return false;
		}
		if (ModManager.MSC)
		{
			if (ID == MoreSlugcatsEnums.EndgameID.Gourmand)
			{
				return true;
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Martyr)
			{
				return false;
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Mother)
			{
				return false;
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Nomad)
			{
				return false;
			}
			if (ID == MoreSlugcatsEnums.EndgameID.Pilgrim)
			{
				return false;
			}
		}
		return CustomPassages.PassageForID(ID)?.RequiresCombat ?? false;
	}

	public void CheckAchievementProgress(WinState winState)
	{
		if (!completed && game != null && winState != null)
		{
			WinState.EndgameTracker tracker = winState.GetTracker(ID, addIfMissing: true);
			if (tracker != null && tracker.GoalFullfilled)
			{
				CompleteChallenge();
			}
		}
	}

	public override string ToString()
	{
		return "AchievementChallenge" + "~" + ValueConverter.ConvertToString(ID) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			ID = new WinState.EndgameID(array[0]);
			completed = array[1] == "1";
			hidden = array[2] == "1";
			revealed = array[3] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: AchievementChallenge FromString() encountered an error: " + ex.Message);
		}
	}
}
