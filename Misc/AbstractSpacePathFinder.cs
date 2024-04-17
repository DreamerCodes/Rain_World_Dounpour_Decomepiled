using System.Collections.Generic;
using System.Linq;
using Expedition;
using RWCustom;
using UnityEngine;

public static class AbstractSpacePathFinder
{
	public class Node
	{
		public WorldCoordinate pos;

		public Node oneStepCloserToGoal;

		public int costToGoal;

		public Node(WorldCoordinate pos, Node oneStepCloserToGoal, int costToGoal)
		{
			this.pos = pos;
			this.oneStepCloserToGoal = oneStepCloserToGoal;
			this.costToGoal = costToGoal;
		}
	}

	public static List<WorldCoordinate> Path(World world, WorldCoordinate start, WorldCoordinate goal, CreatureTemplate creatureType, IOwnAnAbstractSpacePathFinder owner)
	{
		if (start == goal)
		{
			return new List<WorldCoordinate>();
		}
		if (!world.IsRoomInRegion(start.room))
		{
			return null;
		}
		if (!world.IsRoomInRegion(goal.room))
		{
			return null;
		}
		AbstractRoom abstractRoom = world.GetAbstractRoom(start);
		AbstractRoom abstractRoom2 = world.GetAbstractRoom(goal);
		if (abstractRoom == null || abstractRoom2 == null || abstractRoom.nodes.Length == 0 || abstractRoom2.nodes.Length == 0)
		{
			return null;
		}
		if (!start.NodeDefined || start.abstractNode >= abstractRoom.nodes.Length)
		{
			start.abstractNode = Random.Range(0, abstractRoom.nodes.Length);
		}
		if (!goal.NodeDefined || goal.abstractNode >= abstractRoom2.nodes.Length)
		{
			goal.abstractNode = Random.Range(0, abstractRoom2.nodes.Length);
		}
		if ((world.GetNode(start).type.Index == -1 || !creatureType.mappedNodeTypes[world.GetNode(start).type.Index]) && !abstractRoom.offScreenDen)
		{
			Custom.LogWarning("Looking for path from inside node that's inaccessible to creature:", creatureType.name);
			Custom.LogWarning($"start: {start}");
			return null;
		}
		if (world.GetNode(goal).type.Index == -1 || !creatureType.mappedNodeTypes[world.GetNode(goal).type.Index])
		{
			Custom.LogWarning(creatureType.name, "is looking for path to inaccessible node. Attempting to assign random other node in room.");
			List<WorldCoordinate> list = new List<WorldCoordinate>();
			for (int i = 0; i < abstractRoom2.nodes.Length; i++)
			{
				if (abstractRoom2.nodes[i].type.Index != -1 && creatureType.mappedNodeTypes[abstractRoom2.nodes[i].type.Index])
				{
					list.Add(new WorldCoordinate(goal.room, -1, -1, i));
				}
			}
			if (list.Count < 1)
			{
				Custom.LogWarning("However... failed, as there are no possible nodes!");
				return null;
			}
			goal = list[Random.Range(0, list.Count)];
			if (start == goal)
			{
				return new List<WorldCoordinate>();
			}
		}
		List<WorldCoordinate> list2 = new List<WorldCoordinate>();
		if (start.room == goal.room && (float)abstractRoom.ConnectivityCost(start.abstractNode, goal.abstractNode, creatureType) >= 0f)
		{
			Custom.Log("in same room super short path");
			list2.Add(goal);
			return list2;
		}
		for (int j = 0; j < world.GetAbstractRoom(start).connections.Length; j++)
		{
			if (world.GetAbstractRoom(start).connections[j] == goal.room)
			{
				int num = j;
				int num2 = abstractRoom2.ExitIndex(start.room);
				bool flag = true;
				if (num2 == -1)
				{
					flag = false;
				}
				if (flag && start.abstractNode != num && (float)abstractRoom.ConnectivityCost(start.abstractNode, num, creatureType) < 0f)
				{
					flag = false;
				}
				if (flag && num2 != goal.abstractNode && (float)abstractRoom2.ConnectivityCost(num2, goal.abstractNode, creatureType) < 0f)
				{
					flag = false;
				}
				if (!flag)
				{
					break;
				}
				if (goal.abstractNode != num2)
				{
					list2.Add(new WorldCoordinate(goal.room, -1, -1, goal.abstractNode));
				}
				list2.Add(new WorldCoordinate(goal.room, -1, -1, num2));
				if (start.abstractNode != num)
				{
					list2.Add(new WorldCoordinate(start.room, -1, -1, num));
				}
				return list2;
			}
		}
		start.Tile = new IntVector2(-1, -1);
		goal.Tile = new IntVector2(-1, -1);
		List<Node> checkNext = new List<Node>
		{
			new Node(goal, null, 0)
		};
		bool[,] alreadyChecked = new bool[world.NumberOfRooms, world.mostNodesInARoom];
		Node foundStart = null;
		int num3 = 0;
		while (checkNext.Any())
		{
			Node node = checkNext[0];
			foreach (Node item in checkNext)
			{
				if (item.costToGoal < node.costToGoal)
				{
					node = item;
				}
			}
			checkNext.Remove(node);
			AbstractRoom abstractRoom3 = world.GetAbstractRoom(node.pos);
			for (int k = 0; k < abstractRoom3.nodes.Length; k++)
			{
				if (k != node.pos.abstractNode)
				{
					WorldCoordinate pos = new WorldCoordinate(abstractRoom3.index, -1, -1, k);
					float num4 = (float)abstractRoom3.ConnectivityCost(pos.abstractNode, node.pos.abstractNode, creatureType) * creatureType.ConnectionResistance(MovementConnection.MovementType.OffScreenMovement).resistance;
					if (num4 >= 0f && !alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
					{
						AddNode(pos, node, num4, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
					}
				}
			}
			if (abstractRoom3.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.Exit)
			{
				if (creatureType.mappedNodeTypes[0])
				{
					int abstractNode = node.pos.abstractNode;
					if (abstractNode < abstractRoom3.connections.Length && abstractRoom3.connections[abstractNode] > -1)
					{
						WorldCoordinate pos = new WorldCoordinate(abstractRoom3.connections[abstractNode], -1, -1, world.GetAbstractRoom(abstractRoom3.connections[abstractNode]).ExitIndex(abstractRoom3.index));
						float num4 = (float)world.TotalShortCutLengthBetweenTwoConnectedRooms(abstractRoom3.index, abstractRoom3.connections[abstractNode]) * creatureType.ConnectionResistance(MovementConnection.MovementType.ShortCut).resistance;
						if (num4 > 0f)
						{
							num4 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
							if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
							{
								AddNode(pos, node, num4, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
							}
						}
					}
				}
			}
			else if (abstractRoom3.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.SideExit)
			{
				if (creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SideExit] && creatureType.ConnectionResistance(MovementConnection.MovementType.SideHighway).Allowed)
				{
					for (int l = 0; l < world.sideAccessNodes.Length; l++)
					{
						if (world.sideAccessNodes[l].CompareDisregardingTile(node.pos))
						{
							continue;
						}
						WorldCoordinate pos = world.sideAccessNodes[l];
						float num4 = (float)world.SideHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SideHighway).resistance;
						if (num4 > 0f)
						{
							num4 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
							if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
							{
								AddNode(pos, node, num4, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
							}
						}
					}
				}
			}
			else if (abstractRoom3.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.SkyExit)
			{
				if (creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SkyExit] && creatureType.ConnectionResistance(MovementConnection.MovementType.SkyHighway).Allowed)
				{
					for (int m = 0; m < world.skyAccessNodes.Length; m++)
					{
						if (world.skyAccessNodes[m].CompareDisregardingTile(node.pos))
						{
							continue;
						}
						WorldCoordinate pos = world.skyAccessNodes[m];
						float num4 = (float)world.SkyHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SkyHighway).resistance;
						if (num4 > 0f)
						{
							num4 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
							if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
							{
								AddNode(pos, node, num4, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
							}
						}
					}
				}
			}
			else if (abstractRoom3.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.SeaExit)
			{
				if (creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.SeaExit] && creatureType.ConnectionResistance(MovementConnection.MovementType.SeaHighway).Allowed)
				{
					for (int n = 0; n < world.seaAccessNodes.Length; n++)
					{
						if (world.seaAccessNodes[n].CompareDisregardingTile(node.pos))
						{
							continue;
						}
						WorldCoordinate pos = world.seaAccessNodes[n];
						float num4 = (float)world.SeaHighwayDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SeaHighway).resistance;
						if (num4 > 0f)
						{
							num4 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
							if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
							{
								AddNode(pos, node, num4, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
							}
						}
					}
				}
			}
			else if (abstractRoom3.nodes[node.pos.abstractNode].type == AbstractRoomNode.Type.RegionTransportation && creatureType.mappedNodeTypes[(int)AbstractRoomNode.Type.RegionTransportation] && creatureType.ConnectionResistance(MovementConnection.MovementType.RegionTransportation).Allowed)
			{
				for (int num5 = 0; num5 < world.regionAccessNodes.Length; num5++)
				{
					if (world.regionAccessNodes[num5].CompareDisregardingTile(node.pos))
					{
						continue;
					}
					WorldCoordinate pos = world.regionAccessNodes[num5];
					float num4 = (float)world.RegionTransportationDistanceBetweenNodes(node.pos, pos) * creatureType.ConnectionResistance(MovementConnection.MovementType.SideHighway).resistance;
					if (num4 > 0f)
					{
						num4 += creatureType.ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
						if (!alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode])
						{
							AddNode(pos, node, num4, ref alreadyChecked, ref checkNext, ref foundStart, start, world, owner);
						}
					}
				}
			}
			num3++;
			if (num3 > 500)
			{
				Custom.LogWarning("EMERGENCY EXIT AbstractSpacePathFinder after", num3.ToString(), "generations!");
				return null;
			}
		}
		if (foundStart == null)
		{
			if (RainWorld.ShowLogs)
			{
				if (ModManager.Expedition && world.game.rainWorld.ExpeditionMode && (ExpeditionGame.activeUnlocks.Contains("bur-hunted") || ExpeditionGame.activeUnlocks.Contains("bur-pursued")))
				{
					return null;
				}
				Custom.LogWarning("Abstract path finder has given up after", num3.ToString(), "generations");
				Custom.LogWarning($"Tried to find path between r:{world.GetAbstractRoom(start.room).name}, n:{start.abstractNode} and r:{world.GetAbstractRoom(goal.room).name}, n:{goal.abstractNode}");
			}
			return null;
		}
		Node node2 = foundStart;
		list2.Add(node2.pos);
		while (!node2.pos.Equals(goal))
		{
			node2 = node2.oneStepCloserToGoal;
			list2.Add(node2.pos);
		}
		list2.Reverse();
		return list2;
	}

	private static void AddNode(WorldCoordinate pos, Node parent, float cost, ref bool[,] alreadyChecked, ref List<Node> checkNext, ref Node foundStart, WorldCoordinate start, World world, IOwnAnAbstractSpacePathFinder owner)
	{
		if (owner != null)
		{
			cost += owner.CostAddOfNode(pos);
		}
		Node node = new Node(pos, parent, parent.costToGoal + (int)cost);
		alreadyChecked[pos.room - world.firstRoomIndex, pos.abstractNode] = true;
		checkNext.Add(node);
		if (pos.abstractNode == start.abstractNode && pos.room == start.room)
		{
			foundStart = node;
		}
	}
}
