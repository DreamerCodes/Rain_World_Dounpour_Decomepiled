using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public abstract class PathFinder : AIModule
{
	public class PathingCell
	{
		public int generation;

		public PathCost heuristicValue;

		public PathCost costToGoal;

		public bool inCheckNextList;

		public bool reachable;

		public bool possibleToGetBackFrom;

		public WorldCoordinate worldCoordinate;

		public PathingCell(WorldCoordinate worldCoordinate)
		{
			this.worldCoordinate = worldCoordinate;
			generation = -1;
			heuristicValue = new PathCost(123456f * UnityEngine.Random.value, PathCost.Legality.Allowed);
			costToGoal = new PathCost(123456f * UnityEngine.Random.value, PathCost.Legality.Allowed);
			reachable = false;
			possibleToGetBackFrom = false;
			inCheckNextList = false;
		}
	}

	protected class AccessibilityMapper
	{
		public List<PathFinder> clients;

		private readonly List<WorldCoordinate> accessibilityCheckNext;

		protected int mapAccessibilityMode;

		public bool finished;

		private IntVector2[] alreadyCheckedTiles;

		public bool taintedByForbiddenNode;

		public Room mappingRoom;

		private List<PathFinder> potentialMergers = new List<PathFinder>();

		private PathFinder parent => clients[0];

		public static bool CreaturesPathingIdentical(AbstractCreature A, AbstractCreature B)
		{
			return A.creatureTemplate.PreBakedPathingIndex == B.creatureTemplate.PreBakedPathingIndex;
		}

		public AccessibilityMapper(PathFinder initiator, WorldCoordinate accessibilityCenter, IntVector2[] alreadyCheckedTiles, bool taintedByForbiddenNode)
		{
			accessibilityCheckNext = new List<WorldCoordinate>(500);
			this.taintedByForbiddenNode = taintedByForbiddenNode;
			mappingRoom = initiator.realizedRoom;
			initiator.accessibilityCenter = accessibilityCenter;
			clients = new List<PathFinder> { initiator };
			this.alreadyCheckedTiles = alreadyCheckedTiles;
			mapAccessibilityMode = 1;
			UpdateAccessibilityMapping();
			finished = false;
		}

		public void UpdateAccessibilityMapping()
		{
			switch (mapAccessibilityMode)
			{
			case 1:
			{
				for (int j = 0; j < parent.WorldCells.Length; j++)
				{
					for (int k = 0; k < parent.WorldCells[j].Length; k++)
					{
						parent.WorldCells[j][k].reachable = false;
						parent.WorldCells[j][k].possibleToGetBackFrom = false;
					}
				}
				if (mappingRoom != null)
				{
					for (int l = 0; l < parent.CurrentRoomCells.GetLength(0); l++)
					{
						for (int m = 0; m < parent.CurrentRoomCells.GetLength(1); m++)
						{
							parent.CurrentRoomCells[l, m].reachable = false;
							parent.CurrentRoomCells[l, m].possibleToGetBackFrom = false;
						}
					}
				}
				accessibilityCheckNext.Clear();
				accessibilityCheckNext.Add(parent.accessibilityCenter);
				if (alreadyCheckedTiles != null)
				{
					for (int n = 0; n < alreadyCheckedTiles.Length; n++)
					{
						IntVector2 intVector = alreadyCheckedTiles[n];
						accessibilityCheckNext.Add(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector.x, intVector.y, -1));
						parent.PathingCellAtWorldCoordinate(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector.x, intVector.y, -1)).reachable = true;
						parent.PathingCellAtWorldCoordinate(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector.x, intVector.y, -1)).possibleToGetBackFrom = true;
						parent.PathingCellAtWorldCoordinate(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector.x, intVector.y, -1)).costToGoal = new PathCost(alreadyCheckedTiles.Length - n, PathCost.Legality.Allowed);
						parent.PathingCellAtWorldCoordinate(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector.x, intVector.y, -1)).generation = parent.pathGeneration;
						if (parent.debugDrawer != null)
						{
							parent.debugDrawer.GetBackAble(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector.x, intVector.y, -1), reachAbleAlso: true);
						}
					}
				}
				mapAccessibilityMode = 2;
				UpdatePotentialMergers();
				break;
			}
			case 2:
			{
				if (accessibilityCheckNext.Count < 1)
				{
					mapAccessibilityMode = 3;
					break;
				}
				WorldCoordinate coord2 = accessibilityCheckNext[0];
				accessibilityCheckNext.RemoveAt(0);
				int num5 = 0;
				while (true)
				{
					MovementConnection movementConnection2 = parent.ConnectionAtCoordinate(outGoing: true, coord2, num5);
					num5++;
					if (movementConnection2 == default(MovementConnection))
					{
						break;
					}
					if (!parent.PathingCellAtWorldCoordinate(movementConnection2.destinationCoord).reachable && mappingRoom.aimap.IsConnectionAllowedForCreature(movementConnection2, parent.creatureType) && (movementConnection2.startCoord.room == movementConnection2.destinationCoord.room || parent.creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).Allowed) && parent.CoordinateCost(movementConnection2.destinationCoord).Allowed)
					{
						accessibilityCheckNext.Add(movementConnection2.destinationCoord);
						for (int num6 = 0; num6 < clients.Count; num6++)
						{
							clients[num6].PathingCellAtWorldCoordinate(movementConnection2.destinationCoord).reachable = true;
						}
						if (parent.debugDrawer != null)
						{
							parent.debugDrawer.Reachable(movementConnection2.destinationCoord);
						}
					}
				}
				break;
			}
			case 3:
				accessibilityCheckNext.Clear();
				accessibilityCheckNext.Add(parent.accessibilityCenter);
				if (alreadyCheckedTiles != null)
				{
					IntVector2[] array = alreadyCheckedTiles;
					for (int num3 = 0; num3 < array.Length; num3++)
					{
						IntVector2 intVector2 = array[num3];
						accessibilityCheckNext.Add(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector2.x, intVector2.y, -1));
						for (int num4 = 0; num4 < clients.Count; num4++)
						{
							clients[num4].PathingCellAtWorldCoordinate(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector2.x, intVector2.y, -1)).reachable = true;
							clients[num4].PathingCellAtWorldCoordinate(new WorldCoordinate(mappingRoom.abstractRoom.index, intVector2.x, intVector2.y, -1)).possibleToGetBackFrom = true;
						}
					}
				}
				mapAccessibilityMode = 4;
				UpdatePotentialMergers();
				break;
			case 4:
			{
				if (accessibilityCheckNext.Count < 1)
				{
					mapAccessibilityMode = 5;
					break;
				}
				WorldCoordinate coord = accessibilityCheckNext[0];
				accessibilityCheckNext.RemoveAt(0);
				int num = 0;
				while (true)
				{
					MovementConnection movementConnection = parent.ConnectionAtCoordinate(outGoing: false, coord, num);
					num++;
					if (movementConnection == default(MovementConnection))
					{
						break;
					}
					if (!parent.PathingCellAtWorldCoordinate(movementConnection.startCoord).possibleToGetBackFrom && mappingRoom.aimap.IsConnectionAllowedForCreature(movementConnection, parent.creatureType) && (movementConnection.startCoord.room == movementConnection.destinationCoord.room || parent.creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).Allowed) && parent.CoordinateCost(movementConnection.startCoord).Allowed)
					{
						accessibilityCheckNext.Add(movementConnection.startCoord);
						for (int num2 = 0; num2 < clients.Count; num2++)
						{
							clients[num2].PathingCellAtWorldCoordinate(movementConnection.startCoord).possibleToGetBackFrom = true;
						}
						if (parent.debugDrawer != null)
						{
							parent.debugDrawer.GetBackAble(movementConnection.startCoord, parent.PathingCellAtWorldCoordinate(movementConnection.startCoord).reachable);
						}
					}
				}
				break;
			}
			case 5:
			{
				for (int i = 0; i < clients.Count; i++)
				{
					clients[i].AccessibilityMappingDone();
					clients[i].accessibilityMapper = null;
					if (taintedByForbiddenNode)
					{
						clients[i].forbiddenNode = null;
						clients[i].InitiAccessibilityMapping(clients[i].creature.pos, alreadyCheckedTiles);
					}
				}
				break;
			}
			}
			if (mapAccessibilityMode == 2 || mapAccessibilityMode == 4)
			{
				CheckMerge();
			}
		}

		public void CullClients(PathFinder dontReInitiateMe)
		{
			for (int num = clients.Count - 1; num >= 0; num--)
			{
				bool showLogs = RainWorld.ShowLogs;
				if (clients[num].realizedRoom != mappingRoom)
				{
					if (clients[num].accessibilityMapper == this)
					{
						clients[num].accessibilityMapper = null;
						if (clients[num] != dontReInitiateMe)
						{
							clients[num].InitiAccessibilityMapping(clients[num].creature.pos, new IntVector2[0]);
							if (showLogs)
							{
								Custom.Log("~~~~Telling client in different room to restart acc mapping");
							}
						}
						else
						{
							Custom.Log("~~~ refraining from mapper re-init");
						}
					}
					clients.RemoveAt(num);
					if (showLogs)
					{
						Custom.Log("~~~~Removing client in different room ", clients.Count.ToString());
					}
				}
			}
		}

		private void UpdatePotentialMergers()
		{
			potentialMergers.Clear();
			if (mappingRoom == null || taintedByForbiddenNode)
			{
				return;
			}
			for (int i = 0; i < mappingRoom.abstractRoom.creatures.Count; i++)
			{
				if (!CreaturesPathingIdentical(parent.creature, mappingRoom.abstractRoom.creatures[i]) || mappingRoom.abstractRoom.creatures[i].abstractAI == null || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI == null || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder == null || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.accessibilityMapper == null || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.accessibilityMapper == this || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.realizedRoom != mappingRoom || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.accessibilityMapper.mapAccessibilityMode != mapAccessibilityMode || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.accessibilityMapper.taintedByForbiddenNode || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.WorldCells.Length != parent.WorldCells.Length || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.CurrentRoomCells.GetLength(0) != parent.CurrentRoomCells.GetLength(0) || mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder.CurrentRoomCells.GetLength(1) != parent.CurrentRoomCells.GetLength(1))
				{
					continue;
				}
				bool flag = true;
				for (int j = 0; j < clients.Count; j++)
				{
					if (clients[j] == mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder)
					{
						flag = false;
						break;
					}
				}
				for (int k = 0; k < potentialMergers.Count; k++)
				{
					if (potentialMergers[k] == mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder)
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					potentialMergers.Add(mappingRoom.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder);
				}
			}
		}

		private void CheckMerge()
		{
			for (int num = potentialMergers.Count - 1; num >= 0; num--)
			{
				if (potentialMergers[num].accessibilityMapper == null || potentialMergers[num].realizedRoom != mappingRoom || potentialMergers[num].accessibilityMapper.mapAccessibilityMode != mapAccessibilityMode)
				{
					potentialMergers.RemoveAt(num);
				}
				else if ((mapAccessibilityMode == 2 && parent.CoordinateReachable(potentialMergers[num].accessibilityCenter)) || (mapAccessibilityMode == 4 && parent.CoordinateReachableAndGetbackable(potentialMergers[num].accessibilityCenter)))
				{
					AccessibilityMapper accessibilityMapper = potentialMergers[num].accessibilityMapper;
					potentialMergers.RemoveAt(num);
					Absorb(accessibilityMapper);
				}
			}
		}

		private void Absorb(AccessibilityMapper otherMapper)
		{
			if (otherMapper == this)
			{
				return;
			}
			CullClients(null);
			otherMapper.CullClients(null);
			if (clients.Count == 0 || otherMapper.clients.Count == 0)
			{
				Custom.Log("cancel mapper merge:", clients.Count.ToString(), otherMapper.clients.Count.ToString());
				return;
			}
			for (int i = 0; i < otherMapper.clients.Count; i++)
			{
				otherMapper.clients[i].accessibilityMapper = this;
				if (!clients.Contains(otherMapper.clients[i]))
				{
					clients.Add(otherMapper.clients[i]);
				}
			}
			for (int j = 0; j < parent.WorldCells.Length && j < otherMapper.parent.WorldCells.Length; j++)
			{
				for (int k = 0; k < parent.WorldCells[j].Length && k < otherMapper.parent.WorldCells[j].Length; k++)
				{
					for (int l = 0; l < clients.Count; l++)
					{
						clients[l].WorldCells[j][k].reachable = parent.WorldCells[j][k].reachable || otherMapper.parent.WorldCells[j][k].reachable;
						clients[l].WorldCells[j][k].possibleToGetBackFrom = parent.WorldCells[j][k].possibleToGetBackFrom || otherMapper.parent.WorldCells[j][k].possibleToGetBackFrom;
					}
				}
			}
			if (mappingRoom != null && otherMapper.mappingRoom != null)
			{
				for (int m = 0; m < parent.CurrentRoomCells.GetLength(0) && m < otherMapper.parent.CurrentRoomCells.GetLength(0); m++)
				{
					for (int n = 0; n < parent.CurrentRoomCells.GetLength(1) && n < otherMapper.parent.CurrentRoomCells.GetLength(1); n++)
					{
						for (int num = 0; num < clients.Count; num++)
						{
							clients[num].CurrentRoomCells[m, n].reachable = parent.CurrentRoomCells[m, n].reachable || otherMapper.parent.CurrentRoomCells[m, n].reachable;
							clients[num].CurrentRoomCells[m, n].possibleToGetBackFrom = parent.CurrentRoomCells[m, n].possibleToGetBackFrom || otherMapper.parent.CurrentRoomCells[m, n].possibleToGetBackFrom;
						}
					}
				}
			}
			for (int num2 = 0; num2 < otherMapper.accessibilityCheckNext.Count; num2++)
			{
				if (!accessibilityCheckNext.Contains(otherMapper.accessibilityCheckNext[num2]))
				{
					accessibilityCheckNext.Add(otherMapper.accessibilityCheckNext[num2]);
				}
			}
			Custom.Log("Mapper merging", clients.Count.ToString(), "clients");
		}

		public void AddFreshClient(PathFinder newClient)
		{
			int num = 0;
			for (int i = 0; i < parent.WorldCells.Length && i < newClient.WorldCells.Length; i++)
			{
				if (num >= 1000000)
				{
					break;
				}
				for (int j = 0; j < parent.WorldCells[i].Length && j < newClient.WorldCells[i].Length; j++)
				{
					if (num >= 1000000)
					{
						break;
					}
					newClient.WorldCells[i][j].reachable = parent.WorldCells[i][j].reachable;
					newClient.WorldCells[i][j].possibleToGetBackFrom = parent.WorldCells[i][j].possibleToGetBackFrom;
					num++;
				}
			}
			if (mappingRoom != null && newClient.realizedRoom != null)
			{
				for (int k = 0; k < parent.CurrentRoomCells.GetLength(0) && k < newClient.CurrentRoomCells.GetLength(0); k++)
				{
					if (num >= 1000000)
					{
						break;
					}
					for (int l = 0; l < parent.CurrentRoomCells.GetLength(1) && l < newClient.CurrentRoomCells.GetLength(1); l++)
					{
						if (num >= 1000000)
						{
							break;
						}
						newClient.CurrentRoomCells[k, l].reachable = parent.CurrentRoomCells[k, l].reachable;
						newClient.CurrentRoomCells[k, l].possibleToGetBackFrom = parent.CurrentRoomCells[k, l].possibleToGetBackFrom;
						num++;
					}
				}
			}
			if (num >= 1000000)
			{
				Custom.LogWarning("!!!!!!!!BAD LOOP!");
			}
			clients.Add(newClient);
			Custom.Log("client add to mapper:", clients.Count.ToString());
			newClient.accessibilityMapper = this;
		}
	}

	protected WorldCoordinate destination;

	protected WorldCoordinate? nextDestination;

	protected WorldCoordinate currentlyFollowingDestination;

	public int pathGeneration;

	public int creatureFollowingGeneration;

	public int goalFoundGeneration;

	protected List<PathingCell> checkNextList;

	public IntRect coveredArea;

	protected int acceptablePathAge;

	protected PathCost minimumPossiblePathCost;

	public PathingCell fallbackPathingCell;

	protected bool reAssignDestinationOnceAccessibilityMappingIsDone;

	public WorldCoordinate? forbiddenNode;

	protected PathingCell[][] WorldCells;

	protected AItile[][] WorldAITiles;

	protected PathingCell[,] CurrentRoomCells;

	protected Room realizedRoom;

	protected int room;

	protected World world;

	public readonly AbstractCreature creature;

	protected WorldCoordinate accessibilityCenter;

	protected AccessibilityMapper accessibilityMapper;

	public PathfindingVisualizer debugDrawer;

	public bool visualize;

	private PathfinderResourceDivider pathfinderResourceDivider;

	public int stepsPerFrame;

	public int accessibilityStepsPerFrame;

	public WorldCoordinate? nonShortcutRoomEntrancePos;

	public bool walkPastPointOfNoReturn;

	public WorldCoordinate forbiddenEntrance = new WorldCoordinate(-1, -1, -1, -1);

	public int forbiddenEntranceCounter;

	public bool lookingForImpossiblePath { get; protected set; }

	public bool DoneMappingAccessibility => accessibilityMapper == null;

	public WorldCoordinate creaturePos => creature.pos;

	public CreatureTemplate creatureType => creature.creatureTemplate;

	public WorldCoordinate GetDestination
	{
		get
		{
			if (nextDestination.HasValue)
			{
				return nextDestination.Value;
			}
			return destination;
		}
	}

	public WorldCoordinate GetEffectualDestination => currentlyFollowingDestination;

	public bool DestInRoom => InThisRealizedRoom(GetDestination);

	public PathFinder(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI)
	{
		this.world = world;
		this.creature = creature;
		pathfinderResourceDivider = world.game.pathfinderResourceDivider;
		destination = creature.abstractAI.destination;
		if (!destination.TileDefined && creature.pos.NodeDefined)
		{
			destination = creature.pos;
		}
		nextDestination = destination;
		currentlyFollowingDestination = destination;
		acceptablePathAge = 5;
		creatureFollowingGeneration = -2;
		stepsPerFrame = 10;
		accessibilityStepsPerFrame = 10;
		checkNextList = new List<PathingCell>(100);
		fallbackPathingCell = new PathingCell(new WorldCoordinate(-1, -1, -1, -1));
		fallbackPathingCell.possibleToGetBackFrom = false;
		fallbackPathingCell.reachable = false;
		SetUpWorld();
		checkNextList.Clear();
		pathGeneration = 1;
	}

	private void SetUpWorld()
	{
		WorldCells = new PathingCell[world.NumberOfRooms][];
		WorldAITiles = new AItile[world.NumberOfRooms][];
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			WorldCells[i] = new PathingCell[world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length];
			WorldAITiles[i] = new AItile[world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length];
			for (int j = 0; j < world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length; j++)
			{
				WorldCells[i][j] = new PathingCell(new WorldCoordinate(i + world.firstRoomIndex, -1, -1, j));
				WorldAITiles[i][j] = new AItile(AItile.Accessibility.OffScreen, 0);
			}
		}
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		List<WorldCoordinate> list2 = new List<WorldCoordinate>();
		List<WorldCoordinate> list3 = new List<WorldCoordinate>();
		List<WorldCoordinate> list4 = new List<WorldCoordinate>();
		for (int k = 0; k < world.NumberOfRooms; k++)
		{
			for (int l = 0; l < world.GetAbstractRoom(k + world.firstRoomIndex).nodes.Length; l++)
			{
				if (!world.DisabledMapIndices.Contains(k + world.firstRoomIndex))
				{
					ConnectAITile(k + world.firstRoomIndex, l);
					if (creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.SideHighway).Allowed && world.GetAbstractRoom(k + world.firstRoomIndex).nodes[l].type == AbstractRoomNode.Type.SideExit)
					{
						list.Add(new WorldCoordinate(k + world.firstRoomIndex, -1, -1, l));
					}
					if (creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.SkyHighway).Allowed && world.GetAbstractRoom(k + world.firstRoomIndex).nodes[l].type == AbstractRoomNode.Type.SkyExit)
					{
						list2.Add(new WorldCoordinate(k + world.firstRoomIndex, -1, -1, l));
					}
					if (creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.SeaHighway).Allowed && world.GetAbstractRoom(k + world.firstRoomIndex).nodes[l].type == AbstractRoomNode.Type.SeaExit)
					{
						list3.Add(new WorldCoordinate(k + world.firstRoomIndex, -1, -1, l));
					}
					if (creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.RegionTransportation).Allowed && world.GetAbstractRoom(k + world.firstRoomIndex).nodes[l].type == AbstractRoomNode.Type.RegionTransportation)
					{
						list4.Add(new WorldCoordinate(k + world.firstRoomIndex, -1, -1, l));
					}
				}
			}
		}
		if (creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.SideHighway).Allowed)
		{
			for (int m = 0; m < list.Count; m++)
			{
				for (int n = 0; n < list.Count; n++)
				{
					if (m != n)
					{
						AITileAtWorldCoordinate(list[m]).outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.SideHighway, list[m], list[n], world.SideHighwayDistanceBetweenNodes(list[m], list[n])));
						AITileAtWorldCoordinate(list[m]).incomingPaths.Add(new MovementConnection(MovementConnection.MovementType.SideHighway, list[n], list[m], world.SideHighwayDistanceBetweenNodes(list[n], list[m])));
					}
				}
			}
		}
		if (creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.SkyHighway).Allowed)
		{
			for (int num = 0; num < list2.Count; num++)
			{
				for (int num2 = 0; num2 < list2.Count; num2++)
				{
					if (num != num2)
					{
						AITileAtWorldCoordinate(list2[num]).outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.SkyHighway, list2[num], list2[num2], world.SkyHighwayDistanceBetweenNodes(list2[num], list2[num2])));
						AITileAtWorldCoordinate(list2[num]).incomingPaths.Add(new MovementConnection(MovementConnection.MovementType.SkyHighway, list2[num2], list2[num], world.SkyHighwayDistanceBetweenNodes(list2[num2], list2[num])));
					}
				}
			}
		}
		if (creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.SeaHighway).Allowed)
		{
			for (int num3 = 0; num3 < list3.Count; num3++)
			{
				for (int num4 = 0; num4 < list3.Count; num4++)
				{
					if (num3 != num4)
					{
						AITileAtWorldCoordinate(list3[num3]).outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.SeaHighway, list3[num3], list3[num4], world.SeaHighwayDistanceBetweenNodes(list3[num3], list3[num4])));
						AITileAtWorldCoordinate(list3[num3]).incomingPaths.Add(new MovementConnection(MovementConnection.MovementType.SeaHighway, list3[num4], list3[num3], world.SeaHighwayDistanceBetweenNodes(list3[num4], list3[num3])));
					}
				}
			}
		}
		if (!creature.creatureTemplate.ConnectionResistance(MovementConnection.MovementType.RegionTransportation).Allowed)
		{
			return;
		}
		for (int num5 = 0; num5 < list4.Count; num5++)
		{
			for (int num6 = 0; num6 < list4.Count; num6++)
			{
				if (num5 != num6)
				{
					AITileAtWorldCoordinate(list4[num5]).outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.RegionTransportation, list4[num5], list4[num6], world.RegionTransportationDistanceBetweenNodes(list4[num5], list4[num6])));
					AITileAtWorldCoordinate(list4[num5]).incomingPaths.Add(new MovementConnection(MovementConnection.MovementType.RegionTransportation, list4[num6], list4[num5], world.RegionTransportationDistanceBetweenNodes(list4[num6], list4[num5])));
				}
			}
		}
	}

	public override void Update()
	{
		if (world == null)
		{
			Custom.LogWarning($"DANGER the world of pathfinder inside {AI.creature} was NULL!");
			Reset(AI.creature.realizedCreature.room);
		}
		else if (world != AI.creature.world)
		{
			if (AI.creature.creatureTemplate.type.value == "SlugNPC")
			{
				Custom.LogWarning("DANGER the world of pathfinder inside", AI.creature.creatureTemplate.type.value, "was not the same world as its owner! setting world to current world", AI.creature.world.ToString());
				AI.creature.world = world;
			}
			else
			{
				Custom.LogWarning("DANGER the world of pathfinder inside", AI.creature.creatureTemplate.type.value, "was not the same world as its owner!", AI.creature.world.ToString());
				Reset(AI.creature.realizedCreature.room);
			}
		}
		PathingCellAtWorldCoordinate(destination).costToGoal.resistance = 0f;
		bool flag = false;
		if (accessibilityMapper != null)
		{
			flag = true;
			int num = pathfinderResourceDivider.RequesAccesibilityUpdates(accessibilityStepsPerFrame);
			while (num > 0 && accessibilityMapper != null)
			{
				accessibilityMapper.UpdateAccessibilityMapping();
				num--;
			}
		}
		else if (forbiddenEntranceCounter > 0)
		{
			forbiddenEntranceCounter--;
		}
		for (int num2 = (flag ? 1 : pathfinderResourceDivider.RequestPathfinderUpdates(stepsPerFrame)); num2 > 0; num2--)
		{
			if (checkNextList.Count > 0)
			{
				PathingCell pathingCell = checkNextList[0];
				checkNextList.RemoveAt(0);
				pathingCell.inCheckNextList = false;
				if (debugDrawer != null)
				{
					debugDrawer.CellChecked(pathingCell.worldCoordinate, pathGeneration);
				}
				CheckNeighbours(pathingCell);
				if (minimumPossiblePathCost < pathingCell.costToGoal)
				{
					minimumPossiblePathCost = pathingCell.costToGoal;
				}
				if (creatureFollowingGeneration == pathGeneration)
				{
					AbortCurrentGenerationPathFinding();
					currentlyFollowingDestination = destination;
				}
				else if (pathingCell.worldCoordinate.CompareDisregardingNode(creature.pos))
				{
					currentlyFollowingDestination = destination;
				}
			}
			else if (nextDestination.HasValue)
			{
				AssignNewDestination(nextDestination.Value);
				nextDestination = null;
			}
			else if (creatureFollowingGeneration < pathGeneration - 1 && !reAssignDestinationOnceAccessibilityMappingIsDone)
			{
				AssignNewDestination(destination);
			}
		}
	}

	protected void CheckNeighbours(PathingCell checkNow)
	{
		int num = 0;
		while (true)
		{
			MovementConnection movementConnection = ConnectionAtCoordinate(outGoing: false, checkNow.worldCoordinate, num);
			num++;
			if (movementConnection == default(MovementConnection))
			{
				break;
			}
			PathingCell pathingCell = PathingCellAtWorldCoordinate(movementConnection.startCoord);
			if (!pathingCell.worldCoordinate.TileDefined && !pathingCell.worldCoordinate.NodeDefined)
			{
				continue;
			}
			PathCost pathCost = CheckConnectionCost(pathingCell, checkNow, movementConnection, followingPath: false);
			if ((!pathCost.Allowed || !pathingCell.reachable) && (!lookingForImpossiblePath || !pathCost.Considerable))
			{
				continue;
			}
			PathCost pathCost2 = checkNow.costToGoal + pathCost;
			PathCost pathCost3 = HeuristicForCell(pathingCell, pathCost2);
			if (pathingCell.generation == pathGeneration)
			{
				if (pathingCell.inCheckNextList)
				{
					if (pathingCell.heuristicValue > pathCost3)
					{
						checkNextList.Remove(pathingCell);
						pathingCell.heuristicValue = pathCost3;
						AddToCheckNextList(pathingCell);
					}
					if (pathingCell.costToGoal > pathCost2)
					{
						pathingCell.costToGoal = pathCost2;
					}
				}
			}
			else if (pathingCell.generation < pathGeneration)
			{
				pathingCell.costToGoal = pathCost2;
				pathingCell.heuristicValue = pathCost3;
				pathingCell.inCheckNextList = true;
				pathingCell.generation = pathGeneration;
				AddToCheckNextList(pathingCell);
			}
		}
	}

	protected void LeavingRoom()
	{
		if (debugDrawer != null)
		{
			debugDrawer.CleanseSprites();
		}
		if (nextDestination.HasValue && !nextDestination.Value.NodeDefined)
		{
			destination = QuickConnectivity.DefineNodeOfLocalCoordinate(nextDestination.Value, world, creatureType);
		}
		else if (!destination.NodeDefined)
		{
			destination = QuickConnectivity.DefineNodeOfLocalCoordinate(destination, world, creatureType);
		}
		nextDestination = null;
	}

	public override void NewRoom(Room newRealizedRoom)
	{
		if (realizedRoom != newRealizedRoom)
		{
			Reset(newRealizedRoom);
		}
	}

	public void Reset(Room newRealizedRoom)
	{
		currentlyFollowingDestination = creature.pos;
		realizedRoom = newRealizedRoom;
		room = realizedRoom.abstractRoom.index;
		checkNextList.Clear();
		int num = realizedRoom.TileWidth;
		int num2 = 0;
		int num3 = 0;
		int num4 = realizedRoom.TileHeight;
		bool flag = false;
		for (int i = 0; i < realizedRoom.TileWidth; i++)
		{
			for (int j = 0; j < realizedRoom.TileHeight; j++)
			{
				if (realizedRoom == null)
				{
					Custom.LogWarning("REALIZED ROOM NULL !!");
				}
				else if (realizedRoom.aimap == null)
				{
					Custom.LogWarning("AIMAP NULL FOR ROOM", realizedRoom.abstractRoom.name);
				}
				if (realizedRoom.aimap.TileAccessibleToCreature(i, j, creatureType))
				{
					flag = true;
					if (i < num)
					{
						num = i;
					}
					if (j < num4)
					{
						num4 = j;
					}
					if (i > num3)
					{
						num3 = i;
					}
					if (j > num2)
					{
						num2 = j;
					}
				}
			}
		}
		if (!flag)
		{
			num = 0;
			num4 = 0;
			num3 = 1;
			num2 = 1;
		}
		coveredArea = new IntRect(num, num4, num3, num2);
		num3 = realizedRoom.TileWidth - num3 - 1;
		num2 = realizedRoom.TileHeight - num2 - 1;
		CurrentRoomCells = new PathingCell[realizedRoom.TileWidth - num3 - num, realizedRoom.TileHeight - num2 - num4];
		for (int k = 0; k < realizedRoom.TileWidth - num3 - num; k++)
		{
			for (int l = 0; l < realizedRoom.TileHeight - num2 - num4; l++)
			{
				CurrentRoomCells[k, l] = new PathingCell(new WorldCoordinate(room, k + num, l + num4, -1));
			}
		}
		if (visualize && realizedRoom.TileWidth * realizedRoom.TileHeight < 16000)
		{
			debugDrawer = new PathfindingVisualizer(world, realizedRoom, this, CurrentRoomCells.GetLength(0), CurrentRoomCells.GetLength(1), new IntVector2(coveredArea.left, coveredArea.bottom));
		}
		if (accessibilityMapper != null)
		{
			accessibilityMapper.CullClients(this);
		}
		accessibilityMapper = null;
		if (nonShortcutRoomEntrancePos.HasValue)
		{
			InitiatePath(nonShortcutRoomEntrancePos.Value.abstractNode);
		}
		else if (!creature.pos.TileDefined && creature.pos.NodeDefined)
		{
			InitiatePath(creature.pos.abstractNode);
		}
		else
		{
			InitiAccessibilityMapping(creature.pos, null);
		}
		reAssignDestinationOnceAccessibilityMappingIsDone = true;
	}

	public void ForceNextDestination()
	{
		if (!destination.Equals(nextDestination) && nextDestination.HasValue)
		{
			AssignNewDestination(nextDestination.Value);
		}
	}

	protected void InitiatePath(int theDoorICameInThrough)
	{
		forbiddenEntrance = new WorldCoordinate(realizedRoom.abstractRoom.index, -1, -1, theDoorICameInThrough);
		forbiddenEntranceCounter = 40;
		forbiddenNode = new WorldCoordinate(realizedRoom.abstractRoom.index, -1, -1, theDoorICameInThrough);
		WorldCoordinate entranceCoordinate = realizedRoom.LocalCoordinateOfNode(theDoorICameInThrough);
		if (realizedRoom.abstractRoom.nodes[theDoorICameInThrough].borderExit)
		{
			if (nonShortcutRoomEntrancePos.HasValue && room == nonShortcutRoomEntrancePos.Value.room && theDoorICameInThrough == nonShortcutRoomEntrancePos.Value.abstractNode)
			{
				entranceCoordinate = nonShortcutRoomEntrancePos.Value;
			}
			if (!realizedRoom.aimap.TileAccessibleToCreature(entranceCoordinate.Tile, creature.creatureTemplate))
			{
				RoomBorderExit roomBorderExit = realizedRoom.borderExits[theDoorICameInThrough - realizedRoom.exitAndDenIndex.Length];
				float dst = float.MaxValue;
				IntVector2 intVector = entranceCoordinate.Tile;
				IntVector2[] borderTiles = roomBorderExit.borderTiles;
				for (int i = 0; i < borderTiles.Length; i++)
				{
					IntVector2 intVector2 = borderTiles[i];
					if (realizedRoom.aimap.TileAccessibleToCreature(intVector2, creature.creatureTemplate) && Custom.DistLess(intVector2, entranceCoordinate.Tile, dst))
					{
						dst = intVector2.FloatDist(entranceCoordinate.Tile);
						intVector = intVector2;
					}
				}
				entranceCoordinate.x = intVector.x;
				entranceCoordinate.y = intVector.y;
			}
		}
		WorldCoordinate worldCoordinate = new WorldCoordinate(-1, -1, -1, -1);
		if (InThisRealizedRoom(destination))
		{
			worldCoordinate = destination;
		}
		else
		{
			worldCoordinate.room = room;
			int num = DestinationExit(room, theDoorICameInThrough);
			if (num < 0)
			{
				num = theDoorICameInThrough;
			}
			worldCoordinate = realizedRoom.LocalCoordinateOfNode(num);
		}
		QuickPathFinder quickPathFinder = new QuickPathFinder(entranceCoordinate.Tile, worldCoordinate.Tile, realizedRoom.aimap, creatureType);
		while (quickPathFinder.status == 0)
		{
			quickPathFinder.Update();
		}
		QuickPath quickPath = quickPathFinder.ReturnPath();
		reAssignDestinationOnceAccessibilityMappingIsDone = true;
		InitiAccessibilityMapping(entranceCoordinate, quickPath?.tiles);
	}

	public void InitiAccessibilityMapping(WorldCoordinate entranceCoordinate, IntVector2[] alreadyAccessible)
	{
		if (world.GetAbstractRoom(entranceCoordinate) != null)
		{
			for (int i = 0; i < world.GetAbstractRoom(entranceCoordinate).creatures.Count; i++)
			{
				if (!AccessibilityMapper.CreaturesPathingIdentical(creature, world.GetAbstractRoom(entranceCoordinate).creatures[i]) || creature == world.GetAbstractRoom(entranceCoordinate).creatures[i] || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI == null || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI == null || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder == null || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.realizedRoom == null || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.realizedRoom != realizedRoom || !world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.CoordinateReachableAndGetbackable(entranceCoordinate) || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.WorldCells.Length != WorldCells.Length || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.CurrentRoomCells.GetLength(0) != CurrentRoomCells.GetLength(0) || world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.CurrentRoomCells.GetLength(1) != CurrentRoomCells.GetLength(1))
				{
					continue;
				}
				if (world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.DoneMappingAccessibility)
				{
					Custom.Log($"acc map handover {creature} frm {world.GetAbstractRoom(entranceCoordinate).creatures[i]}");
					PathFinder pathFinder = world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder;
					for (int j = 0; j < WorldCells.Length && j < pathFinder.WorldCells.Length; j++)
					{
						for (int k = 0; k < WorldCells[j].Length && k < pathFinder.WorldCells[j].Length; k++)
						{
							WorldCells[j][k].reachable = pathFinder.WorldCells[j][k].reachable;
							WorldCells[j][k].possibleToGetBackFrom = pathFinder.WorldCells[j][k].possibleToGetBackFrom;
						}
					}
					if (realizedRoom != null && pathFinder.realizedRoom != null)
					{
						for (int l = 0; l < CurrentRoomCells.GetLength(0) && l < pathFinder.CurrentRoomCells.GetLength(0); l++)
						{
							for (int m = 0; m < CurrentRoomCells.GetLength(1) && m < pathFinder.CurrentRoomCells.GetLength(1); m++)
							{
								CurrentRoomCells[l, m].reachable = pathFinder.CurrentRoomCells[l, m].reachable;
								CurrentRoomCells[l, m].possibleToGetBackFrom = pathFinder.CurrentRoomCells[l, m].possibleToGetBackFrom;
							}
						}
					}
					AccessibilityMappingDone();
					return;
				}
				if (world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.accessibilityMapper != null && !world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.accessibilityMapper.taintedByForbiddenNode && world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.forbiddenEntrance == forbiddenEntrance)
				{
					world.GetAbstractRoom(entranceCoordinate).creatures[i].abstractAI.RealAI.pathFinder.accessibilityMapper.AddFreshClient(this);
					return;
				}
			}
		}
		accessibilityMapper = new AccessibilityMapper(this, entranceCoordinate, alreadyAccessible, forbiddenNode.HasValue);
		Custom.Log($"zzz InitAccessibilityMapping. stepsPerFrame:{stepsPerFrame} accessibilityStepsPerFrame:{accessibilityStepsPerFrame} creature:{creature} pathFinder:{this}");
	}

	protected void AccessibilityMappingDone()
	{
		AssignNewDestination(destination);
		bool flag = false;
		if (creature.pos.abstractNode < 0 || creature.pos.abstractNode >= realizedRoom.abstractRoom.nodes.Length || realizedRoom.abstractRoom.nodes[creature.pos.abstractNode].type.Index == -1 || !creature.creatureTemplate.mappedNodeTypes[realizedRoom.abstractRoom.nodes[creature.pos.abstractNode].type.Index])
		{
			List<int> list = new List<int>();
			for (int i = 0; i < realizedRoom.abstractRoom.nodes.Length; i++)
			{
				if (realizedRoom.abstractRoom.nodes[i].type.Index != -1 && creature.creatureTemplate.mappedNodeTypes[realizedRoom.abstractRoom.nodes[i].type.Index] && CoordinateReachableAndGetbackable(new WorldCoordinate(creature.pos.room, -1, -1, i)))
				{
					list.Add(i);
				}
			}
			if (list.Count > 0)
			{
				creature.pos.abstractNode = list[UnityEngine.Random.Range(0, list.Count)];
				Custom.Log($"{creature} has been moved to an accessible node");
			}
		}
		if (!CoordinateReachableAndGetbackable(new WorldCoordinate(creature.pos.room, -1, -1, creature.pos.abstractNode)))
		{
			flag = true;
			for (int j = 0; j < creature.Room.nodes.Length; j++)
			{
				if (CoordinateReachable(new WorldCoordinate(creature.pos.room, -1, -1, j)))
				{
					creature.pos.abstractNode = j;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				for (int k = 0; k < creature.Room.nodes.Length; k++)
				{
					if (realizedRoom.aimap.ExitReachableFromTile(creature.pos.Tile, k, creature.creatureTemplate) || realizedRoom.aimap.ExitReachableFromTile(accessibilityCenter.Tile, k, creature.creatureTemplate))
					{
						creature.pos.abstractNode = k;
						flag = false;
						break;
					}
				}
			}
		}
		if (AI != null)
		{
			AI.NewArea(flag);
		}
	}

	public void RestartPathFinding()
	{
		Custom.Log("Path finding restarted", creature.creatureTemplate.name);
		AssignNewDestination(destination);
	}

	public int DestinationExit(int evaluateRoom, int theDoorICameInThrough)
	{
		int num = -1;
		PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
		int num2 = -1;
		for (int i = 0; i < world.GetAbstractRoom(evaluateRoom).nodes.Length; i++)
		{
			if (i == theDoorICameInThrough || world.GetAbstractRoom(evaluateRoom).nodes[theDoorICameInThrough].ConnectionCost(i, creatureType) <= -1)
			{
				continue;
			}
			PathingCell pathingCell = PathingCellAtWorldCoordinate(new WorldCoordinate(evaluateRoom, -1, -1, i));
			if (pathingCell.reachable)
			{
				if (pathingCell.generation > num)
				{
					num = pathingCell.generation;
					pathCost = pathingCell.costToGoal;
					num2 = i;
				}
				else if (pathingCell.generation == num && pathingCell.costToGoal < pathCost)
				{
					num = pathingCell.generation;
					pathCost = pathingCell.costToGoal;
					num2 = i;
				}
			}
		}
		if (num2 == -1)
		{
			foreach (WorldCoordinate item in creature.abstractAI.path)
			{
				if (item.room == room)
				{
					num2 = item.abstractNode;
				}
			}
		}
		if (num2 == -1)
		{
			AbstractRoom abstractRoom = world.GetAbstractRoom(evaluateRoom);
			for (int j = 0; j < abstractRoom.nodes.Length; j++)
			{
				if ((j == creature.abstractAI.destination.abstractNode && creature.abstractAI.destination.room == evaluateRoom) || (j < abstractRoom.connections.Length && abstractRoom.connections[j] > -1 && creature.abstractAI.path.Contains(new WorldCoordinate(abstractRoom.connections[j], -1, -1, world.GetAbstractRoom(abstractRoom.connections[j]).ExitIndex(evaluateRoom)))))
				{
					num2 = j;
					break;
				}
			}
		}
		return num2;
	}

	protected virtual void DestinationHasChanged(WorldCoordinate oldDestination, WorldCoordinate newDestination)
	{
	}

	public void SetDestination(WorldCoordinate newDestination)
	{
		if (destination.Equals(newDestination) || destination.Equals(nextDestination))
		{
			return;
		}
		if (InThisRealizedRoom(newDestination))
		{
			newDestination.x = Custom.IntClamp(newDestination.x, coveredArea.left, coveredArea.right);
			newDestination.y = Custom.IntClamp(newDestination.y, coveredArea.bottom, coveredArea.top);
			newDestination = FindReachableNeighbourIfPossible(newDestination);
		}
		if (newDestination.room != room && !newDestination.NodeDefined)
		{
			if ((world.GetAbstractRoom(room).realizedRoom == null || !world.GetAbstractRoom(room).realizedRoom.readyForAI) && destination.room == newDestination.room && destination.NodeDefined && world.GetNode(destination).type.Index != -1 && creature.creatureTemplate.mappedNodeTypes[world.GetNode(destination).type.Index])
			{
				newDestination.abstractNode = destination.abstractNode;
			}
			else
			{
				newDestination = QuickConnectivity.DefineNodeOfLocalCoordinate(newDestination, world, creatureType);
			}
		}
		if (destination.Equals(newDestination) || destination.Equals(nextDestination))
		{
			return;
		}
		DestinationHasChanged(destination, newDestination);
		if (InThisRealizedRoom(newDestination))
		{
			if (CoordinateCost(newDestination).Considerable)
			{
				if (PathingCellAtWorldCoordinate(newDestination).reachable && !PathingCellAtWorldCoordinate(destination).reachable)
				{
					AssignNewDestination(newDestination);
				}
				else if (QuickConnectivity.Check(realizedRoom, creatureType, destination.Tile, newDestination.Tile, 100) > -1)
				{
					nextDestination = newDestination;
				}
				else
				{
					AssignNewDestination(newDestination);
				}
			}
		}
		else if (newDestination.NodeDefined)
		{
			AssignNewDestination(newDestination);
		}
	}

	protected WorldCoordinate FindReachableNeighbourIfPossible(WorldCoordinate coord)
	{
		for (int i = 0; i < 9; i++)
		{
			WorldCoordinate worldCoordinate = new WorldCoordinate(coord.room, coord.x + Custom.eightDirectionsAndZero[i].x, coord.y + Custom.eightDirectionsAndZero[i].y, coord.abstractNode);
			if (PathingCellAtWorldCoordinate(worldCoordinate).reachable && Custom.InsideRect(worldCoordinate.Tile, coveredArea))
			{
				return worldCoordinate;
			}
		}
		return coord;
	}

	protected void AssignNewDestination(WorldCoordinate dest)
	{
		destination = dest;
		pathGeneration++;
		forbiddenNode = null;
		checkNextList.Clear();
		if (destination.room != room && !destination.NodeDefined)
		{
			destination = QuickConnectivity.DefineNodeOfLocalCoordinate(destination, world, creature.creatureTemplate);
		}
		minimumPossiblePathCost = CoordinateCost(destination);
		lookingForImpossiblePath = !PathingCellAtWorldCoordinate(destination).reachable;
		AddToCheckNextList(PathingCellAtWorldCoordinate(destination));
		PathingCellAtWorldCoordinate(destination).heuristicValue = new PathCost(0f, minimumPossiblePathCost.legality);
		PathingCellAtWorldCoordinate(destination).costToGoal = new PathCost(0f, minimumPossiblePathCost.legality);
	}

	protected void AddToCheckNextList(PathingCell cell)
	{
		if (cell.Equals(fallbackPathingCell))
		{
			return;
		}
		if (debugDrawer != null)
		{
			debugDrawer.CellAddedToCheckNext(cell.worldCoordinate);
		}
		for (int i = 0; i < checkNextList.Count; i++)
		{
			if (cell.heuristicValue <= checkNextList[i].heuristicValue)
			{
				checkNextList.Insert(i, cell);
				return;
			}
		}
		checkNextList.Add(cell);
	}

	protected void AbortCurrentGenerationPathFinding()
	{
		checkNextList.Clear();
	}

	protected bool InThisRealizedRoom(WorldCoordinate coord)
	{
		if (coord.room == room)
		{
			return coord.TileDefined;
		}
		return false;
	}

	public PathingCell PathingCellAtWorldCoordinate(WorldCoordinate coord)
	{
		if (InThisRealizedRoom(coord))
		{
			if (Custom.InsideRect(coord.Tile, coveredArea))
			{
				return CurrentRoomCells[coord.x - coveredArea.left, coord.y - coveredArea.bottom];
			}
		}
		else if (coord.NodeDefined && coord.room >= world.firstRoomIndex && coord.room < world.firstRoomIndex + world.NumberOfRooms && coord.abstractNode < WorldCells[coord.room - world.firstRoomIndex].Length)
		{
			return WorldCells[coord.room - world.firstRoomIndex][coord.abstractNode];
		}
		return fallbackPathingCell;
	}

	protected AItile AITileAtWorldCoordinate(WorldCoordinate coord)
	{
		if (InThisRealizedRoom(coord))
		{
			return realizedRoom.aimap.getAItile(coord.x, coord.y);
		}
		if (coord.NodeDefined && coord.room >= world.firstRoomIndex && coord.room < world.firstRoomIndex + world.NumberOfRooms && coord.abstractNode < WorldAITiles[coord.room - world.firstRoomIndex].Length)
		{
			return WorldAITiles[coord.room - world.firstRoomIndex][coord.abstractNode];
		}
		return new AItile(AItile.Accessibility.Solid, 0);
	}

	public MovementConnection ConnectionAtCoordinate(bool outGoing, WorldCoordinate coord, int index)
	{
		AItile aItile = AITileAtWorldCoordinate(coord);
		List<MovementConnection> list = null;
		list = ((!outGoing) ? aItile.incomingPaths : aItile.outgoingPaths);
		if (coord.TileDefined)
		{
			if (index < list.Count)
			{
				return list[index];
			}
			if (index == list.Count)
			{
				return FromRealizedToWorldConnection(outGoing, coord);
			}
		}
		else if (coord.NodeDefined)
		{
			if (coord.room == room)
			{
				return ConnectionsOfAbstractNodeInRealizedRoom(outGoing, coord, index);
			}
			if (index < list.Count)
			{
				return list[index];
			}
		}
		return default(MovementConnection);
	}

	protected MovementConnection ConnectionsOfAbstractNodeInRealizedRoom(bool outGoing, WorldCoordinate coord, int index)
	{
		if (coord.abstractNode < realizedRoom.exitAndDenIndex.Length)
		{
			if (index == 0)
			{
				return FromWorldToRealizedRoomShortCutConnection(outGoing, coord);
			}
			if (realizedRoom.abstractRoom.nodes[coord.abstractNode].type == AbstractRoomNode.Type.Exit)
			{
				if (index == 1)
				{
					int num = realizedRoom.abstractRoom.connections[coord.abstractNode];
					if (num == -1)
					{
						return default(MovementConnection);
					}
					if (outGoing)
					{
						return new MovementConnection(MovementConnection.MovementType.ShortCut, coord, new WorldCoordinate(num, -1, -1, world.GetAbstractRoom(num).ExitIndex(room)), 1);
					}
					return new MovementConnection(MovementConnection.MovementType.ShortCut, new WorldCoordinate(num, -1, -1, world.GetAbstractRoom(num).ExitIndex(room)), coord, 1);
				}
				return default(MovementConnection);
			}
			if (realizedRoom.abstractRoom.nodes[coord.abstractNode].type == AbstractRoomNode.Type.RegionTransportation)
			{
				index--;
				AItile aItile = AITileAtWorldCoordinate(coord);
				List<MovementConnection> list = null;
				list = ((!outGoing) ? aItile.incomingPaths : aItile.outgoingPaths);
				if (index >= list.Count)
				{
					return default(MovementConnection);
				}
				MovementConnection result = list[index];
				if (result.type != MovementConnection.MovementType.RegionTransportation)
				{
					return new MovementConnection(MovementConnection.MovementType.OffScreenUnallowed, result.startCoord, result.destinationCoord, 1);
				}
				return result;
			}
		}
		else if (realizedRoom.abstractRoom.nodes[coord.abstractNode].borderExit)
		{
			RoomBorderExit roomBorderExit = realizedRoom.borderExits[coord.abstractNode - realizedRoom.exitAndDenIndex.Length];
			if (index < roomBorderExit.borderTiles.Length)
			{
				IntVector2 intVector = roomBorderExit.borderTiles[index];
				WorldCoordinate worldCoordinate = new WorldCoordinate(room, intVector.x, intVector.y, -1);
				if (outGoing)
				{
					return new MovementConnection(MovementConnection.MovementType.OutsideRoom, coord, worldCoordinate, 10);
				}
				return new MovementConnection(MovementConnection.MovementType.OutsideRoom, worldCoordinate, coord, 10);
			}
			index -= roomBorderExit.borderTiles.Length;
			AItile aItile2 = AITileAtWorldCoordinate(coord);
			List<MovementConnection> list2 = null;
			list2 = ((!outGoing) ? aItile2.incomingPaths : aItile2.outgoingPaths);
			if (index >= list2.Count)
			{
				return default(MovementConnection);
			}
			MovementConnection result2 = list2[index];
			if (result2.type != MovementConnection.MovementType.SideHighway && result2.type != MovementConnection.MovementType.SkyHighway && result2.type != MovementConnection.MovementType.SeaHighway)
			{
				return new MovementConnection(MovementConnection.MovementType.OffScreenUnallowed, result2.startCoord, result2.destinationCoord, 1);
			}
			return result2;
		}
		return default(MovementConnection);
	}

	protected MovementConnection FromRealizedToWorldConnection(bool outGoing, WorldCoordinate coord)
	{
		if (!InThisRealizedRoom(coord))
		{
			return default(MovementConnection);
		}
		if (realizedRoom.GetTile(coord.Tile).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
		{
			ShortcutData shortcutData = realizedRoom.shortcutData(coord.Tile);
			if (shortcutData.shortCutType == ShortcutData.Type.RoomExit || shortcutData.shortCutType == ShortcutData.Type.CreatureHole || shortcutData.shortCutType == ShortcutData.Type.RegionTransportation)
			{
				int abstractNode = realizedRoom.exitAndDenIndex.IndexfOf(shortcutData.DestTile);
				if (outGoing)
				{
					return new MovementConnection(MovementConnection.MovementType.ShortCut, coord, new WorldCoordinate(coord.room, -1, -1, abstractNode), 1);
				}
				return new MovementConnection(MovementConnection.MovementType.ShortCut, new WorldCoordinate(coord.room, -1, -1, abstractNode), coord, 1);
			}
		}
		else if (creature.creatureTemplate.UseAnyRoomBorderExit && (coord.x == 0 || coord.y == 0 || coord.x == realizedRoom.TileWidth - 1 || coord.y == realizedRoom.TileHeight - 1))
		{
			for (int i = 0; i < realizedRoom.borderExits.Length; i++)
			{
				if (realizedRoom.borderExits[i].type.Index == -1 || !creature.creatureTemplate.mappedNodeTypes[realizedRoom.borderExits[i].type.Index])
				{
					continue;
				}
				for (int j = 0; j < realizedRoom.borderExits[i].borderTiles.Length; j++)
				{
					if (realizedRoom.borderExits[i].borderTiles[j] == coord.Tile)
					{
						int abstractNode2 = realizedRoom.exitAndDenIndex.Length + i;
						if (outGoing)
						{
							return new MovementConnection(MovementConnection.MovementType.OutsideRoom, coord, new WorldCoordinate(coord.room, -1, -1, abstractNode2), 10);
						}
						return new MovementConnection(MovementConnection.MovementType.OutsideRoom, new WorldCoordinate(coord.room, -1, -1, abstractNode2), coord, 10);
					}
				}
			}
		}
		return default(MovementConnection);
	}

	protected MovementConnection FromWorldToRealizedRoomShortCutConnection(bool outGoing, WorldCoordinate coord)
	{
		ShortcutData shortcutData = realizedRoom.ShortcutLeadingToNode(coord.abstractNode);
		WorldCoordinate worldCoordinate = new WorldCoordinate(room, shortcutData.startCoord.x, shortcutData.startCoord.y, -1);
		if (outGoing)
		{
			return new MovementConnection(MovementConnection.MovementType.ShortCut, coord, worldCoordinate, 1);
		}
		return new MovementConnection(MovementConnection.MovementType.ShortCut, worldCoordinate, coord, 1);
	}

	protected void OutOfElement(WorldCoordinate ps)
	{
		if (realizedRoom != null && accessibilityMapper == null && realizedRoom.aimap.WorldCoordinateAccessibleToCreature(ps, creatureType) && (!PathingCellAtWorldCoordinate(ps).reachable || !PathingCellAtWorldCoordinate(ps).possibleToGetBackFrom))
		{
			InitiAccessibilityMapping(ps, null);
		}
	}

	protected void OutOfElement()
	{
		OutOfElement(creaturePos);
	}

	public bool CoordinateReachable(WorldCoordinate coord)
	{
		return PathingCellAtWorldCoordinate(coord).reachable;
	}

	public bool CoordinatePossibleToGetBackFrom(WorldCoordinate coord)
	{
		return PathingCellAtWorldCoordinate(coord).possibleToGetBackFrom;
	}

	public bool CoordinateReachableAndGetbackable(WorldCoordinate coord)
	{
		if (PathingCellAtWorldCoordinate(coord).reachable)
		{
			return PathingCellAtWorldCoordinate(coord).possibleToGetBackFrom;
		}
		return false;
	}

	public bool CoordinateViable(WorldCoordinate coord)
	{
		if (walkPastPointOfNoReturn)
		{
			return CoordinateReachable(coord);
		}
		return CoordinateReachableAndGetbackable(coord);
	}

	public bool CoordinateAtCurrentPathingGeneration(WorldCoordinate coord)
	{
		return PathingCellAtWorldCoordinate(coord).generation == pathGeneration;
	}

	protected void ConnectAITile(int room, int node)
	{
		AItile aItile = WorldAITiles[room - world.firstRoomIndex][node];
		AbstractRoomNode abstractRoomNode = world.GetAbstractRoom(room).nodes[node];
		WorldCoordinate startCoord = new WorldCoordinate(room, -1, -1, node);
		if (abstractRoomNode.type == AbstractRoomNode.Type.Exit && !world.singleRoomWorld && node < world.GetAbstractRoom(room).connections.Length)
		{
			int num = world.GetAbstractRoom(room).connections[node];
			if (num > -1)
			{
				aItile.outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.ShortCut, startCoord, new WorldCoordinate(num, -1, -1, world.GetAbstractRoom(num).ExitIndex(room)), world.TotalShortCutLengthBetweenTwoConnectedRooms(room, num)));
			}
		}
		for (int i = 0; i < world.GetAbstractRoom(room).nodes.Length; i++)
		{
			if (node != i)
			{
				int num2 = world.GetAbstractRoom(room).nodes[node].ConnectionCost(i, creatureType);
				if (num2 > -1)
				{
					aItile.outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.OffScreenMovement, startCoord, new WorldCoordinate(room, -1, -1, i), num2));
				}
				else
				{
					aItile.outgoingPaths.Add(new MovementConnection(MovementConnection.MovementType.OffScreenUnallowed, startCoord, new WorldCoordinate(room, -1, -1, i), 1));
				}
			}
		}
		foreach (MovementConnection outgoingPath in aItile.outgoingPaths)
		{
			AITileAtWorldCoordinate(outgoingPath.destinationCoord).incomingPaths.Add(new MovementConnection(outgoingPath.type, outgoingPath.startCoord, outgoingPath.destinationCoord, outgoingPath.distance));
		}
	}

	public List<WorldCoordinate> CreatePathForAbstractreature(WorldCoordinate searchDestination)
	{
		if (realizedRoom == null)
		{
			return null;
		}
		if (creature.creatureTemplate.IsVulture)
		{
			return null;
		}
		WorldCoordinate worldCoordinate = creature.pos;
		worldCoordinate.Tile = new IntVector2(-1, -1);
		List<WorldCoordinate> list = new List<WorldCoordinate> { worldCoordinate };
		for (int i = 0; i < 100; i++)
		{
			MovementConnection movementConnection = default(MovementConnection);
			PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
			int num = -acceptablePathAge;
			PathCost.Legality legality = PathCost.Legality.Unallowed;
			PathingCell start = PathingCellAtWorldCoordinate(worldCoordinate);
			foreach (MovementConnection outgoingPath in AITileAtWorldCoordinate(worldCoordinate).outgoingPaths)
			{
				PathingCell pathingCell = PathingCellAtWorldCoordinate(outgoingPath.destinationCoord);
				PathCost pathCost2 = CheckConnectionCost(start, pathingCell, outgoingPath, followingPath: true);
				PathCost pathCost3 = pathingCell.costToGoal + pathCost2;
				if (pathCost2.legality < legality)
				{
					movementConnection = outgoingPath;
					legality = pathCost2.legality;
					num = pathingCell.generation;
					pathCost = pathCost3;
				}
				else if (pathCost2.legality == legality)
				{
					if (pathingCell.generation > num)
					{
						movementConnection = outgoingPath;
						legality = pathCost2.legality;
						num = pathingCell.generation;
						pathCost = pathCost3;
					}
					else if (pathingCell.generation == num && pathCost3 < pathCost)
					{
						movementConnection = outgoingPath;
						legality = pathCost2.legality;
						num = pathingCell.generation;
						pathCost = pathCost3;
					}
				}
				if (pathCost2.Allowed && outgoingPath.destinationCoord.room == searchDestination.room && outgoingPath.destinationCoord.abstractNode == searchDestination.abstractNode)
				{
					list.Add(outgoingPath.destinationCoord);
					list.Reverse();
					return list;
				}
			}
			if (movementConnection == default(MovementConnection))
			{
				return null;
			}
			worldCoordinate = movementConnection.destinationCoord;
			if (list.Contains(worldCoordinate) || !pathCost.Allowed)
			{
				break;
			}
			list.Add(worldCoordinate);
		}
		return null;
	}

	public MovementConnection AwayFromForbiddenEntrance(WorldCoordinate origin)
	{
		if (forbiddenEntrance.abstractNode < 0)
		{
			return default(MovementConnection);
		}
		if (realizedRoom.abstractRoom.nodes[forbiddenEntrance.abstractNode].type.Index == -1 || !creature.creatureTemplate.mappedNodeTypes[realizedRoom.abstractRoom.nodes[forbiddenEntrance.abstractNode].type.Index])
		{
			return default(MovementConnection);
		}
		int num = realizedRoom.abstractRoom.CommonToCreatureSpecificNodeIndex(forbiddenEntrance.abstractNode, creature.creatureTemplate);
		if (num < 0)
		{
			return default(MovementConnection);
		}
		float num2 = realizedRoom.aimap.ExitDistanceForCreature(origin.Tile, num, creature.creatureTemplate);
		float num3 = float.MinValue;
		MovementConnection result = default(MovementConnection);
		int num4 = 0;
		while (true)
		{
			MovementConnection movementConnection = ConnectionAtCoordinate(outGoing: true, origin, num4);
			num4++;
			if (movementConnection == default(MovementConnection))
			{
				break;
			}
			if (movementConnection.type != MovementConnection.MovementType.ShortCut && creature.creatureTemplate.ConnectionResistance(movementConnection.type).Allowed && realizedRoom.aimap.TileAccessibleToCreature(movementConnection.DestTile, creature.creatureTemplate))
			{
				float num5 = realizedRoom.aimap.ExitDistanceForCreature(movementConnection.DestTile, num, creature.creatureTemplate);
				if (num5 > 0f && num5 > num2)
				{
					num5 = UnityEngine.Random.value * 100f + (realizedRoom.aimap.getAItile(movementConnection.DestTile).narrowSpace ? 0f : 100f);
				}
				if (PathingCellAtWorldCoordinate(movementConnection.destinationCoord).reachable)
				{
					num5 += 1000f;
				}
				if (num5 > num3)
				{
					num3 = num5;
					result = movementConnection;
				}
			}
		}
		return result;
	}

	public int ForbiddenEntranceDist(WorldCoordinate origin)
	{
		if (forbiddenEntrance.abstractNode < 0)
		{
			return int.MaxValue;
		}
		if (realizedRoom.abstractRoom.nodes[forbiddenEntrance.abstractNode].type.Index == -1 || !creature.creatureTemplate.mappedNodeTypes[realizedRoom.abstractRoom.nodes[forbiddenEntrance.abstractNode].type.Index])
		{
			return int.MaxValue;
		}
		int num = realizedRoom.abstractRoom.CommonToCreatureSpecificNodeIndex(forbiddenEntrance.abstractNode, creature.creatureTemplate);
		if (num < 0)
		{
			return int.MaxValue;
		}
		return realizedRoom.aimap.ExitDistanceForCreature(origin.Tile, num, creature.creatureTemplate);
	}

	public MovementConnection PathWithExits(WorldCoordinate origin, bool avoidForbiddenEntrance)
	{
		int num = -1;
		PathCost pathCost = new PathCost(100f, PathCost.Legality.Unallowed);
		int num2 = -1;
		for (int i = 0; i < realizedRoom.abstractRoom.nodes.Length; i++)
		{
			int num3 = realizedRoom.aimap.ExitDistanceForCreature(origin.Tile, realizedRoom.abstractRoom.CommonToCreatureSpecificNodeIndex(i, creature.creatureTemplate), creature.creatureTemplate);
			if (realizedRoom.abstractRoom.nodes[i].type.Index != -1 && creature.creatureTemplate.mappedNodeTypes[realizedRoom.abstractRoom.nodes[i].type.Index])
			{
				PathingCell pathingCell = PathingCellAtWorldCoordinate(new WorldCoordinate(realizedRoom.abstractRoom.index, -1, -1, i));
				PathCost pathCost2 = new PathCost(pathingCell.costToGoal.resistance * (float)num3, pathingCell.costToGoal.legality);
				if (avoidForbiddenEntrance && forbiddenEntrance.room == origin.room && forbiddenEntrance.abstractNode == i)
				{
					pathCost2 += new PathCost(0f, PathCost.Legality.Unallowed);
				}
				if (pathingCell.generation > num2)
				{
					num = i;
					num2 = pathingCell.generation;
				}
				else if (pathingCell.generation == num2 && pathCost2 < pathCost)
				{
					num = i;
					pathCost = pathCost2;
				}
			}
		}
		if (num < 0)
		{
			return default(MovementConnection);
		}
		int num4 = realizedRoom.abstractRoom.CommonToCreatureSpecificNodeIndex(num, creature.creatureTemplate);
		if (num4 < 0)
		{
			return default(MovementConnection);
		}
		int num5 = int.MaxValue;
		MovementConnection movementConnection = default(MovementConnection);
		int num6 = 0;
		while (true)
		{
			MovementConnection movementConnection2 = ConnectionAtCoordinate(outGoing: true, origin, num6);
			num6++;
			if (movementConnection2 == default(MovementConnection))
			{
				break;
			}
			int num7 = realizedRoom.aimap.ExitDistanceForCreature(movementConnection2.DestTile, num4, creature.creatureTemplate);
			if ((float)num7 > 0f && num7 < num5)
			{
				num5 = num7;
				movementConnection = movementConnection2;
			}
		}
		if (movementConnection != default(MovementConnection))
		{
			creatureFollowingGeneration = num2;
		}
		return movementConnection;
	}

	protected virtual PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		PathCost pathCost = new PathCost(100f * (float)connection.distance, PathCost.Legality.IllegalConnection);
		if (!realizedRoom.aimap.IsConnectionAllowedForCreature(connection, creature.creatureTemplate))
		{
			pathCost += new PathCost(0f, PathCost.Legality.IllegalConnection);
		}
		else
		{
			PathCost pathCost2 = creatureType.ConnectionResistance(connection.type);
			if (pathCost2.Considerable)
			{
				PathCost pathCost3 = CoordinateCost(goal.worldCoordinate);
				if (pathCost3.Considerable && CoordinateCost(start.worldCoordinate).Considerable)
				{
					pathCost2.resistance *= connection.distance;
					pathCost = pathCost2 + pathCost3 + new PathCost(0f, CoordinateCost(start.worldCoordinate).legality);
				}
			}
		}
		if (start.worldCoordinate.room != goal.worldCoordinate.room)
		{
			pathCost += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms);
		}
		else if (connection.type == MovementConnection.MovementType.NPCTransportation)
		{
			pathCost += creatureType.NPCTravelAversion;
		}
		else if (connection.type == MovementConnection.MovementType.ShortCut)
		{
			pathCost += creatureType.shortcutAversion;
		}
		if (pathCost.Considerable && InThisRealizedRoom(connection.destinationCoord) && InThisRealizedRoom(connection.startCoord))
		{
			pathCost = AI.TravelPreference(connection, pathCost);
		}
		return pathCost;
	}

	protected virtual PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		return costToGoal;
	}

	public WorldCoordinate BestRegionTransportationGoal()
	{
		int num = int.MinValue;
		PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
		WorldCoordinate result = new WorldCoordinate(-1, -1, -1, -1);
		WorldCoordinate[] regionAccessNodes = world.regionAccessNodes;
		foreach (WorldCoordinate worldCoordinate in regionAccessNodes)
		{
			PathingCell pathingCell = PathingCellAtWorldCoordinate(worldCoordinate);
			if (pathingCell.generation > num)
			{
				num = pathingCell.generation;
				pathCost = pathingCell.costToGoal;
				result = worldCoordinate;
			}
			else if (pathingCell.generation == num && pathingCell.costToGoal < pathCost)
			{
				pathCost = pathingCell.costToGoal;
				result = worldCoordinate;
			}
		}
		return result;
	}

	public string DebugInfo(WorldCoordinate coord)
	{
		return " Leg:" + PathingCellAtWorldCoordinate(coord).costToGoal.legality.ToString() + " Gen:" + PathingCellAtWorldCoordinate(coord).generation + " Res:" + PathingCellAtWorldCoordinate(coord).costToGoal.resistance;
	}

	public void LogWorldCellState()
	{
		Custom.Log("PATHING STATE");
		Custom.Log($"DESTINATION: {destination}");
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			Custom.Log($" -------  Room: {i} ------------ ");
			for (int j = 0; j < world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length; j++)
			{
				Custom.Log($"* Node:{j}, Node type:{world.GetAbstractRoom(i + world.firstRoomIndex).nodes[j].type}");
				Custom.Log($"         Generation:{WorldCells[i][j].generation}");
				Custom.Log($"         Legality:{WorldCells[i][j].costToGoal.legality}");
				Custom.Log($"         Resistance:{WorldCells[i][j].costToGoal.resistance}");
			}
		}
	}

	public PathCost CoordinateCost(WorldCoordinate coord)
	{
		if (forbiddenNode.HasValue && coord.room == forbiddenNode.Value.room && coord.abstractNode == forbiddenNode.Value.abstractNode)
		{
			return new PathCost(0f, PathCost.Legality.Unallowed);
		}
		if (InThisRealizedRoom(coord))
		{
			return realizedRoom.aimap.TileCostForCreature(coord, creatureType);
		}
		return creatureType.AccessibilityResistance(AITileAtWorldCoordinate(coord).acc);
	}

	public bool RayTraceInAccessibleTiles(IntVector2 A, IntVector2 B)
	{
		int x = A.x;
		int y = A.y;
		int x2 = B.x;
		int y2 = B.y;
		int num = Math.Abs(x2 - x);
		int num2 = Math.Abs(y2 - y);
		int num3 = x;
		int num4 = y;
		int num5 = 1 + num + num2;
		int num6 = ((x2 > x) ? 1 : (-1));
		int num7 = ((y2 > y) ? 1 : (-1));
		int num8 = num - num2;
		num *= 2;
		num2 *= 2;
		while (num5 > 0)
		{
			if (!CoordinateReachableAndGetbackable(new WorldCoordinate(room, num3, num4, -1)))
			{
				return false;
			}
			if (num8 > 0)
			{
				num3 += num6;
				num8 -= num2;
			}
			else
			{
				num4 += num7;
				num8 += num;
			}
			num5--;
		}
		return true;
	}
}
