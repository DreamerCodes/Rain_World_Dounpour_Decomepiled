using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class SLOrcacleState
{
	public class PlayerOpinion : ExtEnum<PlayerOpinion>
	{
		public static readonly PlayerOpinion NotSpeaking = new PlayerOpinion("NotSpeaking", register: true);

		public static readonly PlayerOpinion Dislikes = new PlayerOpinion("Dislikes", register: true);

		public static readonly PlayerOpinion Neutral = new PlayerOpinion("Neutral", register: true);

		public static readonly PlayerOpinion Likes = new PlayerOpinion("Likes", register: true);

		public PlayerOpinion(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private int[] integers;

	public bool[] miscBools;

	public bool[] unrecognizedMiscBools;

	public List<DataPearl.AbstractDataPearl.DataPearlType> significantPearls;

	public List<SLOracleBehaviorHasMark.MiscItemType> miscItemsDescribed;

	public List<EntityID> alreadyTalkedAboutItems;

	public List<string> unrecognizedSaveStrings;

	public int[] unrecognizedIntegers;

	public bool shownEnergyCell;

	public float likesPlayer;

	public bool isDebugState;

	public bool increaseLikeOnSave = true;

	public bool talkedAboutPebblesDeath;

	public int playerEncounters
	{
		get
		{
			return integers[0];
		}
		set
		{
			integers[0] = value;
		}
	}

	public int playerEncountersWithMark
	{
		get
		{
			return integers[1];
		}
		set
		{
			integers[1] = value;
		}
	}

	public int neuronsLeft
	{
		get
		{
			return integers[2];
		}
		set
		{
			integers[2] = value;
		}
	}

	public int neuronGiveConversationCounter
	{
		get
		{
			return integers[3];
		}
		set
		{
			integers[3] = value;
		}
	}

	public int totNeuronsGiven
	{
		get
		{
			return integers[4];
		}
		set
		{
			integers[4] = value;
		}
	}

	public int leaves
	{
		get
		{
			return integers[5];
		}
		set
		{
			integers[5] = value;
		}
	}

	public int annoyances
	{
		get
		{
			return integers[6];
		}
		set
		{
			integers[6] = value;
		}
	}

	public int totalInterruptions
	{
		get
		{
			return integers[7];
		}
		set
		{
			integers[7] = value;
		}
	}

	public int totalItemsBrought
	{
		get
		{
			return integers[8];
		}
		set
		{
			integers[8] = value;
		}
	}

	public int totalPearlsBrought
	{
		get
		{
			return integers[9];
		}
		set
		{
			integers[9] = value;
		}
	}

	public int miscPearlCounter
	{
		get
		{
			return integers[10];
		}
		set
		{
			integers[10] = value;
		}
	}

	public int chatLogA
	{
		get
		{
			return integers[11];
		}
		set
		{
			integers[11] = value;
		}
	}

	public int chatLogB
	{
		get
		{
			return integers[12];
		}
		set
		{
			integers[12] = value;
		}
	}

	public bool hasToldPlayerNotToEatNeurons
	{
		get
		{
			return miscBools[0];
		}
		set
		{
			miscBools[0] = value;
		}
	}

	public PlayerOpinion GetOpinion => new PlayerOpinion(ExtEnum<PlayerOpinion>.values.GetEntry((int)Mathf.Clamp(Custom.LerpMap(likesPlayer, -1f, 1f, 0f, ExtEnum<PlayerOpinion>.values.Count), 0f, ExtEnum<PlayerOpinion>.values.Count - 1)));

	public bool SpeakingTerms => GetOpinion != PlayerOpinion.NotSpeaking;

	public void InfluenceLike(float influence)
	{
		likesPlayer = Mathf.Clamp(likesPlayer + influence, -1f, 1f);
	}

	public SLOrcacleState(bool isDebugState, SlugcatStats.Name saveStateNumber)
	{
		this.isDebugState = isDebugState;
		ForceResetState(saveStateNumber);
	}

	public void AddItemToAlreadyTalkedAbout(EntityID ID)
	{
		for (int i = 0; i < alreadyTalkedAboutItems.Count; i++)
		{
			if (alreadyTalkedAboutItems[i] == ID)
			{
				return;
			}
		}
		alreadyTalkedAboutItems.Add(ID);
	}

	public bool HaveIAlreadyDescribedThisItem(EntityID ID)
	{
		for (int i = 0; i < alreadyTalkedAboutItems.Count; i++)
		{
			if (alreadyTalkedAboutItems[i] == ID)
			{
				return true;
			}
		}
		return false;
	}

	public void ForceResetState(SlugcatStats.Name saveStateNumber)
	{
		increaseLikeOnSave = true;
		integers = new int[14];
		unrecognizedIntegers = null;
		miscBools = new bool[1];
		unrecognizedMiscBools = null;
		significantPearls = new List<DataPearl.AbstractDataPearl.DataPearlType>();
		miscItemsDescribed = new List<SLOracleBehaviorHasMark.MiscItemType>();
		unrecognizedSaveStrings = new List<string>();
		likesPlayer = 0.3f;
		if (saveStateNumber == SlugcatStats.Name.Red || (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
		{
			neuronsLeft = 0;
		}
		else if ((ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) || (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			neuronsLeft = 7;
		}
		else
		{
			neuronsLeft = 5;
		}
		playerEncountersWithMark = 0;
		alreadyTalkedAboutItems = new List<EntityID>();
		shownEnergyCell = false;
		chatLogA = -1;
		chatLogB = -1;
	}

	public override string ToString()
	{
		string text = "";
		text += "integersArray<slosB>";
		text += SaveUtils.SaveIntegerArray('.', integers, unrecognizedIntegers);
		text += "<slosA>";
		text += "miscBools<slosB>";
		text += SaveUtils.SaveBooleanArray(miscBools, unrecognizedMiscBools);
		text += "<slosA>";
		text += "significantPearls<slosB>";
		for (int i = 0; i < significantPearls.Count; i++)
		{
			text += significantPearls[i];
			if (i < significantPearls.Count - 1)
			{
				text += ",";
			}
		}
		text += "<slosA>";
		text += "miscItemsDescribed<slosB>";
		for (int j = 0; j < miscItemsDescribed.Count; j++)
		{
			text += miscItemsDescribed[j];
			if (j < miscItemsDescribed.Count - 1)
			{
				text += ",";
			}
		}
		text += "<slosA>";
		if (increaseLikeOnSave)
		{
			InfluenceLike(0.15f);
		}
		text += string.Format(CultureInfo.InvariantCulture, "likesPlayer<slosB>{0}<slosA>", likesPlayer);
		if (alreadyTalkedAboutItems.Count > 0)
		{
			text += "itemsAlreadyTalkedAbout<slosB>";
			for (int k = 0; k < alreadyTalkedAboutItems.Count; k++)
			{
				text = text + alreadyTalkedAboutItems[k].ToString() + ((k < alreadyTalkedAboutItems.Count - 1) ? "<slosC>" : "");
			}
			text += "<slosA>";
		}
		if (ModManager.MSC && talkedAboutPebblesDeath)
		{
			text += "talkedPebblesDeath<slosA>";
		}
		if (ModManager.MSC && shownEnergyCell)
		{
			text += "shownEnergyCell<slosA>";
		}
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<slosA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		unrecognizedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<slosA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<slosB>");
			switch (array2[0])
			{
			case "integersArray":
				unrecognizedIntegers = SaveUtils.LoadIntegersArray(array2[1], '.', integers);
				break;
			case "miscBools":
				unrecognizedMiscBools = SaveUtils.LoadBooleanArray(array2[1], miscBools);
				break;
			case "significantPearls":
			{
				if (Custom.IsDigitString(array2[1]))
				{
					BackwardsCompatibilityRemix.ParseSignificantPearls(array2[1], significantPearls);
					break;
				}
				significantPearls.Clear();
				string[] array4 = array2[1].Split(',');
				foreach (string text2 in array4)
				{
					if (!(text2 == string.Empty))
					{
						significantPearls.Add(new DataPearl.AbstractDataPearl.DataPearlType(text2));
					}
				}
				break;
			}
			case "miscItemsDescribed":
			{
				if (Custom.IsDigitString(array2[1]))
				{
					BackwardsCompatibilityRemix.ParseMiscItems(array2[1], miscItemsDescribed);
					break;
				}
				miscItemsDescribed.Clear();
				string[] array4 = array2[1].Split(',');
				foreach (string text in array4)
				{
					if (!(text == string.Empty))
					{
						miscItemsDescribed.Add(new SLOracleBehaviorHasMark.MiscItemType(text));
					}
				}
				break;
			}
			case "likesPlayer":
				likesPlayer = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "itemsAlreadyTalkedAbout":
			{
				string[] array3 = Regex.Split(array2[1], "<slosC>");
				for (int j = 0; j < array3.Length; j++)
				{
					if (array3[j].Length > 0)
					{
						alreadyTalkedAboutItems.Add(EntityID.FromString(array3[j]));
					}
				}
				break;
			}
			case "talkedPebblesDeath":
				if (ModManager.MSC)
				{
					talkedAboutPebblesDeath = true;
				}
				else
				{
					unrecognizedSaveStrings.Add(array[i]);
				}
				break;
			case "shownEnergyCell":
				if (ModManager.MSC)
				{
					shownEnergyCell = true;
				}
				else
				{
					unrecognizedSaveStrings.Add(array[i]);
				}
				break;
			default:
				if (array[i].Trim().Length > 0 && array2.Length >= 1)
				{
					unrecognizedSaveStrings.Add(array[i]);
				}
				break;
			}
		}
	}
}
