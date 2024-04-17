using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class StandardPather : PathFinder
{
	private static readonly AGLog<StandardPather> Log = new AGLog<StandardPather>();

	public float heuristicCostFac = 40f;

	public float heuristicDestFac = 1f;

	public List<MovementConnection> pastConnections;

	private int s;

	public int numnerOfTimesConnectionHasToHaveBeenFollowedToBeOffLimits;

	public int savedPastConnections
	{
		get
		{
			return s;
		}
		set
		{
			s = value;
			if (s < pastConnections.Count)
			{
				for (int num = pastConnections.Count - 1; num > s; num--)
				{
					pastConnections.RemoveAt(num);
				}
			}
		}
	}

	public StandardPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
		pastConnections = new List<MovementConnection>();
		savedPastConnections = 20;
		numnerOfTimesConnectionHasToHaveBeenFollowedToBeOffLimits = 3;
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			if (base.lookingForImpossiblePath && !cell.reachable)
			{
				return new PathCost(costToGoal.resistance * heuristicCostFac + (float)(Mathf.Abs(cell.worldCoordinate.x - base.creaturePos.x) + Mathf.Abs(cell.worldCoordinate.y - base.creaturePos.y)) * 1.5f + (float)(Mathf.Abs(cell.worldCoordinate.x - destination.x) + Mathf.Abs(cell.worldCoordinate.y - destination.y)) * 0.75f, costToGoal.legality);
			}
			return new PathCost(costToGoal.resistance * heuristicCostFac + base.creaturePos.Tile.FloatDist(cell.worldCoordinate.Tile) * heuristicDestFac, costToGoal.legality);
		}
		return costToGoal;
	}

	public MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		if (originPos.TileDefined)
		{
			originPos.x = Custom.IntClamp(originPos.x, 0, realizedRoom.TileWidth - 1);
			originPos.y = Custom.IntClamp(originPos.y, 0, realizedRoom.TileHeight - 1);
		}
		if (forbiddenEntranceCounter > 0)
		{
			if (actuallyFollowingThisPath && ForbiddenEntranceDist(originPos) > 6)
			{
				forbiddenEntranceCounter = 0;
			}
			return AwayFromForbiddenEntrance(originPos);
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
					if ((pathingCell2.costToGoal + pathCost2).Allowed && !pathingCell2.possibleToGetBackFrom)
					{
						_ = walkPastPointOfNoReturn;
					}
					if (movementConnection2.destinationCoord.Tile == destination.Tile)
					{
						pathCost3.resistance = 0f;
					}
					else if (savedPastConnections > 0 && ConnectionAlreadyFollowedSeveralTimes(movementConnection2))
					{
						pathCost2 += new PathCost(0f, PathCost.Legality.Unwanted);
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
					if (savedPastConnections > 0)
					{
						if (actuallyFollowingThisPath && (pastConnections.Count == 0 || movementConnection != pastConnections[0]))
						{
							pastConnections.Insert(0, movementConnection);
						}
						if (pastConnections.Count > savedPastConnections)
						{
							pastConnections.RemoveAt(savedPastConnections);
						}
					}
					return movementConnection;
				}
			}
		}
		return default(MovementConnection);
	}

	private bool ConnectionAlreadyFollowedSeveralTimes(MovementConnection connection)
	{
		int num = 0;
		for (int i = 0; i < pastConnections.Count; i++)
		{
			if (pastConnections[i] == connection)
			{
				num++;
				if (num >= numnerOfTimesConnectionHasToHaveBeenFollowedToBeOffLimits)
				{
					return true;
				}
			}
		}
		return false;
	}
}
