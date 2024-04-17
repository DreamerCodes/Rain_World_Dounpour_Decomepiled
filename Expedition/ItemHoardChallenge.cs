using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class ItemHoardChallenge : Challenge
{
	public AbstractPhysicalObject.AbstractObjectType target;

	public int amount;

	public override void UpdateDescription()
	{
		description = ChallengeTools.IGT.Translate("Store <amount> <target_item> in the same shelter").Replace("<amount>", ValueConverter.ConvertToString(amount)).Replace("<target_item>", ChallengeTools.ItemName(target));
		base.UpdateDescription();
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is ItemHoardChallenge)
		{
			if ((challenge as ItemHoardChallenge).target != target)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Item Collecting");
	}

	public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
	{
		if (ModManager.MSC && slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer && target == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
		{
			return false;
		}
		return true;
	}

	public override Challenge Generate()
	{
		AbstractPhysicalObject.AbstractObjectType abstractObjectType = ChallengeTools.ObjectTypes[UnityEngine.Random.Range(0, ChallengeTools.ObjectTypes.Count - 1)];
		return new ItemHoardChallenge
		{
			amount = (int)Mathf.Lerp(2f, 8f, ExpeditionData.challengeDifficulty),
			target = abstractObjectType
		};
	}

	public override int Points()
	{
		int num = 7 * amount * (int)(hidden ? 2f : 1f);
		if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Gourmand)
		{
			num = Mathf.RoundToInt((float)num * 0.75f);
		}
		return num;
	}

	public override bool CombatRequired()
	{
		return false;
	}

	public override void Update()
	{
		base.Update();
		for (int i = 0; i < game.Players.Count; i++)
		{
			if (game.Players[i] == null || game.Players[i].realizedCreature == null || game.Players[i].realizedCreature.room == null || !game.Players[i].Room.shelter)
			{
				continue;
			}
			int num = 0;
			for (int j = 0; j < game.Players[i].realizedCreature.room.updateList.Count; j++)
			{
				if (game.Players[i].realizedCreature.room.updateList[j] is PhysicalObject && (game.Players[i].realizedCreature.room.updateList[j] as PhysicalObject).abstractPhysicalObject.type == target)
				{
					num++;
				}
			}
			if (num >= amount)
			{
				CompleteChallenge();
			}
		}
	}

	public override string ToString()
	{
		return "ItemHoardChallenge" + "~" + ValueConverter.ConvertToString(amount) + "><" + ValueConverter.ConvertToString(target.value) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			amount = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			target = new AbstractPhysicalObject.AbstractObjectType(array[1]);
			completed = array[2] == "1";
			hidden = array[3] == "1";
			revealed = array[4] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: ItemHoardChallenge FromString() encountered an error: " + ex.Message);
		}
	}
}
