using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using MoreSlugcats;

public class MiscWorldSaveData
{
	public int SSaiConversationsHad;

	public int SSaiThrowOuts;

	private SLOrcacleState privSlOracleState;

	public PlayerGuideState playerGuideState;

	public List<string> unrecognizedSaveStrings;

	public bool moonRevived;

	public bool pebblesSeenGreenNeuron;

	public bool memoryArraysFrolicked;

	public SlugcatStats.Name saveStateNumber;

	public int cyclesSinceSSai;

	public bool pebblesEnergyTaken;

	public int energySeenState;

	public bool moonHeartRestored;

	public bool moonGivenRobe;

	public bool smPearlTagged;

	public bool discussedHalcyon;

	public bool halcyonStolen;

	public bool pebblesRivuletPostgame;

	public bool hrMelted;

	public int cyclesSinceLastSlugpup;

	public SLOrcacleState SLOracleState
	{
		get
		{
			if (privSlOracleState == null)
			{
				privSlOracleState = new SLOrcacleState(isDebugState: false, saveStateNumber);
			}
			return privSlOracleState;
		}
	}

	public bool EverMetMoon
	{
		get
		{
			if (privSlOracleState == null)
			{
				return false;
			}
			return privSlOracleState.playerEncounters > 0;
		}
	}

	public MiscWorldSaveData(SlugcatStats.Name saveStateNumber)
	{
		this.saveStateNumber = saveStateNumber;
		playerGuideState = new PlayerGuideState();
		unrecognizedSaveStrings = new List<string>();
	}

	public override string ToString()
	{
		string text = "";
		if (SSaiConversationsHad > 0)
		{
			text += string.Format(CultureInfo.InvariantCulture, "SSaiConversationsHad<mwB>{0}<mwA>", SSaiConversationsHad);
		}
		if (SSaiThrowOuts > 0)
		{
			text += string.Format(CultureInfo.InvariantCulture, "SSaiThrowOuts<mwB>{0}<mwA>", SSaiThrowOuts);
		}
		if (privSlOracleState != null && (privSlOracleState.playerEncounters > 0 || (ModManager.MSC && saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer)))
		{
			text = text + "SLaiState<mwB>" + privSlOracleState.ToString() + "<mwA>";
		}
		text = text + "playerGuideState<mwB>" + playerGuideState?.ToString() + "<mwA>";
		if (moonRevived)
		{
			text += "MOONREVIVED<mwA>";
		}
		if (pebblesSeenGreenNeuron)
		{
			text += "PEBBLESHELPED<mwA>";
		}
		if (memoryArraysFrolicked)
		{
			text += "MEMORYFROLICK<mwA>";
		}
		if (ModManager.MSC)
		{
			if (cyclesSinceSSai > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "CyclesSinceSSai<mwB>{0}<mwA>", cyclesSinceSSai);
			}
			if (pebblesEnergyTaken)
			{
				text += "ENERGYRAILOFF<mwA>";
			}
			if (energySeenState > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "EnergySeenState<mwB>{0}<mwA>", energySeenState);
			}
			if (moonHeartRestored)
			{
				text += "MOONHEART<mwA>";
			}
			if (moonGivenRobe)
			{
				text += "MOONROBE<mwA>";
			}
			if (smPearlTagged)
			{
				text += "SMPEARLTAGGED<mwA>";
			}
			if (discussedHalcyon)
			{
				text += "HALCYONTALK<mwA>";
			}
			if (halcyonStolen)
			{
				text += "HALCYONSTOLE<mwA>";
			}
			if (pebblesRivuletPostgame)
			{
				text += "PEBRIVPOST<mwA>";
			}
			if (hrMelted)
			{
				text += "HRMELT<mwA>";
			}
			if (cyclesSinceLastSlugpup > 0)
			{
				text += string.Format(CultureInfo.InvariantCulture, "CyclesSinceSlugpup<mwB>{0}<mwA>", cyclesSinceLastSlugpup);
			}
		}
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<mwA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		moonRevived = false;
		pebblesSeenGreenNeuron = false;
		memoryArraysFrolicked = false;
		SSaiConversationsHad = 0;
		SSaiThrowOuts = 0;
		if (ModManager.MSC)
		{
			cyclesSinceSSai = 0;
			hrMelted = false;
			cyclesSinceLastSlugpup = 0;
			if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				moonHeartRestored = true;
			}
			if (saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Rivulet || saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				moonGivenRobe = true;
			}
		}
		unrecognizedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<mwA>");
		for (int i = 0; i < array.Length; i++)
		{
			bool flag = false;
			string[] array2 = Regex.Split(array[i], "<mwB>");
			switch (array2[0])
			{
			case "SSaiConversationsHad":
				SSaiConversationsHad = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SSaiThrowOuts":
				SSaiThrowOuts = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "SLaiState":
				privSlOracleState = new SLOrcacleState(isDebugState: false, saveStateNumber);
				privSlOracleState.FromString(array2[1]);
				break;
			case "playerGuideState":
				playerGuideState.FromString(array2[1]);
				break;
			case "MOONREVIVED":
				moonRevived = true;
				break;
			case "PEBBLESHELPED":
				pebblesSeenGreenNeuron = true;
				break;
			case "MEMORYFROLICK":
				memoryArraysFrolicked = true;
				break;
			case "CyclesSinceSSai":
				if (ModManager.MSC)
				{
					cyclesSinceSSai = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag = true;
				}
				break;
			case "ENERGYRAILOFF":
				if (ModManager.MSC)
				{
					pebblesEnergyTaken = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "EnergySeenState":
				if (ModManager.MSC)
				{
					energySeenState = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag = true;
				}
				break;
			case "MOONHEART":
				if (ModManager.MSC)
				{
					moonHeartRestored = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "MOONROBE":
				if (ModManager.MSC)
				{
					moonGivenRobe = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "SMPEARLTAGGED":
				if (ModManager.MSC)
				{
					smPearlTagged = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "HALCYONTALK":
				if (ModManager.MSC)
				{
					discussedHalcyon = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "HALCYONSTOLE":
				if (ModManager.MSC)
				{
					halcyonStolen = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "PEBRIVPOST":
				if (ModManager.MSC)
				{
					pebblesRivuletPostgame = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "HRMELT":
				if (ModManager.MSC)
				{
					hrMelted = true;
				}
				else
				{
					flag = true;
				}
				break;
			case "CyclesSinceSlugpup":
				if (ModManager.MSC)
				{
					cyclesSinceLastSlugpup = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				else
				{
					flag = true;
				}
				break;
			default:
				flag = true;
				break;
			}
			if (flag && array[i].Trim().Length > 0 && array2.Length >= 1)
			{
				unrecognizedSaveStrings.Add(array[i]);
			}
		}
	}
}
