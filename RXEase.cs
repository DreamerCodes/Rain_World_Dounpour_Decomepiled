using System;
using UnityEngine;

public static class RXEase
{
	public static float QuadOut(float input)
	{
		return (0f - input) * (input - 2f);
	}

	public static float QuadIn(float input)
	{
		return input * input;
	}

	public static float QuadInOut(float input)
	{
		if (input < 0.5f)
		{
			return 2f * input * input;
		}
		input = (input - 0.5f) * 2f;
		return 0.5f - 0.5f * input * (input - 2f);
	}

	public static float ExpoOut(float input)
	{
		return 0f - Mathf.Pow(2f, -10f * input) + 1f;
	}

	public static float ExpoIn(float input)
	{
		return Mathf.Pow(2f, 10f * (input - 1f));
	}

	public static float ExpoInOut(float input)
	{
		if (input < 0.5f)
		{
			return Mathf.Pow(2f, 10f * (input * 2f - 1f)) * 0.5f;
		}
		return 0.5f + (0f - Mathf.Pow(2f, -20f * (input - 0.5f)) + 1f) * 0.5f;
	}

	public static float BackOut(float input)
	{
		return BackOut(input, 1.7f);
	}

	public static float BackOut(float input, float backAmount)
	{
		input -= 1f;
		return input * input * ((backAmount + 1f) * input + backAmount) + 1f;
	}

	public static float BackIn(float input)
	{
		return BackIn(input, 1.7f);
	}

	public static float BackIn(float input, float backAmount)
	{
		return input * input * ((backAmount + 1f) * input - backAmount);
	}

	public static float BackInOut(float input)
	{
		return BackInOut(input, 1.7f);
	}

	public static float BackInOut(float input, float backAmount)
	{
		if (input < 0.5f)
		{
			return BackIn(input * 2f, backAmount) * 0.5f;
		}
		return 0.5f + BackOut((input - 0.5f) * 2f, backAmount) * 0.5f;
	}

	public static float SinInOut(float input)
	{
		return -0.5f * (Mathf.Cos((float)Math.PI * input) - 1f);
	}
}
