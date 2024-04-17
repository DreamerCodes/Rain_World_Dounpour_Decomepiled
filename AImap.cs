using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RWCustom;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

public class AImap : IDisposable
{
	public AItile[,] map;

	public NativeArray<int> terrainProximity;

	public AItile standardTile;

	public Room room;

	public int width;

	public int height;

	public CreatureSpecificAImap[] creatureSpecificAImaps;

	public AImap(Room rm, int w, int h)
	{
		map = new AItile[w, h];
		terrainProximity = new NativeArray<int>(w * h, Allocator.Persistent);
		standardTile = new AItile(AItile.Accessibility.Solid, 0);
		room = rm;
		width = w;
		height = h;
		creatureSpecificAImaps = new CreatureSpecificAImap[StaticWorld.preBakedPathingCreatures.Length];
	}

	public void Dispose()
	{
		terrainProximity.Dispose();
	}

	public void NewWorld(int newRoomIndex)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				for (int k = 0; k < map[i, j].outgoingPaths.Count; k++)
				{
					MovementConnection value = map[i, j].outgoingPaths[k];
					value.startCoord.room = newRoomIndex;
					value.destinationCoord.room = newRoomIndex;
					map[i, j].outgoingPaths[k] = value;
				}
				for (int l = 0; l < map[i, j].incomingPaths.Count; l++)
				{
					MovementConnection value2 = map[i, j].incomingPaths[l];
					value2.startCoord.room = newRoomIndex;
					value2.destinationCoord.room = newRoomIndex;
					map[i, j].incomingPaths[l] = value2;
				}
			}
		}
	}

	public bool IsConnectionAllowedForCreature(MovementConnection connection, CreatureTemplate crit)
	{
		if (IsConnectionForceAllowedForCreature(connection, crit, out var forceAllow))
		{
			return forceAllow;
		}
		if (!WorldCoordinateAccessibleToCreature(connection.startCoord, crit) || !WorldCoordinateAccessibleToCreature(connection.destinationCoord, crit) || !crit.ConnectionResistance(connection.type).Allowed)
		{
			return false;
		}
		if (connection.type == MovementConnection.MovementType.DropToClimb || connection.type == MovementConnection.MovementType.DropToFloor || connection.type == MovementConnection.MovementType.ReachDown || connection.type == MovementConnection.MovementType.ReachOverGap || connection.type == MovementConnection.MovementType.ReachUp)
		{
			if (connection.type == MovementConnection.MovementType.ReachUp && getAItile(connection.StartTile).acc == AItile.Accessibility.Floor && getAItile(connection.DestTile).acc == AItile.Accessibility.Floor)
			{
				if (!TileAccessibleToCreature(connection.StartTile + new IntVector2(0, 1), crit))
				{
					if (!TileAccessibleToCreature(connection.StartTile + new IntVector2(0, -1), crit))
					{
						return getAItile(connection.StartTile + new IntVector2(0, -1)).acc == AItile.Accessibility.Solid;
					}
					return true;
				}
				return false;
			}
			IntVector2 intVector = IntVector2.ClampAtOne(new IntVector2(connection.destinationCoord.x - connection.startCoord.x, connection.destinationCoord.y - connection.startCoord.y));
			return !TileAccessibleToCreature(connection.StartTile + intVector, crit);
		}
		if (connection.type == MovementConnection.MovementType.SemiDiagonalReach)
		{
			IntVector2 intVector2 = IntVector2.ClampAtOne(new IntVector2(connection.destinationCoord.x - connection.startCoord.x, connection.destinationCoord.y - connection.startCoord.y));
			if (Math.Abs(connection.destinationCoord.x - connection.startCoord.x) > Math.Abs(connection.destinationCoord.y - connection.startCoord.y))
			{
				intVector2.y = 0;
			}
			else
			{
				intVector2.x = 0;
			}
			return !TileAccessibleToCreature(connection.StartTile + intVector2, crit);
		}
		if (connection.type == MovementConnection.MovementType.DoubleReachUp)
		{
			if ((int)getAItile(connection.StartTile).acc > crit.doubleReachUpConnectionParams[0] || (int)getAItile(connection.StartTile + new IntVector2(0, 1)).acc > crit.doubleReachUpConnectionParams[1] || (int)getAItile(connection.StartTile + new IntVector2(0, 2)).acc > crit.doubleReachUpConnectionParams[1] || (int)getAItile(connection.StartTile + new IntVector2(0, 3)).acc > crit.doubleReachUpConnectionParams[2])
			{
				return false;
			}
			if (!TileAccessibleToCreature(connection.StartTile + new IntVector2(0, 1), crit))
			{
				return !TileAccessibleToCreature(connection.StartTile + new IntVector2(0, 2), crit);
			}
			return false;
		}
		if (connection.type == MovementConnection.MovementType.OpenDiagonal)
		{
			if (!TileAccessibleToCreature(new IntVector2(connection.startCoord.x, connection.destinationCoord.y), crit))
			{
				return !TileAccessibleToCreature(new IntVector2(connection.destinationCoord.x, connection.startCoord.y), crit);
			}
			return false;
		}
		return true;
	}

	private bool IsConnectionForceAllowedForCreature(MovementConnection connection, CreatureTemplate crit, out bool forceAllow)
	{
		forceAllow = false;
		if (ModManager.MMF && crit.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && room.gravity <= Lizard.zeroGravityMovementThreshold && (connection.type == MovementConnection.MovementType.DropToFloor || connection.type == MovementConnection.MovementType.DropToClimb))
		{
			return true;
		}
		if (crit.type == CreatureTemplate.Type.Snail)
		{
			if (connection.type == MovementConnection.MovementType.DropToFloor && !room.GetTile(connection.DestTile).DeepWater)
			{
				return true;
			}
		}
		else if (crit.type == CreatureTemplate.Type.DaddyLongLegs || crit.type == CreatureTemplate.Type.BrotherLongLegs)
		{
			if (connection.type == MovementConnection.MovementType.ShortCut)
			{
				if (connection.startCoord.TileDefined && room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
				{
					return true;
				}
				if (connection.destinationCoord.TileDefined && room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
				{
					return true;
				}
			}
			else if (connection.type == MovementConnection.MovementType.BigCreatureShortCutSqueeze)
			{
				if (room.GetTile(connection.startCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(connection.StartTile).shortCutType == ShortcutData.Type.Normal)
				{
					return true;
				}
				if (room.GetTile(connection.destinationCoord).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(connection.DestTile).shortCutType == ShortcutData.Type.Normal)
				{
					return true;
				}
			}
		}
		return false;
	}

	public PathCost ConnectionCostForCreature(MovementConnection connection, CreatureTemplate crit)
	{
		PathCost pathCost = new PathCost(0f, PathCost.Legality.Allowed);
		if (!IsConnectionAllowedForCreature(connection, crit))
		{
			pathCost += new PathCost(0f, PathCost.Legality.IllegalConnection);
		}
		PathCost pathCost2 = crit.ConnectionResistance(connection.type);
		pathCost2.resistance *= connection.distance;
		pathCost += pathCost2;
		return pathCost + TileCostForCreature(connection.destinationCoord, crit);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getAItile(Vector2 pos)
	{
		return getAItile(room.GetTilePosition(pos));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getAItile(IntVector2 pos)
	{
		return getAItile(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getAItile(WorldCoordinate pos)
	{
		if (pos.room == room.abstractRoom.index && pos.TileDefined)
		{
			return getAItile(pos.x, pos.y);
		}
		return standardTile;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getAItile(int2 pos)
	{
		return getAItile(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getAItile(float2 pos)
	{
		return getAItile(Room.StaticGetTilePosition(pos));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getAItile(int x, int y)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			return map[x, y];
		}
		return standardTile;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getClampedAItile(Vector2 pos)
	{
		return getClampedAItile(room.GetTilePosition(pos).x, room.GetTilePosition(pos).y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public AItile getClampedAItile(int x, int y)
	{
		return map[Custom.IntClamp(x, 0, room.TileWidth - 1), Custom.IntClamp(y, 0, room.TileHeight - 1)];
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTerrainProximity(Vector2 pos)
	{
		return getTerrainProximity(room.GetTilePosition(pos));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTerrainProximity(IntVector2 pos)
	{
		return getTerrainProximity(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTerrainProximity(WorldCoordinate pos)
	{
		if (pos.room == room.abstractRoom.index && pos.TileDefined)
		{
			return getTerrainProximity(pos.x, pos.y);
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTerrainProximity(int2 pos)
	{
		return getTerrainProximity(pos.x, pos.y);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTerrainProximity(float2 pos)
	{
		return getTerrainProximity(Room.StaticGetTilePosition(pos));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int getTerrainProximity(int x, int y)
	{
		if (x >= 0 && x < width && y >= 0 && y < height)
		{
			return terrainProximity[ExtraExtentions.ind(x, y, height)];
		}
		return -1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float Visibility(IntVector2 pos)
	{
		return getAItile(pos).visibility;
	}

	public bool TileAccessibleToCreature(int x, int y, CreatureTemplate crit)
	{
		return TileAccessibleToCreature(new IntVector2(x, y), crit);
	}

	public bool WorldCoordinateAccessibleToCreature(WorldCoordinate pos, CreatureTemplate crit)
	{
		if (!pos.TileDefined)
		{
			return crit.AccessibilityResistance(AItile.Accessibility.OffScreen).Allowed;
		}
		return TileAccessibleToCreature(pos.Tile, crit);
	}

	public bool TileOrNeighborsAccessibleToCreature(IntVector2 pos, CreatureTemplate crit)
	{
		for (int i = 0; i < 5; i++)
		{
			if (TileAccessibleToCreature(pos + Custom.fourDirectionsAndZero[i], crit))
			{
				return true;
			}
		}
		return false;
	}

	public bool ClampedTileAccessibleToCreature(IntVector2 pos, CreatureTemplate crit)
	{
		return TileAccessibleToCreature(new IntVector2(Custom.IntClamp(pos.x, 0, room.TileWidth - 1), Custom.IntClamp(pos.y, 0, room.TileHeight - 1)), crit);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TileAccessibleToCreature(Vector2 pos, CreatureTemplate crit)
	{
		return TileAccessibleToCreature(room.GetTilePosition(pos), crit);
	}

	public bool TileAccessibleToCreature(IntVector2 pos, CreatureTemplate crit)
	{
		AItile aItile = getAItile(pos);
		if (!crit.MovementLegalInRelationToWater(aItile.DeepWater, aItile.WaterSurface))
		{
			return false;
		}
		if (crit.PreBakedPathingIndex == -1)
		{
			return false;
		}
		for (int i = 0; i < room.accessModifiers.Count; i++)
		{
			if (!room.accessModifiers[i].IsTileAccessible(pos, crit))
			{
				return false;
			}
		}
		if (IsTooCloseToTerrain(pos, crit, out var result))
		{
			return result;
		}
		if (ModManager.MMF && crit.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && room.gravity <= Lizard.zeroGravityMovementThreshold)
		{
			if (!StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard).AccessibilityResistance(aItile.acc).Allowed)
			{
				if (crit.canSwim && aItile.acc != AItile.Accessibility.Solid)
				{
					return aItile.AnyWater;
				}
				return false;
			}
			return true;
		}
		if (!crit.AccessibilityResistance(aItile.acc).Allowed)
		{
			if (crit.canSwim && aItile.acc != AItile.Accessibility.Solid)
			{
				return aItile.AnyWater;
			}
			return false;
		}
		return true;
	}

	private bool IsTooCloseToTerrain(IntVector2 pos, CreatureTemplate crit, out bool result)
	{
		result = false;
		if (crit.type == CreatureTemplate.Type.Vulture || crit.type == CreatureTemplate.Type.KingVulture)
		{
			if (getTerrainProximity(pos) < 2)
			{
				return true;
			}
		}
		else if (crit.type == CreatureTemplate.Type.BigEel)
		{
			if (getTerrainProximity(pos) < 4)
			{
				return true;
			}
		}
		else if (crit.type == CreatureTemplate.Type.Deer)
		{
			if (getTerrainProximity(pos) < 3)
			{
				return true;
			}
			if (getAItile(pos).smoothedFloorAltitude > 17)
			{
				return true;
			}
		}
		else if (crit.type == CreatureTemplate.Type.DaddyLongLegs || crit.type == CreatureTemplate.Type.BrotherLongLegs)
		{
			if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
			{
				result = true;
				return true;
			}
			int num = getTerrainProximity(pos);
			if (num < 2 || num > 11)
			{
				return true;
			}
		}
		else if (crit.type == CreatureTemplate.Type.MirosBird)
		{
			int num2 = getTerrainProximity(pos);
			if (num2 < 2)
			{
				return true;
			}
			AItile aItile = getAItile(pos);
			if (aItile.smoothedFloorAltitude > 2 && (float)(aItile.smoothedFloorAltitude + aItile.floorAltitude) > Custom.LerpMap(num2, 2f, 6f, 6f, 4f) * 2f)
			{
				return true;
			}
		}
		return false;
	}

	public PathCost TileCostForCreature(int x, int y, CreatureTemplate crit)
	{
		return TileCostForCreature(new IntVector2(x, y), crit);
	}

	public PathCost TileCostForCreature(WorldCoordinate pos, CreatureTemplate crit)
	{
		return TileCostForCreature(pos.Tile, crit);
	}

	public PathCost TileCostForCreature(IntVector2 pos, CreatureTemplate crit)
	{
		if (!TileAccessibleToCreature(pos, crit))
		{
			return new PathCost(0f, PathCost.Legality.IllegalTile);
		}
		if (ModManager.MMF && crit.TopAncestor().type == CreatureTemplate.Type.LizardTemplate && room.gravity <= Lizard.zeroGravityMovementThreshold)
		{
			crit = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.BlueLizard);
		}
		PathCost pathCost = crit.AccessibilityResistance(getAItile(pos).acc);
		if (getAItile(pos).AnyWater && crit.canSwim && new PathCost(crit.waterPathingResistance, PathCost.Legality.Allowed) < pathCost)
		{
			pathCost = new PathCost(crit.waterPathingResistance, PathCost.Legality.Allowed);
		}
		if (crit.type == CreatureTemplate.Type.Fly)
		{
			pathCost.resistance = 100f / (float)getTerrainProximity(pos);
		}
		return pathCost;
	}

	public CreatureSpecificAImap CreatureSpecificAImap(CreatureTemplate crit)
	{
		return creatureSpecificAImaps[crit.PreBakedPathingIndex];
	}

	public IntVector2 TryForAccessibleNeighbor(IntVector2 tile, CreatureTemplate crit)
	{
		if (TileAccessibleToCreature(tile, crit))
		{
			return tile;
		}
		for (int i = 0; i < 4; i++)
		{
			if (TileAccessibleToCreature(tile + Custom.fourDirections[i], crit))
			{
				return tile + Custom.fourDirections[i];
			}
		}
		return tile;
	}

	public int TriangulateDistance(IntVector2 A, IntVector2 B, CreatureTemplate crit)
	{
		return creatureSpecificAImaps[crit.PreBakedPathingIndex].TriangulateDistance(A, B);
	}

	public int TriangulateDistance(WorldCoordinate A, WorldCoordinate B, CreatureTemplate crit)
	{
		if (A.room != B.room)
		{
			return int.MaxValue;
		}
		return creatureSpecificAImaps[crit.PreBakedPathingIndex].TriangulateDistance(A.Tile, B.Tile);
	}

	public float AccessibilityForCreature(IntVector2 pos, CreatureTemplate crit)
	{
		if (crit.PreBakedPathingIndex < 0)
		{
			return 0f;
		}
		return creatureSpecificAImaps[crit.PreBakedPathingIndex].GetAccessibility(pos.x, pos.y);
	}

	public int ExitDistanceForCreatureAndCheckNeighbours(IntVector2 pos, int creatureSpecificExitIndex, CreatureTemplate crit)
	{
		if (creatureSpecificExitIndex < 0)
		{
			return -1;
		}
		for (int i = 0; i < 5; i++)
		{
			if (creatureSpecificAImaps[crit.PreBakedPathingIndex].GetDistanceToExit(pos.x + Custom.fourDirectionsAndZero[i].x, pos.y + Custom.fourDirectionsAndZero[i].y, creatureSpecificExitIndex) > -1)
			{
				return creatureSpecificAImaps[crit.PreBakedPathingIndex].GetDistanceToExit(pos.x + Custom.fourDirectionsAndZero[i].x, pos.y + Custom.fourDirectionsAndZero[i].y, creatureSpecificExitIndex);
			}
		}
		return -1;
	}

	public int ExitDistanceForCreature(IntVector2 pos, int creatureSpecificExitIndex, CreatureTemplate crit)
	{
		if (!TileAccessibleToCreature(pos, crit))
		{
			return -1;
		}
		return creatureSpecificAImaps[crit.PreBakedPathingIndex].GetDistanceToExit(pos.x, pos.y, creatureSpecificExitIndex);
	}

	public int ExitDistanceForCreature(Vector2 pos, int exitNumber, CreatureTemplate crit)
	{
		return ExitDistanceForCreature(room.GetTilePosition(pos), exitNumber, crit);
	}

	public bool ExitReachableFromTile(IntVector2 pos, int globalNodeIndex, CreatureTemplate crit)
	{
		return ExitDistanceForCreature(pos, room.abstractRoom.CommonToCreatureSpecificNodeIndex(globalNodeIndex, crit), crit) > 0;
	}

	public bool AnyExitReachableFromTile(IntVector2 pos, CreatureTemplate crit)
	{
		if (crit.PreBakedPathingIndex == -1)
		{
			return false;
		}
		for (int i = 0; i < creatureSpecificAImaps[crit.PreBakedPathingIndex].numberOfNodes; i++)
		{
			if (creatureSpecificAImaps[crit.PreBakedPathingIndex].GetDistanceToExit(pos.x, pos.y, i) > 0)
			{
				return true;
			}
		}
		return false;
	}

	public int[] GetCompressedVisibilityMap()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < room.TileWidth; i++)
		{
			for (int j = 0; j < room.TileHeight; j++)
			{
				if (!room.GetTile(i, j).Solid)
				{
					list.Add(getAItile(i, j).visibility);
				}
			}
		}
		return list.ToArray();
	}

	public void SetVisibilityMapFromCompressedArray(int[] ca)
	{
		int num = 0;
		for (int i = 0; i < room.TileWidth; i++)
		{
			for (int j = 0; j < room.TileHeight; j++)
			{
				if (!room.GetTile(i, j).Solid)
				{
					getAItile(i, j).visibility = ca[num];
					num++;
				}
			}
		}
	}
}
