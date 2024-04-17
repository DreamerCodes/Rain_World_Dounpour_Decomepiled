using UnityEngine;

public class PersistentData
{
	public RainWorld rainWorld;

	public Texture2D[,] cameraTextures;

	public PersistentData(RainWorld rainWorld)
	{
		cameraTextures = new Texture2D[2, 2];
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				cameraTextures[i, j] = new Texture2D(1400, 800, TextureFormat.ARGB32, mipChain: false);
				cameraTextures[i, j].anisoLevel = 0;
				cameraTextures[i, j].filterMode = FilterMode.Point;
				cameraTextures[i, j].wrapMode = TextureWrapMode.Clamp;
				if (j == 0)
				{
					Futile.atlasManager.LoadAtlasFromTexture("LevelTexture" + ((i == 0) ? "" : i.ToString()), cameraTextures[i, j], textureFromAsset: false);
				}
				if (j == 1)
				{
					Futile.atlasManager.LoadAtlasFromTexture("BackgroundTexture" + ((i == 0) ? "" : i.ToString()), cameraTextures[i, j], textureFromAsset: false);
				}
			}
		}
	}
}
