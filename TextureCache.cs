using System.Collections.Generic;
using UnityEngine;

public class TextureCache
{
	private static List<string> futileAtlasListings = new List<string>();

	public static void LoadAtlasFromTexture(string atlas, Texture texture, bool textureFromAsset)
	{
		Futile.atlasManager.LoadAtlasFromTexture(atlas, texture, textureFromAsset);
		futileAtlasListings.Add(atlas);
	}

	public static FAtlas GetAtlasWithName(string atlas)
	{
		return Futile.atlasManager.GetAtlasWithName(atlas);
	}

	public static void ClearRegisteredFutileAtlases()
	{
		for (int i = 0; i < futileAtlasListings.Count; i++)
		{
			Futile.atlasManager.UnloadAtlas(futileAtlasListings[i]);
		}
		futileAtlasListings.Clear();
	}
}
