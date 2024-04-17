using UnityEngine;

namespace MoreSlugcats;

public class DustWave : CosmeticSprite, IDrawable
{
	private RoomCamera rCam;

	public Material mat;

	public Texture2D tex;

	private RenderTexture render;

	private RenderTexture render2;

	private float mPos;

	private float lastmPos;

	public float dustWaveProgress;

	public float direction;

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		Vector2 vector = rCam.pos - room.cameraPositions[this.rCam.currentCameraPosition];
		Vector2 vector2 = room.game.rainWorld.options.ScreenSize * 0.5f;
		sLeaser.sprites[0].x = vector2.x - vector.x;
		sLeaser.sprites[0].y = vector2.y - vector.y;
		sLeaser.sprites[0].color = new Color(Mathf.Lerp(lastmPos, mPos, timeStacker), 0f, 0f);
		sLeaser.sprites[0].isVisible = mPos < 1f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		base.InitiateSprites(sLeaser, rCam);
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].anchorX = 0.5f;
		sLeaser.sprites[0].anchorY = 0.5f;
		sLeaser.sprites[0].scaleX = 100f;
		sLeaser.sprites[0].scaleY = 100f;
		sLeaser.sprites[0].shader = room.game.rainWorld.Shaders[(ModManager.MMF && room.game.rainWorld.options.quality == Options.Quality.LOW) ? "DustWaveLevelLow" : "DustWaveLevel"];
		sLeaser.sprites[0].isVisible = mPos > 0f;
		RenderFlowMap();
		AddToContainer(sLeaser, rCam, null);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		Shader.SetGlobalVector(value: new Vector4(rCam.sSize.x / (((float)rCam.room.TileWidth + 40f) * 20f) * (1366f / rCam.sSize.x) * 1.02f, rCam.sSize.y / (((float)rCam.room.TileHeight + 40f) * 20f) * 1.04f, (rCam.room.cameraPositions[rCam.currentCameraPosition].x + 400f) / (((float)rCam.room.TileWidth + 40f) * 20f), (rCam.room.cameraPositions[rCam.currentCameraPosition].y + 400f) / (((float)rCam.room.TileHeight + 40f) * 20f)), nameID: RainWorld.ShadPropTileCorrection);
		lastmPos = mPos;
		mPos = Mathf.InverseLerp(-0.04f, 0.35f, room.DustStormIntensity);
	}

	private void RenderFlowMap()
	{
		tex = new Texture2D(rCam.room.TileWidth, rCam.room.TileHeight);
		tex.filterMode = FilterMode.Bilinear;
		tex.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < rCam.room.TileWidth; i++)
		{
			for (int num = rCam.room.TileHeight - 1; num >= 0; num--)
			{
				bool flag = true;
				if (rCam.room.GetTile(i, num).Solid)
				{
					flag = false;
				}
				tex.SetPixel(i, num, (!flag) ? new Color(0f, 0f, 0f) : new Color(1f, 0f, 0f));
			}
		}
		tex.Apply();
		mat.SetFloat(RainWorld.ShadPropTopBottom, direction);
		render = RenderTexture.GetTemporary(tex.width + 40, tex.height + 40, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		render.filterMode = FilterMode.Point;
		render.wrapMode = TextureWrapMode.Clamp;
		render2 = RenderTexture.GetTemporary(tex.width + 40, tex.height + 40, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		render2.filterMode = FilterMode.Point;
		render2.wrapMode = TextureWrapMode.Clamp;
		mat.SetTexture(RainWorld.ShadPropMainTex, render);
		Shader.SetGlobalTexture(RainWorld.ShadPropOriginal, tex);
		mat.SetFloat(RainWorld.ShadPropFirstPass, 1f);
		mat.SetFloat(RainWorld.ShadPropStep, 0f);
		Graphics.Blit(tex, render, mat);
		mat.SetFloat(RainWorld.ShadPropFirstPass, 0.6f);
		for (int j = 0; j < 127; j++)
		{
			mat.SetFloat(RainWorld.ShadPropStep, (float)j * 2f / 256f);
			Graphics.Blit(render, render2, mat);
			mat.SetFloat(RainWorld.ShadPropFirstPass, 0.4f);
			mat.SetFloat(RainWorld.ShadPropStep, ((float)j * 2f + 1f) / 256f);
			Graphics.Blit(render2, render, mat);
		}
		mat.SetFloat(RainWorld.ShadPropFirstPass, 0f);
		Graphics.Blit(render, render2, mat);
		render.filterMode = FilterMode.Bilinear;
		render2.filterMode = FilterMode.Bilinear;
		Shader.SetGlobalTexture(RainWorld.ShadPropDustFlowTex, render2);
		tex = new Texture2D(render2.width, render2.height, TextureFormat.ARGB32, mipChain: false);
		tex.ReadPixels(new Rect(0f, 0f, render2.width, render2.height), 0, 0);
		RenderTexture.ReleaseTemporary(render);
		RenderTexture.ReleaseTemporary(render2);
		tex.Apply();
	}

	public DustWave(RoomCamera rCam, float dir)
	{
		direction = dir;
		this.rCam = rCam;
		mat = new Material(rCam.game.rainWorld.Shaders["DustFlowRenderer"].shader);
	}

	public float GetWindPixel(Vector2 pos)
	{
		if (tex != null)
		{
			pos.x /= 20f;
			pos.y /= 20f;
			return tex.GetPixel((int)pos.x + 20, (int)pos.y + 20).g - 0.05f;
		}
		return 0f;
	}
}
