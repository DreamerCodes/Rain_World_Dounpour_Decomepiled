using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class EchoChallenge : Challenge
{
	public GhostWorldPresence.GhostID ghost;

	public override void UpdateDescription()
	{
		description = ChallengeTools.IGT.Translate("Visit the <echo_location> Echo").Replace("<echo_location>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(ghost.value, ExpeditionData.slugcatPlayer)));
		base.UpdateDescription();
	}

	public override void Update()
	{
		base.Update();
		for (int i = 0; i < game.Players.Count; i++)
		{
			if (game.Players[i] == null || game.Players[i].realizedCreature == null || game.Players[i].realizedCreature.room == null)
			{
				continue;
			}
			for (int j = 0; j < game.Players[i].realizedCreature.room.updateList.Count; j++)
			{
				if (game.Players[i].realizedCreature.room.updateList[j] is Ghost && game.Players[i].world.worldGhost != null && (game.Players[i].realizedCreature.room.updateList[j] as Ghost).onScreenCounter > 30 && game.Players[i].world.worldGhost.ghostID.value == ghost.value)
				{
					CompleteChallenge();
					ExpLog.Log("EchoChallenge Complete!");
				}
			}
		}
	}

	public override int Points()
	{
		if (ghost == null || ghost.Index == -1)
		{
			return 0;
		}
		return ChallengeTools.echoScores[(int)ghost] * (int)(hidden ? 2f : 1f);
	}

	public override Challenge Generate()
	{
		List<string> list = new List<string>();
		for (int i = 0; i < ExtEnum<GhostWorldPresence.GhostID>.values.entries.Count; i++)
		{
			if (ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] != "NoGhost" && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "MS")) && (!ModManager.MSC || !(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i] == "SL") || !(ExpeditionData.slugcatPlayer != MoreSlugcatsEnums.SlugcatStatsName.Saint)) && SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).Contains(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]))
			{
				list.Add(ExtEnum<GhostWorldPresence.GhostID>.values.entries[i]);
			}
		}
		GhostWorldPresence.GhostID ghostID = new GhostWorldPresence.GhostID(list[UnityEngine.Random.Range(0, list.Count)]);
		return new EchoChallenge
		{
			ghost = ghostID
		};
	}

	public override bool CombatRequired()
	{
		return false;
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is EchoChallenge)
		{
			if ((challenge as EchoChallenge).ghost.value == ghost.value)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Echo Sighting");
	}

	public override string ToString()
	{
		return "EchoChallenge" + "~" + ValueConverter.ConvertToString(ghost) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			ghost = new GhostWorldPresence.GhostID(array[0]);
			completed = array[1] == "1";
			hidden = array[2] == "1";
			revealed = array[3] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: EchoChallenge FromString() encountered an error: " + ex.Message);
		}
	}
}
