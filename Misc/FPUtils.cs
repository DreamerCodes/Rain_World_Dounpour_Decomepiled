using System.Collections.Generic;
using UnityEngine;

public static class FPUtils
{
	public static bool CheckIfConvex(Vector2[] sourceVertices)
	{
		int num = sourceVertices.Length;
		int num2 = 0;
		if (num < 3)
		{
			return true;
		}
		for (int i = 0; i < num; i++)
		{
			int num3 = (i + 1) % num;
			int num4 = (i + 2) % num;
			double num5 = (sourceVertices[num3].x - sourceVertices[i].x) * (sourceVertices[num4].y - sourceVertices[num3].y);
			num5 -= (double)((sourceVertices[num3].y - sourceVertices[i].y) * (sourceVertices[num4].x - sourceVertices[num3].x));
			if (num5 < 0.0)
			{
				num2 |= 1;
			}
			else if (num5 > 0.0)
			{
				num2 |= 2;
			}
			if (num2 == 3)
			{
				return false;
			}
		}
		return true;
	}

	public static int[] Triangulate(Vector2[] points)
	{
		List<int> list = new List<int>();
		int num = points.Length;
		if (num < 3)
		{
			return list.ToArray();
		}
		int[] array = new int[num];
		if (Triangulator_Area(points) > 0f)
		{
			for (int i = 0; i < num; i++)
			{
				array[i] = i;
			}
		}
		else
		{
			for (int j = 0; j < num; j++)
			{
				array[j] = num - 1 - j;
			}
		}
		int num2 = num;
		int num3 = 2 * num2;
		int num4 = 0;
		int num5 = num2 - 1;
		while (num2 > 2)
		{
			if (num3-- <= 0)
			{
				return list.ToArray();
			}
			int num6 = num5;
			if (num2 <= num6)
			{
				num6 = 0;
			}
			num5 = num6 + 1;
			if (num2 <= num5)
			{
				num5 = 0;
			}
			int num7 = num5 + 1;
			if (num2 <= num7)
			{
				num7 = 0;
			}
			if (Triangulator_Snip(points, num6, num5, num7, num2, array))
			{
				int item = array[num6];
				int item2 = array[num5];
				int item3 = array[num7];
				list.Add(item);
				list.Add(item2);
				list.Add(item3);
				num4++;
				int num8 = num5;
				for (int k = num5 + 1; k < num2; k++)
				{
					array[num8] = array[k];
					num8++;
				}
				num2--;
				num3 = 2 * num2;
			}
		}
		list.Reverse();
		return list.ToArray();
	}

	private static float Triangulator_Area(Vector2[] points)
	{
		int num = points.Length;
		float num2 = 0f;
		int num3 = num - 1;
		int num4 = 0;
		while (num4 < num)
		{
			Vector2 vector = points[num3];
			Vector2 vector2 = points[num4];
			num2 += vector.x * vector2.y - vector2.x * vector.y;
			num3 = num4++;
		}
		return num2 * 0.5f;
	}

	private static bool Triangulator_Snip(Vector2[] points, int u, int v, int w, int n, int[] V)
	{
		Vector2 a = points[V[u]];
		Vector2 b = points[V[v]];
		Vector2 c = points[V[w]];
		if (Mathf.Epsilon > (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x))
		{
			return false;
		}
		for (int i = 0; i < n; i++)
		{
			if (i != u && i != v && i != w)
			{
				Vector2 p = points[V[i]];
				if (Triangulator_InsideTriangle(a, b, c, p))
				{
					return false;
				}
			}
		}
		return true;
	}

	private static bool Triangulator_InsideTriangle(Vector2 A, Vector2 B, Vector2 C, Vector2 P)
	{
		float num = C.x - B.x;
		float num2 = C.y - B.y;
		float num3 = A.x - C.x;
		float num4 = A.y - C.y;
		float num5 = B.x - A.x;
		float num6 = B.y - A.y;
		float num7 = P.x - A.x;
		float num8 = P.y - A.y;
		float num9 = P.x - B.x;
		float num10 = P.y - B.y;
		float num11 = P.x - C.x;
		float num12 = P.y - C.y;
		float num13 = num * num10 - num2 * num9;
		float num14 = num5 * num8 - num6 * num7;
		float num15 = num3 * num12 - num4 * num11;
		if (num13 >= 0f && num15 >= 0f)
		{
			return num14 >= 0f;
		}
		return false;
	}

	public static Vector2 ToVector2InPoints(this Vector3 vec)
	{
		return new Vector2(vec.x * FPhysics.METERS_TO_POINTS, vec.y * FPhysics.METERS_TO_POINTS);
	}

	public static Vector3 ToVector3InMeters(this Vector2 vec)
	{
		return new Vector3(vec.x * FPhysics.POINTS_TO_METERS, vec.y * FPhysics.POINTS_TO_METERS, 0f);
	}
}
