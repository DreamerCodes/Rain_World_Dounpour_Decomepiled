using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ProjectedImage : CosmeticSprite
{
	public List<string> imageNames;

	public Vector2? setPos;

	public float lastAlpha;

	public float alpha;

	public float? setAlpha;

	public int cycleTime;

	public int counter;

	public int currImg;

	public ProjectedImage(List<string> imageNames, int cycleTime)
	{
		this.imageNames = imageNames;
		this.cycleTime = cycleTime;
		LoadFile();
		setAlpha = 1f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (imageNames.Count > 1)
		{
			counter++;
			if (counter >= cycleTime)
			{
				counter = 0;
				room.PlaySound(SoundID.SS_AI_Image, 0f, 1f, 1f);
				currImg++;
				if (currImg >= imageNames.Count)
				{
					currImg = 0;
				}
			}
		}
		if (setPos.HasValue)
		{
			pos = setPos.Value;
			setPos = null;
		}
		lastAlpha = alpha;
		if (setAlpha.HasValue)
		{
			alpha = setAlpha.Value;
			setAlpha = null;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite(imageNames[0]);
		if (ModManager.MSC && room.abstractRoom.name == "SL_AI")
		{
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["MoonProjection"];
		}
		else
		{
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Projection"];
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(imageNames[currImg]);
		if (!ModManager.MSC || rCam.room.abstractRoom.name != "SL_AI")
		{
			sLeaser.sprites[0].color = new Color(Mathf.InverseLerp(1f, 1000f, Futile.atlasManager.GetElementWithName(imageNames[currImg]).sourceSize.x), Mathf.InverseLerp(1f, 1000f, Futile.atlasManager.GetElementWithName(imageNames[currImg]).sourceSize.y), 0f);
		}
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public void LoadFile()
	{
		foreach (string imageName in imageNames)
		{
			if (Futile.atlasManager.GetAtlasWithName(imageName) != null)
			{
				break;
			}
			string text = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + imageName + ".png");
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: true);
			Futile.atlasManager.LoadAtlasFromTexture(imageName, texture2D, textureFromAsset: false);
		}
	}
}
