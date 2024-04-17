using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using UnityEngine;

namespace Expedition;

public class NeuronDeliveryChallenge : Challenge
{
	public int neurons;

	public int delivered;

	public override bool ValidForThisSlugcat(SlugcatStats.Name slugcat)
	{
		if (ModManager.MSC && (slugcat == MoreSlugcatsEnums.SlugcatStatsName.Spear || slugcat == MoreSlugcatsEnums.SlugcatStatsName.Saint || slugcat == MoreSlugcatsEnums.SlugcatStatsName.Artificer))
		{
			return false;
		}
		return true;
	}

	public override void UpdateDescription()
	{
		description = ChallengeTools.IGT.Translate("Neurons delivered to Looks to the Moon <progress>").Replace("<progress>", "[" + delivered + "/" + neurons + "]");
		base.UpdateDescription();
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Neuron Gifting");
	}

	public override void Update()
	{
		base.Update();
		if (game != null && game.rainWorld.progression.currentSaveState != null)
		{
			if (game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.totNeuronsGiven > delivered)
			{
				delivered = game.rainWorld.progression.currentSaveState.miscWorldSaveData.SLOracleState.totNeuronsGiven;
				UpdateDescription();
			}
			if (!completed && delivered >= neurons)
			{
				CompleteChallenge();
			}
		}
	}

	public override void Reset()
	{
		delivered = 0;
		base.Reset();
	}

	public override bool CombatRequired()
	{
		return false;
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is NeuronDeliveryChallenge)
		{
			return false;
		}
		return true;
	}

	public override Challenge Generate()
	{
		return new NeuronDeliveryChallenge
		{
			neurons = Mathf.RoundToInt(UnityEngine.Random.Range(1f, Mathf.Lerp(1f, 4f, Mathf.InverseLerp(0.4f, 1f, ExpeditionData.challengeDifficulty))))
		};
	}

	public override int Points()
	{
		return 70 * neurons * (int)(hidden ? 2f : 1f);
	}

	public override string ToString()
	{
		return "NeuronDeliveryChallenge" + "~" + ValueConverter.ConvertToString(neurons) + "><" + ValueConverter.ConvertToString(delivered) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			neurons = int.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			delivered = int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture);
			completed = array[2] == "1";
			hidden = array[3] == "1";
			revealed = array[4] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: NeuronDeliveryChallenge FromString() encountered an error: " + ex.Message);
		}
	}

	public override bool CanBeHidden()
	{
		return false;
	}
}
