using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public static class QuickConnectivity
{
	private static readonly AGLog<FromStaticClass> Log;

	private static Dictionary<int, bool> alreadyChecked;

	private static List<IntVector2> checkNext;

	private const int QUICK_PATH_FRAME_QUOTA_MAX = 1000;

	private static int quickPathFrameIterationQuota;

	private static Dictionary<int, PathCost> scratchMatrix;

	private static List<IntVector2> scratchCheckNext;

	private static bool[] _cachedFloodFillalreadyChecked;

	private static List<IntVector2> _cachedFloodFillCheckNext;

	static QuickConnectivity()
	{
		Log = new AGLog<FromStaticClass>();
		alreadyChecked = new Dictionary<int, bool>();
		checkNext = new List<IntVector2>();
		quickPathFrameIterationQuota = 0;
		scratchMatrix = new Dictionary<int, PathCost>();
		scratchCheckNext = new List<IntVector2>();
		_cachedFloodFillalreadyChecked = new bool[100];
		_cachedFloodFillCheckNext = new List<IntVector2>(100);
	}

	public static int Check(Room room, CreatureTemplate creatureType, IntVector2 start, IntVector2 goal, int maxGenerations)
	{
		if (start == goal)
		{
			return 0;
		}
		start.x = Custom.IntClamp(start.x, 0, room.TileWidth - 1);
		start.y = Custom.IntClamp(start.y, 0, room.TileHeight - 1);
		goal.x = Custom.IntClamp(goal.x, 0, room.TileWidth - 1);
		goal.y = Custom.IntClamp(goal.y, 0, room.TileHeight - 1);
		PathCost.Legality legality = room.aimap.TileCostForCreature(start, creatureType).legality;
		alreadyChecked.Clear();
		checkNext.Clear();
		checkNext.Add(goal);
		for (int i = 0; i < maxGenerations; i++)
		{
			if (checkNext.Count < 1)
			{
				return -1;
			}
			IntVector2 intVector = checkNext[0];
			checkNext.RemoveAt(0);
			List<MovementConnection> incomingPaths = room.aimap.map[intVector.x, intVector.y].incomingPaths;
			for (int j = 0; j < incomingPaths.Count; j++)
			{
				MovementConnection movementConnection = incomingPaths[j];
				int key = movementConnection.startCoord.x * room.TileHeight + movementConnection.startCoord.y;
				if (alreadyChecked.ContainsKey(key) || creatureType.ConnectionResistance(movementConnection.type).legality > legality || room.aimap.TileCostForCreature(movementConnection.startCoord, creatureType).legality > legality)
				{
					continue;
				}
				if (movementConnection.startCoord.Tile.Equals(start))
				{
					return i;
				}
				alreadyChecked.Add(key, value: true);
				if (checkNext.Count < 1 || Math.Abs(checkNext[checkNext.Count - 1].x - start.x) + Math.Abs(checkNext[checkNext.Count - 1].y - start.y) < Math.Abs(intVector.x - start.x) + Math.Abs(intVector.y - start.y))
				{
					checkNext.Add(movementConnection.startCoord.Tile);
					continue;
				}
				int k;
				for (k = 0; k < checkNext.Count && Math.Abs(checkNext[k].x - start.x) + Math.Abs(checkNext[k].y - start.y) < Math.Abs(intVector.x - start.x) + Math.Abs(intVector.y - start.y); k++)
				{
				}
				checkNext.Insert(k, movementConnection.startCoord.Tile);
			}
		}
		return -1;
	}

	public static void ResetFrameIterationQuota()
	{
		quickPathFrameIterationQuota = 1000;
	}

	public static int QuickPath(Room room, CreatureTemplate creatureType, IntVector2 start, IntVector2 goal, int maxDistanceFromStart, int maxGenerations, bool inOpenMedium, ref List<IntVector2> path)
	{
		if (start == goal)
		{
			if (path == null)
			{
				path = new List<IntVector2>();
			}
			if (path.Count < 1)
			{
				path.Add(start);
			}
			else
			{
				path[0] = start;
			}
			return 1;
		}
		if (start.FloatDist(goal) > (float)maxDistanceFromStart || !room.aimap.TileAccessibleToCreature(start, creatureType) || !room.aimap.TileAccessibleToCreature(goal, creatureType))
		{
			return 0;
		}
		start.x = Custom.IntClamp(start.x, 0, room.TileWidth - 1);
		start.y = Custom.IntClamp(start.y, 0, room.TileHeight - 1);
		goal.x = Custom.IntClamp(goal.x, 0, room.TileWidth - 1);
		goal.y = Custom.IntClamp(goal.y, 0, room.TileHeight - 1);
		scratchMatrix.Clear();
		scratchCheckNext.Clear();
		scratchCheckNext.Add(goal);
		scratchMatrix.Add(goal.x * room.TileHeight + goal.y, new PathCost(0f, PathCost.Legality.Allowed));
		IntVector2? intVector = null;
		bool flag = false;
		for (int i = 0; i < maxGenerations; i++)
		{
			if (intVector.HasValue)
			{
				break;
			}
			if (scratchCheckNext.Count < 1)
			{
				return 0;
			}
			if (flag && quickPathFrameIterationQuota <= 0)
			{
				break;
			}
			int index = 0;
			if (inOpenMedium)
			{
				float num = float.MaxValue;
				for (int j = 0; j < scratchCheckNext.Count; j++)
				{
					float num2 = scratchCheckNext[j].FloatDist(start);
					if (num2 < num)
					{
						index = j;
						num = num2;
					}
				}
			}
			IntVector2 intVector2 = scratchCheckNext[index];
			int key = intVector2.x * room.TileHeight + intVector2.y;
			PathCost pathCost;
			if (scratchMatrix.ContainsKey(key))
			{
				pathCost = scratchMatrix[key];
			}
			else
			{
				pathCost = new PathCost(-1f, PathCost.Legality.Unallowed);
				scratchMatrix.Add(key, pathCost);
			}
			scratchCheckNext.RemoveAt(index);
			AItile aItile = room.aimap.map[intVector2.x, intVector2.y];
			for (int k = 0; k < aItile.incomingPaths.Count; k++)
			{
				MovementConnection movementConnection = aItile.incomingPaths[k];
				PathCost pathCost2 = creatureType.ConnectionResistance(movementConnection.type) + room.aimap.TileCostForCreature(movementConnection.startCoord, creatureType);
				if (!pathCost2.Allowed)
				{
					continue;
				}
				int key2 = movementConnection.startCoord.x * room.TileHeight + movementConnection.startCoord.y;
				PathCost value;
				if (scratchMatrix.ContainsKey(key2))
				{
					value = scratchMatrix[key2];
				}
				else
				{
					value = new PathCost(-1f, PathCost.Legality.Unallowed);
					scratchMatrix.Add(key2, value);
				}
				if (value.resistance < 0f && movementConnection.StartTile.FloatDist(start) <= (float)maxDistanceFromStart)
				{
					scratchMatrix[key2] = pathCost + pathCost2;
					scratchCheckNext.Add(movementConnection.startCoord.Tile);
					if (movementConnection.StartTile == start)
					{
						intVector = start;
						break;
					}
				}
			}
		}
		if (!intVector.HasValue)
		{
			return 0;
		}
		if (path == null)
		{
			path = new List<IntVector2>();
		}
		int num3 = 0;
		for (int l = 0; l < maxGenerations; l++)
		{
			PathCost pathCost3 = new PathCost(0f, PathCost.Legality.Unallowed);
			IntVector2 intVector3 = intVector.Value;
			AItile aItile2 = room.aimap.map[intVector.Value.x, intVector.Value.y];
			for (int m = 0; m < aItile2.outgoingPaths.Count; m++)
			{
				MovementConnection movementConnection2 = aItile2.outgoingPaths[m];
				int key3 = movementConnection2.destinationCoord.x * room.TileHeight + movementConnection2.destinationCoord.y;
				PathCost pathCost4 = ((!scratchMatrix.ContainsKey(key3)) ? new PathCost(-1f, PathCost.Legality.Unallowed) : scratchMatrix[key3]);
				if (pathCost4 < pathCost3)
				{
					pathCost3 = pathCost4;
					intVector3 = movementConnection2.DestTile;
				}
			}
			if (pathCost3.Allowed)
			{
				if (path.Count <= num3)
				{
					path.Add(intVector.Value);
				}
				else
				{
					path[num3] = intVector.Value;
				}
				num3++;
				intVector = intVector3;
				if (intVector3 == goal)
				{
					return num3;
				}
				continue;
			}
			return 0;
		}
		return 0;
	}

	public static List<IntVector2> FloodFill(Room room, CreatureTemplate creatureType, IntVector2 start, int numberOfTiles, int maxGenerations, List<IntVector2> rtrn)
	{
		int num = room.TileWidth * room.TileHeight;
		if (_cachedFloodFillalreadyChecked.Length < num)
		{
			Custom.LogWarning($"zzz Will have to increase _cachedFloodFillalreadyChecked from:{_cachedFloodFillalreadyChecked.Length} to:{num}");
			_cachedFloodFillalreadyChecked = new bool[num];
		}
		else
		{
			for (int i = 0; i < _cachedFloodFillalreadyChecked.Length; i++)
			{
				_cachedFloodFillalreadyChecked[i] = false;
			}
		}
		start.x = Custom.IntClamp(start.x, 0, room.TileWidth - 1);
		start.y = Custom.IntClamp(start.y, 0, room.TileHeight - 1);
		_cachedFloodFillCheckNext.Clear();
		_cachedFloodFillCheckNext.Add(start);
		rtrn.Clear();
		rtrn.Add(start);
		_cachedFloodFillalreadyChecked[start.x * room.TileHeight + start.y] = true;
		int num2 = 0;
		for (int j = 0; j < maxGenerations; j++)
		{
			if (_cachedFloodFillCheckNext.Count <= 0)
			{
				break;
			}
			if (num2 >= numberOfTiles)
			{
				break;
			}
			IntVector2 intVector = _cachedFloodFillCheckNext[0];
			_cachedFloodFillCheckNext.RemoveAt(0);
			AItile aItile = room.aimap.map[intVector.x, intVector.y];
			for (int k = 0; k < aItile.outgoingPaths.Count; k++)
			{
				MovementConnection connection = aItile.outgoingPaths[k];
				int num3 = connection.destinationCoord.x * room.TileHeight + connection.destinationCoord.y;
				if (!_cachedFloodFillalreadyChecked[num3] && room.aimap.IsConnectionAllowedForCreature(connection, creatureType))
				{
					_cachedFloodFillCheckNext.Add(connection.destinationCoord.Tile);
					rtrn.Add(connection.destinationCoord.Tile);
					_cachedFloodFillalreadyChecked[num3] = true;
					num2++;
					if (num2 >= numberOfTiles)
					{
						break;
					}
				}
			}
		}
		return rtrn;
	}

	public static List<IntVector2> FloodFill(Room room, CreatureTemplate creatureType, IntVector2 start, int numberOfTiles, int maxGenerations)
	{
		return FloodFill(room, creatureType, start, numberOfTiles, maxGenerations, new List<IntVector2>());
	}

	public static WorldCoordinate DefineNodeOfLocalCoordinate(WorldCoordinate coord, World world, CreatureTemplate creatureType)
	{
		if (coord.NodeDefined)
		{
			return coord;
		}
		int specific = UnityEngine.Random.Range(0, world.GetAbstractRoom(coord).NodesRelevantToCreature(creatureType));
		specific = world.GetAbstractRoom(coord).CreatureSpecificToCommonNodeIndex(specific, creatureType);
		if (specific < 0)
		{
			specific = UnityEngine.Random.Range(0, world.GetAbstractRoom(coord).nodes.Length);
		}
		Room realizedRoom = world.GetAbstractRoom(coord.room).realizedRoom;
		if (realizedRoom == null || !realizedRoom.readyForAI)
		{
			return new WorldCoordinate(coord.room, coord.x, coord.y, specific);
		}
		if (TileOrNeighboursReachableFromAnyExit(coord, world, creatureType))
		{
			IntVector2 pos = coord.Tile;
			for (int i = 0; i < 5; i++)
			{
				if (realizedRoom.aimap.AnyExitReachableFromTile(coord.Tile + Custom.fourDirectionsAndZero[i], creatureType))
				{
					pos = coord.Tile + Custom.fourDirectionsAndZero[i];
					break;
				}
			}
			int num = int.MaxValue;
			int abstractNode = specific;
			for (int j = 0; j < realizedRoom.abstractRoom.NodesRelevantToCreature(creatureType); j++)
			{
				int num2 = realizedRoom.aimap.ExitDistanceForCreature(pos, j, creatureType);
				if (num2 > 0 && num2 < num)
				{
					num = realizedRoom.aimap.ExitDistanceForCreature(pos, j, creatureType);
					abstractNode = realizedRoom.abstractRoom.CreatureSpecificToCommonNodeIndex(j, creatureType);
				}
			}
			return new WorldCoordinate(coord.room, coord.x, coord.y, abstractNode);
		}
		float num3 = (float)realizedRoom.TileWidth + (float)realizedRoom.TileHeight;
		int num4 = 400;
		int abstractNode2 = specific;
		for (int k = 0; k < realizedRoom.abstractRoom.nodes.Length; k++)
		{
			WorldCoordinate worldCoordinate = realizedRoom.LocalCoordinateOfNode(k);
			float num5 = Vector2.Distance(Custom.IntVector2ToVector2(coord.Tile), Custom.IntVector2ToVector2(worldCoordinate.Tile));
			int num6 = Check(realizedRoom, creatureType, worldCoordinate.Tile, coord.Tile, num4 / 2);
			if (num6 == -1)
			{
				num6 = 200;
			}
			int num7 = Check(realizedRoom, creatureType, coord.Tile, worldCoordinate.Tile, num4 / 2);
			if (num7 == -1)
			{
				num7 = 200;
			}
			int num8 = num6 + num7;
			if (num8 < num4)
			{
				num3 = num5;
				num4 = num8;
				abstractNode2 = k;
			}
			else if (num8 == num4 && num5 < num3)
			{
				num3 = num5;
				num4 = num8;
				abstractNode2 = k;
			}
		}
		return new WorldCoordinate(coord.room, coord.x, coord.y, abstractNode2);
	}

	private static bool TileOrNeighboursReachableFromAnyExit(WorldCoordinate coord, World world, CreatureTemplate creatureType)
	{
		if (world.GetAbstractRoom(coord).realizedRoom == null || !world.GetAbstractRoom(coord).realizedRoom.readyForAI)
		{
			return false;
		}
		for (int i = 0; i < 5; i++)
		{
			if (world.GetAbstractRoom(coord).realizedRoom.aimap.AnyExitReachableFromTile(coord.Tile + Custom.fourDirectionsAndZero[i], creatureType))
			{
				return true;
			}
		}
		return false;
	}
}
