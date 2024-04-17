using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class PearlDeliveryChallenge : Challenge
{
	public string region;

	public int iterator = -1;

	public override void UpdateDescription()
	{
		string newValue = ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ? ChallengeTools.IGT.Translate("Five Pebbles") : ChallengeTools.IGT.Translate("Looks To The Moon"));
		description = ChallengeTools.IGT.Translate("<region> pearl delivered to <iterator>").Replace("<region>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer))).Replace("<iterator>", newValue);
		base.UpdateDescription();
	}

	public override void Update()
	{
		base.Update();
		if (iterator == -1)
		{
			iterator = ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer) ? 1 : 0);
		}
		for (int i = 0; i < game.Players.Count; i++)
		{
			if (game.Players[i] == null || game.Players[i].realizedCreature == null || game.Players[i].realizedCreature.room == null || (!(game.Players[i].realizedCreature.room.abstractRoom.name == ((iterator == 0) ? "SL_AI" : "SS_AI")) && (!ModManager.MSC || !(ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear) || !(game.Players[i].realizedCreature.room.abstractRoom.name == "DM_AI"))))
			{
				continue;
			}
			for (int j = 0; j < game.Players[i].realizedCreature.room.updateList.Count; j++)
			{
				if (game.Players[i].realizedCreature.room.updateList[j] is DataPearl && ChallengeTools.ValidRegionPearl(region, (game.Players[i].realizedCreature.room.updateList[j] as DataPearl).AbstractPearl.dataPearlType) && ((game.Players[i].realizedCreature.room.updateList[j] as DataPearl).firstChunk.pos.x > ((iterator == 0) ? 1400f : 0f) || (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear)))
				{
					CompleteChallenge();
				}
			}
		}
	}

	public override Challenge Generate()
	{
		List<string> list = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer);
		List<string> list2 = new List<string>();
		List<string> list3 = ChallengeTools.PearlRegionBlackList();
		for (int i = 0; i < list.Count; i++)
		{
			if (!list3.Contains(list[i].ToUpper()))
			{
				list2.Add(list[i]);
			}
		}
		string text = list2[UnityEngine.Random.Range(0, list2.Count)];
		return new PearlDeliveryChallenge
		{
			region = text
		};
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Pearl Delivery");
	}

	public override int Points()
	{
		return ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear) ? 50 : (30 * (int)(hidden ? 2f : 1f))) + RegionPoints();
	}

	public int RegionPoints()
	{
		if (region == "SU" || region == "HI" || region == "DS" || region == "SH" || region == "GW")
		{
			return 10;
		}
		if (region == "SI" || region == "UW" || region == "SS")
		{
			return 20;
		}
		if (region == "SB" || region == "LF")
		{
			return 30;
		}
		if (region == "SL")
		{
			return -10;
		}
		if (ModManager.MSC)
		{
			if (region == "VS")
			{
				return 15;
			}
			if (region == "CL" || region == "LC" || region == "UG")
			{
				return 25;
			}
			if (region == "OE" || region == "RM")
			{
				return 35;
			}
		}
		return 0;
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is PearlDeliveryChallenge)
		{
			if ((challenge as PearlDeliveryChallenge).region == region)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
	{
		if ((ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint) || (!ModManager.MSC && slugcat == SlugcatStats.Name.Yellow))
		{
			return false;
		}
		return true;
	}

	public override bool CombatRequired()
	{
		return false;
	}

	public override string ToString()
	{
		return "PearlDeliveryChallenge" + "~" + region + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			region = array[0];
			completed = array[1] == "1";
			hidden = array[2] == "1";
			revealed = array[3] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: PearlDeliveryChallenge FromString() encountered an error: " + ex.Message);
		}
	}

	public override bool CanBeHidden()
	{
		return false;
	}
}
