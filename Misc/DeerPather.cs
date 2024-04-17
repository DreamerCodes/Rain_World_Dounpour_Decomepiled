using RWCustom;
using UnityEngine;

public class DeerPather : BorderExitPather
{
	public Deer deer => creature.realizedCreature as Deer;

	public DeerPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
		walkPastPointOfNoReturn = true;
	}

	protected override void DestinationHasChanged(WorldCoordinate oldDestination, WorldCoordinate newDestination)
	{
	}

	public override void Update()
	{
		base.Update();
	}

	protected override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		return base.CheckConnectionCost(start, goal, connection, followingPath);
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		return costToGoal;
	}

	public MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		int num = int.MinValue;
		PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
		WorldCoordinate dest = originPos;
		if (!originPos.TileDefined && !originPos.NodeDefined)
		{
			return default(MovementConnection);
		}
		WorldCoordinate worldCoordinate = RestrictedOriginPos(originPos);
		if (actuallyFollowingThisPath && debugDrawer != null)
		{
			debugDrawer.Blink(worldCoordinate);
		}
		PathingCell pathingCell = PathingCellAtWorldCoordinate(worldCoordinate);
		if (pathingCell != null)
		{
			if (!pathingCell.reachable || !pathingCell.possibleToGetBackFrom)
			{
				OutOfElement(worldCoordinate);
			}
			MovementConnection movementConnection = default(MovementConnection);
			PathCost pathCost2 = new PathCost(0f, PathCost.Legality.Unallowed);
			int num2 = -acceptablePathAge;
			PathCost.Legality legality = PathCost.Legality.Unallowed;
			int num3 = -acceptablePathAge;
			float num4 = float.MaxValue;
			int num5 = 0;
			while (true)
			{
				MovementConnection movementConnection2 = ConnectionAtCoordinate(outGoing: true, worldCoordinate, num5);
				num5++;
				if (movementConnection2 == default(MovementConnection))
				{
					break;
				}
				if (movementConnection2.type == MovementConnection.MovementType.Standard && movementConnection2.StartTile.FloatDist(movementConnection2.DestTile) > 1f)
				{
					Custom.Log($"WTF {movementConnection2}");
				}
				if (movementConnection2.destinationCoord.TileDefined && !Custom.InsideRect(movementConnection2.DestTile, coveredArea))
				{
					continue;
				}
				PathingCell pathingCell2 = PathingCellAtWorldCoordinate(movementConnection2.destinationCoord);
				PathCost pathCost3 = CheckConnectionCost(pathingCell, pathingCell2, movementConnection2, followingPath: true);
				if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
				{
					pathCost3.legality = PathCost.Legality.Unallowed;
				}
				PathCost pathCost4 = pathingCell2.costToGoal + pathCost3;
				if (movementConnection2.destinationCoord.TileDefined && destination.TileDefined && movementConnection2.destinationCoord.Tile == destination.Tile)
				{
					pathCost4.resistance = 0f;
				}
				else if (realizedRoom.IsPositionInsideBoundries(creature.pos.Tile) && ConnectionAlreadyFollowedSeveralTimes(movementConnection2))
				{
					pathCost3 += new PathCost(0f, PathCost.Legality.Unwanted);
				}
				if (movementConnection2.type == MovementConnection.MovementType.OutsideRoom && !(AI as DeerAI).AllowMovementBetweenRooms)
				{
					pathCost3 += new PathCost(0f, PathCost.Legality.Unallowed);
				}
				if (Input.GetKey("u") && actuallyFollowingThisPath)
				{
					Custom.LogImportant("                     ");
					Custom.LogImportant($"{movementConnection2.startCoord}");
					Custom.LogImportant($"{movementConnection2.type} :: {movementConnection2.destinationCoord}");
					Custom.LogImportant($"conn: {pathCost3.legality} {pathCost3.resistance}");
					Custom.LogImportant($"costToGoal: {pathingCell2.costToGoal.legality} {pathingCell2.costToGoal.resistance}");
					Custom.LogImportant($"totCost: {pathCost4.legality} {pathCost4.resistance}");
					Custom.LogImportant($"generation: {pathingCell2.generation}");
					if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
					{
						Custom.LogImportant("PONOR");
					}
				}
				if (pathingCell2.generation > num3)
				{
					num3 = pathingCell2.generation;
					num4 = pathCost4.resistance;
				}
				else if (pathingCell2.generation == num3 && pathCost4.resistance < num4)
				{
					num4 = pathCost4.resistance;
				}
				if (pathCost3.legality < legality)
				{
					movementConnection = movementConnection2;
					legality = pathCost3.legality;
					num2 = pathingCell2.generation;
					pathCost2 = pathCost4;
				}
				else if (pathCost3.legality == legality)
				{
					if (pathingCell2.generation > num2)
					{
						movementConnection = movementConnection2;
						legality = pathCost3.legality;
						num2 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
					else if (pathingCell2.generation == num2 && pathCost4 <= pathCost2)
					{
						movementConnection = movementConnection2;
						legality = pathCost3.legality;
						num2 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
				}
			}
			if (Input.GetKey("u") && actuallyFollowingThisPath)
			{
				Custom.LogImportant($"{worldCoordinate}, chosen move: {movementConnection}");
			}
			if (legality <= PathCost.Legality.Unwanted)
			{
				if (actuallyFollowingThisPath)
				{
					if (movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.ShortCut && realizedRoom.shortcutData(movementConnection.StartTile).shortCutType == ShortcutData.Type.RoomExit)
					{
						LeavingRoom();
					}
					creatureFollowingGeneration = num2;
				}
				if (actuallyFollowingThisPath && movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.OutsideRoom && !movementConnection.destinationCoord.TileDefined && deer.shortcutDelay < 1)
				{
					int num6 = 30;
					if (!Custom.InsideRect(originPos.Tile, new IntRect(-num6, -num6, realizedRoom.TileWidth + num6, realizedRoom.TileHeight + num6)))
					{
						WorldCoordinate[] sideAccessNodes = world.sideAccessNodes;
						for (int i = 0; i < sideAccessNodes.Length; i++)
						{
							WorldCoordinate worldCoordinate2 = sideAccessNodes[i];
							PathingCell pathingCell3 = PathingCellAtWorldCoordinate(worldCoordinate2);
							if (pathingCell3.generation > num)
							{
								num = pathingCell3.generation;
								pathCost = pathingCell3.costToGoal;
								dest = worldCoordinate2;
							}
							else if (pathingCell3.generation == num && pathingCell3.costToGoal < pathCost)
							{
								pathCost = pathingCell3.costToGoal;
								dest = worldCoordinate2;
							}
							if (worldCoordinate2.CompareDisregardingTile(destination))
							{
								dest = worldCoordinate2;
								break;
							}
						}
						if (!dest.CompareDisregardingTile(movementConnection.destinationCoord))
						{
							deer.AccessSideSpace(movementConnection.destinationCoord, dest);
							if (dest.room != base.creaturePos.room)
							{
								LeavingRoom();
							}
						}
						return default(MovementConnection);
					}
					IntVector2 intVector = new IntVector2(0, 1);
					if (movementConnection.startCoord.x == 0)
					{
						intVector = new IntVector2(-1, 0);
					}
					else if (movementConnection.startCoord.x == realizedRoom.TileWidth - 1)
					{
						intVector = new IntVector2(1, 0);
					}
					else if (movementConnection.startCoord.y == 0)
					{
						intVector = new IntVector2(0, -1);
					}
					return new MovementConnection(MovementConnection.MovementType.Standard, originPos, new WorldCoordinate(originPos.room, originPos.x + intVector.x * 10, originPos.y + intVector.y * 10, originPos.abstractNode), 1);
				}
				return movementConnection;
			}
		}
		return default(MovementConnection);
	}

	private bool ConnectionAlreadyFollowedSeveralTimes(MovementConnection connection)
	{
		return false;
	}
}
