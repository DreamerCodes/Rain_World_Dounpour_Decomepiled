using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using RWCustom;

public static class BackwardsCompability
{
	public class SpawnerMap
	{
		public class Region
		{
			public int index;

			public List<string> oldSpawners;

			public List<string> newSpawners;

			public List<int> addressInNewSpawners;

			public List<string> oldLineages;

			public List<string> newLineages;

			public List<int> addressInNewLineages;

			public Region(int index)
			{
				this.index = index;
				oldSpawners = new List<string>();
				newSpawners = new List<string>();
				addressInNewSpawners = new List<int>();
				oldLineages = new List<string>();
				newLineages = new List<string>();
				addressInNewLineages = new List<int>();
			}

			private string SpawnerStringWithoutCharacter(string s)
			{
				if (!s.Contains("(") || !s.Contains(")"))
				{
					return s;
				}
				return s.Split('(')[0] + s.Split(')')[1];
			}

			private bool SpawnerCharacterMatch(SlugcatStats.Name slugcat, string s)
			{
				if (!s.Contains("(") || !s.Contains(")"))
				{
					return true;
				}
				string[] array = s.Split('(')[1].Split(')')[0].Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					int result;
					SlugcatStats.Name name = ((!int.TryParse(array[i], NumberStyles.Any, CultureInfo.InvariantCulture, out result)) ? new SlugcatStats.Name(array[i]) : BackwardsCompatibilityRemix.ParsePlayerNumber(result));
					if (name.value == slugcat.value)
					{
						return true;
					}
				}
				return false;
			}

			public void FeedOldSpawner(SlugcatStats.Name slugcat, string s)
			{
				if (SpawnerCharacterMatch(slugcat, s))
				{
					string text = SpawnerStringWithoutCharacter(s);
					if (text.Length > 7 && text.Substring(0, 7) == "LINEAGE")
					{
						FeedOldLineage(text);
					}
					oldSpawners.Add(text);
					addressInNewSpawners.Add(-1);
				}
			}

			public void FeedNewSpawner(SlugcatStats.Name slugcat, string s)
			{
				if (!SpawnerCharacterMatch(slugcat, s))
				{
					return;
				}
				string text = SpawnerStringWithoutCharacter(s);
				if (text.Length > 7 && text.Substring(0, 7) == "LINEAGE")
				{
					FeedNewLineage(text);
				}
				newSpawners.Add(text);
				for (int i = 0; i < oldSpawners.Count; i++)
				{
					if (oldSpawners[i] == text)
					{
						addressInNewSpawners[i] = newSpawners.Count - 1;
						break;
					}
				}
			}

			private void FeedOldLineage(string s)
			{
				oldLineages.Add(s);
				addressInNewLineages.Add(-1);
			}

			private void FeedNewLineage(string s)
			{
				newLineages.Add(s);
				for (int i = 0; i < oldLineages.Count; i++)
				{
					if (oldLineages[i] == s)
					{
						addressInNewLineages[i] = newLineages.Count - 1;
						return;
					}
				}
				Custom.LogWarning(index.ToString(), "old world doesn't contain lineage:", s);
			}

			public int TranslateSpawner(int spawnerNumber)
			{
				spawnerNumber -= index * 1000;
				if (spawnerNumber < 0 || spawnerNumber >= addressInNewSpawners.Count)
				{
					return -1;
				}
				return index * 1000 + addressInNewSpawners[spawnerNumber];
			}
		}

		public List<Region> regions;

		public int RegionOfSpawner(int spawnerNumber)
		{
			int num = spawnerNumber % 1000;
			int num2 = (spawnerNumber - num) / 1000;
			if (num2 >= regions.Count)
			{
				for (int i = regions.Count; i <= num2; i++)
				{
					regions.Add(new Region(i));
				}
			}
			return num2;
		}

		public SpawnerMap(SlugcatStats.Name slugcat, int oldWorldVersion, int newWorldVersion)
		{
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "spawnerIndexMap" + oldWorldVersion + ".txt"));
			string[] array2 = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "spawnerIndexMap" + newWorldVersion + ".txt"));
			regions = new List<Region>();
			for (int i = 0; i < 12; i++)
			{
				regions.Add(new Region(i));
			}
			for (int j = 0; j < array.Length; j++)
			{
				string[] array3 = Regex.Split(array[j], " ");
				int spawnerNumber = int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				int num = RegionOfSpawner(spawnerNumber);
				if (num != -1)
				{
					regions[num].FeedOldSpawner(slugcat, array3[1]);
				}
			}
			for (int k = 0; k < array2.Length; k++)
			{
				string[] array4 = Regex.Split(array2[k], " ");
				int spawnerNumber2 = int.Parse(array4[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				int num2 = RegionOfSpawner(spawnerNumber2);
				if (num2 != -1)
				{
					regions[num2].FeedNewSpawner(slugcat, array4[1]);
				}
			}
			for (int l = 0; l < regions.Count; l++)
			{
				for (int m = 0; m < regions[l].oldSpawners.Count; m++)
				{
					if (regions[l].addressInNewSpawners[m] == -1)
					{
						Custom.LogImportant("No equivalent spawner:", regions[l].oldSpawners[m]);
					}
				}
			}
		}

		public int TranslateSpawnerNumber(int oldSpawnerNumber)
		{
			int num = RegionOfSpawner(oldSpawnerNumber);
			if (num == -1)
			{
				return -1;
			}
			return regions[num].TranslateSpawner(oldSpawnerNumber);
		}
	}

	public static void IndexMapWorld(int worldVersion, PlayerProgression prog, string overrideOutputPath)
	{
		string text = "";
		string text2 = "";
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < prog.regionNames.Length; i++)
		{
			string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + prog.regionNames[i] + Path.DirectorySeparatorChar + "world_" + prog.regionNames[i] + ".txt"));
			bool flag = false;
			bool flag2 = false;
			int num3 = 1;
			for (int j = 0; j < array.Length; j++)
			{
				if (array[j] == "END ROOMS")
				{
					flag = false;
					text = text + num + " " + prog.regionNames[i] + "OffscreenDen\r\n";
					num++;
				}
				if (flag && array[j].Contains(" : "))
				{
					num3++;
					text = text + num + " " + Regex.Split(array[j], " : ")[0] + "\r\n";
					num++;
				}
				if (array[j] == "ROOMS")
				{
					flag = true;
				}
				if (array[j] == "END CREATURES")
				{
					flag2 = false;
				}
				if (flag2 && array[j].Length > 2 && array[j].Substring(0, 2) != "//")
				{
					string input = array[j];
					string text3 = "";
					if (array[j][0] == '(')
					{
						for (int k = 0; k < array[j].Length; k++)
						{
							text3 += array[j][k];
							if (array[j][k] == ')')
							{
								input = array[j].Substring(k + 1, array[j].Length - k - 1);
								break;
							}
						}
					}
					string[] array2 = Regex.Split(input, " : ");
					if (array2.Length > 1)
					{
						if (array2[0] == "LINEAGE")
						{
							if (array2[1].Length > 3 && array2[1].Substring(0, 3) == "OFF")
							{
								array2[1] = prog.regionNames[i] + array2[1];
							}
							text2 = text2 + (i * 1000 + num2) + " " + array2[0] + text3 + array2[1] + array2[2] + "\r\n";
							num2++;
						}
						else
						{
							string[] array3 = Regex.Split(Custom.ValidateSpacedDelimiter(array2[1], ","), ", ");
							for (int l = 0; l < array3.Length; l++)
							{
								if (array3[l].Length > 0)
								{
									if (array2[0].Length > 3 && array2[0].Substring(0, 3) == "OFF")
									{
										array2[0] = prog.regionNames[i] + array2[0];
									}
									text2 = text2 + (i * 1000 + num2) + " " + array2[0] + text3 + array3[l] + "\r\n";
									num2++;
								}
							}
						}
					}
				}
				if (array[j] == "CREATURES")
				{
					flag2 = true;
				}
			}
			num2 = 0;
		}
		int[,] array4 = new int[prog.regionNames.Length, 2];
		int num4 = 0;
		for (int m = 0; m < prog.regionNames.Length; m++)
		{
			array4[m, 0] = num4;
			array4[m, 1] = Region.NumberOfRoomsInRegion(prog.regionNames[m]);
			num4 += array4[m, 1];
		}
		for (int n = 0; n < array4.GetLength(0); n++)
		{
			Custom.Log(n.ToString(), "number of rooms in", prog.regionNames[n], "f:", array4[n, 0].ToString(), "t:", array4[n, 1].ToString());
		}
		string text4 = "";
		for (int num5 = 0; num5 < array4.GetLength(0); num5++)
		{
			text4 = text4 + array4[num5, 0] + " " + array4[num5, 1] + "\r\n";
		}
		string text5 = ((overrideOutputPath != null) ? overrideOutputPath : (Custom.RootFolderDirectory() + Path.DirectorySeparatorChar + "World" + Path.DirectorySeparatorChar + "IndexMaps"));
		using (StreamWriter streamWriter = File.CreateText((text5 + Path.DirectorySeparatorChar + "roomIndexMap" + worldVersion + ".txt").ToLowerInvariant()))
		{
			streamWriter.Write(text);
		}
		using (StreamWriter streamWriter2 = File.CreateText((text5 + Path.DirectorySeparatorChar + "spawnerIndexMap" + worldVersion + ".txt").ToLowerInvariant()))
		{
			streamWriter2.Write(text2);
		}
		using StreamWriter streamWriter3 = File.CreateText((text5 + Path.DirectorySeparatorChar + "regionsIndexMap" + worldVersion + ".txt").ToLowerInvariant());
		streamWriter3.Write(text4);
	}

	public static void UpdateWorldVersion(SaveState saveState, int newWorldVersion, PlayerProgression prog)
	{
		Custom.LogImportant("UPDATING SAVE FILE FROM WORLD VERSION [", saveState.worldVersion.ToString(), "] TO [", newWorldVersion.ToString(), "]");
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "roomIndexMap" + saveState.worldVersion + ".txt"));
		string[] array2 = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "roomIndexMap" + newWorldVersion + ".txt"));
		prog.BackUpSave("_v" + saveState.worldVersion + "_Backup");
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Split(' ')[1];
		}
		for (int j = 0; j < array2.Length; j++)
		{
			array2[j] = array2[j].Split(' ')[1];
		}
		bool[] array3 = new bool[array2.Length];
		List<int> roomIndexMap = new List<int>();
		for (int k = 0; k < array.Length; k++)
		{
			bool flag = false;
			for (int l = 0; l < 2; l++)
			{
				if (flag)
				{
					break;
				}
				for (int m = 0; m < array2.Length; m++)
				{
					if (array[k] == array2[m] && (!array3[m] || l > 0))
					{
						roomIndexMap.Add(m);
						array3[m] = true;
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				roomIndexMap.Add(-1);
			}
		}
		int[,] oldRegionRooms = LoadRegionRooms(saveState.worldVersion);
		int[,] newRegionRooms = LoadRegionRooms(newWorldVersion);
		SpawnerMap spawnerMap = new SpawnerMap(saveState.saveStateNumber, saveState.worldVersion, newWorldVersion);
		for (int n = 0; n < saveState.regionLoadStrings.Length; n++)
		{
			if (saveState.regionLoadStrings[n] != null)
			{
				saveState.regionLoadStrings[n] = UpdateRegionStateString(n, saveState.regionLoadStrings[n], ref roomIndexMap, ref spawnerMap, ref oldRegionRooms, ref newRegionRooms);
			}
		}
		UpdateWorldVersionOfDeathPersistentData(ref saveState.deathPersistentSaveData, saveState.worldVersion, newWorldVersion, ref roomIndexMap);
		List<int> list = new List<int>();
		List<int> list2 = new List<int>();
		for (int num = 0; num < saveState.respawnCreatures.Count; num++)
		{
			int num2 = spawnerMap.TranslateSpawnerNumber(saveState.respawnCreatures[num]);
			if (num2 != -1)
			{
				list.Add(num2);
			}
		}
		for (int num3 = 0; num3 < saveState.waitRespawnCreatures.Count; num3++)
		{
			int num4 = spawnerMap.TranslateSpawnerNumber(saveState.waitRespawnCreatures[num3]);
			if (num4 != -1)
			{
				list2.Add(num4);
			}
		}
		saveState.respawnCreatures = list;
		saveState.waitRespawnCreatures = list2;
		saveState.worldVersion = newWorldVersion;
	}

	private static int[,] LoadRegionRooms(int worldVersion)
	{
		string[] array = File.ReadAllLines(AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + "IndexMaps" + Path.DirectorySeparatorChar + "regionsIndexMap" + worldVersion + ".txt"));
		int[,] array2 = new int[array.Length, 2];
		for (int i = 0; i < array.Length; i++)
		{
			string[] array3 = Regex.Split(array[i], " ");
			array2[i, 0] = int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture);
			array2[i, 1] = int.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
		return array2;
	}

	public static string UpdateRegionStateString(int regionIndex, string regionState, ref List<int> roomIndexMap, ref SpawnerMap spawnerMap, ref int[,] oldRegionRooms, ref int[,] newRegionRooms)
	{
		string text = "";
		string[] array = Regex.Split(regionState, "<rgA>");
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(array[i], "<rgB>");
			if (array2.Length <= 1)
			{
				continue;
			}
			switch (array2[0])
			{
			case "POPULATION":
				text = text + "POPULATION<rgB>" + MigrateCreatures(array2[1], ref roomIndexMap, ref spawnerMap) + "<rgA>";
				continue;
			case "OBJECTS":
				text = text + "OBJECTS<rgB>" + MigrateObjects(array2[1], ref roomIndexMap) + "<rgA>";
				continue;
			case "CONSUMEDITEMS":
				text = text + "CONSUMEDITEMS<rgB>" + MigrateConsumedItems(array2[1], ref roomIndexMap) + "<rgA>";
				continue;
			case "LINEAGES":
				text = text + "LINEAGES<rgB>" + MigrateLineages(regionIndex, array2[1], ref spawnerMap) + "<rgA>";
				continue;
			case "ROOMSVISITED":
				text = text + "ROOMSVISITED<rgB>" + MigrateRoomsVisited(regionIndex, array2[1], ref roomIndexMap, ref oldRegionRooms, ref newRegionRooms) + "<rgA>";
				continue;
			}
			for (int j = 0; j < array2.Length; j++)
			{
				text = text + array2[j] + ((j < array2.Length - 1) ? "<rgB>" : "");
			}
			text += "<rgA>";
		}
		return text;
	}

	private static string MigrateRoomsVisited(int region, string str, ref List<int> roomIndexMap, ref int[,] oldRegionRooms, ref int[,] newRegionRooms)
	{
		bool[] array = new bool[newRegionRooms[region, 1]];
		if (str.Length != oldRegionRooms[region, 1])
		{
			Custom.LogWarning(region.ToString(), "region room length mismatch:", str.Length.ToString(), oldRegionRooms[region, 1].ToString());
		}
		else
		{
			Custom.Log(region.ToString(), "region rooms length MATCHING:", str.Length.ToString(), oldRegionRooms[region, 1].ToString());
		}
		for (int i = 0; i < str.Length; i++)
		{
			if (str[i] == '1')
			{
				int num = oldRegionRooms[region, 0] + i;
				int num2 = MapRoom(num, ref roomIndexMap);
				int num3 = num2 - newRegionRooms[region, 0];
				if (num3 >= 0 && num3 < array.Length)
				{
					array[num3] = true;
					continue;
				}
				Custom.LogWarning($"Failed to migrate visited room! old:{num} new:{num2} indx:{num3} rng:{newRegionRooms[region, 0]}-{newRegionRooms[region, 0] + newRegionRooms[region, 1]}");
			}
		}
		string text = "";
		for (int j = 0; j < array.Length; j++)
		{
			text += (array[j] ? "1" : "0");
		}
		return text;
	}

	private static string MigrateLineages(int region, string lineagesString, ref SpawnerMap spawnerMap)
	{
		int[] array = new int[spawnerMap.regions[region].newLineages.Count];
		string[] array2 = lineagesString.Split('.');
		for (int i = 0; i < array2.Length; i++)
		{
			if (array2[i].Length <= 0 || !(array2[i] != "0"))
			{
				continue;
			}
			try
			{
				if (i < spawnerMap.regions[region].addressInNewLineages.Count)
				{
					int num = spawnerMap.regions[region].addressInNewLineages[i];
					if (num >= 0 && num < array.Length)
					{
						Custom.Log(region.ToString(), " lineage migrate   f:", i.ToString(), "(", spawnerMap.regions[region].oldLineages[i], ") t:", num.ToString(), "(", spawnerMap.regions[region].newLineages[num], ")  val:", array2[i]);
						array[num] = int.Parse(array2[i], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
				}
				else
				{
					Custom.LogWarning(region.ToString(), " Lineage not found");
				}
			}
			catch
			{
				Custom.LogWarning(region.ToString(), " Lineage migration error");
			}
		}
		string text = "";
		for (int j = 0; j < array.Length; j++)
		{
			text = text + array[j] + ((j < array.Length - 1) ? "." : "");
		}
		return text;
	}

	private static string MigrateConsumedItems(string itemsString, ref List<int> roomIndexMap)
	{
		string[] array = Regex.Split(itemsString, "<rgC>");
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 2)
			{
				continue;
			}
			try
			{
				string[] array2 = array[i].Split('.');
				int num = int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture);
				if (num != roomIndexMap[num])
				{
					Custom.Log("consumable", num.ToString(), "->", roomIndexMap[num].ToString(), "(", array2[1], ",", array2[2], ")");
				}
				num = roomIndexMap[num];
				text = text + num + "." + array2[1] + "." + array2[2] + "<rgC>";
			}
			catch
			{
				Custom.LogWarning("Consumable migrate fail", array[0]);
			}
		}
		return text;
	}

	private static string MigrateLineages(string lineagesString, ref SpawnerMap spawnerMap)
	{
		return "";
	}

	private static string MigrateCreatures(string creaturesString, ref List<int> roomIndexMap, ref SpawnerMap spawnerMap)
	{
		string[] array = Regex.Split(creaturesString, "<rgC>");
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				string[] array2 = Regex.Split(array[i], "<cA>");
				if (array2.Length > 2)
				{
					string[] array3 = array2[2].Split('.');
					int index = int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					array2[2] = roomIndexMap[index] + "." + array3[1];
					string[] array4 = array2[1].Split('.');
					array4[1] = spawnerMap.TranslateSpawnerNumber(int.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture)).ToString();
					array2[1] = array4[0] + "." + array4[1] + "." + array4[2];
					string text2 = "";
					for (int j = 0; j < array2.Length; j++)
					{
						text2 = text2 + array2[j] + ((j < array2.Length - 1) ? "<cA>" : "");
					}
					text = text + text2 + "<rgC>";
				}
			}
			catch
			{
				Custom.LogWarning("creature migrate fail:", array[i]);
			}
		}
		return text;
	}

	private static string MigrateObjects(string objectsString, ref List<int> roomIndexMap)
	{
		string[] array = Regex.Split(objectsString, "<rgC>");
		string text = "";
		for (int i = 0; i < array.Length; i++)
		{
			try
			{
				string[] array2 = Regex.Split(array[i], "<oA>");
				if (array2.Length > 2)
				{
					string[] array3 = array2[2].Split('.');
					int index = int.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					index = roomIndexMap[index];
					array2[2] = index + "." + array3[1] + "." + array3[2] + "." + array3[3];
					string text2 = "";
					for (int j = 0; j < array2.Length; j++)
					{
						text2 = text2 + array2[j] + ((j < array2.Length - 1) ? "<oA>" : "");
					}
					text = text + text2 + "<rgC>";
				}
			}
			catch
			{
				Custom.LogWarning("object migrate fail:", array[i]);
			}
		}
		return text;
	}

	public static int MapRoom(int oldRoom, ref List<int> roomIndexMap)
	{
		if (oldRoom < 0 || oldRoom >= roomIndexMap.Count)
		{
			return 0;
		}
		return roomIndexMap[oldRoom];
	}

	public static void UpdateWorldVersionOfDeathPersistentData(ref DeathPersistentSaveData dpDt, int oldWorldVersion, int newWorldVersion, ref List<int> roomIndexMap)
	{
		if (newWorldVersion == dpDt.worldVersion)
		{
			return;
		}
		Custom.LogImportant("UPDATING DEATHPER. DATA world version from [", dpDt.worldVersion.ToString(), "] to [", newWorldVersion.ToString(), "]");
		if (oldWorldVersion != dpDt.worldVersion)
		{
			Custom.LogWarning("ERROR! OLD WORLD VERSION MISMATCH");
			return;
		}
		if (dpDt.karmaFlowerPosition.HasValue && !dpDt.karmaFlowerPosition.Value.Valid)
		{
			dpDt.karmaFlowerPosition = new WorldCoordinate(dpDt.karmaFlowerPosition.Value.unknownName, dpDt.karmaFlowerPosition.Value.x, dpDt.karmaFlowerPosition.Value.y, dpDt.karmaFlowerPosition.Value.abstractNode);
		}
		else if (dpDt.karmaFlowerPosition.HasValue && dpDt.karmaFlowerPosition.Value.room >= 0 && dpDt.karmaFlowerPosition.Value.room < roomIndexMap.Count)
		{
			dpDt.karmaFlowerPosition = new WorldCoordinate(roomIndexMap[dpDt.karmaFlowerPosition.Value.room], dpDt.karmaFlowerPosition.Value.x, dpDt.karmaFlowerPosition.Value.y, dpDt.karmaFlowerPosition.Value.abstractNode);
		}
		else
		{
			dpDt.karmaFlowerPosition = null;
		}
		for (int num = dpDt.deathPositions.Count - 1; num >= 0; num--)
		{
			if (!dpDt.deathPositions[num].Valid)
			{
				dpDt.deathPositions[num] = new WorldCoordinate(dpDt.deathPositions[num].unknownName, dpDt.deathPositions[num].x, dpDt.deathPositions[num].y, dpDt.deathPositions[num].abstractNode);
			}
			else if (dpDt.deathPositions[num].room >= 0 && dpDt.deathPositions[num].room < roomIndexMap.Count)
			{
				dpDt.deathPositions[num] = new WorldCoordinate(roomIndexMap[dpDt.deathPositions[num].room], dpDt.deathPositions[num].x, dpDt.deathPositions[num].y, dpDt.deathPositions[num].abstractNode);
			}
			else
			{
				dpDt.deathPositions.RemoveAt(num);
			}
		}
		for (int num2 = dpDt.consumedFlowers.Count - 1; num2 >= 0; num2--)
		{
			if (dpDt.consumedFlowers[num2].Valid && dpDt.consumedFlowers[num2].originRoom >= 0 && dpDt.consumedFlowers[num2].originRoom < roomIndexMap.Count)
			{
				dpDt.consumedFlowers[num2].originRoom = roomIndexMap[dpDt.consumedFlowers[num2].originRoom];
			}
			else
			{
				dpDt.consumedFlowers.RemoveAt(num2);
			}
		}
		dpDt.worldVersion = newWorldVersion;
	}
}
