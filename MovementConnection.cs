using System;
using RWCustom;

public struct MovementConnection : IEquatable<MovementConnection>
{
	public enum MovementType
	{
		Standard,
		ReachOverGap,
		ReachUp,
		DoubleReachUp,
		ReachDown,
		SemiDiagonalReach,
		DropToFloor,
		DropToClimb,
		DropToWater,
		LizardTurn,
		OpenDiagonal,
		Slope,
		CeilingSlope,
		ShortCut,
		NPCTransportation,
		BigCreatureShortCutSqueeze,
		OutsideRoom,
		SideHighway,
		SkyHighway,
		SeaHighway,
		RegionTransportation,
		BetweenRooms,
		OffScreenMovement,
		OffScreenUnallowed
	}

	public MovementType type;

	public WorldCoordinate startCoord;

	public WorldCoordinate destinationCoord;

	public int distance;

	public bool IsDrop
	{
		get
		{
			if (type != MovementType.DropToFloor && type != MovementType.DropToClimb)
			{
				return type == MovementType.DropToWater;
			}
			return true;
		}
	}

	public IntVector2 StartTile => new IntVector2(startCoord.x, startCoord.y);

	public IntVector2 DestTile => new IntVector2(destinationCoord.x, destinationCoord.y);

	public bool Equals(MovementConnection other)
	{
		if (type == other.type && startCoord.Equals(other.startCoord) && destinationCoord.Equals(other.destinationCoord))
		{
			return distance == other.distance;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is MovementConnection other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((((((int)type * 397) ^ startCoord.GetHashCode()) * 397) ^ destinationCoord.GetHashCode()) * 397) ^ distance;
	}

	public static bool operator ==(MovementConnection left, MovementConnection right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(MovementConnection left, MovementConnection right)
	{
		return !left.Equals(right);
	}

	public MovementConnection(MovementType type, WorldCoordinate startCoord, WorldCoordinate destinationCoord, int distance)
	{
		this.type = type;
		this.startCoord = startCoord;
		this.destinationCoord = destinationCoord;
		this.distance = distance;
	}

	public override string ToString()
	{
		return type.ToString() + " " + startCoord.Tile.ToString() + " to " + destinationCoord.Tile.ToString();
	}
}
