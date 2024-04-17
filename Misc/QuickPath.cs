using RWCustom;

public class QuickPath
{
	public CreatureTemplate calculatedFor;

	public PathCost cost;

	public IntVector2[] tiles;

	public int Length => tiles.Length;

	public QuickPath(CreatureTemplate calculatedFor, PathCost cost, IntVector2[] tiles)
	{
		this.calculatedFor = calculatedFor;
		this.cost = cost;
		this.tiles = tiles;
	}
}
