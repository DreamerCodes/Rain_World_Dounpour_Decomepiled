using RWCustom;

public class CentipedePather : StandardPather
{
	public CentipedePather(ArtificialIntelligence AI, World world, AbstractCreature creature)
		: base(AI, world, creature)
	{
	}

	protected override PathCost HeuristicForCell(PathingCell cell, PathCost costToGoal)
	{
		if (!(AI as CentipedeAI).centipede.Centiwing)
		{
			return base.HeuristicForCell(cell, costToGoal);
		}
		if (InThisRealizedRoom(cell.worldCoordinate))
		{
			if (base.lookingForImpossiblePath && !cell.reachable)
			{
				return costToGoal;
			}
			return new PathCost(Custom.LerpMap(base.creaturePos.Tile.FloatDist(cell.worldCoordinate.Tile), 20f, 50f, costToGoal.resistance, base.creaturePos.Tile.FloatDist(cell.worldCoordinate.Tile) * costToGoal.resistance), costToGoal.legality);
		}
		return costToGoal;
	}

	public bool TileClosestToGoal(WorldCoordinate A, WorldCoordinate B)
	{
		if (CoordinateReachableAndGetbackable(A) && !CoordinateReachableAndGetbackable(B))
		{
			return true;
		}
		if (CoordinateReachableAndGetbackable(B) && !CoordinateReachableAndGetbackable(A))
		{
			return false;
		}
		PathingCell pathingCell = PathingCellAtWorldCoordinate(A);
		PathingCell pathingCell2 = PathingCellAtWorldCoordinate(B);
		if (pathingCell.costToGoal.legality < pathingCell2.costToGoal.legality)
		{
			return true;
		}
		if (pathingCell.costToGoal.legality > pathingCell2.costToGoal.legality)
		{
			return false;
		}
		if (pathingCell.generation > pathingCell2.generation)
		{
			return true;
		}
		if (pathingCell.generation < pathingCell2.generation)
		{
			return false;
		}
		if (pathingCell.costToGoal.resistance < pathingCell2.costToGoal.resistance)
		{
			return true;
		}
		return false;
	}
}
