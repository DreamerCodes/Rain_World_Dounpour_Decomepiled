using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class HuntChallenge : Challenge
{
	public CreatureTemplate.Type target;

	public int current;

	public int amount;

	public override void UpdateDescription()
	{
		if (ChallengeTools.creatureNames == null)
		{
			ChallengeTools.CreatureName(ref ChallengeTools.creatureNames);
		}
		string newValue = "Unknown";
		try
		{
			if (target.Index >= 0)
			{
				newValue = ChallengeTools.IGT.Translate(ChallengeTools.creatureNames[target.Index]);
			}
		}
		catch (Exception ex)
		{
			ExpLog.Log("Error getting creature name for HuntChallenge | " + ex.Message);
		}
		description = ChallengeTools.IGT.Translate("Kill <target_creature> [<current_kills>/<target_kills>]").Replace("<target_creature>", newValue).Replace("<current_kills>", ValueConverter.ConvertToString(current))
			.Replace("<target_kills>", ValueConverter.ConvertToString(amount));
		base.UpdateDescription();
	}

	public override Challenge Generate()
	{
		ChallengeTools.ExpeditionCreature expeditionCreature = ChallengeTools.GetExpeditionCreature(ExpeditionData.slugcatPlayer, ExpeditionData.challengeDifficulty);
		int num = (int)Mathf.Lerp(3f, 15f, (float)Math.Pow(ExpeditionData.challengeDifficulty, 2.5));
		if (expeditionCreature.points < 7)
		{
			num += UnityEngine.Random.Range(3, 6);
		}
		if (num > expeditionCreature.spawns)
		{
			num = expeditionCreature.spawns;
		}
		if (num > 15)
		{
			num = 15;
		}
		return new HuntChallenge
		{
			target = expeditionCreature.creature,
			amount = num
		};
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Creature Hunting");
	}

	public override int Points()
	{
		int result = 0;
		try
		{
			float num = 1f;
			CreatureTemplate.Type critTarget = target;
			if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				num = 1.35f;
			}
			if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear && target == CreatureTemplate.Type.DaddyLongLegs)
			{
				critTarget = CreatureTemplate.Type.BrotherLongLegs;
			}
			result = (int)((float)(ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature c) => c.creature == critTarget).points * amount) * num) * (int)(hidden ? 2f : 1f);
		}
		catch (Exception ex)
		{
			ExpLog.Log("Creature not found: " + ex.Message);
		}
		return result;
	}

	public override void Reset()
	{
		current = 0;
		base.Reset();
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is HuntChallenge)
		{
			if ((challenge as HuntChallenge).target.value == target.value)
			{
				ExpLog.Log("CONFLICT: " + (challenge as HuntChallenge).target.value + " | " + target.value);
				return false;
			}
			return true;
		}
		return true;
	}

	public override string ToString()
	{
		return "HuntChallenge" + "~" + ValueConverter.ConvertToString(target) + "><" + ValueConverter.ConvertToString(amount) + "><" + ValueConverter.ConvertToString(current) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override bool CombatRequired()
	{
		return true;
	}

	public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
	{
		return true;
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			target = new CreatureTemplate.Type(array[0]);
			amount = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			current = int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			completed = array[3] == "1";
			hidden = array[4] == "1";
			revealed = array[5] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: HuntChallenge FromString() encountered an error: " + ex.Message);
		}
	}

	public override bool RespondToCreatureKill()
	{
		return true;
	}

	public override void CreatureKilled(Creature crit, int playerNumber)
	{
		if (completed || game == null || crit == null)
		{
			return;
		}
		CreatureTemplate.Type type = crit.abstractCreature.creatureTemplate.type;
		bool flag = type == target;
		if (target == CreatureTemplate.Type.DaddyLongLegs && type == CreatureTemplate.Type.BrotherLongLegs && (crit as DaddyLongLegs).colorClass)
		{
			flag = true;
		}
		if (flag)
		{
			current++;
			ExpLog.Log("Player " + (playerNumber + 1) + " killed " + type.value);
			UpdateDescription();
			if (current >= amount)
			{
				CompleteChallenge();
			}
		}
	}
}
