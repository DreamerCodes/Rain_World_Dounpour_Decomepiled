using RWCustom;
using UnityEngine;

public struct HSLColor
{
	public float hue;

	public float saturation;

	public float lightness;

	public Color rgb => Custom.HSL2RGB(hue, saturation, lightness);

	public HSLColor(float hue, float saturation, float lightness)
	{
		if (hue < 0f)
		{
			hue += Mathf.Floor(hue) + 3f;
		}
		this.hue = hue - Mathf.Floor(hue);
		this.saturation = Mathf.Clamp(saturation, 0f, 1f);
		this.lightness = Mathf.Clamp(lightness, 0f, 1f);
	}

	public static HSLColor Lerp(HSLColor from, HSLColor to, float lrp)
	{
		return new HSLColor((Mathf.LerpAngle(from.hue * 360f, to.hue * 360f, lrp) + 0f) / 360f, Mathf.Lerp(from.saturation, to.saturation, lrp), Mathf.Lerp(from.lightness, to.lightness, lrp));
	}
}
