using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class QuickPathFinder
{
	public class PathNode
	{
		public PathNode oneStepCloserToGoal;

		public PathNode nextInHeap;

		public int x;

		public int y;

		public PathCost stepsToGoal;

		public PathCost heuresticValue;

		public IntVector2 Pos => new IntVector2(x, y);

		public PathNode(int x, int y, PathNode oneStepCloserToGoal, PathCost stepsToGoal, PathCost heuresticValue)
		{
			this.oneStepCloserToGoal = oneStepCloserToGoal;
			this.x = x;
			this.y = y;
			this.stepsToGoal = stepsToGoal;
			this.heuresticValue = heuresticValue;
		}
	}

	public class MinHeap
	{
		private PathNode head;

		public MinHeap(PathNode hd)
		{
			head = hd;
		}

		public bool Empty()
		{
			return head == null;
		}

		public void Add(PathNode addNode)
		{
			if (head == null)
			{
				head = addNode;
				return;
			}
			if (head.nextInHeap == null && addNode.heuresticValue > head.heuresticValue)
			{
				head.nextInHeap = addNode;
				return;
			}
			if (head.heuresticValue > addNode.heuresticValue)
			{
				addNode.nextInHeap = head;
				head = addNode;
				return;
			}
			PathNode nextInHeap = head;
			while (nextInHeap.nextInHeap != null && nextInHeap.nextInHeap.heuresticValue < addNode.heuresticValue)
			{
				nextInHeap = nextInHeap.nextInHeap;
			}
			addNode.nextInHeap = nextInHeap.nextInHeap;
			nextInHeap.nextInHeap = addNode;
		}

		public void FindAndReplace(PathNode addNode)
		{
			PathNode pathNode = null;
			PathNode nextInHeap = head;
			while (nextInHeap.x != addNode.x || nextInHeap.y != addNode.y)
			{
				pathNode = nextInHeap;
				nextInHeap = nextInHeap.nextInHeap;
			}
			if (pathNode != null && addNode.heuresticValue < nextInHeap.heuresticValue)
			{
				pathNode.nextInHeap = nextInHeap.nextInHeap;
				Add(addNode);
			}
		}

		public PathNode ExtractFirst()
		{
			PathNode result = head;
			head = head.nextInHeap;
			return result;
		}
	}

	private AImap map;

	private IntVector2 start;

	private IntVector2 goal;

	private CreatureTemplate creatureType;

	private bool startFound;

	private PathNode walker;

	private List<IntVector2> path;

	private PathCost pathCost;

	private int pathLength;

	private int[] checkedNodes;

	private MinHeap nodeHeap;

	public int status;

	public QuickPathFinder(IntVector2 start, IntVector2 goal, AImap map, CreatureTemplate creatureType)
	{
		this.start = new IntVector2(Custom.IntClamp(start.x, 0, map.width - 1), Custom.IntClamp(start.y, 0, map.height - 1));
		this.goal = new IntVector2(Custom.IntClamp(goal.x, 0, map.width - 1), Custom.IntClamp(goal.y, 0, map.height - 1));
		this.map = map;
		this.creatureType = creatureType;
		pathCost = new PathCost(0f, PathCost.Legality.Allowed);
		pathLength = 0;
		if (this.start == this.goal)
		{
			status = 1;
			path = new List<IntVector2> { this.start };
		}
		else
		{
			checkedNodes = new int[map.width * map.height];
			nodeHeap = new MinHeap(new PathNode(this.goal.x, this.goal.y, null, new PathCost(0f, PathCost.Legality.Allowed), new PathCost(0f, PathCost.Legality.Allowed)));
			MarkNodeAsChecked(this.goal);
			startFound = false;
			status = 0;
		}
	}

	public void Update()
	{
		if (status != 0)
		{
			return;
		}
		if (!startFound)
		{
			if (nodeHeap.Empty())
			{
				status = -1;
				return;
			}
			PathNode pathNode = nodeHeap.ExtractFirst();
			MarkNodeAsClosed(pathNode.Pos);
			for (int i = 0; i < map.map[pathNode.x, pathNode.y].incomingPaths.Count; i++)
			{
				MovementConnection connection = map.map[pathNode.x, pathNode.y].incomingPaths[i];
				if (connection.StartTile.x < 0 || connection.StartTile.x >= map.width || connection.StartTile.y < 0 || connection.StartTile.y >= map.height || !map.IsConnectionAllowedForCreature(connection, creatureType) || IsNodeClosed(connection.StartTile))
				{
					continue;
				}
				if (connection.StartTile.x == start.x && connection.StartTile.y == start.y)
				{
					startFound = true;
					path = new List<IntVector2>();
					walker = new PathNode(connection.StartTile.x, connection.StartTile.y, pathNode, new PathCost(pathNode.stepsToGoal.resistance + 1f, pathNode.stepsToGoal.legality), new PathCost(0f, PathCost.Legality.Allowed));
					continue;
				}
				PathCost stepsToGoal = pathNode.stepsToGoal;
				stepsToGoal += creatureType.ConnectionResistance(map.map[pathNode.x, pathNode.y].incomingPaths[i].type) * connection.distance;
				stepsToGoal += map.TileCostForCreature(connection.startCoord, creatureType);
				if (IsNodePreviouslyChecked(connection.StartTile))
				{
					nodeHeap.FindAndReplace(new PathNode(connection.StartTile.x, connection.StartTile.y, pathNode, stepsToGoal, AddHeuresticForTile(connection.StartTile, stepsToGoal)));
				}
				else
				{
					nodeHeap.Add(new PathNode(connection.StartTile.x, connection.StartTile.y, pathNode, stepsToGoal, AddHeuresticForTile(connection.StartTile, stepsToGoal)));
				}
				MarkNodeAsChecked(connection.StartTile);
			}
			return;
		}
		path.Add(walker.Pos);
		this.pathCost += map.TileCostForCreature(walker.x, walker.y, creatureType);
		PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
		for (int j = 0; j < map.map[walker.x, walker.y].outgoingPaths.Count; j++)
		{
			if (map.map[walker.x, walker.y].outgoingPaths[j].DestTile.x == walker.oneStepCloserToGoal.x && map.map[walker.x, walker.y].outgoingPaths[j].DestTile.y == walker.oneStepCloserToGoal.y && creatureType.ConnectionResistance(map.map[walker.x, walker.y].outgoingPaths[j].type).Allowed && creatureType.ConnectionResistance(map.map[walker.x, walker.y].outgoingPaths[j].type) < pathCost)
			{
				pathCost = creatureType.ConnectionResistance(map.map[walker.x, walker.y].outgoingPaths[j].type);
			}
		}
		this.pathCost += pathCost;
		pathLength++;
		walker = walker.oneStepCloserToGoal;
		if (walker.x == goal.x && walker.y == goal.y)
		{
			status = 1;
			pathLength++;
			path.Add(walker.Pos);
		}
	}

	public PathCost AddHeuresticForTile(IntVector2 tl, PathCost distanceToGoal)
	{
		return new PathCost(distanceToGoal.resistance + (float)Mathf.Abs(tl.x - start.x) + (float)Mathf.Abs(tl.y - start.y), distanceToGoal.legality);
	}

	public bool IsNodePreviouslyChecked(IntVector2 tl)
	{
		if (tl.x > -1 && tl.y > -1 && tl.x + tl.y * map.width < checkedNodes.Length)
		{
			return checkedNodes[tl.x + tl.y * map.width] > 0;
		}
		return true;
	}

	public bool IsNodeClosed(IntVector2 tl)
	{
		if (tl.x > -1 && tl.y > -1 && tl.x + tl.y * map.width < checkedNodes.Length)
		{
			return checkedNodes[tl.x + tl.y * map.width] == 2;
		}
		return true;
	}

	public void MarkNodeAsChecked(IntVector2 tl)
	{
		if (tl.x > -1 && tl.y > -1 && tl.x + tl.y * map.width < checkedNodes.Length && checkedNodes[tl.x + tl.y * map.width] == 0)
		{
			checkedNodes[tl.x + tl.y * map.width] = 1;
		}
	}

	public void MarkNodeAsClosed(IntVector2 tl)
	{
		if (tl.x > -1 && tl.y > -1 && tl.x + tl.y * map.width < checkedNodes.Length)
		{
			checkedNodes[tl.x + tl.y * map.width] = 2;
		}
	}

	public QuickPath ReturnPath()
	{
		if (status == 1)
		{
			return new QuickPath(creatureType, pathCost, path.ToArray());
		}
		return null;
	}
}
