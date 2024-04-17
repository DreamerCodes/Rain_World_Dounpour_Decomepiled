using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class World
{
	public abstract class CreatureSpawner
	{
		public int region;

		public int inRegionSpawnerIndex;

		public WorldCoordinate den;

		public bool nightCreature;

		public int SpawnerID => region * 1000 + inRegionSpawnerIndex;

		public CreatureSpawner(int region, int inRegionSpawnerIndex, WorldCoordinate den)
		{
			this.region = region;
			this.inRegionSpawnerIndex = inRegionSpawnerIndex;
			this.den = den;
		}
	}

	public class SimpleSpawner : CreatureSpawner
	{
		public CreatureTemplate.Type creatureType;

		public string spawnDataString;

		public int amount;

		public SimpleSpawner(int region, int inRegionSpawnerIndex, WorldCoordinate den, CreatureTemplate.Type creatureType, string spawnDataString, int amount)
			: base(region, inRegionSpawnerIndex, den)
		{
			this.creatureType = creatureType;
			this.spawnDataString = spawnDataString;
			this.amount = amount;
		}

		public override string ToString()
		{
			return "Simple Spawner " + ((amount == 1) ? "" : ("(" + amount + " creatures)"));
		}
	}

	public class Lineage : CreatureSpawner
	{
		public int[] creatureTypes;

		public float[] progressionChances;

		public string[] spawnData;

		public string denString;

		public Lineage(int region, int inRegionSpawnerIndex, WorldCoordinate den, int[] types, float[] progChances, string[] spwnDt, int conflictNumber)
			: base(region, inRegionSpawnerIndex, den)
		{
			creatureTypes = types;
			progressionChances = progChances;
			spawnData = spwnDt;
			denString = den.SaveToString();
			if (conflictNumber > 0)
			{
				denString = denString + ";" + conflictNumber;
			}
		}

		public CreatureTemplate.Type CurrentType(SaveState saveState)
		{
			if (saveState.regionStates[region] == null)
			{
				Custom.LogWarning("lineage couldn't find region state");
				return null;
			}
			if (!saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				Custom.LogWarning("couldn't find lineage for", denString);
				return null;
			}
			int num = saveState.regionStates[region].lineageCounters[denString];
			if (num < 0 || num >= creatureTypes.Length)
			{
				Custom.LogWarning("lineage progression out of range:", num.ToString(), "/", creatureTypes.Length.ToString());
				return null;
			}
			if (creatureTypes[num] < 0)
			{
				return null;
			}
			return new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(creatureTypes[num]));
		}

		public string CurrentSpawnData(SaveState saveState)
		{
			if (saveState.regionStates[region] == null)
			{
				Custom.LogWarning("lineage couldn't find region state");
				return null;
			}
			if (!saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				Custom.LogWarning("couldn't find lineage for", denString);
				return null;
			}
			int num = saveState.regionStates[region].lineageCounters[denString];
			if (num < 0 || num >= creatureTypes.Length)
			{
				Custom.LogWarning("lineage progression out of range:", num.ToString(), "/", creatureTypes.Length.ToString());
				return null;
			}
			if (creatureTypes[num] < 0)
			{
				return null;
			}
			return spawnData[num];
		}

		public bool LastWasNONE(SaveState saveState)
		{
			if (saveState.regionStates[region] == null)
			{
				Custom.LogWarning("lineage couldn't find region state");
				return false;
			}
			if (!saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				Custom.LogWarning("couldn't find lineage for", denString);
				return false;
			}
			int num = saveState.regionStates[region].lineageCounters[denString];
			if (num < 1)
			{
				return true;
			}
			if (num - 1 >= creatureTypes.Length)
			{
				return false;
			}
			return creatureTypes[num - 1] < 0;
		}

		public void ChanceToProgress(World world)
		{
			if (!(world.game.session is StoryGameSession))
			{
				return;
			}
			if (!(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters.ContainsKey(denString))
			{
				(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString] = 0;
			}
			int num = (world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString];
			if (num >= 0 && num < progressionChances.Length && num < progressionChances.Length - 1 && Random.value < progressionChances[num])
			{
				num++;
				(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString] = num;
				string text = ExtEnum<CreatureTemplate.Type>.values.GetEntry(creatureTypes[(world.game.session as StoryGameSession).saveState.regionStates[region].lineageCounters[denString]]);
				if (text == null)
				{
					text = "NONE";
				}
				Custom.Log($"Lineage {base.SpawnerID} progressed to {text}");
			}
		}

		public override string ToString()
		{
			string text = "LINEAGE ";
			for (int i = 0; i < creatureTypes.Length; i++)
			{
				text = text + new CreatureTemplate.Type(ExtEnum<CreatureTemplate.Type>.values.GetEntry(creatureTypes[i]))?.ToString() + "-" + progressionChances[i] + ((i < creatureTypes.Length - 1) ? ", " : "");
			}
			return text;
		}
	}

	public interface IMigrationInfluence
	{
		float AttractionValueForCreature(AbstractRoom room, CreatureTemplate.Type tp, float defValue);
	}

	public abstract class WorldProcess
	{
		public World world;

		public WorldProcess(World world)
		{
			this.world = world;
		}

		public virtual void Update()
		{
		}
	}

	private static readonly AGLog<World> Log = new AGLog<World>();

	public AbstractRoom offScreenDen;

	private AbstractRoom[] abstractRooms;

	public List<Room> activeRooms;

	public List<RoomPreparer> loadingRooms;

	public readonly int preProcessingGeneration;

	public int[] swarmRooms;

	public int[] shelters;

	public int[] gates;

	public bool[] brokenShelters;

	public int brokenShelterIndexDueToPrecycle = -1;

	public WorldCoordinate[] sideAccessNodes;

	public WorldCoordinate[] skyAccessNodes;

	public WorldCoordinate[] seaAccessNodes;

	public WorldCoordinate[] regionAccessNodes;

	public int mostNodesInARoom;

	public string name;

	public bool singleRoomWorld;

	public List<WorldProcess> worldProcesses;

	public FliesWorldAI fliesWorldAI;

	public ScavengersWorldAI scavengersWorldAI;

	public OverseersWorldAI overseersWorldAI;

	public VoidSpawnWorldAI voidSpawnWorldAI;

	public RainCycle rainCycle;

	public Region region;

	public CreatureSpawner[] spawners = new CreatureSpawner[0];

	public Lineage[] lineages = new Lineage[0];

	public IMigrationInfluence migrationInfluence;

	public GhostWorldPresence worldGhost;

	public bool logCreatures = true;

	public List<string> DisabledMapRooms;

	public List<int> DisabledMapIndices;

	public RainWorldGame game { get; private set; }

	public int NumberOfRooms => abstractRooms.Length;

	public int RegionNumber
	{
		get
		{
			if (region == null)
			{
				return -1;
			}
			return region.regionNumber;
		}
	}

	public RegionState regionState
	{
		get
		{
			if (game.session is StoryGameSession && !singleRoomWorld)
			{
				return (game.session as StoryGameSession).saveState.regionStates[region.regionNumber];
			}
			return null;
		}
	}

	public int firstRoomIndex
	{
		get
		{
			if (region == null)
			{
				return 0;
			}
			return region.firstRoomIndex;
		}
	}

	public World(RainWorldGame game, Region region, string name, bool singleRoomWorld)
	{
		this.game = game;
		this.region = region;
		this.name = name;
		this.singleRoomWorld = singleRoomWorld;
		activeRooms = new List<Room>();
		loadingRooms = new List<RoomPreparer>();
		DisabledMapRooms = new List<string>();
		DisabledMapIndices = new List<int>();
		worldProcesses = new List<WorldProcess>();
		preProcessingGeneration = 20;
		if (game == null)
		{
			return;
		}
		if (game.IsStorySession)
		{
			float minutes = ((!(game.GetStorySession.characterStats.name == SlugcatStats.Name.Yellow) && (!ModManager.MSC || (!(game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Rivulet) && !(game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Gourmand) && !(game.GetStorySession.characterStats.name == MoreSlugcatsEnums.SlugcatStatsName.Saint)))) ? (Mathf.Lerp(game.rainWorld.setup.cycleTimeMin, game.rainWorld.setup.cycleTimeMax, Random.value) / 60f) : (Mathf.Lerp(game.rainWorld.setup.cycleTimeMin, game.rainWorld.setup.cycleTimeMax, 0.35f + 0.65f * Mathf.Pow(Random.value, 1.2f)) / 60f));
			if (ModManager.MMF && MMF.cfgNoRandomCycles.Value)
			{
				minutes = (float)game.rainWorld.setup.cycleTimeMax / 60f;
			}
			rainCycle = new RainCycle(this, minutes);
			if (ModManager.MSC && name == "SB")
			{
				rainCycle.filtrationPowerBehavior = new FiltrationPowerController(this);
			}
		}
		else
		{
			rainCycle = new RainCycle(this, game.GetArenaGameSession.rainCycleTimeInMinutes);
		}
	}

	public void AddWorldProcess(WorldProcess process)
	{
		worldProcesses.Add(process);
		if (process is FliesWorldAI)
		{
			fliesWorldAI = process as FliesWorldAI;
		}
		else if (process is ScavengersWorldAI)
		{
			scavengersWorldAI = process as ScavengersWorldAI;
		}
		else if (process is OverseersWorldAI)
		{
			overseersWorldAI = process as OverseersWorldAI;
		}
		else if (process is VoidSpawnWorldAI)
		{
			voidSpawnWorldAI = process as VoidSpawnWorldAI;
		}
	}

	public void LoadWorld(SlugcatStats.Name slugcatNumber, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
	{
		GenerateOffScreenDen(abstractRoomsList.Count);
		abstractRoomsList.Add(offScreenDen);
		abstractRooms = abstractRoomsList.ToArray();
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			abstractRooms[i].world = this;
		}
		this.shelters = shelters;
		brokenShelters = new bool[shelters.Length];
		LoadMapConfig(slugcatNumber);
		this.swarmRooms = swarmRooms;
		this.gates = gates;
		mostNodesInARoom = 0;
		for (int j = 0; j < abstractRooms.Length; j++)
		{
			if (abstractRooms[j].nodes.Length > mostNodesInARoom)
			{
				mostNodesInARoom = abstractRooms[j].nodes.Length;
			}
		}
		foreach (string disabledMapRoom in DisabledMapRooms)
		{
			AbstractRoom abstractRoom = GetAbstractRoom(disabledMapRoom);
			if (abstractRoom != null)
			{
				DisabledMapIndices.Add(abstractRoom.index);
			}
		}
		MapBorderConnections();
		if (game != null && !singleRoomWorld && game.session is StoryGameSession)
		{
			if ((game.session as StoryGameSession).saveState.regionStates[region.regionNumber] == null)
			{
				Custom.Log("Creating region state for world:", name);
				(game.session as StoryGameSession).saveState.regionStates[region.regionNumber] = new RegionState((game.session as StoryGameSession).saveState, this);
			}
			else
			{
				(game.session as StoryGameSession).saveState.regionStates[region.regionNumber].world = this;
				Custom.Log("World assigned to existing region state:", name);
			}
			regionState.AdaptWorldToRegionState();
			SpawnGhost();
		}
	}

	private void SpawnGhost()
	{
		if (game.setupValues.ghosts < 0 || !CheckForRegionGhost((game.session as StoryGameSession).saveStateNumber, region.name))
		{
			return;
		}
		GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(region.name);
		if ((ModManager.MSC && ((ghostID == MoreSlugcatsEnums.GhostID.SL && game.session is StoryGameSession && (game.session as StoryGameSession).saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Saint) || (ghostID == MoreSlugcatsEnums.GhostID.MS && game.session is StoryGameSession && (game.session as StoryGameSession).saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Saint))) || ghostID == GhostWorldPresence.GhostID.NoGhost)
		{
			return;
		}
		int num = 0;
		if ((game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID))
		{
			num = (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID];
		}
		Custom.Log("Save state properly loaded:", (game.session as StoryGameSession).saveState.loaded.ToString());
		Custom.Log($"GHOST TALKED TO {ghostID} {num}");
		Custom.Log("Karma:", (game.session as StoryGameSession).saveState.deathPersistentSaveData.karma.ToString());
		bool flag = game.setupValues.ghosts > 0 || GhostWorldPresence.SpawnGhost(ghostID, (game.session as StoryGameSession).saveState.deathPersistentSaveData.karma, (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap, num, game.StoryCharacter == SlugcatStats.Name.Red);
		if (ModManager.MSC && (!ModManager.Expedition || (ModManager.Expedition && !game.rainWorld.ExpeditionMode)))
		{
			if (game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Artificer && !flag)
			{
				if (!(game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) || (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] != 1)
				{
					flag = false;
				}
				else if (((game.session as StoryGameSession).saveState.deathPersistentSaveData.karma == (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap && (game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma) || (ModManager.Expedition && game.rainWorld.ExpeditionMode && (game.session as StoryGameSession).saveState.deathPersistentSaveData.karma == (game.session as StoryGameSession).saveState.deathPersistentSaveData.karmaCap))
				{
					flag = true;
				}
			}
			if (game.StoryCharacter == MoreSlugcatsEnums.SlugcatStatsName.Saint)
			{
				flag = false;
				if ((game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo.ContainsKey(ghostID) && (game.session as StoryGameSession).saveState.deathPersistentSaveData.ghostsTalkedTo[ghostID] == 2)
				{
					flag = false;
				}
				else if ((game.session as StoryGameSession).saveState.cycleNumber > 0)
				{
					flag = true;
				}
			}
		}
		if (game.rainWorld.safariMode)
		{
			flag = false;
		}
		if (flag)
		{
			worldGhost = new GhostWorldPresence(this, ghostID);
			migrationInfluence = worldGhost;
			Custom.Log("Ghost in region");
		}
		else
		{
			Custom.Log("No ghost in region");
		}
	}

	public void ActivateRoom(int room)
	{
		ActivateRoom(GetAbstractRoom(room));
	}

	public void ActivateRoom(string room)
	{
		for (int i = 0; i < abstractRooms.Length - 1; i++)
		{
			if (abstractRooms[i].name == room)
			{
				ActivateRoom(abstractRooms[i]);
				break;
			}
		}
	}

	public void ActivateRoom(AbstractRoom abstractRoom)
	{
		if (abstractRoom.realizedRoom == null)
		{
			abstractRoom.RealizeRoom(this, game);
		}
	}

	public AbstractRoom GetSwarmRoom(int swarmRoomIndex)
	{
		return GetAbstractRoom(swarmRooms[swarmRoomIndex]);
	}

	public AbstractRoom GetAbstractRoom(WorldCoordinate coord)
	{
		return GetAbstractRoom(coord.room);
	}

	public AbstractRoom GetAbstractRoom(int room)
	{
		if (room < firstRoomIndex || room >= firstRoomIndex + NumberOfRooms)
		{
			return null;
		}
		return abstractRooms[room - firstRoomIndex];
	}

	public AbstractRoom GetAbstractRoom(string room)
	{
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			if (abstractRooms[i].name == room)
			{
				return abstractRooms[i];
			}
		}
		if (room == "OFFSCREEN")
		{
			return abstractRooms[abstractRooms.Length - 1];
		}
		return null;
	}

	public AbstractRoomNode GetNode(WorldCoordinate c)
	{
		return GetAbstractRoom(c.room).nodes[c.abstractNode];
	}

	public bool VisualContactBetweenWorldCoordinates(WorldCoordinate a, WorldCoordinate b)
	{
		if (a.room != b.room)
		{
			return false;
		}
		if (GetAbstractRoom(a.room).realizedRoom == null)
		{
			return false;
		}
		return GetAbstractRoom(a.room).realizedRoom.VisualContact(GetAbstractRoom(a.room).realizedRoom.MiddleOfTile(a.Tile), GetAbstractRoom(a.room).realizedRoom.MiddleOfTile(b.Tile));
	}

	public int TotalShortCutLengthBetweenTwoConnectedRooms(int room1, int room2)
	{
		return TotalShortCutLengthBetweenTwoConnectedRooms(GetAbstractRoom(room1), GetAbstractRoom(room2));
	}

	public int TotalShortCutLengthBetweenTwoConnectedRooms(AbstractRoom room1, AbstractRoom room2)
	{
		if (room1 != null && room2 != null && room1.ExitIndex(room2.index) != -1 && room2.ExitIndex(room1.index) != -1)
		{
			return room1.nodes[room1.ExitIndex(room2.index)].shortCutLength + room2.nodes[room2.ExitIndex(room1.index)].shortCutLength;
		}
		return -1;
	}

	public bool IsRoomInRegion(int room)
	{
		if (room >= firstRoomIndex)
		{
			return room < firstRoomIndex + NumberOfRooms;
		}
		return false;
	}

	public WorldCoordinate NodeInALeadingToB(int roomA, int roomB)
	{
		return NodeInALeadingToB(GetAbstractRoom(roomA), GetAbstractRoom(roomB));
	}

	public WorldCoordinate NodeInALeadingToB(AbstractRoom roomA, AbstractRoom roomB)
	{
		if (roomA != null && roomB != null && roomA.ExitIndex(roomB.index) != -1)
		{
			return new WorldCoordinate(roomA.index, -1, -1, roomA.ExitIndex(roomB.index));
		}
		return new WorldCoordinate(-1, -1, -1, -1);
	}

	public void MoveQuantifiedCreatureFromAbstractRoom(CreatureTemplate.Type crit, AbstractRoom fromRoom, AbstractRoom toRoom)
	{
		if (fromRoom.realizedRoom != null)
		{
			Custom.LogWarning("TRYING TO USE MoveQuantifiedCreatureFromAbstractRoom FROM REALIZED ROOM!");
		}
		else
		{
			if (fromRoom.NumberOfQuantifiedCreatureInRoom(crit) == 0)
			{
				return;
			}
			int abstractNode = NodeInALeadingToB(fromRoom, toRoom).abstractNode;
			if (fromRoom.NumberOfQuantifiedCreatureInNode(crit, abstractNode) > 0)
			{
				fromRoom.RemoveQuantifiedCreature(abstractNode, crit);
			}
			else
			{
				int num = 0;
				int num2 = -1;
				for (int i = 0; i < fromRoom.nodes.Length; i++)
				{
					if (i != abstractNode && fromRoom.ConnectionPossible(i, abstractNode, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)) && fromRoom.NumberOfQuantifiedCreatureInNode(crit, i) > num)
					{
						num = fromRoom.NumberOfQuantifiedCreatureInNode(crit, i);
						num2 = i;
					}
				}
				if (num2 == -1)
				{
					return;
				}
				fromRoom.RemoveQuantifiedCreature(num2, crit);
			}
			int abstractNode2 = NodeInALeadingToB(toRoom, fromRoom).abstractNode;
			toRoom.AddQuantifiedCreature(abstractNode2, crit);
			if (toRoom.realizedRoom != null)
			{
				new AbstractCreature(this, StaticWorld.GetCreatureTemplate(crit), null, new WorldCoordinate(fromRoom.index, -1, -1, abstractNode), game.GetNewID()).Move(new WorldCoordinate(toRoom.index, -1, -1, abstractNode2));
			}
		}
	}

	public int SideHighwayDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos, GetAbstractRoom(B).mapPos) - 100f) * 0.15f) + 1;
	}

	public int SkyHighwayDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos, GetAbstractRoom(B).mapPos) - 100f) * 0.15f) + 1;
	}

	public int SeaHighwayDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos, GetAbstractRoom(B).mapPos) - 100f) * 0.15f) + 1;
	}

	public int RegionTransportationDistanceBetweenNodes(WorldCoordinate A, WorldCoordinate B)
	{
		if (A.room == B.room || GetAbstractRoom(A).offScreenDen || GetAbstractRoom(B).offScreenDen)
		{
			return 1;
		}
		return (int)(Mathf.Max(0f, Vector2.Distance(GetAbstractRoom(A).mapPos + GetAbstractRoom(A).size.ToVector2() * 0.5f, GetAbstractRoom(B).mapPos + GetAbstractRoom(A).size.ToVector2() * 0.5f) - 100f) * 0.15f) + 1;
	}

	public Vector2 RoomToWorldPos(Vector2 inRoomPos, int roomIndex)
	{
		AbstractRoom abstractRoom = GetAbstractRoom(roomIndex);
		if (abstractRoom == null)
		{
			return Vector2.zero;
		}
		return (abstractRoom.mapPos / 3f + new Vector2(10f, 10f)) * 20f + inRoomPos - new Vector2((float)abstractRoom.size.x * 20f, (float)abstractRoom.size.y * 20f) / 2f;
	}

	private void MapBorderConnections()
	{
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		List<WorldCoordinate> list2 = new List<WorldCoordinate>();
		List<WorldCoordinate> list3 = new List<WorldCoordinate>();
		List<WorldCoordinate> list4 = new List<WorldCoordinate>();
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			for (int j = 0; j < abstractRooms[i].nodes.Length; j++)
			{
				if (!DisabledMapIndices.Contains(i + firstRoomIndex))
				{
					if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.SideExit)
					{
						list.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
					else if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.SkyExit)
					{
						list2.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
					else if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.SeaExit)
					{
						list3.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
					else if (abstractRooms[i].nodes[j].type == AbstractRoomNode.Type.RegionTransportation)
					{
						list4.Add(new WorldCoordinate(i + firstRoomIndex, -1, -1, j));
					}
				}
			}
		}
		sideAccessNodes = list.ToArray();
		skyAccessNodes = list2.ToArray();
		seaAccessNodes = list3.ToArray();
		regionAccessNodes = list4.ToArray();
	}

	private void LoadMapConfig(SlugcatStats.Name slugcatNumber)
	{
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + name + ".txt");
		if (slugcatNumber != null)
		{
			Custom.Log("-- mapconfig as player: " + slugcatNumber.value);
			string text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + name + "-" + slugcatNumber.value + ".txt");
			if (File.Exists(text))
			{
				path = text;
			}
		}
		if (!File.Exists(path))
		{
			return;
		}
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
			if (array2.Length != 2)
			{
				continue;
			}
			for (int j = 0; j < NumberOfRooms; j++)
			{
				if (abstractRooms[j].name == array2[0])
				{
					string[] array3 = Regex.Split(array2[1], "><");
					abstractRooms[j].mapPos.x = float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture);
					abstractRooms[j].mapPos.y = float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					if (Regex.Split(array2[1], "><").Length >= 5)
					{
						abstractRooms[j].layer = int.Parse(array3[4], NumberStyles.Any, CultureInfo.InvariantCulture);
					}
					if (Regex.Split(array2[1], "><").Length >= 6)
					{
						abstractRooms[j].subregionName = ((array3[5].Trim() == "") ? null : array3[5]);
						abstractRooms[j].altSubregionName = null;
					}
					break;
				}
			}
		}
		bool flag = false;
		List<string> list = new List<string>();
		list.Add("");
		path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "Properties.txt");
		if (slugcatNumber != null)
		{
			string text2 = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "Properties-" + slugcatNumber.value + ".txt");
			if (File.Exists(text2))
			{
				path = text2;
				flag = true;
			}
		}
		if (!File.Exists(path))
		{
			return;
		}
		array = File.ReadAllLines(path);
		for (int k = 0; k < array.Length; k++)
		{
			string[] array4 = Regex.Split(Custom.ValidateSpacedDelimiter(array[k], ":"), ": ");
			if (array4.Length == 2 && flag && array4[0] == "Subregion")
			{
				list.Add(array4[1]);
			}
			if (array4.Length != 3)
			{
				continue;
			}
			if (array4[0] == "Room_Attr")
			{
				for (int l = 0; l < NumberOfRooms; l++)
				{
					if (!(abstractRooms[l].name == array4[1]))
					{
						continue;
					}
					string[] array5 = Regex.Split(array4[2], ",");
					for (int m = 0; m < array5.Length; m++)
					{
						if (!(array5[m] != ""))
						{
							continue;
						}
						string[] array6 = Regex.Split(array5[m], "-");
						int num = -1;
						for (int n = 0; n < ExtEnum<CreatureTemplate.Type>.values.Count; n++)
						{
							if (ExtEnum<CreatureTemplate.Type>.values.entries[n] == array6[0])
							{
								num = n;
								break;
							}
						}
						if (num >= 0)
						{
							if (int.TryParse(array6[1], NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
							{
								abstractRooms[l].roomAttractions[num] = BackwardsCompatibilityRemix.ParseRoomAttraction(result);
							}
							else
							{
								abstractRooms[l].roomAttractions[num] = new AbstractRoom.CreatureRoomAttraction(array6[1]);
							}
						}
					}
					break;
				}
			}
			else
			{
				if (!(slugcatNumber != null) || !(array4[0] == "Broken Shelters") || !(slugcatNumber.ToString() == array4[1].Trim()))
				{
					continue;
				}
				string[] array5 = Regex.Split(Custom.ValidateSpacedDelimiter(array4[2], ","), ", ");
				for (int num2 = 0; num2 < array5.Length; num2++)
				{
					if (GetAbstractRoom(array5[num2]) != null && GetAbstractRoom(array5[num2]).shelter)
					{
						Custom.Log($"--slugcat {slugcatNumber} has a broken shelter at:{array5[num2]} (shelter index {GetAbstractRoom(array5[num2]).shelterIndex})");
						brokenShelters[GetAbstractRoom(array5[num2]).shelterIndex] = true;
					}
				}
			}
		}
		if (flag)
		{
			string path2 = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "Properties.txt");
			List<string> list2 = new List<string> { "" };
			if (File.Exists(path2))
			{
				string[] array7 = File.ReadAllLines(path);
				for (int num3 = 0; num3 < array7.Length; num3++)
				{
					string[] array8 = Regex.Split(Custom.ValidateSpacedDelimiter(array7[num3], ":"), ": ");
					if (array8.Length == 2 && array8[0] == "Subregion")
					{
						list2.Add(array8[1]);
					}
				}
				for (int num4 = 0; num4 < NumberOfRooms; num4++)
				{
					if (abstractRooms[num4].subregionName == null)
					{
						continue;
					}
					for (int num5 = 1; num5 < list2.Count; num5++)
					{
						if (abstractRooms[num4].subregionName == list2[num5] && num5 < list.Count)
						{
							abstractRooms[num4].altSubregionName = list[num5];
							break;
						}
					}
				}
			}
		}
		Vector2 vector = new Vector2(float.MaxValue, float.MaxValue);
		for (int num6 = 0; num6 < NumberOfRooms; num6++)
		{
			if (abstractRooms[num6].mapPos.x - (float)abstractRooms[num6].size.x * 3f * 0.5f < vector.x)
			{
				vector.x = abstractRooms[num6].mapPos.x - (float)abstractRooms[num6].size.x * 3f * 0.5f;
			}
			if (abstractRooms[num6].mapPos.y - (float)abstractRooms[num6].size.y * 3f * 0.5f < vector.y)
			{
				vector.y = abstractRooms[num6].mapPos.y - (float)abstractRooms[num6].size.y * 3f * 0.5f;
			}
		}
		for (int num7 = 0; num7 < NumberOfRooms; num7++)
		{
			abstractRooms[num7].mapPos -= vector;
			if (DisabledMapRooms.Contains(abstractRooms[num7].name))
			{
				for (int num8 = 0; num8 < abstractRooms[num7].roomAttractions.Length; num8++)
				{
					abstractRooms[num7].roomAttractions[num8] = AbstractRoom.CreatureRoomAttraction.Forbidden;
				}
			}
		}
	}

	private void GenerateOffScreenDen(int index)
	{
		offScreenDen = new AbstractRoom("OffScreenDen_" + name, new int[0], firstRoomIndex + index, -1, -1, -1);
		List<AbstractRoomNode> list = new List<AbstractRoomNode>();
		AbstractRoomNode.Type[] array = new AbstractRoomNode.Type[5]
		{
			AbstractRoomNode.Type.Den,
			AbstractRoomNode.Type.RegionTransportation,
			AbstractRoomNode.Type.SideExit,
			AbstractRoomNode.Type.SkyExit,
			AbstractRoomNode.Type.SeaExit
		};
		foreach (AbstractRoomNode.Type type in array)
		{
			AbstractRoomNode item = new AbstractRoomNode(type, 0, 5, type == AbstractRoomNode.Type.SeaExit, 0, 100);
			for (int j = 0; j < item.connectivity.GetLength(0); j++)
			{
				for (int k = 0; k < item.connectivity.GetLength(1); k++)
				{
					for (int l = 0; l < item.connectivity.GetLength(2); l++)
					{
						item.connectivity[j, k, l] = 1;
					}
				}
			}
			list.Add(item);
		}
		offScreenDen.nodes = list.ToArray();
		offScreenDen.offScreenDen = true;
	}

	public CreatureSpawner GetSpawner(EntityID ID)
	{
		if (ID.spawner < 0 || singleRoomWorld)
		{
			return null;
		}
		int num = ID.spawner % 1000;
		int num2 = (ID.spawner - num) / 1000;
		if (spawners.Length < 1 || num2 != region.regionNumber || num < 0 || num >= spawners.Length)
		{
			return null;
		}
		return spawners[num];
	}

	public int RegionNumberOfSpawner(EntityID ID)
	{
		if (ID.spawner < 0)
		{
			return -1;
		}
		int num = ID.spawner % 1000;
		return (ID.spawner - num) / 1000;
	}

	public float GetAttractionValueForRoom(WorldCoordinate coord, CreatureTemplate.Type tp)
	{
		if (coord.room < firstRoomIndex || coord.room >= firstRoomIndex + NumberOfRooms)
		{
			return 0f;
		}
		return GetAbstractRoom(coord).AttractionValueForCreature(tp);
	}

	public AbstractRoom.CreatureRoomAttraction GetAttractionForRoom(WorldCoordinate coord, CreatureTemplate.Type tp)
	{
		if (coord.room < firstRoomIndex || coord.room >= firstRoomIndex + NumberOfRooms)
		{
			return AbstractRoom.CreatureRoomAttraction.Forbidden;
		}
		return GetAbstractRoom(coord).AttractionForCreature(tp);
	}

	public void LogCreatures()
	{
		logCreatures = false;
		List<string> list = new List<string>();
		for (int i = firstRoomIndex; i < firstRoomIndex + NumberOfRooms; i++)
		{
			for (int j = 0; j < GetAbstractRoom(i).creatures.Count; j++)
			{
				list.Add(GetAbstractRoom(i).name + " " + GetAbstractRoom(i).creatures[j].pos.abstractNode + " " + GetAbstractRoom(i).creatures[j].creatureTemplate.type?.ToString() + " ~ " + PrintSpawner(GetAbstractRoom(i).creatures[j].ID.spawner));
			}
			for (int k = 0; k < GetAbstractRoom(i).entitiesInDens.Count; k++)
			{
				if (GetAbstractRoom(i).entitiesInDens[k] is AbstractCreature)
				{
					list.Add(GetAbstractRoom(i).name + " " + GetAbstractRoom(i).entitiesInDens[k].pos.abstractNode + " " + (GetAbstractRoom(i).entitiesInDens[k] as AbstractCreature).creatureTemplate.type?.ToString() + " ~ " + PrintSpawner(GetAbstractRoom(i).entitiesInDens[k].ID.spawner));
				}
			}
		}
		list.Sort();
		using StreamWriter streamWriter = File.CreateText(Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "loggedCreatures.txt").ToLowerInvariant());
		for (int l = 0; l < list.Count; l++)
		{
			streamWriter.Write(list[l] + "\r\n");
		}
	}

	private string PrintSpawner(int spawner)
	{
		if (spawner < 0 || spawner >= spawners.Length || spawners[spawner] == null)
		{
			return "no spawner";
		}
		return spawners[spawner].ToString();
	}

	public int SpawnPupNPCs()
	{
		if (game.world.singleRoomWorld || game.rainWorld.safariMode || game.wasAnArtificerDream || !game.IsStorySession || game.GetStorySession.Players.Count == 0)
		{
			return 0;
		}
		int num = 0;
		int num2 = 0;
		AbstractRoom room = game.GetStorySession.Players[0].Room;
		foreach (AbstractCreature creature in room.creatures)
		{
			if (creature.state.alive && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				num++;
				num2++;
			}
		}
		Random.State state = Random.state;
		game.GetStorySession.SetRandomSeedToCycleSeed(region.regionNumber);
		if (Random.value >= region.regionParams.slugPupSpawnChance && game.GetStorySession.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Sofanthiel && game.GetStorySession.saveState.forcePupsNextCycle != 1)
		{
			Random.state = state;
			return num;
		}
		if (num < game.GetStorySession.slugPupMaxCount)
		{
			AbstractRoom abstractRoom = null;
			List<AbstractRoom> list = new List<AbstractRoom>();
			AbstractRoom[] array = abstractRooms;
			foreach (AbstractRoom abstractRoom2 in array)
			{
				if (abstractRoom2 != room && abstractRoom2.shelter && abstractRoom2.name != "SU_S05")
				{
					list.Add(abstractRoom2);
				}
			}
			int num3 = 1;
			if (game.GetStorySession.saveState.forcePupsNextCycle == 1)
			{
				abstractRoom = room;
				num3 = game.GetStorySession.slugPupMaxCount - num;
				game.GetStorySession.saveState.forcePupsNextCycle = 2;
			}
			else
			{
				if (list.Count == 0)
				{
					Custom.LogWarning("No shelters for pup spawns");
					return num;
				}
				if (list.Count == 1)
				{
					Custom.LogWarning("only a SINGLE shelter for pup spawns");
					abstractRoom = list[0];
				}
				else
				{
					abstractRoom = list[Random.Range(0, list.Count)];
				}
			}
			for (int j = 0; j < num3; j++)
			{
				AbstractCreature abstractCreature = new AbstractCreature(this, StaticWorld.GetCreatureTemplate(MoreSlugcatsEnums.CreatureTemplateType.SlugNPC), null, new WorldCoordinate(abstractRoom.index, -1, -1, 0), game.GetNewID());
				abstractRoom.AddEntity(abstractCreature);
				if (abstractRoom.realizedRoom != null)
				{
					abstractCreature.RealizeInRoom();
				}
				(abstractCreature.state as PlayerNPCState).foodInStomach = 1;
				num++;
				Custom.Log($"Created slugpup! {abstractCreature} at {abstractRoom.name} ({abstractRoom.index})");
			}
		}
		Random.state = state;
		return num2;
	}

	public void LoadWorldForFastTravel(SlugcatStats.Name slugcatNumber, List<AbstractRoom> abstractRoomsList, int[] swarmRooms, int[] shelters, int[] gates)
	{
		GenerateOffScreenDen(abstractRoomsList.Count);
		abstractRoomsList.Add(offScreenDen);
		abstractRooms = abstractRoomsList.ToArray();
		this.shelters = shelters;
		brokenShelters = new bool[shelters.Length];
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + name + ".txt");
		string text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + name + Path.DirectorySeparatorChar + "map_" + name + "-" + slugcatNumber.value + ".txt");
		if (File.Exists(text))
		{
			path = text;
		}
		if (File.Exists(path))
		{
			string[] array = File.ReadAllLines(path);
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
				if (array2.Length != 2)
				{
					continue;
				}
				for (int j = 0; j < NumberOfRooms; j++)
				{
					if (abstractRooms[j].name == array2[0])
					{
						string[] array3 = Regex.Split(array2[1], "><");
						if (array3.Length >= 8)
						{
							abstractRooms[j].size.x = int.Parse(array3[6], NumberStyles.Any, CultureInfo.InvariantCulture);
							abstractRooms[j].size.y = int.Parse(array3[7], NumberStyles.Any, CultureInfo.InvariantCulture);
						}
						break;
					}
				}
			}
		}
		foreach (string disabledMapRoom in DisabledMapRooms)
		{
			AbstractRoom abstractRoom = GetAbstractRoom(disabledMapRoom);
			if (abstractRoom != null)
			{
				DisabledMapIndices.Add(abstractRoom.index);
			}
		}
		LoadMapConfig(slugcatNumber);
		this.swarmRooms = swarmRooms;
		this.gates = gates;
		mostNodesInARoom = 0;
	}

	public static bool CheckForRegionGhost(SlugcatStats.Name slugcatIndex, string regionString)
	{
		GhostWorldPresence.GhostID ghostID = GhostWorldPresence.GetGhostID(regionString);
		if (!ModManager.MSC)
		{
			return ghostID != GhostWorldPresence.GhostID.NoGhost;
		}
		if (ghostID != GhostWorldPresence.GhostID.NoGhost && (ghostID != MoreSlugcatsEnums.GhostID.SL || slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint))
		{
			if (!(ghostID != MoreSlugcatsEnums.GhostID.MS))
			{
				return slugcatIndex == MoreSlugcatsEnums.SlugcatStatsName.Saint;
			}
			return true;
		}
		return false;
	}

	public void ToggleCreatureAccessFromCutscene(string roomName, CreatureTemplate.Type CritterType, bool allowAccess)
	{
		AbstractRoom abstractRoom = GetAbstractRoom(roomName);
		for (int i = 0; i < abstractRoom.roomAttractions.Length; i++)
		{
			if (StaticWorld.creatureTemplates[i].type == CreatureTemplate.Type.Scavenger || (ModManager.MSC && StaticWorld.creatureTemplates[i].type == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite))
			{
				abstractRoom.roomAttractions[i] = (allowAccess ? AbstractRoom.CreatureRoomAttraction.Neutral : AbstractRoom.CreatureRoomAttraction.Forbidden);
			}
		}
	}

	public void LogDebugStats()
	{
		_ = abstractRooms.Length;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int i = 0; i < abstractRooms.Length; i++)
		{
			AbstractRoom abstractRoom = abstractRooms[i];
			num4 += abstractRoom.nodes.Length;
			foreach (AbstractCreature creature in abstractRoom.creatures)
			{
				num2++;
				if (creature.realizedCreature != null)
				{
					num3++;
				}
			}
			if (abstractRoom.realizedRoom != null)
			{
				num++;
			}
		}
	}
}
