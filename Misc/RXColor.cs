using UnityEngine;

public class RXColor
{
	public static Color ColorFromRGBString(string rgbString)
	{
		return Color.red;
	}

	public static Color ColorFromHSLString(string hslString)
	{
		return Color.green;
	}

	public static Color ColorFromHSL(RXColorHSL hsl)
	{
		return ColorFromHSL(hsl.h, hsl.s, hsl.l);
	}

	public static Color ColorFromHSL(float hue, float sat, float lum)
	{
		return ColorFromHSL(hue, sat, lum, 1f);
	}

	public static Color ColorFromHSL(float hue, float sat, float lum, float alpha)
	{
		hue = (100000f + hue) % 1f;
		float r = lum;
		float g = lum;
		float b = lum;
		float num = ((lum <= 0.5f) ? (lum * (1f + sat)) : (lum + sat - lum * sat));
		if (num > 0f)
		{
			float num2 = lum + lum - num;
			float num3 = (num - num2) / num;
			hue *= 6f;
			int num4 = (int)hue;
			float num5 = hue - (float)num4;
			float num6 = num * num3 * num5;
			float num7 = num2 + num6;
			float num8 = num - num6;
			switch (num4)
			{
			case 0:
				r = num;
				g = num7;
				b = num2;
				break;
			case 1:
				r = num8;
				g = num;
				b = num2;
				break;
			case 2:
				r = num2;
				g = num;
				b = num7;
				break;
			case 3:
				r = num2;
				g = num8;
				b = num;
				break;
			case 4:
				r = num7;
				g = num2;
				b = num;
				break;
			case 5:
				r = num;
				g = num2;
				b = num8;
				break;
			}
		}
		return new Color(r, g, b, alpha);
	}

	public static RXColorHSL HSLFromColor(Color rgb)
	{
		RXColorHSL rXColorHSL = new RXColorHSL();
		float r = rgb.r;
		float g = rgb.g;
		float b = rgb.b;
		float num = Mathf.Min(r, g, b);
		float num2 = Mathf.Max(r, g, b);
		float num3 = num2 - num;
		rXColorHSL.l = (num2 + num) * 0.5f;
		if (Mathf.Abs(num3) <= 0.0001f)
		{
			rXColorHSL.h = 0f;
			rXColorHSL.s = 0f;
		}
		else
		{
			if (rXColorHSL.l < 0.5f)
			{
				rXColorHSL.s = num3 / (num2 + num);
			}
			else
			{
				rXColorHSL.s = num3 / (2f - num2 - num);
			}
			float num4 = ((num2 - r) / 6f + num3 * 0.5f) / num3;
			float num5 = ((num2 - g) / 6f + num3 * 0.5f) / num3;
			float num6 = ((num2 - b) / 6f + num3 * 0.5f) / num3;
			if (Mathf.Approximately(r, num2))
			{
				rXColorHSL.h = num6 - num5;
			}
			else if (Mathf.Approximately(g, num2))
			{
				rXColorHSL.h = 1f / 3f + num4 - num6;
			}
			else if (Mathf.Approximately(b, num2))
			{
				rXColorHSL.h = 2f / 3f + num5 - num4;
			}
			if (rXColorHSL.h < 0f)
			{
				rXColorHSL.h += 1f;
			}
			else if (rXColorHSL.h > 1f)
			{
				rXColorHSL.h -= 1f;
			}
		}
		return rXColorHSL;
	}

	public static Color GetColorFromHex(uint hex)
	{
		uint num = hex >> 16;
		uint num2 = hex - (num << 16);
		uint num3 = num2 >> 8;
		uint num4 = num2 - (num3 << 8);
		return new Color((float)num / 255f, (float)num3 / 255f, (float)num4 / 255f);
	}
}
