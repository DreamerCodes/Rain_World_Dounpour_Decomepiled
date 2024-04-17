using System;
using System.Globalization;
using UnityEngine;

public static class RXUtils
{
	public static float GetAngle(this Vector2 vector)
	{
		return Mathf.Atan2(0f - vector.y, vector.x) * (180f / (float)Math.PI);
	}

	public static float GetRadians(this Vector2 vector)
	{
		return Mathf.Atan2(0f - vector.y, vector.x);
	}

	public static Rect ExpandRect(Rect rect, float paddingX, float paddingY)
	{
		return new Rect(rect.x - paddingX, rect.y - paddingY, rect.width + paddingX * 2f, rect.height + paddingY * 2f);
	}

	public static void LogRect(string name, Rect rect)
	{
	}

	public static void LogVectors(string name, params Vector2[] args)
	{
		string text = name + ": " + args.Length + " Vector2 " + args[0].ToString();
		for (int i = 1; i < args.Length; i++)
		{
			Vector2 vector = args[i];
			text = text + ", " + vector.ToString();
		}
	}

	public static void LogVectors(string name, params Vector3[] args)
	{
		string text = name + ": " + args.Length + " Vector3 " + args[0].ToString();
		for (int i = 1; i < args.Length; i++)
		{
			Vector3 vector = args[i];
			text = text + ", " + vector.ToString();
		}
	}

	public static void LogVectorsDetailed(string name, params Vector2[] args)
	{
		string text = name + ": " + args.Length + " Vector2 " + VectorDetailedToString(args[0]);
		for (int i = 1; i < args.Length; i++)
		{
			Vector2 vector = args[i];
			text = text + ", " + VectorDetailedToString(vector);
		}
	}

	public static string VectorDetailedToString(Vector2 vector)
	{
		return "(" + vector.x + "," + vector.y + ")";
	}

	public static Color GetColorFromHex(uint hex)
	{
		uint num = hex >> 16;
		uint num2 = hex - (num << 16);
		uint num3 = num2 >> 8;
		uint num4 = num2 - (num3 << 8);
		return new Color((float)num / 255f, (float)num3 / 255f, (float)num4 / 255f);
	}

	public static Color GetColorFromHex(string hexString)
	{
		return GetColorFromHex(Convert.ToUInt32(hexString, 16));
	}

	public static Vector2 GetVector2FromString(string input)
	{
		string[] array = input.Split(',');
		return new Vector2(float.Parse(array[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array[1], NumberStyles.Any, CultureInfo.InvariantCulture));
	}
}
