using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class GlobalScoreChallenge : Challenge
{
	public int target;

	public int score;

	public int increase;

	public int[] killScores;

	public override void UpdateDescription()
	{
		description = ChallengeTools.IGT.Translate("Earn <score_target> points from creature kills [<current_score>/<score_target>]").Replace("<score_target>", ValueConverter.ConvertToString(target)).Replace("<current_score>", ValueConverter.ConvertToString(score));
		base.UpdateDescription();
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is GlobalScoreChallenge)
		{
			return false;
		}
		return true;
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Overall Score");
	}

	public override Challenge Generate()
	{
		int num = Mathf.RoundToInt(Mathf.Lerp(150f, 300f, ExpeditionData.challengeDifficulty) / 10f) * 10;
		return new GlobalScoreChallenge
		{
			target = num
		};
	}

	public override void Reset()
	{
		score = 0;
		increase = 0;
		base.Reset();
	}

	public override int Points()
	{
		float num = 1f;
		if (ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Saint)
		{
			num = 1.35f;
		}
		return (int)((float)(target / 4) * num) * (int)(hidden ? 2f : 1f);
	}

	public override bool CombatRequired()
	{
		return true;
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
		if (type != null && ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type) != null)
		{
			int points = ChallengeTools.creatureSpawns[ExpeditionData.slugcatPlayer.value].Find((ChallengeTools.ExpeditionCreature f) => f.creature == type).points;
			score += points;
			ExpLog.Log("Player " + (playerNumber + 1) + " killed " + type.value + " | +" + points);
		}
		UpdateDescription();
		if (score >= target)
		{
			score = target;
			CompleteChallenge();
		}
	}

	public override string ToString()
	{
		return "GlobalScoreChallenge" + "~" + ValueConverter.ConvertToString(score) + "><" + ValueConverter.ConvertToString(target) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			score = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			target = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			completed = array[2] == "1";
			hidden = array[3] == "1";
			revealed = array[4] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: GlobalScoreChallenge FromString() encountered an error: " + ex.Message);
		}
	}
}
