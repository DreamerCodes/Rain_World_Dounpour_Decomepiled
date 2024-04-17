using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using UnityEngine;

namespace Expedition;

public class PinChallenge : Challenge
{
	public int current;

	public int target;

	public List<Creature> pinList = new List<Creature>();

	public List<Spear> spearList = new List<Spear>();

	public override void UpdateDescription()
	{
		description = ChallengeTools.IGT.Translate("Pin <pin_amount> creatures to walls or floors [<current_pin>/<pin_amount>]").Replace("<current_pin>", ValueConverter.ConvertToString(current)).Replace("<pin_amount>", ValueConverter.ConvertToString(target));
		base.UpdateDescription();
	}

	public override int Points()
	{
		return 8 * target * (int)(hidden ? 2f : 1f);
	}

	public override Challenge Generate()
	{
		int maxExclusive = Mathf.RoundToInt(Mathf.Lerp(4f, 20f, ExpeditionData.challengeDifficulty));
		return new PinChallenge
		{
			target = UnityEngine.Random.Range(5, maxExclusive)
		};
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
				if (game.Players[i].realizedCreature.room.updateList[j] is Spear && (game.Players[i].realizedCreature.room.updateList[j] as Spear).thrownBy != null && (game.Players[i].realizedCreature.room.updateList[j] as Spear).thrownBy is Player && !spearList.Contains(game.Players[i].realizedCreature.room.updateList[j] as Spear))
				{
					ExpLog.Log("Spear added to spearList");
					spearList.Add(game.Players[i].realizedCreature.room.updateList[j] as Spear);
				}
			}
		}
		for (int k = 0; k < spearList.Count; k++)
		{
			if ((spearList[k].thrownBy != null && !(spearList[k].thrownBy is Player)) || spearList[k] == null)
			{
				ExpLog.Log("Spear removed from spearList");
				spearList.Remove(spearList[k]);
				break;
			}
			if (spearList[k].stuckInObject != null && spearList[k].stuckInObject is Creature && spearList[k].stuckInWall.HasValue && !pinList.Contains(spearList[k].stuckInObject as Creature))
			{
				ExpLog.Log("Creature pinned!");
				pinList.Add(spearList[k].stuckInObject as Creature);
				current++;
				UpdateDescription();
				spearList.Remove(spearList[k]);
				return;
			}
		}
		if (current >= target)
		{
			CompleteChallenge();
		}
	}

	public override bool CombatRequired()
	{
		return true;
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is PinChallenge)
		{
			return false;
		}
		return true;
	}

	public override void Reset()
	{
		current = 0;
		pinList = new List<Creature>();
		spearList = new List<Spear>();
		base.Reset();
	}

	public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
	{
		return true;
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Spear Pinning");
	}

	public override string ToString()
	{
		return "PinChallenge" + "~" + ValueConverter.ConvertToString(current) + "><" + ValueConverter.ConvertToString(target) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			current = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			target = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			completed = array[2] == "1";
			hidden = array[3] == "1";
			revealed = array[4] == "1";
			pinList = new List<Creature>();
			spearList = new List<Spear>();
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: PinChallenge FromString() encountered an error: " + ex.Message);
		}
	}
}
