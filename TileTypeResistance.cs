public struct TileTypeResistance
{
	public AItile.Accessibility accessibility;

	public PathCost cost;

	public TileTypeResistance(AItile.Accessibility accessibility, float resistance, PathCost.Legality legality)
	{
		this.accessibility = accessibility;
		cost = new PathCost(resistance, legality);
	}
}
