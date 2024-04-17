using System.Collections.Generic;
using UnityEngine;

public class HeavyTexturesCache
{
	private static List<string> futileAtlasListings = new List<string>();

	public static void LoadAndCacheAtlasFromTexture(string atlas, Texture texture, bool textureFromAsset)
	{
		if (Futile.atlasManager.GetAtlasWithName(atlas) == null)
		{
			Futile.atlasManager.LoadAtlasFromTexture(atlas, texture, textureFromAsset);
			futileAtlasListings.Add(atlas);
		}
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
