using System;
using System.Collections.Generic;
using System.Globalization;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class AbstractRoom
{
	public class CreatureRoomAttraction : ExtEnum<CreatureRoomAttraction>
	{
		public static readonly CreatureRoomAttraction Neutral = new CreatureRoomAttraction("Neutral", register: true);

		public static readonly CreatureRoomAttraction Forbidden = new CreatureRoomAttraction("Forbidden", register: true);

		public static readonly CreatureRoomAttraction Avoid = new CreatureRoomAttraction("Avoid", register: true);

		public static readonly CreatureRoomAttraction Like = new CreatureRoomAttraction("Like", register: true);

		public static readonly CreatureRoomAttraction Stay = new CreatureRoomAttraction("Stay", register: true);

		public CreatureRoomAttraction(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public World world;

	public string name;

	public int[] connections;

	public int index;

	public AbstractRoomNode[] nodes;

	public Room realizedRoom;

	public List<AbstractCreature> creatures;

	public List<AbstractWorldEntity> entities;

	public List<AbstractWorldEntity> entitiesInDens;

	public int[,] quantifiedCreatures;

	private bool evenUpdate;

	public int shelterIndex;

	public bool singleRealizedRoom;

	private List<string> roomTags;

	public bool offScreenDen;

	public int exits;

	public int dens;

	public int regionTransportations;

	public int sideExits;

	public int skyExits;

	public int seaExits;

	public int batHives;

	public int garbageHoles;

	public int gateIndex;

	public bool firstTimeRealized = true;

	public Vector2 mapPos;

	public IntVector2 size;

	public int layer = 1;

	public string subregionName;

	public string altSubregionName;

	public bool NOTRACKERS;

	public bool isBattleArena;

	public int battleArenaTriggeredTime;

	public bool isAncientShelter;

	public CreatureRoomAttraction[] roomAttractions;

	public int swarmRoomIndex { get; private set; }

	public bool swarmRoom => swarmRoomIndex > -1;

	public bool shelter => shelterIndex > -1;

	public string DisplaySubregionName
	{
		get
		{
			if (altSubregionName != null)
			{
				return altSubregionName;
			}
			return subregionName;
		}
	}

	public bool scavengerOutpost
	{
		get
		{
			if (roomTags == null)
			{
				return false;
			}
			return roomTags.Contains("SCAVOUTPOST");
		}
	}

	public bool scavengerTrader
	{
		get
		{
			if (roomTags == null)
			{
				return false;
			}
			return roomTags.Contains("SCAVTRADER");
		}
	}

	public int borderExits => sideExits + skyExits + seaExits;

	public int TotalNodes => exits + dens + regionTransportations + borderExits + batHives;

	public bool AnySkyAccess
	{
		get
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i].type == AbstractRoomNode.Type.SkyExit)
				{
					return true;
				}
			}
			return false;
		}
	}

	public bool gate => gateIndex > -1;

	public bool AnySideAccess
	{
		get
		{
			for (int i = 0; i < nodes.Length; i++)
			{
				if (nodes[i].type == AbstractRoomNode.Type.SideExit)
				{
					return true;
				}
			}
			return false;
		}
	}

	public void AddTag(string tg)
	{
		if (tg == "PERF_HEAVY")
		{
			Custom.Log("heavy room", name);
			singleRealizedRoom = true;
			return;
		}
		if (ModManager.MSC && tg == "ARENA")
		{
			Custom.Log("battle arena room", name);
			isBattleArena = true;
			singleRealizedRoom = true;
			return;
		}
		if (ModManager.MMF && tg == "NOTRACKERS")
		{
			Custom.Log("No trackers in room", name);
			NOTRACKERS = true;
			return;
		}
		if (tg == "ANCIENTSHELTER")
		{
			Custom.Log("ancient shelter", name);
			isAncientShelter = true;
			return;
		}
		if (roomTags == null)
		{
			roomTags = new List<string>();
		}
		roomTags.Add(tg);
		Custom.Log("Room", name, "tagged as:", tg);
	}

	public AbstractRoom(string name, int[] connections, int index, int swarmRoomIndex, int shelterIndex, int gateIndex)
	{
		this.name = name;
		this.connections = connections;
		this.index = index;
		this.swarmRoomIndex = swarmRoomIndex;
		this.shelterIndex = shelterIndex;
		this.gateIndex = gateIndex;
		if (shelter || gate)
		{
			subregionName = null;
			altSubregionName = null;
		}
		evenUpdate = false;
		creatures = new List<AbstractCreature>();
		entities = new List<AbstractWorldEntity>();
		entitiesInDens = new List<AbstractWorldEntity>();
		roomAttractions = new CreatureRoomAttraction[Math.Max(StaticWorld.creatureTemplates.Length, ExtEnum<CreatureTemplate.Type>.values.Count)];
	}

	public void InitNodes(AbstractRoomNode[] nodes, string line2)
	{
		this.nodes = nodes;
		size = new IntVector2(int.Parse(line2.Split('|')[0].Split('*')[0], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(line2.Split('|')[0].Split('*')[1], NumberStyles.Any, CultureInfo.InvariantCulture));
		quantifiedCreatures = new int[nodes.Length, StaticWorld.quantifiedCreatures.Length];
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].type == AbstractRoomNode.Type.Exit)
			{
				exits++;
			}
			else if (nodes[i].type == AbstractRoomNode.Type.Den)
			{
				dens++;
			}
			else if (nodes[i].type == AbstractRoomNode.Type.RegionTransportation)
			{
				regionTransportations++;
			}
			else if (nodes[i].type == AbstractRoomNode.Type.SideExit)
			{
				sideExits++;
			}
			else if (nodes[i].type == AbstractRoomNode.Type.SkyExit)
			{
				skyExits++;
			}
			else if (nodes[i].type == AbstractRoomNode.Type.SeaExit)
			{
				seaExits++;
			}
			else if (nodes[i].type == AbstractRoomNode.Type.BatHive)
			{
				batHives++;
			}
			else if (nodes[i].type == AbstractRoomNode.Type.GarbageHoles)
			{
				garbageHoles++;
			}
		}
		if (nodes.Length < connections.Length)
		{
			Custom.LogWarning(name, "less nodes than connections!");
		}
	}

	public void Update(int timePassed)
	{
		evenUpdate = !evenUpdate;
		bool flag = false;
		while (!flag)
		{
			flag = true;
			for (int i = 0; i < entities.Count; i++)
			{
				if (entities[i].evenUpdate != evenUpdate)
				{
					int room = entities[i].pos.room;
					entities[i].evenUpdate = evenUpdate;
					entities[i].Update(timePassed);
					if (i >= entities.Count || entities[i].pos.room != room || entities[i].InDen)
					{
						flag = false;
						break;
					}
				}
			}
		}
		if (ModManager.MMF && realizedRoom != null && realizedRoom.world.game.rainWorld.options.quality == Options.Quality.LOW)
		{
			singleRealizedRoom = true;
		}
		if (battleArenaTriggeredTime > 0)
		{
			battleArenaTriggeredTime--;
		}
		UpdateCreaturesInDens(timePassed);
	}

	public void UpdateCreaturesInDens(int timePassed)
	{
		for (int num = entitiesInDens.Count - 1; num >= 0; num--)
		{
			if (num < entitiesInDens.Count && entitiesInDens[num] is AbstractCreature)
			{
				(entitiesInDens[num] as AbstractCreature).InDenUpdate(timePassed);
			}
		}
	}

	public void RealizeRoom(World world, RainWorldGame game)
	{
		if (ModManager.MMF && world.game.rainWorld.options.quality == Options.Quality.LOW)
		{
			singleRealizedRoom = true;
		}
		if (realizedRoom == null && !offScreenDen)
		{
			int num = ((game.IsStorySession && game.StoryCharacter == SlugcatStats.Name.Red) ? 5 : 25);
			if (ModManager.MSC && shelter && !world.singleRoomWorld && !game.rainWorld.safariMode && !game.wasAnArtificerDream && game.IsStorySession && game.GetStorySession.slugPupMaxCount > 0 && game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup >= num && name != game.GetStorySession.saveState.denPosition)
			{
				AbstractCreature abstractCreature = new AbstractCreature(world, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(index, -1, -1, 0), game.GetNewID());
				AddEntity(abstractCreature);
				(abstractCreature.state as PlayerNPCState).foodInStomach = 1;
				game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup = -game.GetStorySession.saveState.miscWorldSaveData.cyclesSinceLastSlugpup;
			}
			Room room = new Room(game, world, this);
			world.loadingRooms.Add(new RoomPreparer(room, loadAiHeatMaps: true, !game.setupValues.bake, shortcutsOnly: false));
			realizedRoom = room;
			world.activeRooms.Add(realizedRoom);
		}
	}

	public void Abstractize()
	{
		if (realizedRoom == null)
		{
			return;
		}
		if (ModManager.MMF && shelter)
		{
			for (int i = 0; i < realizedRoom.updateList.Count; i++)
			{
				if (realizedRoom.updateList[i] is ShelterDoor)
				{
					if ((realizedRoom.updateList[i] as ShelterDoor).gasketMoverLoop.emitter != null)
					{
						(realizedRoom.updateList[i] as ShelterDoor).gasketMoverLoop.emitter.slatedForDeletetion = true;
					}
					if ((realizedRoom.updateList[i] as ShelterDoor).workingLoop.emitter != null)
					{
						(realizedRoom.updateList[i] as ShelterDoor).workingLoop.emitter.slatedForDeletetion = true;
					}
				}
			}
		}
		for (int num = entities.Count - 1; num >= 0; num--)
		{
			WorldCoordinate coord = new WorldCoordinate(entities[num].pos.room, entities[num].pos.x, entities[num].pos.y, entities[num].pos.abstractNode);
			if (entities[num] is AbstractCreature && !(entities[num] as AbstractCreature).creatureTemplate.quantified)
			{
				coord = QuickConnectivity.DefineNodeOfLocalCoordinate(entities[num].pos, realizedRoom.game.world, (entities[num] as AbstractCreature).creatureTemplate);
			}
			entities[num].Abstractize(coord);
		}
		CountQuantifiedCreatures();
		for (int num2 = creatures.Count - 1; num2 >= 0; num2--)
		{
			if (creatures[num2].creatureTemplate.quantified && creatures[num2].Quantify)
			{
				RemoveEntity(creatures[num2]);
			}
		}
		for (int num3 = entities.Count - 1; num3 >= 0; num3--)
		{
			if (entities[num3].slatedForDeletion)
			{
				RemoveEntity(entities[num3]);
			}
		}
		realizedRoom.Unloaded();
		realizedRoom.game.world.activeRooms.Remove(realizedRoom);
		realizedRoom = null;
	}

	public void MoveEntityToDen(AbstractWorldEntity ent)
	{
		ent.IsEnteringDen(ent.pos);
		entities.Remove(ent);
		if (ent is AbstractCreature)
		{
			creatures.Remove((AbstractCreature)ent);
		}
		if (entitiesInDens.IndexOf(ent) == -1)
		{
			entitiesInDens.Add(ent);
		}
	}

	public void MoveEntityOutOfDen(AbstractWorldEntity ent)
	{
		ent.IsExitingDen();
		entitiesInDens.Remove(ent);
		AddEntity(ent);
	}

	public void AddEntity(AbstractWorldEntity ent)
	{
		if (entities.IndexOf(ent) == -1)
		{
			entities.Add(ent);
		}
		if (ent is AbstractCreature && creatures.IndexOf(ent as AbstractCreature) == -1)
		{
			creatures.Add((AbstractCreature)ent);
		}
	}

	public void RemoveEntity(EntityID ID)
	{
		for (int i = 0; i < entities.Count; i++)
		{
			if (entities[i].ID == ID)
			{
				RemoveEntity(entities[i]);
				break;
			}
		}
	}

	public void RemoveEntity(AbstractWorldEntity ent)
	{
		entities.Remove(ent);
		entitiesInDens.Remove(ent);
		if (ent is AbstractCreature)
		{
			creatures.Remove((AbstractCreature)ent);
		}
		if (ent is AbstractCreature && (ent as AbstractCreature).realizedCreature != null && (ent as AbstractCreature).realizedCreature.room == realizedRoom && realizedRoom != null)
		{
			Custom.LogWarning("An abstractCreature is trying to remove itself from room, but realizedCreature is remaining.");
		}
	}

	public int ExitIndex(int targetRoom)
	{
		return Array.IndexOf(connections, targetRoom);
	}

	public bool ConnectionAndBackPossible(int startNode, int destNode, CreatureTemplate creatureType)
	{
		if (startNode != destNode)
		{
			if (ConnectionPossible(startNode, destNode, creatureType))
			{
				return ConnectionPossible(destNode, startNode, creatureType);
			}
			return false;
		}
		return true;
	}

	public bool ConnectionPossible(int startNode, int destNode, CreatureTemplate creatureType)
	{
		return ConnectivityCost(startNode, destNode, creatureType) > -1;
	}

	public int ConnectivityCost(int startNode, int destNode, CreatureTemplate creatureType)
	{
		if (startNode < 0 || destNode < 0 || startNode >= nodes.Length || destNode >= nodes.Length)
		{
			return -1;
		}
		return nodes[startNode].ConnectionCost(destNode, creatureType);
	}

	public int ConnectionLength(int startNode, int destNode, CreatureTemplate creatureType)
	{
		if (startNode < 0 || destNode < 0 || startNode >= nodes.Length || destNode >= nodes.Length)
		{
			return -1;
		}
		return nodes[startNode].ConnectionLength(destNode, creatureType);
	}

	public WorldCoordinate RandomNodeInRoom()
	{
		return new WorldCoordinate(index, -1, -1, (nodes.Length == 0) ? (-1) : UnityEngine.Random.Range(0, nodes.Length));
	}

	public AbstractRoomNode GetNode(WorldCoordinate c)
	{
		return nodes[c.abstractNode];
	}

	public int NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type crit)
	{
		int num = 0;
		for (int i = 0; i < nodes.Length; i++)
		{
			num += NumberOfQuantifiedCreatureInNode(crit, i);
		}
		for (int j = 0; j < creatures.Count; j++)
		{
			if (!creatures[j].pos.NodeDefined && creatures[j].creatureTemplate.type == crit && creatures[j].Quantify)
			{
				num++;
			}
		}
		return num;
	}

	public int NumberOfQuantifiedCreatureInNode(CreatureTemplate.Type crit, int node)
	{
		if (quantifiedCreatures == null)
		{
			return 0;
		}
		if (realizedRoom != null && realizedRoom.quantifiedCreaturesPlaced)
		{
			int num = 0;
			for (int i = 0; i < creatures.Count; i++)
			{
				if (creatures[i].pos.abstractNode == node && creatures[i].creatureTemplate.type == crit && creatures[i].Quantify)
				{
					num++;
				}
			}
			return num;
		}
		return quantifiedCreatures[node, StaticWorld.GetCreatureTemplate(crit).quantifiedIndex];
	}

	private void ResetQuantifiedCreatures()
	{
		for (int i = 0; i < nodes.Length; i++)
		{
			for (int j = 0; j < StaticWorld.quantifiedCreatures.Length; j++)
			{
				quantifiedCreatures[i, j] = 0;
			}
		}
	}

	private void CountQuantifiedCreatures()
	{
		ResetQuantifiedCreatures();
		for (int i = 0; i < creatures.Count; i++)
		{
			if (creatures[i].Quantify)
			{
				AddQuantifiedCreature(creatures[i].pos.abstractNode, creatures[i].creatureTemplate.type);
			}
		}
	}

	public void AddQuantifiedCreature(int node, CreatureTemplate.Type crit, int amount)
	{
		if (offScreenDen)
		{
			return;
		}
		if (node == -1)
		{
			if (connections.Length == 0)
			{
				return;
			}
			node = UnityEngine.Random.Range(0, connections.Length);
		}
		quantifiedCreatures[node, StaticWorld.GetCreatureTemplate(crit).quantifiedIndex] += amount;
	}

	public void AddQuantifiedCreature(int node, CreatureTemplate.Type crit)
	{
		AddQuantifiedCreature(node, crit, 1);
	}

	public void RemoveQuantifiedCreature(int node, CreatureTemplate.Type crit)
	{
		if (NumberOfQuantifiedCreatureInNode(crit, node) > 0)
		{
			quantifiedCreatures[node, StaticWorld.GetCreatureTemplate(crit).quantifiedIndex]--;
			return;
		}
		Custom.LogWarning("Couldn't remove quantified creature! Nowhere to draw from!");
	}

	public int NodesRelevantToCreature(CreatureTemplate crit)
	{
		int num = 0;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].type.Index != -1 && crit.mappedNodeTypes[nodes[i].type.Index])
			{
				num++;
			}
		}
		return num;
	}

	public int RandomRelevantNode(CreatureTemplate crit)
	{
		if (NodesRelevantToCreature(crit) < 1)
		{
			return -1;
		}
		return CreatureSpecificToCommonNodeIndex(UnityEngine.Random.Range(0, NodesRelevantToCreature(crit)), crit);
	}

	public int CreatureSpecificToCommonNodeIndex(int specific, CreatureTemplate crit)
	{
		int num = 0;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].type.Index != -1 && crit.mappedNodeTypes[nodes[i].type.Index])
			{
				if (specific == num)
				{
					return i;
				}
				num++;
			}
		}
		return -1;
	}

	public int CommonToCreatureSpecificNodeIndex(int common, CreatureTemplate crit)
	{
		int num = 0;
		for (int i = 0; i < nodes.Length; i++)
		{
			if (nodes[i].type.Index != -1 && crit.mappedNodeTypes[nodes[i].type.Index])
			{
				if (i == common)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public CreatureRoomAttraction AttractionForCreature(CreatureTemplate.Type tp)
	{
		if (shelter || gate)
		{
			return CreatureRoomAttraction.Forbidden;
		}
		if (ModManager.MSC && offScreenDen && world.game.IsArenaSession && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			return CreatureRoomAttraction.Forbidden;
		}
		if (world.migrationInfluence != null && tp.Index != -1 && roomAttractions[tp.Index] != CreatureRoomAttraction.Forbidden)
		{
			float defValue = CreatureAttractionToFloat(roomAttractions[tp.Index]);
			defValue = world.migrationInfluence.AttractionValueForCreature(this, tp, defValue);
			return FloatToCreatureAttraction(defValue);
		}
		if (tp.Index == -1)
		{
			return CreatureRoomAttraction.Forbidden;
		}
		return roomAttractions[tp.Index];
	}

	public float AttractionValueForCreature(CreatureTemplate.Type tp)
	{
		if (shelter || gate)
		{
			return 0f;
		}
		if (ModManager.MSC && offScreenDen && world.game.IsArenaSession && world.game.GetArenaGameSession.arenaSitting.gameTypeSetup.gameType == MoreSlugcatsEnums.GameTypeID.Challenge)
		{
			return 0f;
		}
		if (tp.Index == -1)
		{
			return 0f;
		}
		if (world.migrationInfluence != null && roomAttractions[tp.Index] != CreatureRoomAttraction.Forbidden)
		{
			return world.migrationInfluence.AttractionValueForCreature(this, tp, CreatureAttractionToFloat(roomAttractions[tp.Index]));
		}
		return CreatureAttractionToFloat(roomAttractions[tp.Index]);
	}

	public float SizeDependentAttractionValueForCreature(CreatureTemplate.Type tp)
	{
		if (StaticWorld.GetCreatureTemplate(tp).canFly)
		{
			return AttractionValueForCreature(tp) * Mathf.Lerp((float)(size.x * size.y) / 2000f, 1f, 0.985f);
		}
		return AttractionValueForCreature(tp) * Mathf.Lerp((float)size.x * Mathf.Lerp(size.x, 1f, 0.5f) * (float)size.y / 50000f, 1f, 0.985f);
	}

	public static float CreatureAttractionToFloat(CreatureRoomAttraction cra)
	{
		if (cra == CreatureRoomAttraction.Forbidden)
		{
			return 0f;
		}
		if (cra == CreatureRoomAttraction.Avoid)
		{
			return 0.25f;
		}
		if (cra == CreatureRoomAttraction.Like)
		{
			return 0.75f;
		}
		if (cra == CreatureRoomAttraction.Stay)
		{
			return 1f;
		}
		return 0.5f;
	}

	public static CreatureRoomAttraction FloatToCreatureAttraction(float f)
	{
		if (f < 0.125f)
		{
			return CreatureRoomAttraction.Forbidden;
		}
		if (f < 0.375f)
		{
			return CreatureRoomAttraction.Avoid;
		}
		if (f < 0.625f)
		{
			return CreatureRoomAttraction.Neutral;
		}
		if (f < 0.875f)
		{
			return CreatureRoomAttraction.Like;
		}
		return CreatureRoomAttraction.Stay;
	}
}
