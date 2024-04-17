using RWCustom;

public class BorderExitPather : PathFinder
{
	public BorderExitPather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
	}

	public WorldCoordinate RestrictedOriginPos(WorldCoordinate originPos)
	{
		WorldCoordinate result = new WorldCoordinate(originPos.room, originPos.x, originPos.y, originPos.abstractNode);
		if (originPos.TileDefined)
		{
			result.Tile = Custom.RestrictInRect(result.Tile, coveredArea);
			if (result.TileDefined && (result.x == 0 || result.y == 0 || result.x == realizedRoom.TileWidth - 1 || result.y == realizedRoom.TileHeight - 1))
			{
				int num = -1;
				int num2 = int.MaxValue;
				for (int i = 0; i < realizedRoom.borderExits.Length; i++)
				{
					if (realizedRoom.borderExits[i].type.Index == -1 || !creature.creatureTemplate.mappedNodeTypes[realizedRoom.borderExits[i].type.Index])
					{
						continue;
					}
					for (int j = 0; j < realizedRoom.borderExits[i].borderTiles.Length; j++)
					{
						if (Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], result.Tile) < num2)
						{
							num = i;
							num2 = Custom.ManhattanDistance(realizedRoom.borderExits[i].borderTiles[j], result.Tile);
							if (num2 < 1)
							{
								break;
							}
						}
					}
				}
				if (num > -1)
				{
					int num3 = -1;
					PathCost pathCost = new PathCost(0f, PathCost.Legality.Unallowed);
					for (int k = 0; k < realizedRoom.borderExits[num].borderTiles.Length; k++)
					{
						if (realizedRoom.aimap.TileAccessibleToCreature(realizedRoom.borderExits[num].borderTiles[k], base.creatureType) && !PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).inCheckNextList)
						{
							if (PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).generation > num3)
							{
								result = realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k]);
								num3 = PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).generation;
								pathCost = PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).costToGoal;
							}
							else if (PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).generation == num3 && PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).costToGoal < pathCost)
							{
								result = realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k]);
								num3 = PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).generation;
								pathCost = PathingCellAtWorldCoordinate(realizedRoom.GetWorldCoordinate(realizedRoom.borderExits[num].borderTiles[k])).costToGoal;
							}
						}
					}
				}
			}
		}
		return result;
	}
}
