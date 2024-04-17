using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Region
{
	public class RegionParams
	{
		public int batDepleteCyclesMin;

		public int batDepleteCyclesMax;

		public int batDepleteCyclesMaxIfLessThanTwoLeft;

		public int batDepleteCyclesMaxIfLessThanFiveLeft;

		public float overseersSpawnChance;

		public float playerGuideOverseerSpawnChance;

		public int overseersMin;

		public int overseersMax;

		public int batsPerActiveSwarmRoom;

		public int batsPerInactiveSwarmRoom;

		public int scavsMin;

		public int scavsMax;

		public float scavsSpawnChance;

		public Color corruptionEffectColor;

		public Color corruptionEyeColor;

		public Color? kelpColor;

		public bool albinos;

		public float blackSalamanderChance;

		public int scavengerDelayInitialMin;

		public int scavengerDelayInitialMax;

		public int scavengerDelayRepeatMin;

		public int scavengerDelayRepeatMax;

		public float slugPupSpawnChance;

		public bool glacialWasteland;

		public float earlyCycleChance;

		public float earlyCycleFloodChance;

		public RegionParams()
		{
			batDepleteCyclesMin = 2;
			batDepleteCyclesMax = 7;
			batDepleteCyclesMaxIfLessThanTwoLeft = 3;
			batDepleteCyclesMaxIfLessThanFiveLeft = 4;
			overseersSpawnChance = 0.8f;
			overseersMin = 1;
			overseersMax = 3;
			playerGuideOverseerSpawnChance = 1f;
			batsPerActiveSwarmRoom = 10;
			batsPerInactiveSwarmRoom = 4;
			scavsMin = 0;
			scavsMax = 5;
			scavsSpawnChance = 0.3f;
			corruptionEffectColor = new Color(0f, 0f, 1f);
			corruptionEyeColor = new Color(0f, 0f, 1f);
			kelpColor = null;
			albinos = false;
			blackSalamanderChance = 1f / 3f;
			scavengerDelayInitialMin = 900;
			scavengerDelayInitialMax = 1100;
			scavengerDelayRepeatMin = 4100;
			scavengerDelayRepeatMax = 8200;
			slugPupSpawnChance = 0f;
			glacialWasteland = false;
			earlyCycleChance = 0.02f;
			earlyCycleFloodChance = 0.33f;
		}
	}

	public string name;

	public int numberOfRooms;

	public int firstRoomIndex;

	public int regionNumber;

	public RoomSettings[] roomSettingsTemplates;

	public string[] roomSettingTemplateNames;

	public RegionParams regionParams;

	public List<string> subRegions;

	public List<string> altSubRegions;

	public Color propertiesWaterColor;

	public static List<string> GetFullRegionOrder()
	{
		List<string> list = new List<string>
		{
			"SU", "HI", "DS", "CC", "GW", "SH", "SL", "SI", "LF", "UW",
			"SS", "SB"
		};
		if (ModManager.MSC)
		{
			list.Insert(list.IndexOf("DS") + 1, "UG");
			list.Insert(list.IndexOf("SH") + 1, "VS");
			list.Insert(list.IndexOf("SL") + 1, "LM");
			list.Insert(list.IndexOf("SS") + 1, "RM");
			list.AddRange(new List<string> { "OE", "LC", "MS", "DM", "CL", "HR" });
		}
		if (ModManager.ModdedRegionsEnabled)
		{
			(new string[1])[0] = "";
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllLines(path);
				foreach (string text in array)
				{
					if (!list.Contains(text.Trim()))
					{
						list.Add(text.Trim());
					}
				}
			}
		}
		return list;
	}

	public static MenuScene.SceneID GetRegionLandscapeScene(string regionAcro)
	{
		switch (regionAcro)
		{
		case "SU":
			return MenuScene.SceneID.Landscape_SU;
		case "HI":
			return MenuScene.SceneID.Landscape_HI;
		case "DS":
			return MenuScene.SceneID.Landscape_DS;
		case "CC":
			return MenuScene.SceneID.Landscape_CC;
		case "GW":
			return MenuScene.SceneID.Landscape_GW;
		case "SH":
			return MenuScene.SceneID.Landscape_SH;
		case "SL":
			return MenuScene.SceneID.Landscape_SL;
		case "SI":
			return MenuScene.SceneID.Landscape_SI;
		case "LF":
			return MenuScene.SceneID.Landscape_LF;
		case "UW":
			return MenuScene.SceneID.Landscape_UW;
		case "SS":
			return MenuScene.SceneID.Landscape_SS;
		case "SB":
			return MenuScene.SceneID.Landscape_SB;
		default:
			if (ModManager.MSC)
			{
				switch (regionAcro)
				{
				case "DM":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_DM;
				case "LM":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_LM;
				case "MS":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_MS;
				case "LC":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_LC;
				case "OE":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_OE;
				case "HR":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_HR;
				case "CL":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_CL;
				case "UG":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_UG;
				case "RM":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_RM;
				case "VS":
					return MoreSlugcatsEnums.MenuSceneID.Landscape_VS;
				}
			}
			return MenuScene.SceneID.Empty;
		}
	}

	public static Color RegionColor(string regionName)
	{
		if (EquivalentRegion(regionName, "SU"))
		{
			return new Color(0.66f, 0.87f, 0.89f, 1f);
		}
		if (EquivalentRegion(regionName, "HI"))
		{
			return new Color(0.4f, 0.48f, 0.82f, 1f);
		}
		if (EquivalentRegion(regionName, "DS"))
		{
			return new Color(0.14f, 0.49f, 0.27f, 1f);
		}
		if (EquivalentRegion(regionName, "CC"))
		{
			return new Color(0.83f, 0.52f, 0.45f, 1f);
		}
		if (EquivalentRegion(regionName, "GW"))
		{
			return new Color(0.8f, 0.89f, 0.44f, 1f);
		}
		if (EquivalentRegion(regionName, "SH"))
		{
			return new Color(0.35f, 0.21f, 0.6f, 1f);
		}
		if (EquivalentRegion(regionName, "SL"))
		{
			return new Color(0.19f, 0.73f, 0.7f, 1f);
		}
		if (EquivalentRegion(regionName, "SI"))
		{
			return new Color(0.91f, 0.35f, 0.5f, 1f);
		}
		if (EquivalentRegion(regionName, "LF"))
		{
			return new Color(0.75f, 0.16f, 0.12f, 1f);
		}
		if (EquivalentRegion(regionName, "UW"))
		{
			return new Color(0.94f, 0.84f, 0.73f, 1f);
		}
		if (EquivalentRegion(regionName, "SS"))
		{
			return new Color(1f, 0.65f, 0.21f, 1f);
		}
		if (EquivalentRegion(regionName, "SB"))
		{
			return new Color(0.61f, 0.35f, 0.2f, 1f);
		}
		if (ModManager.MSC)
		{
			if (EquivalentRegion(regionName, "DM"))
			{
				return new Color(5f / 51f, 0.30980393f, 77f / 85f);
			}
			if (EquivalentRegion(regionName, "VS"))
			{
				return new Color(1f, 0.53f, 0.51f, 1f);
			}
		}
		if (ModManager.ModdedRegionsEnabled)
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionName + Path.DirectorySeparatorChar + "regioncolor.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllText(path).Trim().Split(',');
				if (array.Length == 3)
				{
					return new Color(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture), 1f);
				}
				if (array.Length == 4)
				{
					return new Color(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
			}
		}
		return new Color(1f, 1f, 1f, 1f);
	}

	public static bool EquivalentRegion(string regionA, string regionB)
	{
		if (regionA == regionB)
		{
			return true;
		}
		if (ModManager.MSC)
		{
			if ((regionA == "SL" || regionA == "LM") && (regionB == "SL" || regionB == "LM"))
			{
				return true;
			}
			if ((regionA == "SS" || regionA == "RM") && (regionB == "SS" || regionB == "RM"))
			{
				return true;
			}
			if ((regionA == "DS" || regionA == "UG") && (regionB == "UG" || regionB == "DS"))
			{
				return true;
			}
			if ((regionA == "SH" || regionA == "CL") && (regionB == "SH" || regionB == "CL"))
			{
				return true;
			}
			if ((regionA == "DM" || regionA == "MS") && (regionB == "DM" || regionB == "MS"))
			{
				return true;
			}
		}
		if (ModManager.ModdedRegionsEnabled)
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionA + Path.DirectorySeparatorChar + "equivalences.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllText(path).Trim().Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Contains("-"))
					{
						array[i] = array[i].Substring(array[i].IndexOf('-') + 1);
					}
					if (regionB == array[i])
					{
						return true;
					}
				}
			}
			string path2 = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionB + Path.DirectorySeparatorChar + "equivalences.txt");
			if (File.Exists(path2))
			{
				string[] array2 = File.ReadAllText(path2).Trim().Split(',');
				for (int j = 0; j < array2.Length; j++)
				{
					if (array2[j].Contains("-"))
					{
						array2[j] = array2[j].Substring(array2[j].IndexOf('-') + 1);
					}
					if (regionA == array2[j])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public static string GetProperRegionAcronym(SlugcatStats.Name character, string baseAcronym)
	{
		string text = baseAcronym;
		if (text == "UX")
		{
			text = "UW";
		}
		else if (text == "SX")
		{
			text = "SS";
		}
		if (ModManager.MSC && character != null)
		{
			if ((character == MoreSlugcatsEnums.SlugcatStatsName.Spear || character == MoreSlugcatsEnums.SlugcatStatsName.Artificer) && text == "SL")
			{
				text = "LM";
			}
			if (character == MoreSlugcatsEnums.SlugcatStatsName.Saint && text == "DS")
			{
				text = "UG";
			}
			if ((character == MoreSlugcatsEnums.SlugcatStatsName.Saint || character == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) && text == "SS")
			{
				text = "RM";
			}
			if (character == MoreSlugcatsEnums.SlugcatStatsName.Saint && text == "SH")
			{
				text = "CL";
			}
		}
		string[] array = AssetManager.ListDirectory("World", directories: true);
		for (int i = 0; i < array.Length; i++)
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + Path.GetFileName(array[i]) + Path.DirectorySeparatorChar + "equivalences.txt");
			if (!File.Exists(path))
			{
				continue;
			}
			string[] array2 = File.ReadAllText(path).Trim().Split(',');
			for (int j = 0; j < array2.Length; j++)
			{
				string text2 = null;
				string text3 = array2[j];
				if (array2[j].Contains("-"))
				{
					text3 = array2[j].Split('-')[0];
					text2 = array2[j].Split('-')[1];
				}
				if (text3 == baseAcronym && (text2 == null || character.value.ToLower() == text2.ToLower()))
				{
					text = Path.GetFileName(array[i]).ToUpper();
				}
			}
		}
		return text;
	}

	public static string GetVanillaEquivalentRegionAcronym(string baseAcronym)
	{
		if (ModManager.MSC)
		{
			switch (baseAcronym)
			{
			case "UG":
				return "DS";
			case "RM":
				return "SS";
			case "CL":
				return "SH";
			case "LM":
				return "SL";
			}
		}
		if (ModManager.ModdedRegionsEnabled)
		{
			string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + baseAcronym + Path.DirectorySeparatorChar + "equivalences.txt");
			if (File.Exists(path))
			{
				string[] array = File.ReadAllText(path).Trim().Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].Contains("-"))
					{
						array[i] = array[i].Substring(array[i].IndexOf('-') + 1);
					}
					switch (array[i])
					{
					case "SU":
					case "GW":
					case "CC":
					case "HI":
					case "SI":
					case "SS":
					case "SB":
					case "SL":
					case "UW":
					case "DS":
					case "SH":
					case "LF":
						return array[i];
					}
				}
				return array[0];
			}
		}
		return baseAcronym;
	}

	public static string GetRegionFullName(string regionAcro, SlugcatStats.Name slugcatIndex)
	{
		string text = "";
		if (slugcatIndex != null)
		{
			text = "-" + slugcatIndex.value;
		}
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionAcro + Path.DirectorySeparatorChar + "DisplayName" + text + ".txt");
		if (text != "" && !File.Exists(path))
		{
			path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + regionAcro + Path.DirectorySeparatorChar + "DisplayName.txt");
		}
		if (File.Exists(path))
		{
			return File.ReadAllLines(path)[0];
		}
		return "Unknown Region";
	}

	public static int NumberOfRoomsInRegion(string name)
	{
		string[] array = new string[1] { "" };
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "world_" + name + ".txt");
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		bool flag = false;
		int num = 1;
		for (int i = 0; i < array.Length && !(array[i] == "END ROOMS"); i++)
		{
			if (flag && array[i].Contains(" : "))
			{
				num++;
			}
			if (array[i] == "ROOMS")
			{
				flag = true;
			}
		}
		return num;
	}

	public static Region[] LoadAllRegions(SlugcatStats.Name storyIndex)
	{
		string[] array = new string[1] { "" };
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "regions.txt");
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		Region[] array2 = new Region[array.Length];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			array2[i] = new Region(array[i], num, i, storyIndex);
			num += array2[i].numberOfRooms;
		}
		return array2;
	}

	public Region(string name, int firstRoomIndex, int regionNumber, SlugcatStats.Name storyIndex)
	{
		this.name = name;
		this.firstRoomIndex = firstRoomIndex;
		this.regionNumber = regionNumber;
		roomSettingsTemplates = new RoomSettings[0];
		roomSettingTemplateNames = new string[0];
		regionParams = new RegionParams();
		subRegions = new List<string> { "" };
		altSubRegions = new List<string> { null };
		numberOfRooms = NumberOfRoomsInRegion(name);
		propertiesWaterColor = RainWorld.DefaultWaterColor;
		string[] array = new string[1] { "" };
		string text = "";
		bool flag = false;
		if (storyIndex != null)
		{
			text = "-" + storyIndex.value;
		}
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "properties" + text + ".txt");
		if (text != "" && !File.Exists(path))
		{
			path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "properties.txt");
		}
		else if (text != "")
		{
			flag = true;
		}
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
			if (array2.Length < 2)
			{
				continue;
			}
			switch (array2[0])
			{
			case "Room Setting Templates":
			{
				string[] array7 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
				roomSettingsTemplates = new RoomSettings[array7.Length];
				roomSettingTemplateNames = new string[array7.Length];
				for (int j = 0; j < array7.Length; j++)
				{
					roomSettingTemplateNames[j] = array7[j];
					ReloadRoomSettingsTemplate(array7[j]);
				}
				break;
			}
			case "batDepleteCyclesMin":
				regionParams.batDepleteCyclesMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "batDepleteCyclesMax":
				regionParams.batDepleteCyclesMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "batDepleteCyclesMaxIfLessThanTwoLeft":
				regionParams.batDepleteCyclesMaxIfLessThanTwoLeft = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "batDepleteCyclesMaxIfLessThanFiveLeft":
				regionParams.batDepleteCyclesMaxIfLessThanFiveLeft = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "overseersSpawnChance":
				regionParams.overseersSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "overseersMin":
				regionParams.overseersMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "overseersMax":
				regionParams.overseersMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "playerGuideOverseerSpawnChance":
				regionParams.playerGuideOverseerSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "scavsMin":
				regionParams.scavsMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "scavsMax":
				regionParams.scavsMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "scavsSpawnChance":
				regionParams.scavsSpawnChance = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "Subregion":
				if (flag)
				{
					altSubRegions.Add(array2[1]);
					break;
				}
				subRegions.Add(array2[1]);
				altSubRegions.Add(null);
				break;
			case "batsPerActiveSwarmRoom":
				regionParams.batsPerActiveSwarmRoom = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "batsPerInactiveSwarmRoom":
				regionParams.batsPerInactiveSwarmRoom = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "blackSalamanderChance":
				regionParams.blackSalamanderChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "corruptionEffectColor":
			{
				string[] array6 = array2[1].Split(',');
				if (array6.Length == 3)
				{
					regionParams.corruptionEffectColor = new Color(float.Parse(array6[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array6[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "corruptionEyeColor":
			{
				string[] array5 = array2[1].Split(',');
				if (array5.Length == 3)
				{
					regionParams.corruptionEyeColor = new Color(float.Parse(array5[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array5[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "kelpColor":
			{
				string[] array4 = array2[1].Split(',');
				if (array4.Length == 3)
				{
					regionParams.kelpColor = new Color(float.Parse(array4[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array4[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "albinos":
				regionParams.albinos = array2[1].Trim().ToLower() == "true";
				break;
			case "waterColorOverride":
			{
				string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
				propertiesWaterColor = new Color(float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture));
				break;
			}
			case "scavsDelayInitialMin":
				regionParams.scavengerDelayInitialMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "scavsDelayInitialMax":
				regionParams.scavengerDelayInitialMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "scavsDelayRepeatMin":
				regionParams.scavengerDelayRepeatMin = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "scavsDelayRepeatMax":
				regionParams.scavengerDelayRepeatMax = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "pupSpawnChance":
				regionParams.slugPupSpawnChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "GlacialWasteland":
				regionParams.glacialWasteland = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture) > 0;
				break;
			case "earlyCycleChance":
				regionParams.earlyCycleChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			case "earlyCycleFloodChance":
				regionParams.earlyCycleFloodChance = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "properties.txt");
		array = new string[1] { "" };
		if (File.Exists(path))
		{
			array = File.ReadAllLines(path);
		}
		for (int k = 0; k < array.Length; k++)
		{
			string[] array8 = Regex.Split(Custom.ValidateSpacedDelimiter(array[k], ":"), ": ");
			if (array8.Length >= 2)
			{
				string text2 = array8[0];
				if (text2 != null && text2 == "Subregion")
				{
					subRegions.Add(array8[1]);
				}
			}
		}
	}

	public RoomSettings GetRoomSettingsTemplate(string templateName)
	{
		if (roomSettingTemplateNames == null || roomSettingsTemplates == null || roomSettingTemplateNames.Length == 0 || roomSettingsTemplates.Length == 0)
		{
			return DefaultRoomSettings.ancestor;
		}
		if (templateName == "FIRST")
		{
			return roomSettingsTemplates[0];
		}
		for (int i = 0; i < roomSettingTemplateNames.Length; i++)
		{
			if (roomSettingTemplateNames[i] == templateName)
			{
				return roomSettingsTemplates[i];
			}
		}
		return DefaultRoomSettings.ancestor;
	}

	public void ReloadRoomSettingsTemplate(string templateName)
	{
		for (int i = 0; i < roomSettingTemplateNames.Length; i++)
		{
			if (roomSettingTemplateNames[i] == templateName)
			{
				roomSettingsTemplates[i] = new RoomSettings(name + "_SettingsTemplate_" + templateName, this, template: true, i == 0, null);
				break;
			}
		}
	}

	public bool IsRoomInRegion(int roomIndex)
	{
		if (roomIndex >= firstRoomIndex)
		{
			return roomIndex < firstRoomIndex + numberOfRooms;
		}
		return false;
	}

	public int CyclesToDepleteASwarmRoom(int activeOnesInRegion)
	{
		if (activeOnesInRegion < 1)
		{
			return 1;
		}
		int batDepleteCyclesMin = regionParams.batDepleteCyclesMin;
		int maxExclusive = regionParams.batDepleteCyclesMax;
		if (activeOnesInRegion < 2)
		{
			maxExclusive = regionParams.batDepleteCyclesMaxIfLessThanTwoLeft;
		}
		else if (activeOnesInRegion < 5)
		{
			maxExclusive = regionParams.batDepleteCyclesMaxIfLessThanFiveLeft;
		}
		return Random.Range(batDepleteCyclesMin, maxExclusive);
	}
}
