using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;
using UnityEngine;

public class PlayerGuideState
{
	public List<AbstractPhysicalObject.AbstractObjectType> itemTypes;

	public List<CreatureTemplate.Type> creatureTypes;

	public List<string> unrecognizedSaveStrings;

	public int[] integers;

	public bool increaseLikeOnSave = true;

	public float likesPlayer;

	public float handHolding;

	public bool hasBeenToASwarmRoomThisCycle;

	public float wantFoodHandHoldingThisCycle = 0.65f;

	public float wantShelterHandHoldingThisCycle = 1f;

	public float wantDirectionHandHoldingThisCycle = 0.6f;

	public List<string> imagesShown;

	public List<KeyValuePair<int, int>> forcedDirectionsGiven;

	public bool playerHasVisitedMoon
	{
		get
		{
			return integers[0] == 1;
		}
		set
		{
			integers[0] = (value ? 1 : 0);
		}
	}

	public int superJumpsShown
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

	public int pickupObjectsShown
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

	public bool scavTradeInstructionCompleted
	{
		get
		{
			return integers[3] > 0;
		}
		set
		{
			integers[3] = (value ? 1 : 0);
		}
	}

	public bool angryWithPlayer
	{
		get
		{
			return integers[4] > 0;
		}
		set
		{
			integers[4] = (value ? 1 : 0);
		}
	}

	public bool displayedAnger
	{
		get
		{
			return integers[5] > 0;
		}
		set
		{
			integers[5] = (value ? 1 : 0);
		}
	}

	public int guideSymbol
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

	public void InfluenceLike(float influence, bool print)
	{
		likesPlayer = Mathf.Clamp(likesPlayer + influence, -1f, 1f);
		if (print)
		{
			Custom.Log("guide likes player:", likesPlayer.ToString());
		}
	}

	public void InfluenceHandHolding(float influence, bool print)
	{
		handHolding = Mathf.Clamp(handHolding + influence, 0f, 1f);
		if (print)
		{
			Custom.Log("guide hand holding player:", handHolding.ToString());
		}
	}

	public void ImageShownInRoom(string roomName)
	{
		if (!imagesShown.Contains(roomName))
		{
			imagesShown.Add(roomName);
		}
	}

	public bool HasImageBeenShownInRoom(string roomName)
	{
		return imagesShown.Contains(roomName);
	}

	public PlayerGuideState()
	{
		integers = new int[7];
		itemTypes = new List<AbstractPhysicalObject.AbstractObjectType>();
		creatureTypes = new List<CreatureTemplate.Type>();
		unrecognizedSaveStrings = new List<string>();
		imagesShown = new List<string>();
		likesPlayer = 0.3f;
		handHolding = 0.8f;
		forcedDirectionsGiven = new List<KeyValuePair<int, int>>();
	}

	public int HowManyTimesHasForcedDirectionBeenGiven(int room)
	{
		for (int i = 0; i < forcedDirectionsGiven.Count; i++)
		{
			if (forcedDirectionsGiven[i].Key == room)
			{
				return forcedDirectionsGiven[i].Value;
			}
		}
		return 0;
	}

	public void IncrementTimesForcedDirectionHasBeenGiven(int room)
	{
		for (int i = 0; i < forcedDirectionsGiven.Count; i++)
		{
			if (forcedDirectionsGiven[i].Key == room)
			{
				forcedDirectionsGiven[i] = new KeyValuePair<int, int>(forcedDirectionsGiven[i].Key, forcedDirectionsGiven[i].Value + 1);
				return;
			}
		}
		forcedDirectionsGiven.Add(new KeyValuePair<int, int>(room, 1));
	}

	public override string ToString()
	{
		string text = "";
		text += "integersArray<pgsB>";
		for (int i = 0; i < integers.Length; i++)
		{
			text += string.Format(CultureInfo.InvariantCulture, "{0}{1}", integers[i], (i < integers.Length - 1) ? "." : "");
		}
		text += "<pgsA>";
		text += "itemTypes<pgsB>";
		for (int j = 0; j < itemTypes.Count; j++)
		{
			text += itemTypes[j];
			if (j < itemTypes.Count - 1)
			{
				text += ",";
			}
		}
		text += "<pgsA>";
		text += "creatureTypes<pgsB>";
		for (int k = 0; k < creatureTypes.Count; k++)
		{
			text += creatureTypes[k];
			if (k < creatureTypes.Count - 1)
			{
				text += ",";
			}
		}
		text += "<pgsA>";
		if (angryWithPlayer)
		{
			likesPlayer = 0f;
		}
		else if (increaseLikeOnSave)
		{
			InfluenceLike(0.1f, print: false);
		}
		InfluenceHandHolding(0.025f, print: false);
		text += string.Format(CultureInfo.InvariantCulture, "likesPlayer<pgsB>{0}<pgsA>", likesPlayer);
		text += string.Format(CultureInfo.InvariantCulture, "directionHandHolding<pgsB>{0}<pgsA>", handHolding);
		if (imagesShown.Count > 0)
		{
			text += "imagesShown<pgsB>";
			for (int l = 0; l < imagesShown.Count; l++)
			{
				text = text + imagesShown[l] + ((l < imagesShown.Count - 1) ? "." : "");
			}
			text += "<pgsA>";
		}
		if (forcedDirectionsGiven.Count > 0)
		{
			text = "forcedDirsGiven<pgsB>";
			for (int m = 0; m < forcedDirectionsGiven.Count; m++)
			{
				text += string.Format(CultureInfo.InvariantCulture, "{0},{1}{2}", forcedDirectionsGiven[m].Key, forcedDirectionsGiven[m].Value, (m < forcedDirectionsGiven.Count - 1) ? "." : "");
			}
			text += "<pgsA>";
		}
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<pgsA>";
		}
		return text;
	}

	public void FromString(string s)
	{
		unrecognizedSaveStrings.Clear();
		string[] array = Regex.Split(s, "<pgsA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<pgsB>");
			switch (array2[0])
			{
			case "integersArray":
			{
				string[] array4 = array2[1].Split('.');
				for (int l = 0; l < array4.Length && l < integers.Length; l++)
				{
					integers[l] = int.Parse(array4[l], NumberStyles.Any, CultureInfo.InvariantCulture);
				}
				break;
			}
			case "itemTypes":
			{
				if (Custom.IsDigitString(array2[1]))
				{
					BackwardsCompatibilityRemix.ParseItemTypes(array2[1], itemTypes);
					break;
				}
				itemTypes.Clear();
				string[] array3 = array2[1].Split(',');
				foreach (string text2 in array3)
				{
					if (text2 != string.Empty)
					{
						itemTypes.Add(new AbstractPhysicalObject.AbstractObjectType(text2));
					}
				}
				break;
			}
			case "creatureTypes":
			{
				if (Custom.IsDigitString(array2[1]))
				{
					BackwardsCompatibilityRemix.ParseCreatureTypes(array2[1], creatureTypes);
					break;
				}
				creatureTypes.Clear();
				string[] array3 = array2[1].Split(',');
				foreach (string text in array3)
				{
					if (!(text == string.Empty))
					{
						creatureTypes.Add(new CreatureTemplate.Type(text));
					}
				}
				break;
			}
			case "likesPlayer":
				likesPlayer = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "directionHandHolding":
				handHolding = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "imagesShown":
			{
				string[] array4 = array2[1].Split('.');
				for (int m = 0; m < array4.Length; m++)
				{
					imagesShown.Add(array4[m]);
				}
				break;
			}
			case "forcedDirsGiven":
			{
				string[] array4 = array2[1].Split('.');
				for (int k = 0; k < array4.Length; k++)
				{
					forcedDirectionsGiven.Add(new KeyValuePair<int, int>(int.Parse(array4[k].Split(',')[0], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array4[k].Split(',')[1], NumberStyles.Any, CultureInfo.InvariantCulture)));
				}
				break;
			}
			default:
				if (array[i].Trim().Length > 0 && array2.Length >= 2)
				{
					unrecognizedSaveStrings.Add(array[i]);
				}
				break;
			}
		}
	}
}
