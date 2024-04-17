using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class PearlHoardChallenge : Challenge
{
	public bool common;

	public string region;

	public int amount;

	public override void UpdateDescription()
	{
		string newValue = (common ? ChallengeTools.IGT.Translate("common pearls") : ChallengeTools.IGT.Translate("colored pearls"));
		description = ChallengeTools.IGT.Translate("Store <amount> <target_pearl> in a shelter in <region_name>").Replace("<amount>", ValueConverter.ConvertToString(amount)).Replace("<target_pearl>", newValue)
			.Replace("<region_name>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer)));
		base.UpdateDescription();
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is PearlHoardChallenge)
		{
			return false;
		}
		return true;
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Pearl Hoarding");
	}

	public override Challenge Generate()
	{
		bool flag = false;
		if ((ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint) || (!ModManager.MSC && ExpeditionData.slugcatPlayer == SlugcatStats.Name.Yellow))
		{
			flag = true;
		}
		List<string> list = SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer);
		list.Remove("HR");
		string text = list[UnityEngine.Random.Range(0, list.Count)];
		return new PearlHoardChallenge
		{
			common = flag,
			amount = (int)Mathf.Lerp(2f, 5f, ExpeditionData.challengeDifficulty),
			region = text
		};
	}

	public override int Points()
	{
		return (common ? 10 : 23) * amount * (int)(hidden ? 2f : 1f);
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
			int num = 0;
			int num2 = 0;
			if (game.Players[i] == null || game.Players[i].realizedCreature == null || game.Players[i].realizedCreature.room == null || !game.Players[i].realizedCreature.room.abstractRoom.shelter || !(game.Players[i].world.name == region))
			{
				continue;
			}
			for (int j = 0; j < game.Players[i].realizedCreature.room.updateList.Count; j++)
			{
				if (game.Players[i].realizedCreature.room.updateList[j] is DataPearl)
				{
					if ((game.Players[i].realizedCreature.room.updateList[j] as DataPearl).AbstractPearl.dataPearlType.value != DataPearl.AbstractDataPearl.DataPearlType.Misc.value && (game.Players[i].realizedCreature.room.updateList[j] as DataPearl).AbstractPearl.dataPearlType.value != DataPearl.AbstractDataPearl.DataPearlType.Misc2.value)
					{
						num2++;
					}
					else
					{
						num++;
					}
				}
				if (game.Players[i].realizedCreature.room.updateList[j] is PebblesPearl)
				{
					num2++;
				}
				if ((common && num >= amount) || (!common && num2 >= amount))
				{
					CompleteChallenge();
				}
			}
		}
	}

	public override string ToString()
	{
		return "PearlHoardChallenge" + "~" + (common ? "1" : "0") + "><" + ValueConverter.ConvertToString(amount) + "><" + ValueConverter.ConvertToString(region) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			common = array[0] == "1";
			amount = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			region = array[2];
			completed = array[3] == "1";
			hidden = array[4] == "1";
			revealed = array[5] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: PearlHoardChallenge FromString() encountered an error: " + ex.Message);
		}
	}

	public override bool CanBeHidden()
	{
		return false;
	}
}
