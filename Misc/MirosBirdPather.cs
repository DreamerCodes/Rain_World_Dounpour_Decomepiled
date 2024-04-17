using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class MirosBirdPather : PathFinder
{
	public List<MovementConnection> pastConnections;

	private int s;

	public int numnerOfTimesConnectionHasToHaveBeenFollowedToBeOffLimits;

	public MirosBird bird => creature.realizedCreature as MirosBird;

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

	public MirosBirdPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
		pastConnections = new List<MovementConnection>();
		savedPastConnections = 100;
		numnerOfTimesConnectionHasToHaveBeenFollowedToBeOffLimits = 3;
		stepsPerFrame = 20;
	}

	protected override PathCost CheckConnectionCost(PathingCell start, PathingCell goal, MovementConnection connection, bool followingPath)
	{
		if (connection.type == MovementConnection.MovementType.SideHighway && connection.startCoord.CompareDisregardingTile(creature.pos))
		{
			return base.CheckConnectionCost(start, goal, connection, followingPath) + new PathCost(0f, PathCost.Legality.Unwanted);
		}
		return base.CheckConnectionCost(start, goal, connection, followingPath);
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			if (base.lookingForImpossiblePath && !cell.reachable)
			{
				return new PathCost(costToGoal.resistance * 1f + (float)(Mathf.Abs(cell.worldCoordinate.x - base.creaturePos.x) + Mathf.Abs(cell.worldCoordinate.y - base.creaturePos.y)) * 1.5f + (float)(Mathf.Abs(cell.worldCoordinate.x - destination.x) + Mathf.Abs(cell.worldCoordinate.y - destination.y)) * 0.75f, costToGoal.legality);
			}
			return new PathCost(costToGoal.resistance * 1f + base.creaturePos.Tile.FloatDist(cell.worldCoordinate.Tile) * 1f, costToGoal.legality);
		}
		return costToGoal;
	}

	public MovementConnection FollowPath(WorldCoordinate originPos, bool actuallyFollowingThisPath)
	{
		if (originPos.x > 4 && originPos.x < realizedRoom.TileWidth - 4 && AI.stuckTracker.Utility() < 0.5f && (currentlyFollowingDestination.room != originPos.room || (currentlyFollowingDestination.NodeDefined && !currentlyFollowingDestination.TileDefined)))
		{
			MovementConnection movementConnection = PathWithExits(originPos, avoidForbiddenEntrance: true);
			if (movementConnection != default(MovementConnection))
			{
				return movementConnection;
			}
		}
		int num = int.MinValue;
		PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
		WorldCoordinate dest = originPos;
		if (!originPos.TileDefined && !originPos.NodeDefined)
		{
			return default(MovementConnection);
		}
		WorldCoordinate worldCoordinate = new WorldCoordinate(originPos.room, originPos.x, originPos.y, originPos.abstractNode);
		if (originPos.TileDefined)
		{
			worldCoordinate.Tile = Custom.RestrictInRect(worldCoordinate.Tile, coveredArea);
			if (worldCoordinate.TileDefined && (worldCoordinate.x == 0 || worldCoordinate.y == 0 || worldCoordinate.x == realizedRoom.TileWidth - 1 || worldCoordinate.y == realizedRoom.TileHeight - 1))
			{
				int num2 = -1;
				int num3 = int.MaxValue;
				for (int i = 0; i < realizedRoom.borderExits.Length; i++)
				{
					if (!(realizedRoom.borderExits[i].type == AbstractRoomNode.Type.SideExit))
					{
						continue;
					}
					for (int j = 0; j < realizedRoom.borderExits[i].borderTiles.Length; j++)
					{
						if (Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], worldCoordinate.Tile) < num3)
						{
							num2 = i;
							num3 = Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], worldCoordinate.Tile);
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
							worldCoordinate = realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k]);
							num4 = PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num2].borderTiles[k])).generation;
						}
					}
				}
			}
		}
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
			MovementConnection movementConnection2 = default(MovementConnection);
			PathCost pathCost2 = new PathCost(0f, PathCost.Legality.Unallowed);
			int num5 = -acceptablePathAge;
			PathCost.Legality legality = PathCost.Legality.Unallowed;
			int num6 = -acceptablePathAge;
			float num7 = float.MaxValue;
			int num8 = 0;
			while (true)
			{
				MovementConnection movementConnection3 = ConnectionAtCoordinate(outGoing: true, worldCoordinate, num8);
				num8++;
				if (movementConnection3 == default(MovementConnection))
				{
					break;
				}
				if (movementConnection3.destinationCoord.TileDefined && !Custom.InsideRect(movementConnection3.DestTile, coveredArea))
				{
					continue;
				}
				PathingCell pathingCell2 = PathingCellAtWorldCoordinate(movementConnection3.destinationCoord);
				PathCost pathCost3 = CheckConnectionCost(pathingCell, pathingCell2, movementConnection3, followingPath: true);
				if (!pathingCell2.possibleToGetBackFrom && !walkPastPointOfNoReturn)
				{
					pathCost3.legality = PathCost.Legality.Unallowed;
				}
				PathCost pathCost4 = pathingCell2.costToGoal + pathCost3;
				if (movementConnection3.destinationCoord.TileDefined && destination.TileDefined && movementConnection3.destinationCoord.Tile == destination.Tile)
				{
					pathCost4.resistance = 0f;
				}
				else if (realizedRoom.IsPositionInsideBoundries(creature.pos.Tile) && (!actuallyFollowingThisPath || ConnectionAlreadyFollowedSeveralTimes(movementConnection3)))
				{
					pathCost3 += new PathCost(100f, PathCost.Legality.Unwanted);
				}
				if (movementConnection3.type == MovementConnection.MovementType.OutsideRoom && !(AI as MirosBirdAI).AllowMovementBetweenRooms)
				{
					pathCost3 += new PathCost(0f, PathCost.Legality.Unallowed);
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
					movementConnection2 = movementConnection3;
					legality = pathCost3.legality;
					num5 = pathingCell2.generation;
					pathCost2 = pathCost4;
				}
				else if (pathCost3.legality == legality)
				{
					if (pathingCell2.generation > num5)
					{
						movementConnection2 = movementConnection3;
						legality = pathCost3.legality;
						num5 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
					else if (pathingCell2.generation == num5 && pathCost4 <= pathCost2)
					{
						movementConnection2 = movementConnection3;
						legality = pathCost3.legality;
						num5 = pathingCell2.generation;
						pathCost2 = pathCost4;
					}
				}
			}
			if (bird.abstractCreature.world.game.devToolsActive && Input.GetKey("u") && actuallyFollowingThisPath)
			{
				Custom.Log($"{worldCoordinate}, chosen move:{movementConnection2}");
			}
			if (legality <= PathCost.Legality.Unwanted)
			{
				if (actuallyFollowingThisPath)
				{
					creatureFollowingGeneration = num5;
					if (movementConnection2 != default(MovementConnection) && movementConnection2.type == MovementConnection.MovementType.ShortCut && realizedRoom.shortcutData(movementConnection2.StartTile).shortCutType == ShortcutData.Type.RoomExit)
					{
						LeavingRoom();
					}
				}
				if (actuallyFollowingThisPath && movementConnection2 != default(MovementConnection) && movementConnection2.type == MovementConnection.MovementType.OutsideRoom && !movementConnection2.destinationCoord.TileDefined && bird.shortcutDelay < 1)
				{
					int num9 = 30;
					if (!Custom.InsideRect(originPos.Tile, new IntRect(-num9, -num9, realizedRoom.TileWidth + num9, realizedRoom.TileHeight + num9)))
					{
						WorldCoordinate[] sideAccessNodes = world.sideAccessNodes;
						for (int l = 0; l < sideAccessNodes.Length; l++)
						{
							WorldCoordinate worldCoordinate2 = sideAccessNodes[l];
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
						if (!dest.CompareDisregardingTile(movementConnection2.destinationCoord))
						{
							realizedRoom.game.shortcuts.CreatureTakeFlight(bird, AbstractRoomNode.Type.SideExit, movementConnection2.destinationCoord, dest);
							if (dest.room != base.creaturePos.room)
							{
								LeavingRoom();
							}
						}
						return default(MovementConnection);
					}
					IntVector2 intVector = new IntVector2(0, 1);
					if (movementConnection2.startCoord.x == 0)
					{
						intVector = new IntVector2(-1, 0);
					}
					else if (movementConnection2.startCoord.x == realizedRoom.TileWidth - 1)
					{
						intVector = new IntVector2(1, 0);
					}
					else if (movementConnection2.startCoord.y == 0)
					{
						intVector = new IntVector2(0, -1);
					}
					return new MovementConnection(MovementConnection.MovementType.Standard, originPos, new WorldCoordinate(originPos.room, originPos.x + intVector.x * 10, originPos.y + intVector.y * 10, originPos.abstractNode), 1);
				}
				if (actuallyFollowingThisPath && (pastConnections.Count == 0 || movementConnection2 != pastConnections[0]))
				{
					pastConnections.Insert(0, movementConnection2);
				}
				if (pastConnections.Count > savedPastConnections)
				{
					pastConnections.RemoveAt(savedPastConnections);
				}
				return movementConnection2;
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
