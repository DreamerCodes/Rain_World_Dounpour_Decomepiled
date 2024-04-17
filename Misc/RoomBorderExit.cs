using RWCustom;

public struct RoomBorderExit
{
	public IntVector2[] borderTiles;

	public AbstractRoomNode.Type type;

	public RoomBorderExit(IntVector2[] borderTiles, AbstractRoomNode.Type type)
	{
		this.borderTiles = borderTiles;
		this.type = type;
	}
}
