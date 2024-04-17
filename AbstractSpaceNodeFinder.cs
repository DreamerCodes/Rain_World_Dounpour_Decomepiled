using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class AbstractSpaceNodeFinder
{
	public class Status : ExtEnum<Status>
	{
		public static readonly Status Flooding = new Status("Flooding", register: true);

		public static readonly Status Stepping = new Status("Stepping", register: true);

		public static readonly Status Finished = new Status("Finished", register: true);

		public static readonly Status Failed = new Status("Failed", register: true);

		public Status(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SearchingFor : ExtEnum<SearchingFor>
	{
		public static readonly SearchingFor Den = new SearchingFor("Den", register: true);

		public static readonly SearchingFor SwarmRoom = new SearchingFor("SwarmRoom", register: true);

		public static readonly SearchingFor AttractiveRoom = new SearchingFor("AttractiveRoom", register: true);

		public SearchingFor(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class FloodMethod : ExtEnum<FloodMethod>
	{
		public static readonly FloodMethod Cost = new FloodMethod("Cost", register: true);

		public static readonly FloodMethod Random = new FloodMethod("Random", register: true);

		public FloodMethod(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private class Node
	{
		public WorldCoordinate pos;

		public Node parent;

		public float costToGoal;

		public Node(WorldCoordinate pos, Node parent, float costToGoal)
		{
			this.pos = pos;
			this.parent = parent;
			this.costToGoal = costToGoal;
		}
	}

	private Status status;

	public int maxGenerations;

	public int generation;

	public SearchingFor searchingFor;

	public FloodMethod floodMethod;

	private List<Node> checkNextList;

	private bool[,] alreadyChecked;

	private World world;

	private CreatureTemplate creatureType;

	private Node walker;

	private List<WorldCoordinate> path;

	private WorldCoordinate startPos;

	public bool finished
	{
		get
		{
			if (!(status == Status.Finished))
			{
				return status == Status.Failed;
			}
			return true;
		}
	}

	public AbstractSpaceNodeFinder(SearchingFor searchingFor, FloodMethod floodMethod, int maxGenerations, WorldCoordinate pos, CreatureTemplate creatureType, World world, float randomFac)
	{
		this.floodMethod = floodMethod;
		this.searchingFor = searchingFor;
		generation = 0;
		this.maxGenerations = maxGenerations;
		WorldCoordinate pos2 = QuickConnectivity.DefineNodeOfLocalCoordinate(pos, world, creatureType);
		status = Status.Flooding;
		checkNextList = new List<Node>
		{
			new Node(pos2, null, 0f)
		};
		alreadyChecked = new bool[world.NumberOfRooms, world.mostNodesInARoom];
		path = new List<WorldCoordinate>();
		this.world = world;
		this.creatureType = creatureType;
		if (IsNodeGoal(pos2))
		{
			status = Status.Finished;
			path.Add(pos2.WashTileData());
		}
		walker = null;
	}

	public void Update()
	{
		if (checkNextList.Count == 0 && status == Status.Flooding)
		{
			status = Status.Failed;
		}
		if (status != Status.Flooding && status != Status.Stepping)
		{
			return;
		}
		if (status == Status.Flooding)
		{
			Node node = checkNextList[0];
			if (floodMethod == FloodMethod.Cost)
			{
				foreach (Node checkNext in checkNextList)
				{
					if (checkNext.costToGoal < node.costToGoal)
					{
						node = checkNext;
					}
				}
			}
			else if (floodMethod == FloodMethod.Random)
			{
				node = checkNextList[Random.Range(0, checkNextList.Count)];
			}
			alreadyChecked[node.pos.room - world.firstRoomIndex, node.pos.abstractNode] = true;
			checkNextList.Remove(node);
			AbstractRoom abstractRoom = world.GetAbstractRoom(node.pos);
			for (int i = 0; i < abstractRoom.nodes.Length; i++)
			{
				if (i != node.pos.abstractNode)
				{
					WorldCoordinate pos = new WorldCoordinate(abstractRoom.index, -1, -1, i);
					float num = (float)abstractRoom.ConnectivityCost(pos.abstractNode, node.pos.abstractNode, creatureType) * creatureType.ConnectionResistance(MovementConnection.MovementType.OffScreenMovement).resistance;
					if (num >= 0f && !alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
					{
						AddNode(pos, node, num);
					}
				}
			}
			if (abstractRoom.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.Exit && !creatureType.mappedNodeTypes[0])
			{
				int abstractNode = node.pos.abstractNode;
				if (abstractRoom.connections[abstractNode] > -1)
				{
					WorldCoordinate pos = new WorldCoordinate(abstractRoom.connections[abstractNode], -1, -1, world.GetAbstractRoom(abstractRoom.connections[abstractNode]).ExitIndex(abstractRoom.index));
					float num = (float)world.TotalShortCutLengthBetweenTwoConnectedRooms(abstractRoom.index, abstractRoom.connections[abstractNode]) * creatureType.ConnectionResistance(MovementConnection.MovementType.ShortCut).resistance;
					if (num > 0f && world.GetAbstractRoom(pos).AttractionForCreature(creatureType.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num);
						}
					}
				}
			}
			else if (abstractRoom.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.SideExit && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SideExit])
			{
				for (int j = 0; j < world.sideAccessNodes.Length; j++)
				{
					if (world.sideAccessNodes[j].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.sideAccessNodes[j];
					float num = (float)world.SideHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SideHighway).resistance;
					if (num > 0f && world.GetAbstractRoom(pos).AttractionForCreature(creatureType.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num);
						}
					}
				}
			}
			else if (abstractRoom.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.SkyExit && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SkyExit])
			{
				for (int k = 0; k < world.skyAccessNodes.Length; k++)
				{
					if (world.skyAccessNodes[k].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.skyAccessNodes[k];
					float num = (float)world.SkyHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SkyHighway).resistance;
					if (num > 0f && world.GetAbstractRoom(pos).AttractionForCreature(creatureType.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num);
						}
					}
				}
			}
			else if (abstractRoom.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.SeaExit && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SeaExit])
			{
				for (int l = 0; l < world.seaAccessNodes.Length; l++)
				{
					if (world.seaAccessNodes[l].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.seaAccessNodes[l];
					float num = (float)world.SeaHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SeaHighway).resistance;
					if (num > 0f && world.GetAbstractRoom(pos).AttractionForCreature(creatureType.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num);
						}
					}
				}
			}
			else if (abstractRoom.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.RegionTransportation && !creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation])
			{
				for (int m = 0; m < world.regionAccessNodes.Length; m++)
				{
					if (world.regionAccessNodes[m].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.regionAccessNodes[m];
					float num = (float)world.RegionTransportationDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.RegionTransportation).resistance;
					if (num > 0f && world.GetAbstractRoom(pos).AttractionForCreature(creatureType.type) != AbstractRoom.CreatureRoomAttraction.Forbidden)
					{
						num += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num);
						}
					}
				}
			}
			if (IsNodeGoal(node.pos))
			{
				walker = node;
				status = Status.Stepping;
			}
		}
		else if (status == Status.Stepping)
		{
			path.Add(walker.pos);
			if (walker.parent != null)
			{
				walker = walker.parent;
			}
			else
			{
				status = Status.Finished;
			}
		}
	}

	private bool IsNodeGoal(WorldCoordinate pos)
	{
		if (searchingFor == SearchingFor.Den)
		{
			return world.GetNode(pos).type == AbstractRoomNode.Type.Den;
		}
		if (searchingFor == SearchingFor.SwarmRoom)
		{
			if (world.GetAbstractRoom(pos).swarmRoom && world.regionState != null)
			{
				return world.regionState.SwarmRoomActive(world.GetAbstractRoom(pos).swarmRoomIndex);
			}
			return false;
		}
		if (searchingFor == SearchingFor.AttractiveRoom)
		{
			return Custom.LerpMap(generation, 0f, maxGenerations, Mathf.Lerp(0.5f, 1.5f, Random.value), 1f) * world.GetAbstractRoom(pos).SizeDependentAttractionValueForCreature(creatureType.type) > world.GetAbstractRoom(startPos).SizeDependentAttractionValueForCreature(creatureType.type);
		}
		return true;
	}

	private void AddNode(WorldCoordinate pos, Node parent, float cost)
	{
		Node item = new Node(pos, parent, parent.costToGoal + (float)(int)cost);
		checkNextList.Add(item);
	}

	public List<WorldCoordinate> ReturnPathToClosest()
	{
		if (status != Status.Finished)
		{
			return null;
		}
		return path;
	}
}
