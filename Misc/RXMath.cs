using System;
using UnityEngine;

public class RXMath
{
	public const float RTOD = 180f / (float)Math.PI;

	public const float DTOR = (float)Math.PI / 180f;

	public const float DOUBLE_PI = (float)Math.PI * 2f;

	public const float HALF_PI = (float)Math.PI / 2f;

	public const float PI = (float)Math.PI;

	public const float INVERSE_PI = 1f / (float)Math.PI;

	public const float INVERSE_DOUBLE_PI = 1f / (2f * (float)Math.PI);

	public static int Wrap(int input, int range)
	{
		return (input + range * 1000000) % range;
	}

	public static float Wrap(float input, float range)
	{
		return (input + range * 1000000f) % range;
	}

	public static float GetDegreeDelta(float startAngle, float endAngle)
	{
		float num = (endAngle - startAngle) % 360f;
		if (num != num % 180f)
		{
			num = ((num < 0f) ? (num + 360f) : (num - 360f));
		}
		return num;
	}

	public static float GetRadianDelta(float startAngle, float endAngle)
	{
		float num = (endAngle - startAngle) % ((float)Math.PI * 2f);
		if (num != num % (float)Math.PI)
		{
			num = ((num < 0f) ? (num + (float)Math.PI * 2f) : (num - (float)Math.PI * 2f));
		}
		return num;
	}

	public static float PingPong(float input, float range)
	{
		float num = (input + range * 1000000f) % range / range;
		if (num < 0.5f)
		{
			return num * 2f;
		}
		return 1f - (num - 0.5f) * 2f;
	}

	public static Vector2 GetOffsetFromAngle(float angle, float distance)
	{
		float f = angle * ((float)Math.PI / 180f);
		return new Vector2(Mathf.Cos(f) * distance, (0f - Mathf.Sin(f)) * distance);
	}
}
