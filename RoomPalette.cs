using UnityEngine;

public struct RoomPalette
{
	public Color blackColor;

	public Color waterColor1;

	public Color waterColor2;

	public Color waterSurfaceColor1;

	public Color waterSurfaceColor2;

	public Color waterShineColor;

	public Color fogColor;

	public Color shortCutSymbol;

	public Color skyColor;

	public float fogAmount;

	public float darkness;

	public Color[] shortcutColors;

	public Texture2D texture;

	public RoomPalette(Texture2D texture, float fogAmount, float darkness, Color blackColor, Color waterColor1, Color waterColor2, Color waterSurfaceColor1, Color waterSurfaceColor2, Color waterShineColor, Color fogColor, Color skyColor, Color shortcut1, Color shortcut2, Color shortcut3, Color shortCutSymbol)
	{
		this.texture = texture;
		this.fogAmount = fogAmount;
		this.blackColor = blackColor;
		this.waterColor1 = waterColor1;
		this.waterColor2 = waterColor2;
		this.waterSurfaceColor1 = waterSurfaceColor1;
		this.waterSurfaceColor2 = waterSurfaceColor2;
		this.waterShineColor = waterShineColor;
		this.fogColor = fogColor;
		this.skyColor = skyColor;
		this.shortCutSymbol = shortCutSymbol;
		this.darkness = darkness;
		shortcutColors = new Color[3] { shortcut1, shortcut2, shortcut3 };
	}
}
