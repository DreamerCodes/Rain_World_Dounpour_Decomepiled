using RWCustom;
using UnityEngine;

public class SnailPather : PathFinder
{
	public SnailPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
	}

	protected override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		if (connection.type == MovementConnection.MovementType.DropToFloor && !realizedRoom.GetTile(connection.StartTile).AnyWater && !(creature.abstractAI.RealAI as SnailAI).CanDropIntoWater)
		{
			return new PathCost((float)connection.distance * 100f, PathCost.Legality.Unwanted);
		}
		if (ModManager.MMF && connection.type == MovementConnection.MovementType.ShortCut)
		{
			return new PathCost(float.MaxValue, PathCost.Legality.Unallowed);
		}
		return base.CheckConnectionCost(start, goal, connection, followingPath);
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			if (base.lookingForImpossiblePath && !cell.reachable)
			{
				return new PathCost(costToGoal.resistance * 0.5f + (float)(Mathf.Abs(cell.worldCoordinate.x - base.creaturePos.x) + Mathf.Abs(cell.worldCoordinate.y - base.creaturePos.y)) * 1.5f + (float)(Mathf.Abs(cell.worldCoordinate.x - destination.x) + Mathf.Abs(cell.worldCoordinate.y - destination.y)) * 0.75f, costToGoal.legality);
			}
			return new PathCost(costToGoal.resistance * 0.5f + (float)Mathf.Abs(cell.worldCoordinate.x - base.creaturePos.x) + (float)Mathf.Abs(cell.worldCoordinate.y - base.creaturePos.y), costToGoal.legality);
		}
		return costToGoal;
	}

	public MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
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
					if (movementConnection2.destinationCoord.Tile == destination.Tile)
					{
						pathCost3.resistance = 0f;
					}
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
