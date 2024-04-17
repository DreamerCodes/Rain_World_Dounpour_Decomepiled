using RWCustom;

public struct ShortcutData
{
	public class Type : ExtEnum<Type>
	{
		public static readonly Type Normal = new Type("Normal", register: true);

		public static readonly Type RoomExit = new Type("RoomExit", register: true);

		public static readonly Type CreatureHole = new Type("CreatureHole", register: true);

		public static readonly Type NPCTransportation = new Type("NPCTransportation", register: true);

		public static readonly Type RegionTransportation = new Type("RegionTransportation", register: true);

		public static readonly Type DeadEnd = new Type("DeadEnd", register: true);

		public Type(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Room room;

	public Type shortCutType;

	public MovementConnection connection;

	public int destNode;

	public IntVector2[] path;

	public bool LeadingSomewhere
	{
		get
		{
			if (!(shortCutType == Type.Normal))
			{
				return shortCutType == Type.RoomExit;
			}
			return true;
		}
	}

	public bool ToNode
	{
		get
		{
			if (!(shortCutType == Type.CreatureHole))
			{
				return shortCutType == Type.RoomExit;
			}
			return true;
		}
	}

	public IntVector2 StartTile => connection.StartTile;

	public IntVector2 DestTile => connection.DestTile;

	public WorldCoordinate startCoord => connection.startCoord;

	public WorldCoordinate destinationCoord => connection.destinationCoord;

	public int length => connection.distance;

	public ShortcutData(Room room, Type shortCutType, int length, IntVector2 start, IntVector2 goal, int destNode, IntVector2[] path)
	{
		this.room = room;
		this.shortCutType = shortCutType;
		this.destNode = destNode;
		this.path = path;
		connection = new MovementConnection(MovementConnection.MovementType.ShortCut, room.GetWorldCoordinate(start), room.GetWorldCoordinate(goal), length);
	}
}
