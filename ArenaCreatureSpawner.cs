using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public static class ArenaCreatureSpawner
{
	private abstract class Spawner
	{
		public float spawnChance = 1f;

		public bool spawn = true;

		public string groupString;

		public string symbolString;

		public string invSymbolString;

		public CreatureGroup group;

		public SpawnSymbol symbol;

		public SpawnSymbol invSymbol;

		public int ID = UnityEngine.Random.Range(0, 10000);

		public Spawner()
		{
		}

		public void Disable()
		{
			spawn = false;
			spawnChance = 0f;
		}
	}

	private class CritterSpawnData : Spawner
	{
		public CreatureTemplate.Type type;

		public List<int> dens;

		public string spawnDataString;

		public bool rare;

		public CritterSpawnData(CreatureTemplate.Type type)
		{
			this.type = type;
			dens = new List<int>();
		}
	}

	private class DenGroup
	{
		public string name;

		public List<int> dens;

		public DenGroup(string name)
		{
			this.name = name;
			dens = new List<int>();
		}
	}

	private class CreatureGroup : Spawner
	{
		public string name;

		public List<Spawner> connectedSpawners;

		public CreatureGroup(string name)
		{
			this.name = name;
			connectedSpawners = new List<Spawner>();
		}

		public bool AnyConnectedSpawnersActive()
		{
			for (int i = 0; i < connectedSpawners.Count; i++)
			{
				if (connectedSpawners[i].spawn)
				{
					return true;
				}
			}
			return false;
		}
	}

	private class SpawnSymbol
	{
		public string name;

		public List<string> possibleOutcomes;

		public bool inverse;

		public string decidedOutcome;

		public List<Spawner> connectedSpawners;

		public SpawnSymbol(string name, string firstPossoutcome, bool inv)
		{
			this.name = name;
			possibleOutcomes = new List<string> { firstPossoutcome };
			inverse = inv;
			connectedSpawners = new List<Spawner>();
		}

		public bool AnyConnectedSpawnersActiveUnderCurrentRoll()
		{
			for (int i = 0; i < connectedSpawners.Count; i++)
			{
				if (connectedSpawners[i].spawn)
				{
					if (!inverse && connectedSpawners[i].symbolString.Substring(1, 1) == decidedOutcome)
					{
						return true;
					}
					if (inverse && connectedSpawners[i].invSymbolString.Substring(1, 1) != decidedOutcome)
					{
						return true;
					}
				}
			}
			return false;
		}
	}

	private static readonly AGLog<FromStaticClass> Log = new AGLog<FromStaticClass>();

	public static bool allowLockedCreatures;

	public static void SpawnArenaCreatures(RainWorldGame game, ArenaSetup.GameTypeSetup.WildLifeSetting wildLifeSetting, ref List<AbstractCreature> availableCreatures, ref MultiplayerUnlocks unlocks)
	{
		string path = AssetManager.ResolveFilePath("Levels" + Path.DirectorySeparatorChar + game.world.GetAbstractRoom(0).name + "_Arena.txt");
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		float num = 1f;
		if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.Off)
		{
			return;
		}
		if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.Low)
		{
			num = 0.5f;
		}
		else if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.Medium)
		{
			num = 1f;
		}
		else if (wildLifeSetting == ArenaSetup.GameTypeSetup.WildLifeSetting.High)
		{
			num = 1.5f;
		}
		AbstractRoom abstractRoom = game.world.GetAbstractRoom(0);
		List<Spawner> list = new List<Spawner>();
		List<CritterSpawnData> spawnList = new List<CritterSpawnData>();
		List<CreatureGroup> list2 = new List<CreatureGroup>();
		List<SpawnSymbol> symbols = new List<SpawnSymbol>();
		List<DenGroup> list3 = new List<DenGroup>();
		float num2 = -1f;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 2 || !(array[i].Substring(0, 2) != "//"))
			{
				continue;
			}
			string[] array2 = Regex.Split(array[i], " - ");
			int num3 = 1;
			Spawner spawner = null;
			switch (array2[0])
			{
			case "Creature":
			{
				CreatureTemplate.Type type = WorldLoader.CreatureTypeFromString(array2[1]);
				if (type != null)
				{
					spawner = new CritterSpawnData(type);
				}
				break;
			}
			case "CreatureGroup":
				spawner = new CreatureGroup(array2[1]);
				break;
			case "DenGroup":
			{
				list3.Add(new DenGroup(array2[1]));
				string[] array3 = array2[2].Split(',');
				for (int j = 0; j < array3.Length; j++)
				{
					list3[list3.Count - 1].dens.Add(int.Parse(array3[j], NumberStyles.Any, CultureInfo.InvariantCulture));
				}
				break;
			}
			case "PlayersGlow":
				game.GetArenaGameSession.playersGlowing = true;
				break;
			case "GoalAmount":
				num2 = float.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
				break;
			}
			if (spawner == null)
			{
				continue;
			}
			for (int k = 2; k < array2.Length; k++)
			{
				string[] array4 = Regex.Split(array2[k], ":");
				switch (array4[0])
				{
				case "chance":
					spawner.spawnChance = float.Parse(array4[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "RARE":
					if (spawner is CritterSpawnData)
					{
						(spawner as CritterSpawnData).rare = true;
					}
					break;
				case "group":
					spawner.groupString = array4[1];
					break;
				case "dens":
				{
					if (!(spawner is CritterSpawnData))
					{
						break;
					}
					bool flag = true;
					for (int l = 0; l < list3.Count; l++)
					{
						if (list3[l].name == array4[1])
						{
							(spawner as CritterSpawnData).dens = list3[l].dens;
							flag = false;
							break;
						}
					}
					if (flag)
					{
						string[] array5 = array4[1].Split(',');
						for (int m = 0; m < array5.Length; m++)
						{
							(spawner as CritterSpawnData).dens.Add(int.Parse(array5[m], NumberStyles.Any, CultureInfo.InvariantCulture));
						}
					}
					break;
				}
				case "spawnDataString":
					if (spawner is CritterSpawnData)
					{
						(spawner as CritterSpawnData).spawnDataString = array4[1];
					}
					break;
				case "amount":
					if (spawner is CritterSpawnData)
					{
						if (array4[1] == "players")
						{
							num3 = game.GetArenaGameSession.arenaSitting.players.Count;
							break;
						}
						string[] array5 = array4[1].Split('-');
						num3 = ((array5.Length != 1) ? UnityEngine.Random.Range(int.Parse(array5[0], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array5[1], NumberStyles.Any, CultureInfo.InvariantCulture) + 1) : int.Parse(array5[0], NumberStyles.Any, CultureInfo.InvariantCulture));
					}
					break;
				case "symbol":
					AddToSymbol(array4[1], inv: false, ref symbols);
					spawner.symbolString = array4[1];
					break;
				case "invSymbol":
					AddToSymbol(array4[1], inv: true, ref symbols);
					spawner.invSymbolString = array4[1];
					break;
				}
			}
			if (num3 > 0)
			{
				if (spawner is CreatureGroup)
				{
					list2.Add(spawner as CreatureGroup);
				}
				else if (spawner is CritterSpawnData)
				{
					spawnList.Add(spawner as CritterSpawnData);
				}
				list.Add(spawner);
				spawner.spawn = true;
				for (int n = 1; n < num3; n++)
				{
					CritterSpawnData critterSpawnData = new CritterSpawnData((spawner as CritterSpawnData).type);
					critterSpawnData.dens = (spawner as CritterSpawnData).dens;
					critterSpawnData.groupString = (spawner as CritterSpawnData).groupString;
					critterSpawnData.symbolString = (spawner as CritterSpawnData).symbolString;
					critterSpawnData.invSymbolString = (spawner as CritterSpawnData).invSymbolString;
					critterSpawnData.spawnChance = (spawner as CritterSpawnData).spawnChance;
					critterSpawnData.spawnDataString = (spawner as CritterSpawnData).spawnDataString;
					critterSpawnData.rare = (spawner as CritterSpawnData).rare;
					spawnList.Add(critterSpawnData);
					list.Add(critterSpawnData);
					critterSpawnData.spawn = true;
				}
			}
		}
		for (int num4 = 0; num4 < list.Count; num4++)
		{
			if (list[num4].symbolString != null)
			{
				for (int num5 = 0; num5 < symbols.Count; num5++)
				{
					if (list[num4].symbolString.Substring(0, 1) == symbols[num5].name)
					{
						list[num4].symbol = symbols[num5];
						symbols[num5].connectedSpawners.Add(list[num4]);
						break;
					}
				}
			}
			if (list[num4].invSymbolString != null)
			{
				for (int num6 = 0; num6 < symbols.Count; num6++)
				{
					if (list[num4].invSymbolString.Substring(0, 1) == symbols[num6].name)
					{
						list[num4].invSymbol = symbols[num6];
						symbols[num6].connectedSpawners.Add(list[num4]);
						break;
					}
				}
			}
			if (list[num4].groupString == null)
			{
				continue;
			}
			for (int num7 = 0; num7 < list2.Count; num7++)
			{
				if (list[num4].groupString == list2[num7].name)
				{
					list[num4].group = list2[num7];
					list2[num7].connectedSpawners.Add(list[num4]);
					break;
				}
			}
		}
		float num8 = 0f;
		List<CreatureTemplate.Type> list4 = new List<CreatureTemplate.Type>();
		List<CreatureTemplate.Type> list5 = new List<CreatureTemplate.Type>();
		int num9 = 0;
		if (ModManager.MSC)
		{
			for (int num10 = 0; num10 < abstractRoom.creatures.Count; num10++)
			{
				if (IsMajorCreature(abstractRoom.creatures[num10].creatureTemplate.type))
				{
					num9++;
				}
			}
		}
		for (int num11 = 0; num11 < spawnList.Count; num11++)
		{
			float num12 = Mathf.Clamp01(spawnList[num11].spawnChance);
			if (spawnList[num11].group != null)
			{
				num12 *= Mathf.Clamp01(spawnList[num11].group.spawnChance);
			}
			if (spawnList[num11].symbol != null)
			{
				num12 *= 1f / (float)spawnList[num11].symbol.possibleOutcomes.Count;
			}
			if (spawnList[num11].invSymbol != null)
			{
				num12 *= 1f - 1f / (float)spawnList[num11].invSymbol.possibleOutcomes.Count;
			}
			bool flag2 = !ModManager.MSC || (allowLockedCreatures && (game.GetArenaGameSession.chMeta == null || game.GetArenaGameSession.chMeta.unlimitedDanger || !IsMajorCreature(spawnList[num11].type) || num9 == 0));
			if (unlocks.IsCreatureUnlockedForLevelSpawn(spawnList[num11].type) || flag2)
			{
				if (!list4.Contains(spawnList[num11].type))
				{
					list4.Add(spawnList[num11].type);
				}
			}
			else
			{
				if (!list5.Contains(spawnList[num11].type))
				{
					list5.Add(spawnList[num11].type);
				}
				CreatureTemplate.Type type2 = unlocks.RecursiveFallBackCritter(spawnList[num11].type);
				if (type2 != null)
				{
					spawnList[num11].type = type2;
					spawnList[num11].spawnChance = Mathf.Clamp01(spawnList[num11].spawnChance) * 0.01f;
					num12 *= 0.5f;
				}
				else
				{
					spawnList[num11].Disable();
					num12 *= 0f;
				}
			}
			num8 += num12;
		}
		float num13 = (float)list4.Count / (float)(list4.Count + list5.Count);
		float num14 = Mathf.InverseLerp(0.7f, 0.3f, num13);
		if (num2 > 0f)
		{
			num2 *= Mathf.Lerp(Mathf.InverseLerp(0.15f, 0.75f, num13), 1f, 0.5f) * num;
		}
		for (int num15 = 0; num15 < list2.Count; num15++)
		{
			if (UnityEngine.Random.value > list2[num15].spawnChance || !list2[num15].AnyConnectedSpawnersActive())
			{
				list2[num15].Disable();
			}
		}
		for (int num16 = 0; num16 < spawnList.Count; num16++)
		{
			if (spawnList[num16].rare && UnityEngine.Random.value > Mathf.Pow(spawnList[num16].spawnChance, Custom.LerpMap(num13, 0.35f, 0.85f, 0.5f, 0.05f)))
			{
				spawnList[num16].Disable();
			}
		}
		for (int num17 = 0; num17 < symbols.Count; num17++)
		{
			if (symbols[num17].possibleOutcomes.Count == 0)
			{
				continue;
			}
			symbols[num17].decidedOutcome = symbols[num17].possibleOutcomes[UnityEngine.Random.Range(0, symbols[num17].possibleOutcomes.Count)];
			for (int num18 = 0; num18 < 10; num18++)
			{
				if (symbols[num17].AnyConnectedSpawnersActiveUnderCurrentRoll())
				{
					break;
				}
				if (symbols[num17].possibleOutcomes.Count == 0)
				{
					break;
				}
				symbols[num17].decidedOutcome = symbols[num17].possibleOutcomes[UnityEngine.Random.Range(0, symbols[num17].possibleOutcomes.Count)];
			}
		}
		for (int num19 = 0; num19 < list.Count; num19++)
		{
			if (list[num19].group != null && !list[num19].group.spawn)
			{
				list[num19].Disable();
			}
			else if (list[num19].symbol != null && list[num19].symbol.decidedOutcome != list[num19].symbolString.Substring(1, 1))
			{
				list[num19].Disable();
			}
			else if (list[num19].invSymbol != null && list[num19].invSymbol.decidedOutcome == list[num19].invSymbolString.Substring(1, 1))
			{
				list[num19].Disable();
			}
		}
		if (num2 > -1f)
		{
			num8 = Mathf.Lerp(num8, num2, 0.5f);
		}
		num8 *= num * Mathf.Lerp(0.8f, 1.2f, UnityEngine.Random.value);
		num8 = ((!(num2 > -1f)) ? (num8 + Mathf.Lerp(-1.2f, 1.2f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(num8, 2f, 10f, 0.25f, 3f)))) : (num8 + Mathf.Lerp(-1.2f, 1.2f, Mathf.Pow(UnityEngine.Random.value, Custom.LerpMap(num8, num2 - 2f, num2 + 2f, 0.25f, 2f)))));
		int num20 = 0;
		if (wildLifeSetting.Index != -1)
		{
			num20 = Custom.IntClamp(Mathf.RoundToInt(num8), wildLifeSetting.Index, 25);
		}
		List<CritterSpawnData> list6 = new List<CritterSpawnData>();
		for (int num21 = 0; num21 < num20; num21++)
		{
			CritterSpawnData critterSpawnData2 = WeightedRandom(spawnList);
			if (critterSpawnData2 != null)
			{
				critterSpawnData2.Disable();
				list6.Add(critterSpawnData2);
				if (num14 > 0f)
				{
					Diversify(ref spawnList, critterSpawnData2.type, num14);
				}
			}
		}
		int[] array6 = new int[abstractRoom.nodes.Length];
		for (int num22 = 0; num22 < list6.Count; num22++)
		{
			CritterSpawnData critterSpawnData3 = list6[num22];
			if (ModManager.MSC && game.GetArenaGameSession.chMeta != null && IsMajorCreature(critterSpawnData3.type))
			{
				num9++;
				if (num9 > 1 && !game.GetArenaGameSession.chMeta.unlimitedDanger)
				{
					continue;
				}
			}
			if (critterSpawnData3.dens.Count < 1 || critterSpawnData3.dens[0] == -1)
			{
				AbstractCreature abstractCreature = CreateAbstractCreature(game.world, critterSpawnData3.type, new WorldCoordinate(game.world.offScreenDen.index, -1, -1, 0), ref availableCreatures);
				if (abstractCreature != null)
				{
					game.world.offScreenDen.AddEntity(abstractCreature);
				}
				continue;
			}
			int num23 = int.MaxValue;
			for (int num24 = 0; num24 < critterSpawnData3.dens.Count; num24++)
			{
				num23 = Math.Min(num23, array6[critterSpawnData3.dens[num24]]);
			}
			List<int> list7 = new List<int>();
			for (int num25 = 0; num25 < critterSpawnData3.dens.Count; num25++)
			{
				if (array6[critterSpawnData3.dens[num25]] <= num23)
				{
					list7.Add(critterSpawnData3.dens[num25]);
				}
			}
			if (list7.Count == 0)
			{
				continue;
			}
			int num26 = list7[UnityEngine.Random.Range(0, list7.Count)];
			array6[num26]++;
			if (StaticWorld.GetCreatureTemplate(critterSpawnData3.type).quantified)
			{
				abstractRoom.AddQuantifiedCreature(num26, critterSpawnData3.type, UnityEngine.Random.Range(7, 11));
				continue;
			}
			AbstractCreature abstractCreature2 = CreateAbstractCreature(game.world, critterSpawnData3.type, new WorldCoordinate(abstractRoom.index, -1, -1, num26), ref availableCreatures);
			if (abstractCreature2 == null)
			{
				continue;
			}
			abstractRoom.MoveEntityToDen(abstractCreature2);
			if (abstractCreature2.creatureTemplate.type == CreatureTemplate.Type.BigNeedleWorm)
			{
				for (int num27 = UnityEngine.Random.Range(0, 4); num27 >= 0; num27--)
				{
					AbstractCreature ent = new AbstractCreature(game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm), null, new WorldCoordinate(abstractRoom.index, -1, -1, num26), game.GetNewID());
					abstractRoom.MoveEntityToDen(ent);
				}
			}
		}
	}

	private static CritterSpawnData WeightedRandom(List<CritterSpawnData> inputList)
	{
		float num = 0f;
		for (int i = 0; i < inputList.Count; i++)
		{
			num += inputList[i].spawnChance;
		}
		float num2 = UnityEngine.Random.value * num;
		for (int j = 0; j < inputList.Count; j++)
		{
			num2 -= inputList[j].spawnChance;
			if (num2 < 0f)
			{
				return inputList[j];
			}
		}
		return null;
	}

	private static void Diversify(ref List<CritterSpawnData> spawnList, CreatureTemplate.Type type, float diversify)
	{
		for (int i = 0; i < spawnList.Count; i++)
		{
			if (spawnList[i].type != type)
			{
				spawnList[i].spawnChance *= 1f + diversify * 10f;
			}
		}
	}

	private static AbstractCreature CreateAbstractCreature(World world, CreatureTemplate.Type critType, WorldCoordinate pos, ref List<AbstractCreature> availableCreatures)
	{
		for (int num = availableCreatures.Count - 1; num >= 0; num--)
		{
			if (availableCreatures[num].creatureTemplate.type == critType)
			{
				AbstractCreature abstractCreature = availableCreatures[num];
				availableCreatures.RemoveAt(num);
				for (int i = 0; i < 2; i++)
				{
					abstractCreature.state.CycleTick();
				}
				abstractCreature.pos = pos;
				string creatureString = SaveState.AbstractCreatureToStringSingleRoomWorld(abstractCreature);
				return SaveState.AbstractCreatureFromString(world, creatureString, onlyInCurrentRegion: false);
			}
		}
		return new AbstractCreature(world, StaticWorld.GetCreatureTemplate(critType), null, pos, world.game.GetNewID());
	}

	private static void AddToSymbol(string symbolString, bool inv, ref List<SpawnSymbol> symbols)
	{
		for (int i = 0; i < symbols.Count; i++)
		{
			if (!(symbols[i].name == symbolString.Substring(0, 1)))
			{
				continue;
			}
			for (int j = 0; j < symbols[i].possibleOutcomes.Count; j++)
			{
				if (symbols[i].possibleOutcomes[j] == symbolString.Substring(1, 1))
				{
					return;
				}
			}
			symbols[i].possibleOutcomes.Add(symbolString.Substring(1, 1));
			return;
		}
		symbols.Add(new SpawnSymbol(symbolString.Substring(0, 1), symbolString.Substring(1, 1), inv));
	}

	public static bool IsMajorCreature(CreatureTemplate.Type type)
	{
		if (!(type == CreatureTemplate.Type.BigEel) && !(type == MoreSlugcatsEnums.CreatureTemplateType.BigJelly) && !(type == CreatureTemplate.Type.BigEel) && !(type == CreatureTemplate.Type.BrotherLongLegs) && !(type == CreatureTemplate.Type.DaddyLongLegs) && !(type == CreatureTemplate.Type.Deer) && !(type == MoreSlugcatsEnums.CreatureTemplateType.Inspector) && !(type == CreatureTemplate.Type.Vulture) && !(type == CreatureTemplate.Type.KingVulture) && !(type == CreatureTemplate.Type.MirosBird) && !(type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture) && !(type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs) && !(type == CreatureTemplate.Type.RedCentipede) && !(type == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti))
		{
			return type == CreatureTemplate.Type.RedLizard;
		}
		return true;
	}
}
