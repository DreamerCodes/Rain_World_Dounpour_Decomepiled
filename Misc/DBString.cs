using RWCustom;
using UnityEngine;

public static class DBString
{
	public static void Log(object obj)
	{
	}

	public static void NamedLog(string name, object obj)
	{
	}

	private static string StringAnyObject(object obj)
	{
		if (obj is WorldCoordinate)
		{
			return String((WorldCoordinate)obj);
		}
		if (obj is IntVector2)
		{
			return String((IntVector2)obj);
		}
		if (obj is Vector2)
		{
			return String((Vector2)obj);
		}
		return obj.ToString();
	}

	public static string String(WorldCoordinate coord)
	{
		return "r:" + coord.room + " x:" + coord.x + " y:" + coord.y + " n:" + coord.abstractNode;
	}

	public static string String(IntVector2 pos)
	{
		return " x:" + pos.x + " y:" + pos.y;
	}

	public static string String(Vector2 pos)
	{
		return " x:" + pos.x + " y:" + pos.y;
	}
}
