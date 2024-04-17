using System;
using RWCustom;
using UnityEngine;

public class FishPather : PathFinder
{
	public FishPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
		accessibilityStepsPerFrame = 60;
	}

	protected override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		PathCost result = base.CheckConnectionCost(start, goal, connection, followingPath);
		if (connection.destinationCoord.TileDefined && destination.TileDefined && Custom.ManhattanDistance(connection.destinationCoord, destination) > 6)
		{
			result.resistance += Mathf.Clamp(10f - (float)realizedRoom.aimap.getTerrainProximity(connection.destinationCoord), 0f, 10f) * 10f;
		}
		return result;
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			return new PathCost(cell.worldCoordinate.Tile.FloatDist(base.creaturePos.Tile), costToGoal.legality);
		}
		return costToGoal;
	}

	public MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		if (originPos.TileDefined)
		{
			originPos.x = Math.Min(Math.Max(originPos.x, 0), realizedRoom.TileWidth - 1);
			originPos.y = Math.Min(Math.Max(originPos.y, 0), realizedRoom.TileHeight - 1);
		}
		if (CoordinateCost(originPos).Allowed)
		{
			PathingCell pathingCell = PathingCellAtWorldCoordinate(originPos);
			if (pathingCell != null)
			{
				if (!pathingCell.reachable || !pathingCell.possibleToGetBackFrom)
				{
					OutOfElement();
				}
				MovementConnection movementConnection = default(MovementConnection);
				PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
				int num = -acceptablePathAge;
				PathCost.Legality legality = PathCost.Legality.Unallowed;
				int num2 = 0;
				while (true)
				{
					MovementConnection movementConnection2 = ConnectionAtCoordinate(outGoing: true, originPos, num2);
					num2++;
					if (movementConnection2 == default(MovementConnection))
					{
						break;
					}
					if (movementConnection2.destinationCoord.TileDefined && !Custom.InsideRect(movementConnection2.DestTile, coveredArea))
					{
						continue;
					}
					PathingCell pathingCell2 = PathingCellAtWorldCoordinate(movementConnection2.destinationCoord);
					PathCost pathCost2 = CheckConnectionCost(pathingCell, pathingCell2, movementConnection2, followingPath: true);
					if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
					{
						pathCost2.legality = PathCost.Legality.Unallowed;
					}
					PathCost pathCost3 = pathingCell2.costToGoal + pathCost2;
					if (pathCost2.legality < legality)
					{
						movementConnection = movementConnection2;
						legality = pathCost2.legality;
						num = pathingCell2.generation;
						pathCost = pathCost3;
					}
					else if (pathCost2.legality == legality)
					{
						if (pathingCell2.generation > num)
						{
							movementConnection = movementConnection2;
							legality = pathCost2.legality;
							num = pathingCell2.generation;
							pathCost = pathCost3;
						}
						else if (pathingCell2.generation == num && pathCost3 <= pathCost)
						{
							movementConnection = movementConnection2;
							legality = pathCost2.legality;
							num = pathingCell2.generation;
							pathCost = pathCost3;
						}
					}
				}
				if (legality <= PathCost.Legality.Unwanted)
				{
					if (actuallyFollowingThisPath)
					{
						if (movementConnection != default(MovementConnection) && !movementConnection.destinationCoord.TileDefined)
						{
							LeavingRoom();
						}
						creatureFollowingGeneration = num;
					}
					return movementConnection;
				}
			}
		}
		return default(MovementConnection);
	}
}
