using System.Globalization;

public struct EntityID
{
	public int spawner;

	public int number;

	private int altSeed;

	public int RandomSeed
	{
		get
		{
			if (altSeed > -1)
			{
				return altSeed;
			}
			return number;
		}
	}

	public EntityID(int spawner, int number)
	{
		this.spawner = spawner;
		this.number = number;
		altSeed = -1;
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj is EntityID)
		{
			return Equals((EntityID)obj);
		}
		return false;
	}

	public bool Equals(EntityID id)
	{
		return number == id.number;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public static bool operator ==(EntityID x, EntityID y)
	{
		return x.number == y.number;
	}

	public static bool operator !=(EntityID x, EntityID y)
	{
		return !(x == y);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "ID.{0}.{1}", spawner, number);
	}

	public static EntityID FromString(string s)
	{
		string[] array = s.Split('.');
		return new EntityID(int.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture));
	}

	public void setAltSeed(int seed)
	{
		altSeed = seed;
	}
}
