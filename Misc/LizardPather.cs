using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class LizardPather : PathFinder
{
	public bool wasLastAllowedUnwanted;

	public LizardPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
	}

	protected override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		return base.CheckConnectionCost(start, goal, connection, followingPath);
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (creature.creatureTemplate.type == CreatureTemplate.Type.YellowLizard)
		{
			return costToGoal;
		}
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			if ((creature.creatureTemplate.type == CreatureTemplate.Type.Salamander || (ModManager.MSC && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)) && creature.Room.realizedRoom.aimap.getAItile(cell.worldCoordinate).AnyWater)
			{
				return new PathCost(cell.worldCoordinate.Tile.FloatDist(base.creaturePos.Tile), costToGoal.legality);
			}
			if (base.lookingForImpossiblePath && !cell.reachable)
			{
				return new PathCost(costToGoal.resistance * 5.5f, costToGoal.legality);
			}
			return new PathCost(costToGoal.resistance * 0.5f + (float)Mathf.Abs(cell.worldCoordinate.x - base.creaturePos.x) + (float)Mathf.Abs(cell.worldCoordinate.y - base.creaturePos.y), costToGoal.legality);
		}
		return costToGoal;
	}

	public MovementConnection FollowPath(WorldCoordinate originPos, int? bodyDirection, bool actuallyFollowingThisPath)
	{
		bool flag = !realizedRoom.aimap.getAItile(originPos).narrowSpace && realizedRoom.aimap.getAItile(originPos).fallRiskTile.y < originPos.y - 5;
		if (forbiddenEntranceCounter > 0)
		{
			if (actuallyFollowingThisPath && ForbiddenEntranceDist(originPos) > 6)
			{
				forbiddenEntranceCounter = 0;
			}
			return AwayFromForbiddenEntrance(originPos);
		}
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
				if (!flag && bodyDirection.HasValue && movementConnection2.type == MovementConnection.MovementType.Standard && realizedRoom.aimap.getAItile(movementConnection2.startCoord).acc == AItile.Accessibility.Floor)
				{
					IntVector2 a = IntVector2.ClampAtOne(new IntVector2(movementConnection2.destinationCoord.x - movementConnection2.startCoord.x, movementConnection2.destinationCoord.y - movementConnection2.startCoord.y));
					if ((a.x != 0 || realizedRoom.aimap.getAItile(movementConnection2.startCoord).narrowSpace) && Custom.IntVectorsOpposite(a, Custom.fourDirections[bodyDirection.Value]))
					{
						pathCost2 += new PathCost(0f, PathCost.Legality.Unwanted);
					}
				}
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
		return default(MovementConnection);
	}

	private int AllowedConnectionsFromCoord(WorldCoordinate coord)
	{
		int num = 0;
		int num2 = 0;
		while (true)
		{
			MovementConnection movementConnection = ConnectionAtCoordinate(outGoing: true, coord, num2);
			num2++;
			if (movementConnection == default(MovementConnection))
			{
				break;
			}
			if (realizedRoom.aimap.IsConnectionAllowedForCreature(movementConnection, creature.creatureTemplate))
			{
				num++;
			}
		}
		return num;
	}
}
