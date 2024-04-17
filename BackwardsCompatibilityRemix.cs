using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

public static class BackwardsCompatibilityRemix
{
	public static int? ParseRoomIndex(string roomString)
	{
		int? result = null;
		int result2 = -1;
		if (int.TryParse(roomString, NumberStyles.Any, CultureInfo.InvariantCulture, out result2))
		{
			int num = Math.Min(1, RainWorld.loadedWorldVersion);
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "roomIndexMap" + num + ".txt"));
			result = result2;
			if (result2 >= 0 && result2 < array.Length)
			{
				string key = array[result2].Split(' ')[1].Trim();
				if (RainWorld.roomNameToIndex.ContainsKey(key))
				{
					result = RainWorld.roomNameToIndex[key];
				}
			}
		}
		else if (RainWorld.roomNameToIndex.ContainsKey(roomString))
		{
			result = RainWorld.roomNameToIndex[roomString];
		}
		return result;
	}

	public static void ParseRoomsVisited(int worldVersion, string regionName, string roomsString, List<string> roomsData)
	{
		roomsData.Clear();
		int num = Math.Min(worldVersion, 1);
		int regionNumber = GetRegionNumber(regionName);
		if (regionNumber < 0)
		{
			return;
		}
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "regionsIndexMap" + num + ".txt"));
		string[] array2 = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "roomIndexMap" + num + ".txt"));
		int num2 = int.Parse(array[regionNumber].Split(' ')[0], NumberStyles.Any, CultureInfo.InvariantCulture);
		int num3 = int.Parse(array[regionNumber].Split(' ')[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		for (int i = 0; i < roomsString.Length && i < num3 - num2 && num2 + i < array2.Length; i++)
		{
			if (roomsString[i] == '1')
			{
				roomsData.Add(array2[num2 + i].Split(' ')[1]);
			}
		}
	}

	public static void ParseSwarmRooms(string regionName, string swarmString, Dictionary<string, int> swarmData)
	{
		swarmData.Clear();
		if (swarmString == string.Empty)
		{
			return;
		}
		string[] array = swarmString.Split('.');
		string[] array2 = null;
		switch (regionName)
		{
		case "CC":
			array2 = new string[6] { "CC_A06", "CC_B04", "CC_A10", "CC_C05", "CC_C08", "CC_C11" };
			break;
		case "DS":
			array2 = new string[4] { "DS_A01", "DS_A19", "DS_B07", "DS_A05" };
			break;
		case "HI":
			array2 = new string[4] { "HI_B13", "HI_B02", "HI_B09", "HI_A06" };
			break;
		case "GW":
			array2 = new string[3] { "GW_E02", "GW_D01", "GW_E01" };
			break;
		case "SI":
			array2 = new string[6] { "SI_C07", "SI_C01", "SI_D01", "SI_A23", "SI_B03", "SI_D07" };
			break;
		case "SU":
			array2 = new string[8] { "SU_A40", "SU_A24", "SU_A42", "SU_A06", "SU_A07", "SU_A13", "SU_B02", "SU_C01" };
			break;
		case "SH":
			array2 = new string[3] { "SH_A06", "SH_A10", "SH_A09" };
			break;
		case "SL":
			array2 = new string[6] { "SL_B01", "SL_C02", "SL_D06", "SL_D04", "SL_H02", "SL_B04" };
			break;
		case "LF":
			array2 = new string[4] { "LF_A10", "LF_D06", "LF_D01", "LF_C02" };
			break;
		case "UW":
			array2 = new string[2] { "UW_A02", "UW_A01" };
			break;
		case "SB":
			array2 = new string[2] { "SB_C01", "SB_J01" };
			break;
		}
		if (array2 != null)
		{
			for (int i = 0; i < array.Length && i < array2.Length; i++)
			{
				swarmData[array2[i]] = int.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
		}
	}

	public static void ParseLineages(string lineageString, string regionName, SlugcatStats.Name character, int worldVersion, Dictionary<string, int> lineageData)
	{
		lineageData.Clear();
		if (lineageString == string.Empty)
		{
			return;
		}
		string[] array = lineageString.Split('.');
		string[] array2 = null;
		if (worldVersion == 0)
		{
			switch (regionName)
			{
			case "CC":
				array2 = new string[13]
				{
					"CC_C07 : 2", "CC_C07 : 3", "CC_A15 : 3", "CC_C05 : 1", "CC_F01 : 3", "CC_A07 : 2", "CC_C11 : 3", "CC_C05 : 6", "CC_C05 : 4", "CC_B01 : 2",
					"CC_C08 : 3", "CC_C08 : 4", "CC_B08 : 2"
				};
				break;
			case "DS":
				array2 = new string[6] { "DS_A08 : 4", "DS_A16 : 2", "DS_A22 : 3", "DS_A06 : 3", "DS_A02 : 4", "DS_A09 : 2" };
				break;
			case "HI":
				array2 = new string[8] { "HI_B06 : 3", "HI_B06 : 4", "HI_C01 : 5", "HI_C01 : 4", "HI_B03 : 3", "HI_B13 : 2", "HI_C02 : 5", "HI_C02 : 6" };
				break;
			case "GW":
				array2 = new string[8] { "GW_C03 : 5", "GW_A11 : 4", "GW_E02 : 6", "GW_E02 : 11", "GW_C09 : 5", "GW_C09 : 6", "GW_B02 : 5", "GW_C06 : 5" };
				break;
			case "SI":
				array2 = new string[9] { "SI_B01 : 4", "SI_B09 : 5", "SI_C07 : 1", "SI_B01 : 6", "SI_C02 : 5", "SI_C02 : 10", "SI_B09 : 3", "SI_C01 : 2", "SI_C01 : 5" };
				break;
			case "SU":
				array2 = new string[12]
				{
					"SU_A20 : 6", "SU_B08 : 2", "SU_B12 : 3", "SU_A40 : 2", "SU_A06 : 3", "SU_B07 : 2", "SU_C02 : 4", "SU_A02 : 2", "SU_A02 : 3", "SU_B05 : 5",
					"SU_A04 : 5", "SU_A31 : 3"
				};
				break;
			case "SH":
				array2 = new string[15]
				{
					"SH_A12 : 2", "SH_B08 : 2", "SH_A11 : 2", "SH_A11 : 3", "SH_B01 : 5", "SH_C08 : 2", "SH_C08 : 3", "SH_B05 : 6", "SH_B05 : 7", "SH_B05 : 8",
					"SH_H01 : 3", "SH_H01 : 5", "SH_B02 : 5", "SH_C05 : 7", "SH_C05 : 5"
				};
				break;
			case "SL":
				array2 = new string[3] { "SL_B04 : 3", "SL_D05 : 5", "SL_D05 : 6" };
				break;
			case "LF":
				array2 = new string[7] { "LF_A03 : 3", "LF_A05 : 4", "LF_E02 : 3", "LF_D03 : 5", "LF_C03 : 4", "LF_E01 : 4", "LF_E03 : 2" };
				break;
			case "UW":
				array2 = new string[4] { "UW_J02 : 11", "UW_D02 : 3", "UW_E02 : 5", "UW_D01 : 2" };
				break;
			case "SB":
				array2 = new string[6] { "SB_A03 : 6", "SB_A04 : 4", "SB_B01 : 3", "SB_B01 : 4", "SB_B02 : 4", "SB_B02 : 5" };
				break;
			}
		}
		else if (character == SlugcatStats.Name.White)
		{
			switch (regionName)
			{
			case "CC":
				array2 = new string[13]
				{
					"CC_C07 : 2", "CC_C07 : 3", "CC_A15 : 3", "OFFSCREEN : 0", "CC_F01 : 3", "CC_A07 : 2", "CC_C11 : 3", "CC_B08 : 2", "CC_B11 : 2", "CC_B01 : 2",
					"CC_C08 : 3", "CC_C08 : 4", "CC_C05 : 3"
				};
				break;
			case "DS":
				array2 = new string[6] { "DS_A08 : 4", "DS_A06 : 3", "DS_A02 : 4", "DS_A22 : 3", "DS_A09 : 2", "DS_A16 : 2" };
				break;
			case "HI":
				array2 = new string[8] { "HI_B06 : 4", "HI_C01 : 5", "HI_B03 : 3", "HI_B13 : 2", "HI_C02 : 5", "HI_C01 : 4", "HI_C02 : 6", "HI_B06 : 3" };
				break;
			case "GW":
				array2 = new string[7] { "GW_C03 : 5", "GW_E02 : 6", "GW_E02 : 11", "GW_C09 : 5", "GW_C09 : 6", "GW_C06 : 4", "GW_B02 : 4" };
				break;
			case "SI":
				array2 = new string[9] { "SI_B01 : 4", "SI_B01 : 6", "SI_C02 : 5", "SI_B09 : 3", "SI_C01 : 2", "SI_C01 : 5", "OFFSCREEN : 0", "SI_B09 : 4", "SI_B09 : 5" };
				break;
			case "SU":
				array2 = new string[13]
				{
					"SU_A30 : 2", "SU_A31 : 3", "SU_A20 : 6", "SU_B08 : 2", "SU_B12 : 3", "SU_A40 : 2", "SU_B07 : 2", "SU_C02 : 4", "SU_A04 : 5", "SU_A06 : 3",
					"SU_A02 : 2", "SU_A02 : 3", "SU_B05 : 5"
				};
				break;
			case "SH":
				array2 = new string[18]
				{
					"SH_A11 : 2", "SH_A11 : 3", "SH_B01 : 5", "SH_C08 : 3", "SH_B05 : 6", "SH_B05 : 8", "SH_H01 : 3", "SH_H01 : 5", "SH_B02 : 5", "SH_C05 : 7",
					"SH_C05 : 5", "SH_B02 : 4", "SH_B17 : 5", "SH_A12 : 4", "SH_C08 : 2", "SH_B05 : 7", "SH_B08 : 2", "SH_C05 : 4"
				};
				break;
			case "SL":
				array2 = new string[3] { "SL_B04 : 3", "SL_D05 : 5", "SL_D05 : 6" };
				break;
			case "LF":
				array2 = new string[6] { "LF_A03 : 3", "LF_A05 : 4", "LF_C03 : 4", "LF_E02 : 3", "LF_E01 : 4", "LF_E03 : 2" };
				break;
			case "UW":
				array2 = new string[3] { "UW_J02 : 11", "UW_D02 : 3", "UW_E02 : 5" };
				break;
			case "SB":
				array2 = new string[7] { "SB_B02 : 4", "SB_B02 : 5", "SB_B01 : 3", "SB_B01 : 4", "SB_A01 : 3", "SB_A03 : 6", "SB_A04 : 4" };
				break;
			}
		}
		else if (character == SlugcatStats.Name.Yellow)
		{
			switch (regionName)
			{
			case "CC":
				array2 = new string[10] { "CC_C07 : 2", "CC_C07 : 3", "CC_A15 : 3", "CC_B01 : 2", "CC_C08 : 3", "CC_C08 : 4", "CC_C05 : 3", "CC_F01 : 3", "CC_B08 : 2", "CC_A07 : 2" };
				break;
			case "DS":
				array2 = new string[6] { "DS_A08 : 4", "DS_A06 : 3", "DS_A02 : 4", "DS_A22 : 3", "DS_A09 : 2", "DS_A16 : 2" };
				break;
			case "HI":
				array2 = new string[8] { "HI_B06 : 4", "HI_C01 : 5", "HI_B03 : 3", "HI_B13 : 2", "HI_C02 : 5", "HI_C01 : 4", "HI_C02 : 6", "HI_B06 : 3" };
				break;
			case "GW":
				array2 = new string[7] { "GW_C03 : 5", "GW_E02 : 6", "GW_E02 : 11", "GW_C09 : 5", "GW_C09 : 6", "GW_C06 : 4", "GW_B02 : 4" };
				break;
			case "SI":
				array2 = new string[8] { "SI_B01 : 4", "SI_B01 : 6", "SI_C02 : 5", "SI_B09 : 3", "SI_C01 : 2", "SI_C01 : 5", "SI_B09 : 4", "SI_B09 : 5" };
				break;
			case "SU":
				array2 = new string[8] { "SU_A30 : 2", "SU_A31 : 3", "SU_A06 : 3", "SU_B08 : 2", "SU_B12 : 3", "SU_A40 : 2", "SU_B07 : 2", "SU_C02 : 4" };
				break;
			case "SH":
				array2 = new string[18]
				{
					"SH_A11 : 2", "SH_A11 : 3", "SH_B01 : 5", "SH_C08 : 3", "SH_B05 : 6", "SH_B05 : 8", "SH_H01 : 3", "SH_H01 : 5", "SH_B02 : 5", "SH_C05 : 7",
					"SH_C05 : 5", "SH_B02 : 4", "SH_B17 : 5", "SH_C08 : 2", "SH_B05 : 7", "SH_B08 : 2", "SH_C05 : 4", "SH_A12 : 4"
				};
				break;
			case "SL":
				array2 = new string[3] { "SL_B04 : 3", "SL_D05 : 5", "SL_D05 : 6" };
				break;
			case "LF":
				array2 = new string[5] { "LF_A03 : 3", "LF_A05 : 4", "LF_C03 : 4", "LF_E01 : 4", "LF_E03 : 2" };
				break;
			case "UW":
				array2 = new string[1] { "UW_J02 : 11" };
				break;
			case "SB":
				array2 = new string[6] { "SB_B02 : 4", "SB_B02 : 5", "SB_B01 : 3", "SB_B01 : 4", "SB_A01 : 3", "SB_A03 : 6" };
				break;
			}
		}
		else if (character == SlugcatStats.Name.Red)
		{
			switch (regionName)
			{
			case "CC":
				array2 = new string[11]
				{
					"CC_C07 : 2", "CC_C07 : 3", "CC_A15 : 3", "CC_C05 : 3", "CC_B01 : 2", "CC_C08 : 4", "CC_F01 : 3", "CC_A07 : 2", "CC_C11 : 3", "CC_B08 : 2",
					"CC_B08 : 4"
				};
				break;
			case "DS":
				array2 = new string[5] { "DS_A02 : 4", "DS_A16 : 2", "DS_A06 : 3", "DS_A22 : 3", "DS_A09 : 2" };
				break;
			case "HI":
				array2 = new string[15]
				{
					"HI_B06 : 4", "HI_C01 : 5", "HI_B03 : 3", "HI_B13 : 2", "HI_C02 : 5", "OFFSCREEN : 0", "OFFSCREEN : 0", "HI_B03 : 3", "HI_C11 : 4", "HI_C01 : 3",
					"HI_C02 : 6", "HI_C01 : 4", "HI_A04 : 2", "HI_C03 : 3", "HI_C04 : 5"
				};
				break;
			case "GW":
				array2 = new string[8] { "GW_C03 : 5", "GW_E02 : 6", "GW_E02 : 11", "GW_C09 : 5", "GW_C09 : 6", "GW_C06 : 4", "GW_A11 : 3", "GW_C04 : 2" };
				break;
			case "SI":
				array2 = new string[9] { "SI_B01 : 4", "SI_B01 : 6", "SI_C02 : 5", "SI_B09 : 3", "SI_C01 : 2", "SI_C01 : 5", "SI_A23 : 2", "SI_C06 : 3", "SI_B09 : 5" };
				break;
			case "SU":
				array2 = new string[18]
				{
					"SU_A30 : 2", "SU_A31 : 3", "SU_A06 : 4", "SU_A06 : 3", "SU_B08 : 2", "SU_B12 : 3", "SU_A40 : 2", "SU_B07 : 2", "SU_C02 : 4", "SU_A04 : 4",
					"SU_B11 : 6", "SU_B11 : 7", "SU_B06 : 4", "SU_B06 : 5", "SU_A04 : 5", "SU_A02 : 2", "SU_A02 : 3", "SU_B05 : 5"
				};
				break;
			case "SH":
				array2 = new string[23]
				{
					"SH_A11 : 2", "SH_A11 : 3", "SH_B01 : 5", "SH_C08 : 3", "SH_B05 : 6", "SH_B05 : 8", "SH_H01 : 3", "SH_H01 : 5", "SH_B02 : 5", "SH_C05 : 7",
					"SH_C05 : 5", "SH_B02 : 4", "SH_B17 : 5", "SH_B08 : 2", "SH_B05 : 7", "SH_C08 : 2", "SH_C03 : 7", "SH_C03 : 4", "SH_C01 : 3", "SH_C05 : 6",
					"SH_C12 : 2", "SH_B17 : 5", "SH_B04 : 5"
				};
				break;
			case "SL":
				array2 = new string[6] { "SL_B04 : 3", "SL_D05 : 5", "SL_D05 : 6", "SL_F01 : 3", "SL_B04 : 2", "SL_B04 : 3" };
				break;
			case "LF":
				array2 = new string[10] { "LF_A03 : 3", "LF_A05 : 4", "LF_C03 : 4", "LF_D03 : 2", "LF_D03 : 3", "LF_B03 : 3", "LF_C02 : 6", "LF_F02 : 4", "LF_E02 : 3", "LF_E04 : 5" };
				break;
			case "UW":
				array2 = new string[3] { "UW_J02 : 11", "UW_D02 : 4", "UW_D07 : 4" };
				break;
			case "SB":
				array2 = new string[11]
				{
					"SB_B02 : 4", "SB_B02 : 5", "SB_B01 : 3", "SB_B01 : 4", "SB_A01 : 3", "SB_J02 : 4", "SB_C07 : 3", "SB_E04 : 5", "SB_A03 : 6", "SB_E02 : 4",
					"SB_A04 : 4"
				};
				break;
			}
		}
		if (array2 == null)
		{
			return;
		}
		for (int i = 0; i < array.Length && i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split(':');
			string key = array3[0].Trim();
			int abstractNode = int.Parse(array3[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture);
			if (RainWorld.roomNameToIndex.ContainsKey(key))
			{
				string text = new WorldCoordinate(RainWorld.roomNameToIndex[key], -1, -1, abstractNode).SaveToString();
				string key2 = text;
				int num = 2;
				while (lineageData.ContainsKey(key2))
				{
					key2 = text + ";" + num;
					num++;
				}
				lineageData[key2] = int.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
			}
		}
	}

	public static int GetRegionNumber(string regionName)
	{
		return regionName switch
		{
			"CC" => 0, 
			"DS" => 1, 
			"HI" => 2, 
			"GW" => 3, 
			"SI" => 4, 
			"SU" => 5, 
			"SH" => 6, 
			"SL" => 7, 
			"LF" => 8, 
			"UW" => 9, 
			"SB" => 10, 
			"SS" => 11, 
			_ => -1, 
		};
	}

	public static void ParsePlayerAvailability(string availabilityString, List<SlugcatStats.Name> availableData)
	{
		availableData.Clear();
		for (int i = 0; i < availabilityString.Length; i++)
		{
			if (availabilityString[i] == '1')
			{
				switch (i)
				{
				case 0:
					availableData.Add(SlugcatStats.Name.White);
					break;
				case 1:
					availableData.Add(SlugcatStats.Name.Yellow);
					break;
				case 2:
					availableData.Add(SlugcatStats.Name.Red);
					break;
				case 3:
					availableData.Add(SlugcatStats.Name.Night);
					break;
				}
			}
		}
	}

	public static SlugcatStats.Name ParseSaveNumber(string firstProgDiv)
	{
		string text = firstProgDiv;
		if (text.Contains("<svA>"))
		{
			text = Regex.Split(firstProgDiv, "<svA>")[0];
		}
		string text2 = Regex.Split(text, "<svB>")[1];
		if (int.TryParse(text2, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			return ParsePlayerNumber(result);
		}
		return new SlugcatStats.Name(text2);
	}

	public static SlugcatStats.Name ParsePlayerNumber(int playerID)
	{
		return playerID switch
		{
			0 => SlugcatStats.Name.White, 
			1 => SlugcatStats.Name.Yellow, 
			2 => SlugcatStats.Name.Red, 
			3 => SlugcatStats.Name.Night, 
			_ => new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(playerID)), 
		};
	}

	public static void ParseGhostString(string ghostString, Dictionary<GhostWorldPresence.GhostID, int> remixGhostData)
	{
		remixGhostData.Clear();
		if (ghostString == string.Empty)
		{
			return;
		}
		string[] array = ghostString.Split('.');
		for (int i = 0; i < array.Length; i++)
		{
			int value = int.Parse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture);
			switch (i)
			{
			case 0:
				remixGhostData[GhostWorldPresence.GhostID.CC] = value;
				break;
			case 1:
				remixGhostData[GhostWorldPresence.GhostID.SI] = value;
				break;
			case 2:
				remixGhostData[GhostWorldPresence.GhostID.LF] = value;
				break;
			case 3:
				remixGhostData[GhostWorldPresence.GhostID.SH] = value;
				break;
			case 4:
				remixGhostData[GhostWorldPresence.GhostID.UW] = value;
				break;
			case 5:
				remixGhostData[GhostWorldPresence.GhostID.SB] = value;
				break;
			}
		}
	}

	public static void ParseMetersShown(string metersString, List<WinState.EndgameID> metersData)
	{
		metersData.Clear();
		for (int i = 0; i < metersString.Length; i++)
		{
			if (metersString[i] != '0')
			{
				switch (i)
				{
				case 0:
					metersData.Add(WinState.EndgameID.Survivor);
					break;
				case 1:
					metersData.Add(WinState.EndgameID.Hunter);
					break;
				case 2:
					metersData.Add(WinState.EndgameID.Saint);
					break;
				case 3:
					metersData.Add(WinState.EndgameID.Traveller);
					break;
				case 4:
					metersData.Add(WinState.EndgameID.Chieftain);
					break;
				case 5:
					metersData.Add(WinState.EndgameID.Monk);
					break;
				case 6:
					metersData.Add(WinState.EndgameID.Outlaw);
					break;
				case 7:
					metersData.Add(WinState.EndgameID.DragonSlayer);
					break;
				case 8:
					metersData.Add(WinState.EndgameID.Scholar);
					break;
				case 9:
					metersData.Add(WinState.EndgameID.Friend);
					break;
				}
			}
		}
	}

	public static void ParseTutorialMessages(string tutString, List<DeathPersistentSaveData.Tutorial> tutorialsData)
	{
		tutorialsData.Clear();
		for (int i = 0; i < tutString.Length; i++)
		{
			if (tutString[i] != '0')
			{
				switch (i)
				{
				case 0:
					tutorialsData.Add(DeathPersistentSaveData.Tutorial.GoExplore);
					break;
				case 1:
					tutorialsData.Add(DeathPersistentSaveData.Tutorial.ScavToll);
					break;
				case 2:
					tutorialsData.Add(DeathPersistentSaveData.Tutorial.ScavMerchant);
					break;
				case 3:
					tutorialsData.Add(DeathPersistentSaveData.Tutorial.DangleFruitInWater);
					break;
				case 4:
					tutorialsData.Add(DeathPersistentSaveData.Tutorial.KarmaFlower);
					break;
				case 5:
					tutorialsData.Add(DeathPersistentSaveData.Tutorial.PoleMimic);
					break;
				}
			}
		}
	}

	public static void ParseSignificantPearls(string pearlsString, List<DataPearl.AbstractDataPearl.DataPearlType> pearlsData)
	{
		pearlsData.Clear();
		for (int i = 0; i < pearlsString.Length; i++)
		{
			if (pearlsString[i] != '0')
			{
				switch (i)
				{
				case 0:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.Misc);
					break;
				case 1:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.Misc2);
					break;
				case 2:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.CC);
					break;
				case 3:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SI_west);
					break;
				case 4:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SI_top);
					break;
				case 5:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.LF_west);
					break;
				case 6:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.LF_bottom);
					break;
				case 7:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.HI);
					break;
				case 8:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SH);
					break;
				case 9:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.DS);
					break;
				case 10:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SB_filtration);
					break;
				case 11:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SB_ravine);
					break;
				case 12:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.GW);
					break;
				case 13:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SL_bridge);
					break;
				case 14:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SL_moon);
					break;
				case 15:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SU);
					break;
				case 16:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.UW);
					break;
				case 17:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl);
					break;
				case 18:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.SL_chimney);
					break;
				case 19:
					pearlsData.Add(DataPearl.AbstractDataPearl.DataPearlType.Red_stomach);
					break;
				}
			}
		}
	}

	public static void ParseMiscItems(string miscString, List<SLOracleBehaviorHasMark.MiscItemType> miscData)
	{
		miscData.Clear();
		for (int i = 0; i < miscString.Length; i++)
		{
			if (miscString[i] != '0')
			{
				switch (i)
				{
				case 0:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.NA);
					break;
				case 1:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.Rock);
					break;
				case 2:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.Spear);
					break;
				case 3:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.FireSpear);
					break;
				case 4:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.WaterNut);
					break;
				case 5:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.KarmaFlower);
					break;
				case 6:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.DangleFruit);
					break;
				case 7:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.FlareBomb);
					break;
				case 8:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.VultureMask);
					break;
				case 9:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.PuffBall);
					break;
				case 10:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.JellyFish);
					break;
				case 11:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.Lantern);
					break;
				case 12:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.Mushroom);
					break;
				case 13:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.FirecrackerPlant);
					break;
				case 14:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.SlimeMold);
					break;
				case 15:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.ScavBomb);
					break;
				case 16:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.BubbleGrass);
					break;
				case 17:
					miscData.Add(SLOracleBehaviorHasMark.MiscItemType.OverseerRemains);
					break;
				}
			}
		}
	}

	public static void ParseLevelTokens(string tokensString, List<MultiplayerUnlocks.LevelUnlockID> tokenData)
	{
		tokenData.Clear();
		for (int i = 0; i < tokensString.Length; i++)
		{
			if (tokensString[i] != '0')
			{
				switch (i)
				{
				case 0:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.Default);
					break;
				case 1:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.Hidden);
					break;
				case 2:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.SU);
					break;
				case 3:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.HI);
					break;
				case 4:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.CC);
					break;
				case 5:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.GW);
					break;
				case 6:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.SL);
					break;
				case 7:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.SH);
					break;
				case 8:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.DS);
					break;
				case 9:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.SI);
					break;
				case 10:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.LF);
					break;
				case 11:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.UW);
					break;
				case 12:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.SB);
					break;
				case 13:
					tokenData.Add(MultiplayerUnlocks.LevelUnlockID.SS);
					break;
				}
			}
		}
	}

	public static void ParseSandboxTokens(string tokensString, List<MultiplayerUnlocks.SandboxUnlockID> tokenData)
	{
		tokenData.Clear();
		for (int i = 0; i < tokensString.Length; i++)
		{
			if (tokensString[i] != '0')
			{
				switch (i)
				{
				case 0:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Slugcat);
					break;
				case 1:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.GreenLizard);
					break;
				case 2:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.PinkLizard);
					break;
				case 3:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BlueLizard);
					break;
				case 4:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.WhiteLizard);
					break;
				case 5:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BlackLizard);
					break;
				case 6:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.YellowLizard);
					break;
				case 7:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.CyanLizard);
					break;
				case 8:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.RedLizard);
					break;
				case 9:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Salamander);
					break;
				case 10:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Fly);
					break;
				case 11:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.CicadaA);
					break;
				case 12:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.CicadaB);
					break;
				case 13:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Snail);
					break;
				case 14:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Leech);
					break;
				case 15:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.SeaLeech);
					break;
				case 16:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.PoleMimic);
					break;
				case 17:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.TentaclePlant);
					break;
				case 18:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Scavenger);
					break;
				case 19:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.VultureGrub);
					break;
				case 20:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Vulture);
					break;
				case 21:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.KingVulture);
					break;
				case 22:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.SmallCentipede);
					break;
				case 23:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.MediumCentipede);
					break;
				case 24:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BigCentipede);
					break;
				case 25:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.RedCentipede);
					break;
				case 26:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Centiwing);
					break;
				case 27:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.TubeWorm);
					break;
				case 28:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Hazer);
					break;
				case 29:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.LanternMouse);
					break;
				case 30:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Spider);
					break;
				case 31:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BigSpider);
					break;
				case 32:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.SpitterSpider);
					break;
				case 33:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.MirosBird);
					break;
				case 34:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BrotherLongLegs);
					break;
				case 35:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.DaddyLongLegs);
					break;
				case 36:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Deer);
					break;
				case 37:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.EggBug);
					break;
				case 38:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.DropBug);
					break;
				case 39:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BigNeedleWorm);
					break;
				case 40:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.SmallNeedleWorm);
					break;
				case 41:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.JetFish);
					break;
				case 42:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BigEel);
					break;
				case 43:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Rock);
					break;
				case 44:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Spear);
					break;
				case 45:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.FireSpear);
					break;
				case 46:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.ScavengerBomb);
					break;
				case 47:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.SporePlant);
					break;
				case 48:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Lantern);
					break;
				case 49:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.FlyLure);
					break;
				case 50:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.Mushroom);
					break;
				case 51:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.FlareBomb);
					break;
				case 52:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.PuffBall);
					break;
				case 53:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.WaterNut);
					break;
				case 54:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.FirecrackerPlant);
					break;
				case 55:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.DangleFruit);
					break;
				case 56:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.JellyFish);
					break;
				case 57:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.BubbleGrass);
					break;
				case 58:
					tokenData.Add(MultiplayerUnlocks.SandboxUnlockID.SlimeMold);
					break;
				}
			}
		}
	}

	public static void ParseCreatureTypes(string creaturesString, List<CreatureTemplate.Type> creatureData)
	{
		creatureData.Clear();
		for (int i = 0; i < creaturesString.Length; i++)
		{
			if (creaturesString[i] != '0')
			{
				switch (i)
				{
				case 0:
					creatureData.Add(CreatureTemplate.Type.StandardGroundCreature);
					break;
				case 1:
					creatureData.Add(CreatureTemplate.Type.Slugcat);
					break;
				case 2:
					creatureData.Add(CreatureTemplate.Type.LizardTemplate);
					break;
				case 3:
					creatureData.Add(CreatureTemplate.Type.PinkLizard);
					break;
				case 4:
					creatureData.Add(CreatureTemplate.Type.GreenLizard);
					break;
				case 5:
					creatureData.Add(CreatureTemplate.Type.BlueLizard);
					break;
				case 6:
					creatureData.Add(CreatureTemplate.Type.YellowLizard);
					break;
				case 7:
					creatureData.Add(CreatureTemplate.Type.WhiteLizard);
					break;
				case 8:
					creatureData.Add(CreatureTemplate.Type.RedLizard);
					break;
				case 9:
					creatureData.Add(CreatureTemplate.Type.BlackLizard);
					break;
				case 10:
					creatureData.Add(CreatureTemplate.Type.Salamander);
					break;
				case 11:
					creatureData.Add(CreatureTemplate.Type.CyanLizard);
					break;
				case 12:
					creatureData.Add(CreatureTemplate.Type.Fly);
					break;
				case 13:
					creatureData.Add(CreatureTemplate.Type.Leech);
					break;
				case 14:
					creatureData.Add(CreatureTemplate.Type.SeaLeech);
					break;
				case 15:
					creatureData.Add(CreatureTemplate.Type.Snail);
					break;
				case 16:
					creatureData.Add(CreatureTemplate.Type.Vulture);
					break;
				case 17:
					creatureData.Add(CreatureTemplate.Type.GarbageWorm);
					break;
				case 18:
					creatureData.Add(CreatureTemplate.Type.LanternMouse);
					break;
				case 19:
					creatureData.Add(CreatureTemplate.Type.CicadaA);
					break;
				case 20:
					creatureData.Add(CreatureTemplate.Type.CicadaB);
					break;
				case 21:
					creatureData.Add(CreatureTemplate.Type.Spider);
					break;
				case 22:
					creatureData.Add(CreatureTemplate.Type.JetFish);
					break;
				case 23:
					creatureData.Add(CreatureTemplate.Type.BigEel);
					break;
				case 24:
					creatureData.Add(CreatureTemplate.Type.Deer);
					break;
				case 25:
					creatureData.Add(CreatureTemplate.Type.TubeWorm);
					break;
				case 26:
					creatureData.Add(CreatureTemplate.Type.DaddyLongLegs);
					break;
				case 27:
					creatureData.Add(CreatureTemplate.Type.BrotherLongLegs);
					break;
				case 28:
					creatureData.Add(CreatureTemplate.Type.TentaclePlant);
					break;
				case 29:
					creatureData.Add(CreatureTemplate.Type.PoleMimic);
					break;
				case 30:
					creatureData.Add(CreatureTemplate.Type.MirosBird);
					break;
				case 31:
					creatureData.Add(CreatureTemplate.Type.TempleGuard);
					break;
				case 32:
					creatureData.Add(CreatureTemplate.Type.Centipede);
					break;
				case 33:
					creatureData.Add(CreatureTemplate.Type.RedCentipede);
					break;
				case 34:
					creatureData.Add(CreatureTemplate.Type.Centiwing);
					break;
				case 35:
					creatureData.Add(CreatureTemplate.Type.SmallCentipede);
					break;
				case 36:
					creatureData.Add(CreatureTemplate.Type.Scavenger);
					break;
				case 37:
					creatureData.Add(CreatureTemplate.Type.Overseer);
					break;
				case 38:
					creatureData.Add(CreatureTemplate.Type.VultureGrub);
					break;
				case 39:
					creatureData.Add(CreatureTemplate.Type.EggBug);
					break;
				case 40:
					creatureData.Add(CreatureTemplate.Type.BigSpider);
					break;
				case 41:
					creatureData.Add(CreatureTemplate.Type.SpitterSpider);
					break;
				case 42:
					creatureData.Add(CreatureTemplate.Type.SmallNeedleWorm);
					break;
				case 43:
					creatureData.Add(CreatureTemplate.Type.BigNeedleWorm);
					break;
				case 44:
					creatureData.Add(CreatureTemplate.Type.DropBug);
					break;
				case 45:
					creatureData.Add(CreatureTemplate.Type.KingVulture);
					break;
				case 46:
					creatureData.Add(CreatureTemplate.Type.Hazer);
					break;
				}
			}
		}
	}

	public static void ParseItemTypes(string itemsString, List<AbstractPhysicalObject.AbstractObjectType> itemData)
	{
		itemData.Clear();
		for (int i = 0; i < itemsString.Length; i++)
		{
			if (itemsString[i] != '0')
			{
				switch (i)
				{
				case 0:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.Creature);
					break;
				case 1:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.Rock);
					break;
				case 2:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.Spear);
					break;
				case 3:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.FlareBomb);
					break;
				case 4:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.VultureMask);
					break;
				case 5:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.PuffBall);
					break;
				case 6:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.DangleFruit);
					break;
				case 7:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.Oracle);
					break;
				case 8:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.PebblesPearl);
					break;
				case 9:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.SLOracleSwarmer);
					break;
				case 10:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer);
					break;
				case 11:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.DataPearl);
					break;
				case 12:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.SeedCob);
					break;
				case 13:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.WaterNut);
					break;
				case 14:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.JellyFish);
					break;
				case 15:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.Lantern);
					break;
				case 16:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.KarmaFlower);
					break;
				case 17:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.Mushroom);
					break;
				case 18:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.VoidSpawn);
					break;
				case 19:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.FirecrackerPlant);
					break;
				case 20:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.SlimeMold);
					break;
				case 21:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.FlyLure);
					break;
				case 22:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.ScavengerBomb);
					break;
				case 23:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.SporePlant);
					break;
				case 24:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.AttachedBee);
					break;
				case 25:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.EggBugEgg);
					break;
				case 26:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.NeedleEgg);
					break;
				case 27:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.DartMaggot);
					break;
				case 28:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.BubbleGrass);
					break;
				case 29:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.NSHSwarmer);
					break;
				case 30:
					itemData.Add(AbstractPhysicalObject.AbstractObjectType.OverseerCarcass);
					break;
				}
			}
		}
	}

	public static InGameTranslator.LanguageID ParseLanguage(int languageID)
	{
		return languageID switch
		{
			0 => InGameTranslator.LanguageID.English, 
			1 => InGameTranslator.LanguageID.French, 
			2 => InGameTranslator.LanguageID.Italian, 
			3 => InGameTranslator.LanguageID.German, 
			4 => InGameTranslator.LanguageID.Spanish, 
			5 => InGameTranslator.LanguageID.Portuguese, 
			6 => InGameTranslator.LanguageID.Japanese, 
			7 => InGameTranslator.LanguageID.Korean, 
			_ => InGameTranslator.LanguageID.English, 
		};
	}

	public static Options.ControlSetup.Preset ParsePreset(int presetID)
	{
		return presetID switch
		{
			0 => Options.ControlSetup.Preset.None, 
			1 => Options.ControlSetup.Preset.KeyboardSinglePlayer, 
			2 => Options.ControlSetup.Preset.PS4DualShock, 
			3 => Options.ControlSetup.Preset.XBox, 
			4 => Options.ControlSetup.Preset.SwitchHandheld, 
			5 => Options.ControlSetup.Preset.SwitchDualJoycon, 
			6 => Options.ControlSetup.Preset.SwitchSingleJoyconL, 
			7 => Options.ControlSetup.Preset.SwitchSingleJoyconR, 
			8 => Options.ControlSetup.Preset.SwitchProController, 
			_ => Options.ControlSetup.Preset.None, 
		};
	}

	public static ArenaSetup.GameTypeSetup.WildLifeSetting ParseWildLifeSetting(int wildLifeID)
	{
		return wildLifeID switch
		{
			0 => ArenaSetup.GameTypeSetup.WildLifeSetting.Off, 
			1 => ArenaSetup.GameTypeSetup.WildLifeSetting.Low, 
			2 => ArenaSetup.GameTypeSetup.WildLifeSetting.Medium, 
			3 => ArenaSetup.GameTypeSetup.WildLifeSetting.High, 
			_ => ArenaSetup.GameTypeSetup.WildLifeSetting.Medium, 
		};
	}

	public static ArenaSetup.GameTypeSetup.DenEntryRule ParseDenEntryRule(int denEntryID)
	{
		return denEntryID switch
		{
			0 => ArenaSetup.GameTypeSetup.DenEntryRule.Score, 
			1 => ArenaSetup.GameTypeSetup.DenEntryRule.Standard, 
			2 => ArenaSetup.GameTypeSetup.DenEntryRule.Always, 
			_ => ArenaSetup.GameTypeSetup.DenEntryRule.Always, 
		};
	}

	public static DataPearl.AbstractDataPearl.DataPearlType ParseDataPearl(int pearlID)
	{
		return pearlID switch
		{
			0 => DataPearl.AbstractDataPearl.DataPearlType.Misc, 
			1 => DataPearl.AbstractDataPearl.DataPearlType.Misc2, 
			2 => DataPearl.AbstractDataPearl.DataPearlType.CC, 
			3 => DataPearl.AbstractDataPearl.DataPearlType.SI_west, 
			4 => DataPearl.AbstractDataPearl.DataPearlType.SI_top, 
			5 => DataPearl.AbstractDataPearl.DataPearlType.LF_west, 
			6 => DataPearl.AbstractDataPearl.DataPearlType.LF_bottom, 
			7 => DataPearl.AbstractDataPearl.DataPearlType.HI, 
			8 => DataPearl.AbstractDataPearl.DataPearlType.SH, 
			9 => DataPearl.AbstractDataPearl.DataPearlType.DS, 
			10 => DataPearl.AbstractDataPearl.DataPearlType.SB_filtration, 
			11 => DataPearl.AbstractDataPearl.DataPearlType.SB_ravine, 
			12 => DataPearl.AbstractDataPearl.DataPearlType.GW, 
			13 => DataPearl.AbstractDataPearl.DataPearlType.SL_bridge, 
			14 => DataPearl.AbstractDataPearl.DataPearlType.SL_moon, 
			15 => DataPearl.AbstractDataPearl.DataPearlType.SU, 
			16 => DataPearl.AbstractDataPearl.DataPearlType.UW, 
			17 => DataPearl.AbstractDataPearl.DataPearlType.PebblesPearl, 
			18 => DataPearl.AbstractDataPearl.DataPearlType.SL_chimney, 
			19 => DataPearl.AbstractDataPearl.DataPearlType.Red_stomach, 
			_ => DataPearl.AbstractDataPearl.DataPearlType.Misc, 
		};
	}

	public static StopMusicEvent.Type ParseStopMusicType(int stopType)
	{
		return stopType switch
		{
			0 => StopMusicEvent.Type.AllSongs, 
			1 => StopMusicEvent.Type.SpecificSong, 
			2 => StopMusicEvent.Type.AllButSpecific, 
			_ => StopMusicEvent.Type.AllSongs, 
		};
	}

	public static ReliableIggyDirection.ReliableIggyDirectionData.Condition ParseReliableIggyCondition(int conditionID)
	{
		return conditionID switch
		{
			0 => ReliableIggyDirection.ReliableIggyDirectionData.Condition.AnyTime, 
			1 => ReliableIggyDirection.ReliableIggyDirectionData.Condition.BeforeMoon, 
			2 => ReliableIggyDirection.ReliableIggyDirectionData.Condition.AfterMoon, 
			3 => ReliableIggyDirection.ReliableIggyDirectionData.Condition.AfterPebblesAndMoon, 
			_ => ReliableIggyDirection.ReliableIggyDirectionData.Condition.AnyTime, 
		};
	}

	public static ReliableIggyDirection.ReliableIggyDirectionData.Symbol ParseReliableIggySymbol(int symbolID)
	{
		return symbolID switch
		{
			0 => ReliableIggyDirection.ReliableIggyDirectionData.Symbol.Shelter, 
			1 => ReliableIggyDirection.ReliableIggyDirectionData.Symbol.DynamicDirection, 
			2 => ReliableIggyDirection.ReliableIggyDirectionData.Symbol.SlugcatFace, 
			3 => ReliableIggyDirection.ReliableIggyDirectionData.Symbol.Food, 
			4 => ReliableIggyDirection.ReliableIggyDirectionData.Symbol.Bat, 
			5 => ReliableIggyDirection.ReliableIggyDirectionData.Symbol.Danger, 
			_ => ReliableIggyDirection.ReliableIggyDirectionData.Symbol.Shelter, 
		};
	}

	public static AbstractRoom.CreatureRoomAttraction ParseRoomAttraction(int attractID)
	{
		return attractID switch
		{
			0 => AbstractRoom.CreatureRoomAttraction.Neutral, 
			1 => AbstractRoom.CreatureRoomAttraction.Forbidden, 
			2 => AbstractRoom.CreatureRoomAttraction.Avoid, 
			3 => AbstractRoom.CreatureRoomAttraction.Like, 
			4 => AbstractRoom.CreatureRoomAttraction.Stay, 
			_ => AbstractRoom.CreatureRoomAttraction.Neutral, 
		};
	}
}
