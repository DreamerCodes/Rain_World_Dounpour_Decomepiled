using RWCustom;

public class BigEelPather : PathFinder
{
	public WorldCoordinate asCloseAsICanGetToDestination;

	public int acaicgtConfirmation;

	public BigEel eel => creature.realizedCreature as BigEel;

	public bool AsFarAlongPathAsPossible
	{
		get
		{
			if (base.lookingForImpossiblePath && acaicgtConfirmation > 50)
			{
				return Custom.ManhattanDistance(base.creaturePos, asCloseAsICanGetToDestination) < 2;
			}
			return false;
		}
	}

	public BigEelPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
	}

	protected override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		return base.CheckConnectionCost(start, goal, connection, followingPath);
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			return new PathCost(cell.worldCoordinate.Tile.FloatDist(creature.pos.Tile), costToGoal.legality);
		}
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
		WorldCoordinate coord = new WorldCoordinate(originPos.room, originPos.x, originPos.y, originPos.abstractNode);
		if (originPos.TileDefined)
		{
			coord.Tile = Custom.RestrictInRect(coord.Tile, coveredArea);
			if (coord.TileDefined && (coord.x == 0 || coord.y == 0 || coord.x == realizedRoom.TileWidth - 1 || coord.y == realizedRoom.TileHeight - 1))
			{
				int num2 = -1;
				int num3 = int.MaxValue;
				for (int i = 0; i < realizedRoom.borderExits.Length; i++)
				{
					if (!(realizedRoom.borderExits[i].type == AbstractRoomNode.Type.SeaExit))
					{
						continue;
					}
					for (int j = 0; j < realizedRoom.borderExits[i].borderTiles.Length; j++)
					{
						if (Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], coord.Tile) < num3)
						{
							num2 = i;
							num3 = Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], coord.Tile);
							if (num3 < 1)
							{
								break;
							}
						}
					}
				}
				if (num2 > -1)
				{
					int num4 = -1;
					for (int k = 0; k < realizedRoom.borderExits[num2].borderTiles.Length; k++)
					{
						if (realizedRoom.aimap.TileAccessibleToCreature(realizedRoom.borderExits[num2].borderTiles[k], base.creatureType) && PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k])).generation >= num4 && !PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k])).inCheckNextList)
						{
							coord = realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k]);
							num4 = PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k])).generation;
						}
					}
				}
			}
		}
		if (actuallyFollowingThisPath && debugDrawer != null)
		{
			debugDrawer.Blink(coord);
		}
		PathingCell pathingCell = PathingCellAtWorldCoordinate(coord);
		if (pathingCell != null)
		{
			if (!pathingCell.reachable || !pathingCell.possibleToGetBackFrom)
			{
				OutOfElement();
			}
			MovementConnection movementConnection = default(MovementConnection);
			PathCost pathCost2 = new PathCost(0f, PathCost.Legality.Unallowed);
			int num5 = -acceptablePathAge;
			PathCost.Legality legality = PathCost.Legality.Unallowed;
			int num6 = -acceptablePathAge;
			float num7 = float.MaxValue;
			int num8 = 0;
			while (true)
			{
				MovementConnection movementConnection2 = ConnectionAtCoordinate(outGoing: true, coord, num8);
				num8++;
				if (movementConnection2 == default(MovementConnection))
				{
					break;
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
				if (movementConnection2.type == MovementConnection.MovementType.OutsideRoom && destination.room == creature.pos.room)
				{
					pathCost4.legality = PathCost.Legality.Unallowed;
				}
				if (movementConnection2.destinationCoord.TileDefined && destination.TileDefined && movementConnection2.destinationCoord.Tile == destination.Tile)
				{
					pathCost4.resistance = 0f;
				}
				if (pathingCell2.generation > num6)
				{
					num6 = pathingCell2.generation;
					num7 = pathCost4.resistance;
				}
				else if (pathingCell2.generation == num6 && pathCost4.resistance < num7)
				{
					num7 = pathCost4.resistance;
				}
				if (pathCost3.legality < legality)
				{
					movementConnection = movementConnection2;
					legality = pathCost3.legality;
					num5 = pathingCell2.generation;
					pathCost2 = pathCost4;
				}
				else if (pathCost3.legality == legality)
				{
					if (pathingCell2.generation > num5)
					{
						movementConnection = movementConnection2;
						legality = pathCost3.legality;
						num5 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
					else if (pathingCell2.generation == num5 && pathCost4 <= pathCost2)
					{
						movementConnection = movementConnection2;
						legality = pathCost3.legality;
						num5 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
				}
			}
			if ((base.lookingForImpossiblePath && num6 > num5) || (num6 == num5 && num7 < pathCost2.resistance))
			{
				if (asCloseAsICanGetToDestination == movementConnection.destinationCoord || Custom.AreIntVectorsNeighbors(asCloseAsICanGetToDestination.Tile, movementConnection.destinationCoord.Tile))
				{
					acaicgtConfirmation++;
				}
				else
				{
					acaicgtConfirmation = 0;
				}
				asCloseAsICanGetToDestination = movementConnection.destinationCoord;
				if (AsFarAlongPathAsPossible && actuallyFollowingThisPath)
				{
					return default(MovementConnection);
				}
			}
			if (legality <= PathCost.Legality.Unwanted)
			{
				if (actuallyFollowingThisPath)
				{
					if (movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.ShortCut && realizedRoom.shortcutData(movementConnection.StartTile).shortCutType == ShortcutData.Type.RoomExit)
					{
						LeavingRoom();
					}
					creatureFollowingGeneration = num5;
				}
				if (actuallyFollowingThisPath && movementConnection != default(MovementConnection) && movementConnection.type == MovementConnection.MovementType.OutsideRoom && !movementConnection.destinationCoord.TileDefined && eel.shortcutDelay < 1)
				{
					int num9 = 85;
					if (!Custom.InsideRect(originPos.Tile, new IntRect(-num9, -num9, realizedRoom.TileWidth + num9, realizedRoom.TileHeight + num9)))
					{
						WorldCoordinate[] seaAccessNodes = world.seaAccessNodes;
						for (int l = 0; l < seaAccessNodes.Length; l++)
						{
							WorldCoordinate worldCoordinate = seaAccessNodes[l];
							PathingCell pathingCell3 = PathingCellAtWorldCoordinate(worldCoordinate);
							if (pathingCell3.generation > num)
							{
								num = pathingCell3.generation;
								pathCost = pathingCell3.costToGoal;
								dest = worldCoordinate;
							}
							else if (pathingCell3.generation == num && pathingCell3.costToGoal < pathCost)
							{
								pathCost = pathingCell3.costToGoal;
								dest = worldCoordinate;
							}
							if (worldCoordinate.CompareDisregardingTile(destination))
							{
								dest = worldCoordinate;
								break;
							}
						}
						if (!dest.CompareDisregardingTile(movementConnection.destinationCoord))
						{
							eel.AccessSwimSpace(movementConnection.destinationCoord, dest);
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
					return new MovementConnection(MovementConnection.MovementType.Standard, originPos, new WorldCoordinate(originPos.room, originPos.x + intVector.x * 10000, originPos.y + intVector.y * 10000, originPos.abstractNode), 1);
				}
				return movementConnection;
			}
		}
		return default(MovementConnection);
	}
}
