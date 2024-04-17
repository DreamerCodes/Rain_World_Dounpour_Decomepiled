using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RegionState
{
	public class ConsumedItem
	{
		public int originRoom;

		public int placedObjectIndex;

		public int waitCycles;

		public string unknownOriginRoom;

		public bool Valid => unknownOriginRoom == null;

		public ConsumedItem(int originRoom, int placedObjectIndex, int waitCycles)
		{
			this.originRoom = originRoom;
			this.placedObjectIndex = placedObjectIndex;
			this.waitCycles = waitCycles;
		}

		public ConsumedItem(string unknownRoom, int placedObjectIndex, int waitCycles)
		{
			originRoom = -1;
			unknownOriginRoom = unknownRoom;
			this.placedObjectIndex = placedObjectIndex;
			this.waitCycles = waitCycles;
		}

		public string ResolveOriginRoom()
		{
			if (Valid)
			{
				if (RainWorld.roomIndexToName.ContainsKey(originRoom))
				{
					return RainWorld.roomIndexToName[originRoom];
				}
				return null;
			}
			return unknownOriginRoom;
		}

		public override string ToString()
		{
			if (!Valid)
			{
				return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", "INV", unknownOriginRoom, placedObjectIndex, waitCycles);
			}
			string text = ResolveOriginRoom();
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}", (text != null) ? text : originRoom.ToString(), placedObjectIndex, waitCycles);
		}

		public void FromString(string s)
		{
			string[] array = s.Split('.');
			int num = 0;
			if (array.Length == 4 && array[0] == "INV")
			{
				num = 1;
			}
			string roomString = array[num];
			int? num2 = BackwardsCompatibilityRemix.ParseRoomIndex(roomString);
			if (num2.HasValue)
			{
				originRoom = num2.Value;
				unknownOriginRoom = null;
			}
			else
			{
				originRoom = -1;
				unknownOriginRoom = roomString;
			}
			placedObjectIndex = int.Parse(array[1 + num], NumberStyles.Any, CultureInfo.InvariantCulture);
			waitCycles = int.Parse(array[2 + num], NumberStyles.Any, CultureInfo.InvariantCulture);
		}
	}

	public SaveState saveState;

	public World world;

	public string regionName;

	private int lastCycleUpdated;

	public Dictionary<string, int> swarmRoomCounters;

	public Dictionary<string, int> lineageCounters;

	public bool[] gatesPassedThrough;

	public List<string> candidatesForDepleteSwarmRooms;

	public List<string> savedObjects;

	public List<string> unrecognizedSavedObjects;

	public List<string> savedPopulation;

	public List<string> unrecognizedPopulation;

	public List<string> savedSticks;

	public List<ConsumedItem> consumedItems;

	public List<string> roomsVisited;

	public List<string> unrecognizedSaveStrings;

	private List<AbstractCreature> loadedCreatures;

	public bool SwarmRoomActive(int swarmRoomIndex)
	{
		if (world == null)
		{
			return false;
		}
		string name = world.GetAbstractRoom(world.swarmRooms[swarmRoomIndex]).name;
		return SwarmRoomActive(name);
	}

	public bool SwarmRoomActive(string swarmRoomName)
	{
		if (!swarmRoomCounters.ContainsKey(swarmRoomName))
		{
			return false;
		}
		return swarmRoomCounters[swarmRoomName] == 0;
	}

	public RegionState(SaveState saveState, World world)
	{
		this.saveState = saveState;
		this.world = world;
		regionName = world.name;
		swarmRoomCounters = new Dictionary<string, int>();
		lineageCounters = new Dictionary<string, int>();
		gatesPassedThrough = new bool[world.gates.Length];
		candidatesForDepleteSwarmRooms = new List<string>();
		savedObjects = new List<string>();
		unrecognizedSavedObjects = new List<string>();
		savedPopulation = new List<string>();
		unrecognizedPopulation = new List<string>();
		savedSticks = new List<string>();
		consumedItems = new List<ConsumedItem>();
		roomsVisited = new List<string>();
		unrecognizedSaveStrings = new List<string>();
		for (int i = 0; i < world.swarmRooms.Length; i++)
		{
			swarmRoomCounters[world.GetAbstractRoom(world.swarmRooms[i]).name] = 0;
		}
		if (saveState.regionLoadStrings[world.region.regionNumber] != null)
		{
			string[] array = Regex.Split(saveState.regionLoadStrings[world.region.regionNumber], "<rgA>");
			List<string[]> list = new List<string[]>();
			for (int j = 0; j < array.Length; j++)
			{
				string[] array2 = Regex.Split(array[j], "<rgB>");
				if (array2.Length >= 2)
				{
					list.Add(array2);
				}
			}
			for (int k = 0; k < list.Count; k++)
			{
				switch (list[k][0])
				{
				case "LASTCYCLEUPDATED":
					lastCycleUpdated = int.Parse(list[k][1], NumberStyles.Any, CultureInfo.InvariantCulture);
					break;
				case "SWARMROOMS":
				{
					if (list[k][1].Contains("."))
					{
						BackwardsCompatibilityRemix.ParseSwarmRooms(regionName, list[k][1], swarmRoomCounters);
					}
					else
					{
						SaveUtils.LoadStringIntDict(list[k][1], swarmRoomCounters);
					}
					for (int num = 0; num < world.swarmRooms.Length; num++)
					{
						string name = world.GetAbstractRoom(world.swarmRooms[num]).name;
						if (!swarmRoomCounters.ContainsKey(name))
						{
							swarmRoomCounters[name] = 0;
						}
					}
					break;
				}
				case "LINEAGES":
					if (list[k][1].Contains(":"))
					{
						SaveUtils.LoadStringIntDict(list[k][1], lineageCounters);
					}
					else
					{
						BackwardsCompatibilityRemix.ParseLineages(list[k][1], regionName, saveState.saveStateNumber, saveState.worldVersion, lineageCounters);
					}
					break;
				case "OBJECTS":
				{
					array = Regex.Split(list[k][1], "<rgC>");
					for (int num2 = 0; num2 < array.Length; num2++)
					{
						if (array[num2] != "")
						{
							savedObjects.Add(array[num2]);
						}
					}
					break;
				}
				case "POPULATION":
				{
					array = Regex.Split(list[k][1], "<rgC>");
					for (int n = 0; n < array.Length; n++)
					{
						if (!(array[n] != ""))
						{
							continue;
						}
						string[] array4 = Regex.Split(array[n], "<cA>");
						if (array4[0] != string.Empty)
						{
							if (new CreatureTemplate.Type(array4[0]).Index >= 0)
							{
								savedPopulation.Add(array[n]);
							}
							else
							{
								unrecognizedPopulation.Add(array[n]);
							}
						}
					}
					break;
				}
				case "STICKS":
				{
					array = Regex.Split(list[k][1], "<rgC>");
					for (int l = 0; l < array.Length; l++)
					{
						if (array[l] != "")
						{
							savedSticks.Add(array[l]);
						}
					}
					break;
				}
				case "CONSUMEDITEMS":
				{
					array = Regex.Split(list[k][1], "<rgC>");
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						if (array[num3].Length > 0 && !(array[num3] == string.Empty))
						{
							ConsumedItem consumedItem = new ConsumedItem(0, 0, 0);
							consumedItem.FromString(array[num3]);
							consumedItems.Add(consumedItem);
						}
					}
					break;
				}
				case "ROOMSVISITED":
				{
					if (Custom.IsDigitString(list[k][1]))
					{
						BackwardsCompatibilityRemix.ParseRoomsVisited(saveState.worldVersion, regionName, list[k][1], roomsVisited);
						break;
					}
					string[] array3 = list[k][1].Split(',');
					roomsVisited = new List<string>();
					for (int m = 0; m < array3.Length; m++)
					{
						if (array3[m] != string.Empty)
						{
							roomsVisited.Add(array3[m]);
						}
					}
					break;
				}
				default:
					if (list[k][0].Trim().Length > 0 && list[k].Length >= 2)
					{
						unrecognizedSaveStrings.Add(list[k][0] + "<rgB>" + list[k][1]);
					}
					break;
				}
			}
		}
		if (swarmRoomCounters.Count <= 0)
		{
			return;
		}
		int num4 = 0;
		foreach (KeyValuePair<string, int> swarmRoomCounter in swarmRoomCounters)
		{
			if (SwarmRoomActive(swarmRoomCounter.Key))
			{
				num4++;
			}
		}
		if (num4 < 1)
		{
			swarmRoomCounters[swarmRoomCounters.ElementAt(UnityEngine.Random.Range(0, swarmRoomCounters.Count)).Key] = 0;
		}
	}

	private void RainCycleTick(int ticks, int foodRepBonus)
	{
		Custom.Log("Region", regionName, "ticking forward", ticks.ToString(), "cycles. (+", foodRepBonus.ToString(), "bonus cycles for bats and consumables)");
		if (ticks > 0)
		{
			foreach (string item in swarmRoomCounters.Keys.ToList())
			{
				if (swarmRoomCounters[item] > 0)
				{
					swarmRoomCounters[item] = Math.Max(0, swarmRoomCounters[item] - ticks - foodRepBonus);
				}
			}
			for (int i = 0; i < world.NumberOfRooms; i++)
			{
				AbstractRoom abstractRoom = world.GetAbstractRoom(world.firstRoomIndex + i);
				for (int j = 0; j < abstractRoom.entities.Count; j++)
				{
					if (abstractRoom.entities[j] is AbstractSpear && (abstractRoom.entities[j] as AbstractSpear).stuckInWall)
					{
						(abstractRoom.entities[j] as AbstractSpear).StuckInWallTick(ticks);
					}
					else if (abstractRoom.entities[j] is AbstractCreature)
					{
						(abstractRoom.entities[j] as AbstractCreature).state.CycleTick();
					}
				}
				for (int k = 0; k < abstractRoom.entitiesInDens.Count; k++)
				{
					if (abstractRoom.entitiesInDens[k] is AbstractCreature)
					{
						(abstractRoom.entitiesInDens[k] as AbstractCreature).state.CycleTick();
					}
				}
			}
			for (int num = consumedItems.Count - 1; num >= 0; num--)
			{
				if (consumedItems[num].waitCycles > -1)
				{
					consumedItems[num].waitCycles -= ticks + foodRepBonus;
					if (consumedItems[num].waitCycles < 1)
					{
						consumedItems.RemoveAt(num);
					}
				}
			}
		}
		lastCycleUpdated = saveState.cycleNumber;
	}

	public void AdaptWorldToRegionState()
	{
		Custom.Log("Adapt world to region state", regionName);
		unrecognizedSavedObjects.Clear();
		List<string> list = new List<string>();
		for (int i = 0; i < savedObjects.Count; i++)
		{
			AbstractPhysicalObject abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(world, savedObjects[i]);
			bool flag = true;
			if (abstractPhysicalObject == null)
			{
				Custom.LogWarning("obj load error:", savedObjects[i]);
				unrecognizedSavedObjects.Add(savedObjects[i]);
				flag = false;
			}
			else if (!abstractPhysicalObject.pos.Valid)
			{
				Custom.LogWarning("trying to respawn object in non-existant room", abstractPhysicalObject.pos.unknownName);
				unrecognizedSavedObjects.Add(savedObjects[i]);
				flag = false;
			}
			else if (abstractPhysicalObject.type.Index == -1)
			{
				Custom.LogWarning("trying to respawn object of non-existant type", abstractPhysicalObject.type.value);
				unrecognizedSavedObjects.Add(savedObjects[i]);
				flag = false;
			}
			else if (abstractPhysicalObject.pos.room >= world.firstRoomIndex && abstractPhysicalObject.pos.room < world.firstRoomIndex + world.NumberOfRooms)
			{
				world.GetAbstractRoom(abstractPhysicalObject.pos).AddEntity(abstractPhysicalObject);
				if (ModManager.MMF && MMF.cfgKeyItemTracking.Value)
				{
					for (int j = 0; j < world.game.GetStorySession.saveState.objectTrackers.Count; j++)
					{
						world.game.GetStorySession.saveState.objectTrackers[j].LinkObjectToTracker(abstractPhysicalObject);
					}
				}
			}
			else
			{
				Custom.LogWarning($"trying to respawn object in room outside of world. {abstractPhysicalObject.type} {abstractPhysicalObject.pos.room}({world.firstRoomIndex}-{world.firstRoomIndex + world.NumberOfRooms})");
				unrecognizedSavedObjects.Add(savedObjects[i]);
				flag = false;
			}
			if (flag)
			{
				list.Add(savedObjects[i]);
			}
		}
		savedObjects = list;
		if (ModManager.MMF && MMF.cfgKeyItemTracking.Value)
		{
			for (int k = 0; k < world.game.GetStorySession.saveState.objectTrackers.Count; k++)
			{
				PersistentObjectTracker persistentObjectTracker = world.game.GetStorySession.saveState.objectTrackers[k];
				if (!(persistentObjectTracker.lastSeenRegion == world.name))
				{
					continue;
				}
				persistentObjectTracker.AbstractizeRepresentation(world);
				if (persistentObjectTracker.obj == null)
				{
					continue;
				}
				AbstractRoom abstractRoom = world.GetAbstractRoom(persistentObjectTracker.desiredSpawnLocation);
				if (abstractRoom == null)
				{
					Custom.LogWarning("ERROR! This tracked object's room doesn't exist any more?? Spawning in shelter as a fallback.");
					abstractRoom = world.GetAbstractRoom(world.game.GetStorySession.saveState.denPosition);
					persistentObjectTracker.ChangeDesiredSpawnLocation(abstractRoom.RandomNodeInRoom());
					persistentObjectTracker.obj.pos = persistentObjectTracker.desiredSpawnLocation;
				}
				if (abstractRoom.shelter)
				{
					bool flag2 = false;
					for (int l = 0; l < savedObjects.Count; l++)
					{
						AbstractPhysicalObject abstractPhysicalObject2 = SaveState.AbstractPhysicalObjectFromString(world, savedObjects[l]);
						if (abstractPhysicalObject2 != null && abstractPhysicalObject2.pos.room == abstractRoom.index && persistentObjectTracker.CompatibleWithTracker(abstractPhysicalObject2))
						{
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						continue;
					}
				}
				bool flag3 = abstractRoom.shelter || abstractRoom.name == "SS_AI" || abstractRoom.name == "RM_AI" || abstractRoom.name == "DM_AI" || abstractRoom.name == "SL_AI" || abstractRoom.name == "SS_D07";
				if (MMF.cfgKeyItemTracking.Value || flag3)
				{
					persistentObjectTracker.obj.pos = persistentObjectTracker.desiredSpawnLocation;
					int abstractNode = persistentObjectTracker.desiredSpawnLocation.abstractNode;
					persistentObjectTracker.desiredSpawnLocation.abstractNode = persistentObjectTracker.obj.pos.abstractNode + 1;
					persistentObjectTracker.obj.Move(persistentObjectTracker.desiredSpawnLocation);
					persistentObjectTracker.desiredSpawnLocation.abstractNode = abstractNode;
					abstractRoom.AddEntity(persistentObjectTracker.obj);
				}
				else if (!abstractRoom.shelter)
				{
					persistentObjectTracker.obj.Destroy();
				}
			}
		}
		if (saveState.deathPersistentSaveData.karmaFlowerPosition.HasValue && saveState.deathPersistentSaveData.karmaFlowerPosition.Value.Valid && world.IsRoomInRegion(saveState.deathPersistentSaveData.karmaFlowerPosition.Value.room))
		{
			world.GetAbstractRoom(saveState.deathPersistentSaveData.karmaFlowerPosition.Value).AddEntity(new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, saveState.deathPersistentSaveData.karmaFlowerPosition.Value, world.game.GetNewID(), -2, -2, null));
			(world.game.session as StoryGameSession).karmaFlowerMapPos = saveState.deathPersistentSaveData.karmaFlowerPosition;
			if (saveState.saveStateNumber != SlugcatStats.Name.Yellow)
			{
				saveState.deathPersistentSaveData.karmaFlowerPosition = null;
			}
		}
		if (saveState.saveStateNumber != SlugcatStats.Name.Red && world.game.rainWorld.progression.miscProgressionData.redsFlower.HasValue && world.IsRoomInRegion(world.game.rainWorld.progression.miscProgressionData.redsFlower.Value.room))
		{
			if (saveState.saveStateNumber == SlugcatStats.Name.White || saveState.saveStateNumber == SlugcatStats.Name.Yellow || (ModManager.MSC && (saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel)))
			{
				world.GetAbstractRoom(world.game.rainWorld.progression.miscProgressionData.redsFlower.Value).AddEntity(new AbstractConsumable(world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, world.game.rainWorld.progression.miscProgressionData.redsFlower.Value, world.game.GetNewID(), -1, -1, null));
			}
			if (ModManager.MSC && (saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand || saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel))
			{
				world.GetAbstractRoom(world.game.rainWorld.progression.miscProgressionData.redsFlower.Value).AddEntity(new AbstractCreature(world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy), null, world.game.rainWorld.progression.miscProgressionData.redsFlower.Value, world.game.GetNewID()));
			}
		}
		loadedCreatures = new List<AbstractCreature>();
		List<string> list2 = new List<string>();
		for (int m = 0; m < savedPopulation.Count; m++)
		{
			AbstractCreature abstractCreature = SaveState.AbstractCreatureFromString(world, savedPopulation[m], onlyInCurrentRegion: true);
			if (abstractCreature != null)
			{
				WorldCoordinate pos = abstractCreature.pos;
				if (world.GetAbstractRoom(pos).shelter)
				{
					world.GetAbstractRoom(pos).AddEntity(abstractCreature);
				}
				else
				{
					world.GetAbstractRoom(pos).MoveEntityToDen(abstractCreature);
				}
				loadedCreatures.Add(abstractCreature);
			}
			else
			{
				string[] array = Regex.Split(savedPopulation[m], "<cA>");
				if (array[0] != string.Empty && new CreatureTemplate.Type(array[0]).Index == -1 && !unrecognizedPopulation.Contains(savedPopulation[m]))
				{
					unrecognizedPopulation.Add(savedPopulation[m]);
					list2.Add(savedPopulation[m]);
				}
			}
		}
		for (int n = 0; n < list2.Count; n++)
		{
			savedPopulation.Remove(list2[n]);
		}
		for (int num = 0; num < savedSticks.Count; num++)
		{
			string[] array2 = Regex.Split(savedSticks[num], "<stkA>");
			if (!(array2[0] == string.Empty))
			{
				AbstractRoom abstractRoom2 = world.GetAbstractRoom(int.Parse(array2[0], NumberStyles.Any, CultureInfo.InvariantCulture));
				if (abstractRoom2 != null)
				{
					AbstractPhysicalObject.AbstractObjectStick.FromString(array2, abstractRoom2);
				}
			}
		}
		string text = "";
		if (saveState.denPosition != null && saveState.denPosition.Contains("_"))
		{
			text = Regex.Split(saveState.denPosition, "_")[0];
		}
		if (text.ToLowerInvariant() == world.name.ToLowerInvariant())
		{
			for (int num2 = saveState.unrecognizedSwallowedItems.Count - 1; num2 >= 0; num2--)
			{
				AbstractPhysicalObject abstractPhysicalObject3 = null;
				bool flag4 = false;
				string text2 = saveState.unrecognizedSwallowedItems[num2];
				if (text2.Contains("<oA>"))
				{
					abstractPhysicalObject3 = SaveState.AbstractPhysicalObjectFromString(world, text2);
					if (abstractPhysicalObject3 != null && abstractPhysicalObject3.type.Index != -1)
					{
						flag4 = true;
					}
				}
				else if (text2.Contains("<cA>"))
				{
					abstractPhysicalObject3 = SaveState.AbstractCreatureFromString(world, text2, onlyInCurrentRegion: false);
					if (abstractPhysicalObject3 != null)
					{
						flag4 = true;
					}
				}
				if (flag4)
				{
					AbstractRoom abstractRoom3 = world.GetAbstractRoom(saveState.denPosition);
					if (abstractPhysicalObject3 != null && abstractRoom3 != null)
					{
						abstractPhysicalObject3.pos = new WorldCoordinate(abstractRoom3.index, -1, -1, -1);
						abstractRoom3.AddEntity(abstractPhysicalObject3);
						saveState.unrecognizedSwallowedItems.RemoveAt(num2);
					}
				}
			}
		}
		RainCycleTick(saveState.cycleNumber - lastCycleUpdated, saveState.deathPersistentSaveData.foodReplenishBonus);
	}

	public void AdaptRegionStateToWorld(int playerShelter, int activeGate)
	{
		Custom.Log("Adapt region state to world", regionName);
		savedObjects.Clear();
		for (int i = 0; i < unrecognizedSavedObjects.Count; i++)
		{
			savedObjects.Add(unrecognizedSavedObjects[i]);
		}
		unrecognizedSavedObjects.Clear();
		savedPopulation.Clear();
		for (int j = 0; j < unrecognizedPopulation.Count; j++)
		{
			savedPopulation.Add(unrecognizedPopulation[j]);
		}
		unrecognizedPopulation.Clear();
		saveState.pendingObjects.Clear();
		savedSticks.Clear();
		List<AbstractPhysicalObject> list = new List<AbstractPhysicalObject>();
		for (int k = 0; k < world.NumberOfRooms; k++)
		{
			AbstractRoom abstractRoom = world.GetAbstractRoom(world.firstRoomIndex + k);
			for (int l = 0; l < abstractRoom.entities.Count; l++)
			{
				if (!(abstractRoom.entities[l] is AbstractPhysicalObject) || !((abstractRoom.entities[l] as AbstractPhysicalObject).type != AbstractPhysicalObject.AbstractObjectType.KarmaFlower))
				{
					continue;
				}
				if (abstractRoom.entities[l] is AbstractSpear && (abstractRoom.entities[l] as AbstractSpear).stuckInWall)
				{
					savedObjects.Add(abstractRoom.entities[l].ToString());
					continue;
				}
				if (ModManager.MMF && ((abstractRoom.shelter && abstractRoom.index == playerShelter) || abstractRoom.name == SaveState.forcedEndRoomToAllowwSave) && (abstractRoom.entities[l] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.Creature)
				{
					AbstractCreature abstractCreature = (abstractRoom.entities[l] as AbstractPhysicalObject) as AbstractCreature;
					float num = -1f;
					foreach (AbstractCreature player in world.game.Players)
					{
						if (abstractCreature != null && abstractCreature.state != null && abstractCreature.state.socialMemory != null)
						{
							num = Mathf.Max(num, abstractCreature.state.socialMemory.GetLike(player.ID));
						}
					}
					if (ModManager.MSC && abstractCreature.creatureTemplate.TopAncestor().type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
					{
						Custom.Log($"Add pup to pendingFriendSpawns {abstractCreature}");
						saveState.pendingFriendCreatures.Add(SaveState.AbstractCreatureToStringStoryWorld(abstractCreature));
						list.Add(abstractCreature);
						abstractCreature.LoseAllStuckObjects();
						abstractCreature.saveCreature = false;
					}
					else if (abstractCreature.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && abstractCreature.state.alive && num > 0.5f)
					{
						Custom.Log($"Add lizard to pendingFriendSpawns {abstractCreature}");
						saveState.pendingFriendCreatures.Add(SaveState.AbstractCreatureToStringStoryWorld(abstractCreature));
						list.Add(abstractCreature);
						abstractCreature.LoseAllStuckObjects();
						abstractCreature.saveCreature = false;
					}
					else
					{
						Custom.Log($"Ignoring unfriendable creature {abstractCreature}");
					}
				}
				if (abstractRoom.shelter && (abstractRoom.entities[l] as AbstractPhysicalObject).type == AbstractPhysicalObject.AbstractObjectType.NeedleEgg)
				{
					savedPopulation.Add(AddHatchedNeedleFly((abstractRoom.entities[l] as AbstractPhysicalObject).pos));
					savedPopulation.Add(AddHatchedNeedleFly((abstractRoom.entities[l] as AbstractPhysicalObject).pos));
				}
				else if (abstractRoom.shelter && !(abstractRoom.entities[l] is AbstractCreature) && (abstractRoom.entities[l] as AbstractPhysicalObject).type != AbstractPhysicalObject.AbstractObjectType.Creature)
				{
					Custom.Log($"Attempting to add {abstractRoom.entities[l].GetType()} object to pendingObjects");
					if (ModManager.MMF && MMF.cfgKeyItemPassaging.Value && abstractRoom.index == playerShelter)
					{
						if ((abstractRoom.entities[l] as AbstractPhysicalObject).tracker != null)
						{
							Custom.Log($"Adding {abstractRoom.entities[l].GetType()} object to pendingObjects");
							saveState.pendingObjects.Add(abstractRoom.entities[l].ToString());
							list.Add(abstractRoom.entities[l] as AbstractPhysicalObject);
						}
						else
						{
							Custom.Log($"Adding {abstractRoom.entities[l].GetType()} object to savedObjects instead of pending");
							savedObjects.Add(abstractRoom.entities[l].ToString());
						}
					}
					else
					{
						Custom.Log($"Adding {abstractRoom.entities[l].GetType()} object to savedObjects");
						savedObjects.Add(abstractRoom.entities[l].ToString());
					}
				}
				if (!abstractRoom.shelter && !(abstractRoom.name == SaveState.forcedEndRoomToAllowwSave))
				{
					continue;
				}
				for (int m = 0; m < (abstractRoom.entities[l] as AbstractPhysicalObject).stuckObjects.Count; m++)
				{
					if ((abstractRoom.entities[l] as AbstractPhysicalObject).stuckObjects[m].A == abstractRoom.entities[l])
					{
						savedSticks.Add((abstractRoom.entities[l] as AbstractPhysicalObject).stuckObjects[m].SaveToString(abstractRoom.index));
					}
				}
			}
			for (int n = 0; n < 2; n++)
			{
				int num2 = ((n == 0) ? abstractRoom.creatures.Count : abstractRoom.entitiesInDens.Count);
				for (int num3 = 0; num3 < num2; num3++)
				{
					AbstractWorldEntity abstractWorldEntity = ((n == 0) ? abstractRoom.creatures[num3] : abstractRoom.entitiesInDens[num3]);
					if (!(abstractWorldEntity is AbstractCreature) || (abstractWorldEntity as AbstractCreature).creatureTemplate.quantified || !(abstractWorldEntity as AbstractCreature).creatureTemplate.saveCreature || !(abstractWorldEntity as AbstractCreature).saveCreature)
					{
						continue;
					}
					string text = CreatureToStringInDenPos(abstractWorldEntity as AbstractCreature, playerShelter, activeGate);
					if (!(text != ""))
					{
						continue;
					}
					savedPopulation.Add(text);
					for (int num4 = 0; num4 < loadedCreatures.Count; num4++)
					{
						if (loadedCreatures[num4] == abstractWorldEntity as AbstractCreature)
						{
							loadedCreatures.RemoveAt(num4);
							break;
						}
					}
				}
			}
		}
		if (ModManager.MMF)
		{
			if (SaveState.forcedEndRoomToAllowwSave != null && SaveState.forcedEndRoomToAllowwSave != "" && saveState.playerGrasps != null)
			{
				for (int num5 = 0; num5 < saveState.playerGrasps.Length; num5++)
				{
					if (saveState.playerGrasps[num5].Contains("<oA>"))
					{
						AbstractPhysicalObject abstractPhysicalObject = SaveState.AbstractPhysicalObjectFromString(null, saveState.playerGrasps[num5]);
						bool flag = false;
						for (int num6 = 0; num6 < list.Count; num6++)
						{
							if (list[num6].ID == abstractPhysicalObject.ID)
							{
								flag = true;
								break;
							}
						}
						if (flag || (ModManager.MSC && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && abstractPhysicalObject is VultureMask.AbstractVultureMask && (abstractPhysicalObject as VultureMask.AbstractVultureMask).scavKing))
						{
							continue;
						}
					}
					if (saveState.playerGrasps[num5].Contains("<cA>"))
					{
						AbstractCreature abstractCreature2 = SaveState.AbstractCreatureFromString(null, saveState.playerGrasps[num5], onlyInCurrentRegion: false);
						bool flag2 = false;
						for (int num7 = 0; num7 < list.Count; num7++)
						{
							if (list[num7].ID == abstractCreature2.ID)
							{
								flag2 = true;
								break;
							}
						}
						if (flag2 || (ModManager.MSC && saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Artificer && abstractCreature2.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerKing))
						{
							continue;
						}
					}
					saveState.pendingObjects.Add(saveState.playerGrasps[num5]);
				}
			}
			for (int num8 = saveState.unrecognizedPlayerGrasps.Count - 1; num8 >= 0; num8--)
			{
				AbstractPhysicalObject abstractPhysicalObject2 = null;
				bool flag3 = false;
				string text2 = saveState.unrecognizedPlayerGrasps[num8];
				if (text2.Contains("<oA>"))
				{
					abstractPhysicalObject2 = SaveState.AbstractPhysicalObjectFromString(world, text2);
					if (abstractPhysicalObject2 != null && abstractPhysicalObject2.type.Index != -1)
					{
						flag3 = true;
					}
				}
				else if (text2.Contains("<cA>"))
				{
					abstractPhysicalObject2 = SaveState.AbstractCreatureFromString(world, text2, onlyInCurrentRegion: false);
					if (abstractPhysicalObject2 != null)
					{
						flag3 = true;
					}
				}
				if (flag3)
				{
					saveState.pendingObjects.Add(abstractPhysicalObject2.ToString());
					saveState.unrecognizedPlayerGrasps.RemoveAt(num8);
				}
			}
		}
		for (int num9 = 0; num9 < loadedCreatures.Count; num9++)
		{
			Custom.Log("Creature which was loaded but not saved");
			Custom.Log($"-- {loadedCreatures[num9].creatureTemplate.name} {loadedCreatures[num9].ID}");
			Custom.Log("-- dead:", loadedCreatures[num9].state.dead.ToString());
			if (!loadedCreatures[num9].state.dead)
			{
				continue;
			}
			bool flag4 = false;
			for (int num10 = 0; num10 < saveState.respawnCreatures.Count; num10++)
			{
				if (flag4)
				{
					break;
				}
				if (loadedCreatures[num9].ID.spawner == saveState.respawnCreatures[num10])
				{
					flag4 = true;
				}
			}
			for (int num11 = 0; num11 < saveState.waitRespawnCreatures.Count; num11++)
			{
				if (flag4)
				{
					break;
				}
				if (loadedCreatures[num9].ID.spawner == saveState.waitRespawnCreatures[num11])
				{
					flag4 = true;
				}
			}
			Custom.Log(flag4 ? "-- is put up for respawn." : "-- is NOT put up for respawn!");
			if (!flag4)
			{
				Custom.Log("Added for respawn");
				saveState.respawnCreatures.Add(loadedCreatures[num9].ID.spawner);
			}
		}
	}

	private string AddHatchedNeedleFly(WorldCoordinate pos)
	{
		Custom.Log("saving noodlefly in shelter");
		AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.SmallNeedleWorm), null, pos, world.game.GetNewID());
		(abstractCreature.state as NeedleWormAbstractAI.NeedleWormState).eggSpawn = true;
		return SaveState.AbstractCreatureToStringStoryWorld(abstractCreature);
	}

	public string SaveToString()
	{
		string text = string.Format(CultureInfo.InvariantCulture, "REGIONNAME<rgB>{0}<rgA>LASTCYCLEUPDATED<rgB>{1}<rgA>", regionName, lastCycleUpdated);
		text += "SWARMROOMS<rgB>";
		bool flag = true;
		foreach (KeyValuePair<string, int> swarmRoomCounter in swarmRoomCounters)
		{
			if (!flag)
			{
				text += ",";
			}
			flag = false;
			text = text + swarmRoomCounter.Key + ":" + swarmRoomCounter.Value;
		}
		text += "<rgA>";
		if (lineageCounters.Count > 0)
		{
			text += "LINEAGES<rgB>";
			flag = true;
			foreach (KeyValuePair<string, int> lineageCounter in lineageCounters)
			{
				if (!flag)
				{
					text += ",";
				}
				flag = false;
				text = text + lineageCounter.Key + ":" + lineageCounter.Value;
			}
			text += "<rgA>";
		}
		if (savedObjects.Count > 0)
		{
			text += "OBJECTS<rgB>";
			for (int i = 0; i < savedObjects.Count; i++)
			{
				text = text + savedObjects[i] + "<rgC>";
			}
			for (int j = 0; j < unrecognizedSavedObjects.Count; j++)
			{
				text = text + unrecognizedSavedObjects[j] + "<rgC>";
			}
			text += "<rgA>";
		}
		if (savedPopulation.Count > 0 || unrecognizedPopulation.Count > 0)
		{
			text += "POPULATION<rgB>";
			for (int k = 0; k < savedPopulation.Count; k++)
			{
				text = text + savedPopulation[k] + "<rgC>";
			}
			for (int l = 0; l < unrecognizedPopulation.Count; l++)
			{
				text = text + unrecognizedPopulation[l] + "<rgC>";
			}
			text += "<rgA>";
		}
		if (savedSticks.Count > 0)
		{
			text += "STICKS<rgB>";
			for (int m = 0; m < savedSticks.Count; m++)
			{
				text = text + savedSticks[m] + "<rgC>";
			}
			text += "<rgA>";
		}
		if (consumedItems.Count > 0)
		{
			text += "CONSUMEDITEMS<rgB>";
			for (int n = 0; n < consumedItems.Count; n++)
			{
				text = text + consumedItems[n].ToString() + ((n < consumedItems.Count - 1) ? "<rgC>" : "");
			}
			text += "<rgA>";
		}
		text += "ROOMSVISITED<rgB>";
		text += string.Join(",", roomsVisited.ToArray());
		text += "<rgA>";
		foreach (string unrecognizedSaveString in unrecognizedSaveStrings)
		{
			text = text + unrecognizedSaveString + "<rgA>";
		}
		return text;
	}

	private string CreatureToStringInDenPos(AbstractCreature critter, int validSaveShelter, int activeGate)
	{
		if (critter.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
		{
			return "";
		}
		if (critter.pos.room == activeGate)
		{
			Custom.Log($"creature {critter} in gate with player - not saving");
			return "";
		}
		try
		{
			WorldCoordinate worldCoordinate = critter.spawnDen;
			if (world.GetAbstractRoom(critter.pos).shelter && ShelterDoor.IsTileInsideShelterRange(world.GetAbstractRoom(critter.pos), critter.pos.Tile) && (critter.pos.room == validSaveShelter || critter.state.dead || critter.creatureTemplate.offScreenSpeed == 0f))
			{
				worldCoordinate = new WorldCoordinate(critter.pos.room, -1, -1, 0);
			}
			else if (critter.abstractAI != null && critter.abstractAI.denPosition.HasValue && world.IsRoomInRegion(critter.abstractAI.denPosition.Value.room))
			{
				worldCoordinate = critter.abstractAI.denPosition.Value;
			}
			if (!world.IsRoomInRegion(worldCoordinate.room))
			{
				Custom.LogWarning($"Did not save creature {critter.creatureTemplate.name} {critter.ID} because sav pos not in region {critter.pos} w:{world.firstRoomIndex}-{world.firstRoomIndex + world.NumberOfRooms}");
				return "";
			}
			if (!world.GetAbstractRoom(worldCoordinate).shelter && !critter.state.alive)
			{
				Custom.LogWarning($"Did not save creature {critter.creatureTemplate.name} {critter.ID} because dead");
				return "";
			}
			return SaveState.AbstractCreatureToStringStoryWorld(critter, worldCoordinate);
		}
		catch (Exception arg)
		{
			Custom.LogWarning($"failed to save creature: {arg}");
			return "";
		}
	}

	public void ReportConsumedItem(int originRoom, int placedObjectIndex, int waitCycles)
	{
		for (int num = consumedItems.Count - 1; num >= 0; num--)
		{
			if (consumedItems[num].originRoom == originRoom && consumedItems[num].placedObjectIndex == placedObjectIndex)
			{
				consumedItems.RemoveAt(num);
			}
		}
		consumedItems.Add(new ConsumedItem(originRoom, placedObjectIndex, waitCycles));
	}

	public bool ItemConsumed(int originRoom, int placedObjectIndex)
	{
		for (int num = consumedItems.Count - 1; num >= 0; num--)
		{
			if (consumedItems[num].originRoom == originRoom && consumedItems[num].placedObjectIndex == placedObjectIndex)
			{
				return true;
			}
		}
		return false;
	}
}
