using System.Collections;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class BlizzardGraphics : CosmeticSprite, IDrawable
{
	private bool UpdatingSoundDir;

	private Vector4 snowVector;

	private RenderTexture windMap;

	public bool windMapUpdate;

	public bool tileTexUpdate;

	private float windStrength;

	private float windAngle;

	private float blizzardIntensity;

	private float snowfallIntensity;

	private Vector4 windVector;

	private RoomCamera rCam;

	private Material blizzardMapMaterial;

	private Material blizzardMapPrerenderMaterial;

	private Texture2D tileTex;

	private Texture2D windTexture;

	private bool snowAnlgeUpdate;

	private bool windAnlgeUpdate;

	private bool windRenderUpdate;

	private Vector2 lastCamPos;

	private float snowAngle;

	private float blizzardAngle;

	public bool needsUpdate;

	private float whiteOut;

	public int updateDelay;

	private int updateCount;

	private float oldSnowAngle;

	private float oldBlizzardAngle;

	private float oldBlizzardIntensity;

	private float oldSnowFallIntensity;

	private float oldWhiteOut;

	private float oldWindStrength;

	private float oldWindAngle;

	private float upDeLerp;

	public bool lerpBypass;

	private float waterTime;

	private RenderTexture reduce32;

	private RenderTexture reduce16;

	private RenderTexture reduce4;

	private RenderTexture reduce2;

	private RenderTexture blizzardMic;

	private Material reductionMaterial;

	private RenderTexture reduce8;

	private BlizzardSound blizzardSound;

	private Vector2 blizzPan;

	public Vector2 oldBlizzPan;

	public Vector2 smoothBlizzPan;

	public float directWindVol;

	public float oldDirectWindVol;

	private float soundLerp;

	private int soundDelay;

	private int soundCount;

	private float submersion;

	private float oldSubmersion;

	private float newSubmersion;

	private Material interpolateWindMap;

	private RenderTexture interpolatedWindMap;

	public float WindAngle
	{
		get
		{
			return windAngle;
		}
		set
		{
			oldWindAngle = windAngle;
			windAngle = value;
			windAnlgeUpdate = true;
		}
	}

	public float WindStrength
	{
		get
		{
			return windStrength;
		}
		set
		{
			oldWindStrength = windStrength;
			windStrength = value;
			snowAnlgeUpdate = true;
		}
	}

	public float BlizzardIntensity
	{
		get
		{
			return blizzardIntensity;
		}
		set
		{
			oldBlizzardIntensity = blizzardIntensity;
			blizzardIntensity = value;
		}
	}

	public float SnowfallIntensity
	{
		get
		{
			return snowfallIntensity;
		}
		set
		{
			oldSnowFallIntensity = snowfallIntensity;
			snowfallIntensity = value;
		}
	}

	public float WhiteOut
	{
		get
		{
			return whiteOut;
		}
		set
		{
			oldWhiteOut = whiteOut;
			whiteOut = value;
		}
	}

	public static bool regionIsWasteland(Region region)
	{
		return region?.regionParams.glacialWasteland ?? false;
	}

	public BlizzardGraphics(RoomCamera rCam)
	{
		updateDelay = 0;
		updateCount = 0;
		soundDelay = 0;
		soundCount = 0;
		needsUpdate = true;
		windMapUpdate = true;
		windRenderUpdate = true;
		WindAngle = 0f;
		WindStrength = 0f;
		SnowfallIntensity = 0f;
		BlizzardIntensity = 0f;
		WhiteOut = 0f;
		this.rCam = rCam;
		blizzardMapMaterial = new Material(this.rCam.room.game.rainWorld.Shaders["BlizzardMap"].shader);
		blizzardMapPrerenderMaterial = new Material(this.rCam.room.game.rainWorld.Shaders["BlizzardMapPrerender"].shader);
		reductionMaterial = new Material(this.rCam.room.game.rainWorld.Shaders["BlizzardReduction"].shader);
		interpolateWindMap = new Material(this.rCam.room.game.rainWorld.Shaders["InterpolateWindMap"].shader);
		reduce32 = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		reduce32.filterMode = FilterMode.Bilinear;
		reduce16 = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		reduce16.filterMode = FilterMode.Bilinear;
		reduce8 = new RenderTexture(8, 8, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		reduce8.filterMode = FilterMode.Bilinear;
		reduce4 = new RenderTexture(4, 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		reduce4.filterMode = FilterMode.Bilinear;
		reduce2 = new RenderTexture(2, 2, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		reduce2.filterMode = FilterMode.Bilinear;
		blizzardMic = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		blizzardSound = new BlizzardSound(this.rCam);
	}

	public override void Destroy()
	{
		base.Destroy();
		if (blizzardSound == null)
		{
			return;
		}
		blizzardSound.windVol = 0f;
		blizzardSound.windStrength = 0f;
		if (blizzardSound.blizzHowl != null && blizzardSound.blizzHowl.emitter != null)
		{
			blizzardSound.blizzHowl.emitter.volume = 0f;
			if (blizzardSound.blizzHowl.emitter.currentSoundObject != null)
			{
				blizzardSound.blizzHowl.emitter.currentSoundObject.Stop();
			}
		}
		if (blizzardSound.blizzWind != null && blizzardSound.blizzWind.emitter != null)
		{
			blizzardSound.blizzWind.emitter.volume = 0f;
			if (blizzardSound.blizzWind.emitter.currentSoundObject != null)
			{
				blizzardSound.blizzWind.emitter.currentSoundObject.Stop();
			}
		}
		if (room != null)
		{
			room.RemoveObject(blizzardSound);
		}
		blizzardSound.Destroy();
		blizzardSound = null;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (!room.water)
		{
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
			rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
			sLeaser.sprites[1].MoveToBack();
			sLeaser.sprites[0].MoveToBack();
		}
		else
		{
			rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
			rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[1]);
			sLeaser.sprites[1].MoveInFrontOfOtherNode(room.waterObject.firstWaterSprite);
			sLeaser.sprites[0].MoveInFrontOfOtherNode(room.waterObject.firstWaterSprite);
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (base.slatedForDeletetion)
		{
			sLeaser.sprites[0].isVisible = false;
			sLeaser.sprites[1].isVisible = false;
			return;
		}
		Vector2 vector = rCam.pos - room.cameraPositions[this.rCam.currentCameraPosition];
		Vector2 vector2 = room.game.rainWorld.options.ScreenSize * 0.5f;
		vector2 = new Vector2(Mathf.Lerp(vector2.x, lastCamPos.x, timeStacker), Mathf.Lerp(vector2.y, lastCamPos.y, timeStacker));
		float num = Mathf.Lerp(oldSnowFallIntensity, SnowfallIntensity, upDeLerp);
		float num2 = Mathf.Lerp(oldBlizzardIntensity, BlizzardIntensity, upDeLerp);
		float num3 = Mathf.Lerp(oldWhiteOut, WhiteOut, upDeLerp);
		float num4 = Mathf.Lerp(oldWindStrength, WindStrength, upDeLerp);
		float num5 = Mathf.Lerp(oldWindAngle, WindAngle, upDeLerp);
		Shader.SetGlobalFloat(RainWorld.ShadPropWindAngle, num5);
		Shader.SetGlobalFloat(RainWorld.ShadPropWindStrength, num4);
		sLeaser.sprites[0].isVisible = num > 0f;
		sLeaser.sprites[1].isVisible = num2 > 0f;
		sLeaser.sprites[0].x = vector2.x - vector.x;
		sLeaser.sprites[0].y = vector2.y * 2f - vector.y;
		sLeaser.sprites[0].color = new Color(num, 0f, 0f);
		sLeaser.sprites[0].scaleY = 170f * (1f + (num4 + (1f - Mathf.Abs(num5)) * num4) * (4f + 4f * num3));
		sLeaser.sprites[0].rotation = Mathf.Lerp(oldSnowAngle, snowAngle, upDeLerp);
		sLeaser.sprites[1].x = vector2.x - vector.x;
		sLeaser.sprites[1].y = vector2.y - vector.y;
		sLeaser.sprites[1].scaleX = 170f * (1f + 0.8f * num3);
		sLeaser.sprites[1].scaleX = 170f * (1f + 0.4f * num3);
		sLeaser.sprites[1].color = new Color(num2, num3, 0f);
		sLeaser.sprites[1].rotation = Mathf.Lerp(oldBlizzardAngle, blizzardAngle, upDeLerp);
		lastCamPos = vector2;
		UpdateWindMap();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		bool flag = room.game.rainWorld.options.quality == Options.Quality.LOW;
		BypassLerp();
		TileTexUpdate();
		UpdateWindMap();
		float num = 1f;
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[0].anchorX = 0.5f;
		sLeaser.sprites[0].anchorY = 0.5f;
		sLeaser.sprites[0].scaleX = 170f;
		sLeaser.sprites[0].scaleY = 170f * num;
		sLeaser.sprites[1].anchorX = 0.5f;
		sLeaser.sprites[1].anchorY = 0.5f;
		sLeaser.sprites[1].scaleX = 170f;
		sLeaser.sprites[1].scaleY = 170f;
		sLeaser.sprites[0].shader = room.game.rainWorld.Shaders[(ModManager.MMF && flag) ? "FastSnowFall" : "SnowFall"];
		sLeaser.sprites[0].color = new Color(Input.mousePosition.x / 35f, 0f, 0f);
		sLeaser.sprites[1].shader = room.game.rainWorld.Shaders[(ModManager.MMF && flag) ? "FastBlizzard" : "Blizzard"];
		sLeaser.sprites[1].color = new Color(0.001f, 0f, 0f);
		AddToContainer(sLeaser, rCam, null);
		updateDelay = ((ModManager.MMF && flag) ? 60 : 30);
		updateCount = updateDelay;
		soundDelay = updateDelay / 3;
		soundCount = soundDelay;
	}

	public override void Update(bool eu)
	{
		if (rCam.room == null)
		{
			return;
		}
		base.Update(eu);
		if (base.slatedForDeletetion)
		{
			return;
		}
		Shader.SetGlobalVector(value: new Vector4(rCam.sSize.x / ((float)rCam.room.TileWidth * 20f) * (1366f / rCam.sSize.x) * 1.02f, rCam.sSize.y / ((float)rCam.room.TileHeight * 20f) * 1.04f, rCam.room.cameraPositions[rCam.currentCameraPosition].x / ((float)rCam.room.TileWidth * 20f), rCam.room.cameraPositions[rCam.currentCameraPosition].y / ((float)rCam.room.TileHeight * 20f)), nameID: RainWorld.ShadPropTileCorrection);
		if (windMap != null && tileTex != null && windRenderUpdate && windTexture != null)
		{
			if (updateCount < updateDelay)
			{
				updateCount++;
			}
			else
			{
				CycleUpdate();
				windRenderUpdate = false;
				updateCount = 0;
			}
			if (soundCount < soundDelay)
			{
				soundCount++;
			}
			else if (!UpdatingSoundDir)
			{
				soundCount = 0;
				UpdateSoundDir();
			}
		}
		if (windAnlgeUpdate)
		{
			oldBlizzardAngle = blizzardAngle;
			blizzardAngle = windAngle * 120f - 90f;
			snowAnlgeUpdate = true;
			windAnlgeUpdate = false;
		}
		if (snowAnlgeUpdate)
		{
			oldSnowAngle = snowAngle;
			snowAngle = windAngle * 120f * windStrength * 0.5f;
			snowAnlgeUpdate = false;
			windRenderUpdate = true;
		}
		upDeLerp = (float)updateCount / (float)updateDelay;
		soundLerp = (float)soundCount / (float)soundDelay;
		if (lerpBypass)
		{
			BypassLerp();
			lerpBypass = false;
		}
		UpdateWaterWaves();
		submersion = Mathf.Lerp(oldSubmersion, newSubmersion, soundLerp);
		float num = Mathf.Lerp(oldWindAngle, WindAngle, upDeLerp);
		waterTime += Time.fixedDeltaTime * num * Mathf.Lerp(oldWindStrength, WindStrength, upDeLerp);
		Shader.SetGlobalFloat(RainWorld.ShadPropWaterTime, waterTime);
		float num2 = Mathf.Lerp(oldWindStrength, WindStrength, upDeLerp);
		Shader.SetGlobalFloat(RainWorld.ShadPropSnowStrength, Mathf.Lerp(oldSnowFallIntensity, SnowfallIntensity, upDeLerp));
		smoothBlizzPan = new Vector2(Mathf.Lerp(oldBlizzPan.x, blizzPan.x, soundLerp), Mathf.Lerp(oldBlizzPan.y, blizzPan.y, soundLerp));
		blizzardSound.blizzHowlPan = (smoothBlizzPan.y - smoothBlizzPan.x) * 0.6f;
		blizzardSound.windStrength = num2 * (0.4f + Mathf.Max(smoothBlizzPan.x, smoothBlizzPan.y) * submersion * 0.6f);
		float num3 = Mathf.Lerp(oldDirectWindVol, directWindVol, soundLerp);
		blizzardSound.windDir = num * num3;
		blizzardSound.windVol = (num3 * submersion * 0.7f + 0.3f) * num2 * 0.6f;
	}

	public Color GetBlizzardPixel(int x, int y)
	{
		if (windTexture != null)
		{
			int num = (int)((windAngle * 0.25f + 0.2f) * 80f);
			float num2 = 0f;
			Color pixel = windTexture.GetPixel(x, y);
			for (int i = 0; i < 4; i++)
			{
				int num3 = Mathf.Clamp(num - 2 + i, 0, 31);
				int num4 = (int)Mathf.Floor((float)num3 / 8f);
				int num5 = (int)Custom.Mod(num3, 8f);
				float num6 = 0f;
				if (num4 == 0)
				{
					num6 = pixel.r;
				}
				if (num4 == 1)
				{
					num6 = pixel.g;
				}
				if (num4 == 2)
				{
					num6 = pixel.b;
				}
				if (num4 == 3)
				{
					num6 = pixel.a;
				}
				int num7 = (int)(num6 * 255f);
				num2 += (((float)((num7 >> num5) & 1) > 0f) ? 1f : 0f);
			}
			num2 *= 0.25f;
			return new Color(num2, num2, num2);
		}
		return new Color(0f, 0f, 0f, 0f);
	}

	private void CycleUpdate()
	{
		RainCycle rainCycle = room.world.rainCycle;
		float num = rainCycle.CycleProgression;
		int num2 = rainCycle.TimeUntilRain;
		if (room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard)
		{
			num = Mathf.InverseLerp(0f, 0.75f, room.roomSettings.RainIntensity);
			num2 = (int)(3000f * Mathf.InverseLerp(1f, 0.5f, room.roomSettings.RainIntensity));
		}
		float a = Mathf.Sin((float)num2 / 900f) * 0.64f;
		a = Mathf.Lerp(a, Mathf.Sin((float)num2 / 240f), 0.3f + Mathf.Sin((float)num2 / 100f) / 4f);
		float num3 = Mathf.Lerp(WindAngle, a, 0.1f) * room.roomSettings.RainIntensity;
		if (room.world.region == null || room.roomSettings.DangerType == RoomRain.DangerType.AerieBlizzard || !regionIsWasteland(room.world.region))
		{
			WindAngle = Mathf.Lerp(num3, Mathf.Sign(a), 0.2f * (0f - Mathf.Abs(num3))) * room.roomSettings.RainIntensity;
			WindStrength = Mathf.Clamp(Mathf.Pow(num * 1.1f, 8f), 0f, 1f) * room.roomSettings.RainIntensity;
			BlizzardIntensity = Mathf.InverseLerp(3000f, 0f, num2) * room.roomSettings.RainIntensity;
			SnowfallIntensity = (Mathf.Lerp(0f, 0.6f, num * 2f) + Mathf.Clamp(Mathf.Pow(num * 3f, 2.5f), 0f, 3f) * 0.4f) * room.roomSettings.RainIntensity;
			WhiteOut = Mathf.InverseLerp(4000f, 0f, num2) * room.roomSettings.RainIntensity;
		}
		else
		{
			WindAngle = Mathf.Lerp(num3, Mathf.Sign(a * 1.2f), 0.2f * (0f - Mathf.Abs(num3))) * Mathf.Lerp(0f, 0.75f, num * 3f);
			WindStrength = Mathf.InverseLerp(rainCycle.cycleLength, 3000f, num2) * room.roomSettings.RainIntensity;
			BlizzardIntensity = Mathf.InverseLerp((float)rainCycle.cycleLength * 0.932f, 3000f, num2) * room.roomSettings.RainIntensity;
			SnowfallIntensity = Mathf.Clamp(WindStrength * 5f, 0f, 4f) * room.roomSettings.RainIntensity;
			WhiteOut = Mathf.Pow(BlizzardIntensity, 1.3f) * room.roomSettings.RainIntensity;
		}
	}

	public void UpdateWindMap()
	{
		Graphics.Blit(windMap, interpolatedWindMap, interpolateWindMap);
	}

	private void PrepareWindMap()
	{
		windMap = new RenderTexture(tileTex.width, tileTex.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		windMap.filterMode = FilterMode.Point;
		windMap.wrapMode = TextureWrapMode.Clamp;
		Shader.SetGlobalTexture(RainWorld.ShadPropWindTex, windMap);
		interpolatedWindMap = new RenderTexture(tileTex.width, tileTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		interpolatedWindMap.filterMode = FilterMode.Trilinear;
		interpolatedWindMap.wrapMode = TextureWrapMode.Clamp;
		Shader.SetGlobalTexture(RainWorld.ShadPropWindTexRendered, interpolatedWindMap);
	}

	public void TileTexUpdate()
	{
		tileTex = new Texture2D(rCam.room.TileWidth, rCam.room.TileHeight);
		tileTex.filterMode = FilterMode.Bilinear;
		tileTex.wrapMode = TextureWrapMode.Clamp;
		for (int i = 0; i < rCam.room.TileWidth; i++)
		{
			for (int num = rCam.room.TileHeight - 1; num >= 0; num--)
			{
				bool flag = true;
				if (rCam.room.GetTile(i, num).Solid)
				{
					flag = false;
				}
				tileTex.SetPixel(i, num, (!flag) ? new Color(0f, 0f, 0f) : new Color(1f, 0f, 0f));
			}
		}
		tileTex.Apply();
		Shader.SetGlobalTexture(RainWorld.ShadPropTileTex, tileTex);
		windTexture = new Texture2D(rCam.room.TileWidth, rCam.room.TileHeight, TextureFormat.ARGB32, mipChain: false, linear: true);
		windTexture.filterMode = FilterMode.Point;
		windTexture.wrapMode = TextureWrapMode.Clamp;
		if (blizzardSound != null)
		{
			rCam.room.AddObject(blizzardSound);
		}
		PrepareWindMap();
		RenderTexture temporary = RenderTexture.GetTemporary(tileTex.width, tileTex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		temporary.filterMode = FilterMode.Point;
		temporary.wrapMode = TextureWrapMode.Clamp;
		Graphics.Blit(tileTex, temporary, blizzardMapPrerenderMaterial);
		RenderTexture.ReleaseTemporary(temporary);
		windTexture.ReadPixels(new Rect(0f, 0f, rCam.room.TileWidth, rCam.room.TileHeight), 0, 0, recalculateMipMaps: false);
		windTexture.Apply();
		Graphics.Blit(tileTex, windMap, blizzardMapMaterial);
	}

	public void BypassLerp()
	{
		oldBlizzardAngle = blizzardAngle;
		oldBlizzardIntensity = blizzardIntensity;
		oldSnowAngle = snowAngle;
		oldSnowFallIntensity = snowfallIntensity;
		oldWhiteOut = whiteOut;
		oldWindAngle = windAngle;
		oldWindStrength = windStrength;
	}

	private void UpdateWaterWaves()
	{
		float num = Mathf.Lerp(oldWindStrength, WindStrength, upDeLerp);
		float num2 = Mathf.Lerp(oldWindAngle, WindAngle, upDeLerp);
		room.roomSettings.WaveAmplitude = Mathf.Lerp(room.roomSettings.WaveAmplitude, num / 20f, 0.1f);
		room.roomSettings.WaveLength = Mathf.Lerp(1f, 0.5f, BlizzardIntensity / 2f + Mathf.Abs(num2) / 2f);
		room.roomSettings.WaveSpeed = 0.5f + -1f * num2 / 2f;
	}

	private void UpdateSoundDir()
	{
		UpdatingSoundDir = true;
		oldSubmersion = newSubmersion;
		newSubmersion = 1f;
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null)
		{
			newSubmersion = Mathf.InverseLerp(1.2f, 0.7f, rCam.followAbstractCreature.realizedCreature.Submersion);
		}
		oldBlizzPan = blizzPan;
		oldDirectWindVol = directWindVol;
		reductionMaterial.SetFloat(RainWorld.ShadPropFirstPass, 1f);
		Graphics.Blit(reduce16, reduce16, reductionMaterial);
		reductionMaterial.SetFloat(RainWorld.ShadPropFirstPass, 0f);
		Graphics.Blit(reduce16, reduce8, reductionMaterial);
		Graphics.Blit(reduce8, reduce4, reductionMaterial);
		Graphics.Blit(reduce4, reduce2, reductionMaterial);
		Graphics.Blit(reduce2, blizzardMic, reductionMaterial);
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBAHalf, mipChain: false, linear: true);
		texture2D.ReadPixels(new Rect(0f, 0f, 1f, 1f), 0, 0, recalculateMipMaps: false);
		Color pixel = texture2D.GetPixel(1, 1);
		blizzPan = new Vector2(pixel.r, pixel.g);
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null)
		{
			BodyChunk mainBodyChunk = rCam.followAbstractCreature.realizedCreature.mainBodyChunk;
			directWindVol = GetBlizzardPixel((int)(mainBodyChunk.pos.x / 20f), (int)(mainBodyChunk.pos.y / 20f)).g;
			if (rCam.followAbstractCreature.realizedCreature.inShortcut)
			{
				blizzPan *= 0f;
				directWindVol *= 0f;
			}
		}
		UpdatingSoundDir = false;
	}

	public IEnumerator SoundDirTask()
	{
		UpdatingSoundDir = true;
		oldSubmersion = newSubmersion;
		newSubmersion = 1f;
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null)
		{
			newSubmersion = Mathf.InverseLerp(1.2f, 0.7f, rCam.followAbstractCreature.realizedCreature.Submersion);
		}
		oldBlizzPan = blizzPan;
		oldDirectWindVol = directWindVol;
		reductionMaterial.SetFloat(RainWorld.ShadPropFirstPass, 1f);
		Graphics.Blit(reduce16, reduce16, reductionMaterial);
		reductionMaterial.SetFloat(RainWorld.ShadPropFirstPass, 0f);
		rCam.room.game.rainWorld.StartCoroutine(SoundDirTask2());
		yield return null;
	}

	public IEnumerator SoundDirTask2()
	{
		Graphics.Blit(reduce16, reduce8, reductionMaterial);
		Graphics.Blit(reduce8, reduce4, reductionMaterial);
		Graphics.Blit(reduce4, reduce2, reductionMaterial);
		Graphics.Blit(reduce2, blizzardMic, reductionMaterial);
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.RGBAHalf, mipChain: false, linear: true);
		texture2D.ReadPixels(new Rect(0f, 0f, 1f, 1f), 0, 0, recalculateMipMaps: false);
		Color pixel = texture2D.GetPixel(1, 1);
		blizzPan = new Vector2(pixel.r, pixel.g);
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null)
		{
			BodyChunk mainBodyChunk = rCam.followAbstractCreature.realizedCreature.mainBodyChunk;
			directWindVol = GetBlizzardPixel((int)(mainBodyChunk.pos.x / 20f), (int)(mainBodyChunk.pos.y / 20f)).g;
			if (rCam.followAbstractCreature.realizedCreature.inShortcut)
			{
				blizzPan *= 0f;
				directWindVol *= 0f;
			}
		}
		UpdatingSoundDir = false;
		yield return null;
	}

	private IEnumerator ReadPixelTask()
	{
		Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false, linear: true);
		texture2D.ReadPixels(new Rect(0f, 0f, 1f, 1f), 0, 0, recalculateMipMaps: false);
		Color pixel = texture2D.GetPixel(1, 1);
		blizzPan = new Vector2(pixel.r, pixel.g);
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null)
		{
			BodyChunk mainBodyChunk = rCam.followAbstractCreature.realizedCreature.mainBodyChunk;
			directWindVol = GetBlizzardPixel((int)(mainBodyChunk.pos.x / 20f), (int)(mainBodyChunk.pos.y / 20f)).g;
			if (rCam.followAbstractCreature.realizedCreature.inShortcut)
			{
				blizzPan *= 0f;
				directWindVol *= 0f;
			}
		}
		UpdatingSoundDir = false;
		yield return null;
	}
}
