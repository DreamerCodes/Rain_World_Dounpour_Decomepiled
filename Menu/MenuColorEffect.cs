using System;
using UnityEngine;

namespace Menu;

public static class MenuColorEffect
{
	private const float midDark = 0.4435f;

	private const float midVDark = 0.2217f;

	private const float GREYR = 0.9924f;

	private const float GREYG = 0.963f;

	private const float GREYB = 1.0452f;

	public static Color rgbWhite;

	public static Color rgbMediumGrey;

	public static Color rgbDarkGrey;

	public static Color rgbVeryDarkGrey;

	public static Color rgbColored;

	public static Color rgbBlack;

	public static Color rgbDarkRed;

	internal static void _Initialize()
	{
		rgbWhite = Menu.MenuRGB(Menu.MenuColors.White);
		rgbMediumGrey = Menu.MenuRGB(Menu.MenuColors.MediumGrey);
		rgbDarkGrey = Menu.MenuRGB(Menu.MenuColors.DarkGrey);
		rgbVeryDarkGrey = Menu.MenuRGB(Menu.MenuColors.VeryDarkGrey);
		rgbColored = Menu.MenuRGB(Menu.MenuColors.Colored);
		rgbBlack = Menu.MenuRGB(Menu.MenuColors.Black);
		rgbDarkRed = Menu.MenuRGB(Menu.MenuColors.DarkRed);
	}

	public static Color MidToDark(Color mid)
	{
		return new Color(mid.r * 0.4435f, mid.g * 0.4435f, mid.b * 0.4435f, mid.a);
	}

	public static Color MidToVeryDark(Color mid)
	{
		return new Color(mid.r * 0.2217f, mid.g * 0.2217f, mid.b * 0.2217f, mid.a);
	}

	public static Color Greyscale(Color orig)
	{
		return new Color(orig.grayscale * 0.9924f, orig.grayscale * 0.963f, orig.grayscale * 1.0452f, orig.a);
	}

	public static Color HexToColor(string hex)
	{
		if (hex == "000000")
		{
			return new Color(0.01f, 0.01f, 0.01f, 1f);
		}
		try
		{
			float a = ((hex.Length == 8) ? ((float)Convert.ToInt32(hex.Substring(6, 2), 16) / 255f) : 1f);
			return new Color((float)Convert.ToInt32(hex.Substring(0, 2), 16) / 255f, (float)Convert.ToInt32(hex.Substring(2, 2), 16) / 255f, (float)Convert.ToInt32(hex.Substring(4, 2), 16) / 255f, a);
		}
		catch
		{
			throw new FormatException("Given input [" + hex + "] is not correct form of HEX Color");
		}
	}

	public static bool IsStringHexColor(string test)
	{
		if (test.Length != 6 && test.Length != 8)
		{
			return false;
		}
		try
		{
			HexToColor(test);
			return true;
		}
		catch
		{
			return false;
		}
	}

	public static string ColorToHex(Color color)
	{
		return Mathf.RoundToInt(color.r * 255f).ToString("X2") + Mathf.RoundToInt(color.g * 255f).ToString("X2") + Mathf.RoundToInt(color.b * 255f).ToString("X2");
	}

	public static Color HSLtoRGB(RXColorHSL hslColor)
	{
		return RXColor.ColorFromHSL(hslColor);
	}

	public static Color HSLtoRGB(float hue, float saturation, float luminosity)
	{
		return RXColor.ColorFromHSL(hue, saturation, luminosity);
	}

	public static RXColorHSL ColorToHSL(Color color)
	{
		return RXColor.HSLFromColor(color);
	}

	public static void TextureGreyscale(ref Texture2D texture)
	{
		for (int i = 0; i < texture.width; i++)
		{
			for (int j = 0; j < texture.height; j++)
			{
				Color pixel = texture.GetPixel(i, j);
				texture.SetPixel(i, j, Greyscale(pixel));
			}
		}
		texture.Apply();
	}

	public static Texture2D Clone(this Texture2D self)
	{
		Texture2D texture2D = new Texture2D(self.width, self.height, self.format, mipChain: false);
		texture2D.filterMode = self.filterMode;
		texture2D.wrapMode = self.wrapMode;
		texture2D.SetPixels(self.GetPixels());
		texture2D.Apply();
		return texture2D;
	}
}
