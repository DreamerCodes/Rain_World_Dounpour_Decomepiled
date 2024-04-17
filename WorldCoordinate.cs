using System.Globalization;
using RWCustom;

public struct WorldCoordinate
{
	public int x;

	public int y;

	public int room;

	public int abstractNode;

	public string unknownName;

	public bool TileDefined
	{
		get
		{
			if (x == -1)
			{
				return y != -1;
			}
			return true;
		}
	}

	public bool NodeDefined => abstractNode > -1;

	public bool Valid => unknownName == null;

	public IntVector2 Tile
	{
		get
		{
			return new IntVector2(x, y);
		}
		set
		{
			x = value.x;
			y = value.y;
		}
	}

	public WorldCoordinate(int room, int x, int y, int abstractNode)
	{
		this.x = x;
		this.y = y;
		this.room = room;
		this.abstractNode = abstractNode;
		unknownName = null;
	}

	public WorldCoordinate(string unknownRoom, int x, int y, int abstractNode)
	{
		this.x = x;
		this.y = y;
		room = -1;
		this.abstractNode = abstractNode;
		unknownName = unknownRoom;
	}

	public bool CompareDisregardingTile(WorldCoordinate other)
	{
		if (Valid)
		{
			if (room == other.room)
			{
				return abstractNode == other.abstractNode;
			}
			return false;
		}
		if (unknownName == other.unknownName)
		{
			return abstractNode == other.abstractNode;
		}
		return false;
	}

	public bool CompareDisregardingNode(WorldCoordinate other)
	{
		if (Valid)
		{
			if (room == other.room && x == other.x)
			{
				return y == other.y;
			}
			return false;
		}
		if (unknownName == other.unknownName && x == other.x)
		{
			return y == other.y;
		}
		return false;
	}

	public static WorldCoordinate AddIntVector(WorldCoordinate wc, IntVector2 iv)
	{
		if (wc.Valid)
		{
			return new WorldCoordinate(wc.room, wc.x + iv.x, wc.y + iv.y, wc.abstractNode);
		}
		return new WorldCoordinate(wc.unknownName, wc.x + iv.x, wc.y + iv.y, wc.abstractNode);
	}

	public WorldCoordinate WashTileData()
	{
		if (Valid)
		{
			return new WorldCoordinate(room, -1, -1, abstractNode);
		}
		return new WorldCoordinate(unknownName, -1, -1, abstractNode);
	}

	public WorldCoordinate WashNode()
	{
		if (Valid)
		{
			return new WorldCoordinate(room, x, y, -1);
		}
		return new WorldCoordinate(unknownName, x, y, -1);
	}

	public override bool Equals(object obj)
	{
		if (obj == null || !(obj is WorldCoordinate))
		{
			return false;
		}
		return Equals((WorldCoordinate)obj);
	}

	public bool Equals(WorldCoordinate coord)
	{
		if (Valid)
		{
			if (room == coord.room && x == coord.x && y == coord.y)
			{
				return abstractNode == coord.abstractNode;
			}
			return false;
		}
		if (unknownName == coord.unknownName && x == coord.x && y == coord.y)
		{
			return abstractNode == coord.abstractNode;
		}
		return false;
	}

	public bool Equals(WorldCoordinate? coord)
	{
		if (coord.HasValue)
		{
			return Equals(coord.Value);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(WorldCoordinate a, WorldCoordinate b)
	{
		if (a.Valid)
		{
			if (a.room == b.room && a.x == b.x && a.y == b.y)
			{
				return a.abstractNode == b.abstractNode;
			}
			return false;
		}
		if (a.unknownName == b.unknownName && a.x == b.x && a.y == b.y)
		{
			return a.abstractNode == b.abstractNode;
		}
		return false;
	}

	public static bool operator !=(WorldCoordinate a, WorldCoordinate b)
	{
		return !(a == b);
	}

	public static WorldCoordinate operator +(WorldCoordinate a, IntVector2 b)
	{
		if (a.Valid)
		{
			return new WorldCoordinate(a.room, a.x + b.x, a.y + b.y, a.abstractNode);
		}
		return new WorldCoordinate(a.unknownName, a.x + b.x, a.y + b.y, a.abstractNode);
	}

	public override string ToString()
	{
		return "WC ~ r: " + room + " x: " + x + " y: " + y + " n:" + abstractNode;
	}

	public string SaveToString()
	{
		if (!Valid)
		{
			return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}.{4}", "INV", unknownName, x, y, abstractNode);
		}
		string text = ResolveRoomName();
		return string.Format(CultureInfo.InvariantCulture, "{0}.{1}.{2}.{3}", (text != null) ? text : room.ToString(), x, y, abstractNode);
	}

	public static WorldCoordinate FromString(string s)
	{
		string[] array = s.Split('.');
		int num = 0;
		if (array.Length == 5 && array[0] == "INV")
		{
			num = 1;
		}
		string text = array[num];
		int num2 = int.Parse(array[1 + num], NumberStyles.Any, CultureInfo.InvariantCulture);
		int num3 = int.Parse(array[2 + num], NumberStyles.Any, CultureInfo.InvariantCulture);
		int num4 = int.Parse(array[3 + num], NumberStyles.Any, CultureInfo.InvariantCulture);
		int? num5 = BackwardsCompatibilityRemix.ParseRoomIndex(text);
		if (num5.HasValue)
		{
			return new WorldCoordinate(num5.Value, num2, num3, num4);
		}
		return new WorldCoordinate(text, num2, num3, num4);
	}

	public string ResolveRoomName()
	{
		if (Valid)
		{
			if (RainWorld.roomIndexToName.ContainsKey(room))
			{
				return RainWorld.roomIndexToName[room];
			}
			return null;
		}
		return unknownName;
	}
}
