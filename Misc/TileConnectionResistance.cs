public struct TileConnectionResistance
{
	public MovementConnection.MovementType movementType;

	public PathCost cost;

	public TileConnectionResistance(MovementConnection.MovementType movementType, float resistance, PathCost.Legality legality)
	{
		this.movementType = movementType;
		cost = new PathCost(resistance, legality);
	}
}
