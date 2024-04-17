using System.IO;
using RWCustom;
using UnityEngine;

public class ShaderTest : CosmeticSprite
{
	public Vector2 PANPOS;

	public Vector2 LASTPANPOS;

	public Vector2 PANVEL;

	public ShaderTest()
	{
		LoadFile("graffiti1");
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		pos = (Vector2)Futile.mousePosition + room.game.cameras[0].pos;
		LASTPANPOS = PANPOS;
		PANPOS += PANVEL;
		PANPOS = Vector2.ClampMagnitude(PANPOS, 1f) * 0.9f;
		PANVEL *= 0.8f;
		PANVEL += Custom.RNV() * 0.1f * Random.value;
		PANVEL -= PANPOS * 0.01f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("graffiti1");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Decal"];
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Foreground"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].alpha = 1f;
	}

	public void LoadFile(string fileName)
	{
		if (Futile.atlasManager.GetAtlasWithName(fileName) == null)
		{
			string path = AssetManager.ResolveFilePath("Illustrations" + Path.DirectorySeparatorChar + fileName + ".png");
			Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
			AssetManager.SafeWWWLoadTexture(ref texture2D, path, clampWrapMode: true, crispPixels: true);
			Futile.atlasManager.LoadAtlasFromTexture(fileName, texture2D, textureFromAsset: false);
		}
	}
}
