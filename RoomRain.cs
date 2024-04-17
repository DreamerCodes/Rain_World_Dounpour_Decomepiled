using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RoomRain : UpdatableAndDeletable, IDrawable
{
	public class DangerType : ExtEnum<DangerType>
	{
		public static readonly DangerType Rain = new DangerType("Rain", register: true);

		public static readonly DangerType Flood = new DangerType("Flood", register: true);

		public static readonly DangerType FloodAndRain = new DangerType("FloodAndRain", register: true);

		public static readonly DangerType None = new DangerType("None", register: true);

		public static readonly DangerType Thunder = new DangerType("Thunder", register: true);

		public static readonly DangerType AerieBlizzard = new DangerType("AerieBlizzard", register: true);

		public DangerType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public GlobalRain globalRain;

	private RoomPalette pal;

	public List<IntVector2> splashTiles;

	public int[] rainReach;

	private int splashes;

	private Texture2D shelterTex;

	public float lastIntensity;

	public float intensity;

	public List<BulletDrip> bulletDrips;

	public DisembodiedDynamicSoundLoop normalRainSound;

	public DisembodiedDynamicSoundLoop heavyRainSound;

	public DisembodiedDynamicSoundLoop deathRainSound;

	public DisembodiedDynamicSoundLoop rumbleSound;

	public DisembodiedDynamicSoundLoop floodingSound;

	public DisembodiedDynamicSoundLoop distantDeathRainSound;

	public DisembodiedDynamicSoundLoop SCREENSHAKESOUND;

	public DangerType dangerType;

	public float RainUnderCeilings => Mathf.InverseLerp(0.5f, 1f, intensity);

	public float SplashSize => Mathf.Pow(Mathf.InverseLerp(0f, 0.5f, intensity), 2f);

	public float InsidePushAround
	{
		get
		{
			if (dangerType == DangerType.AerieBlizzard)
			{
				return globalRain.InsidePushAround;
			}
			if (dangerType == DangerType.Rain)
			{
				return globalRain.InsidePushAround * room.roomSettings.RainIntensity;
			}
			return 0f;
		}
	}

	public float OutsidePushAround
	{
		get
		{
			if (dangerType == DangerType.AerieBlizzard)
			{
				return globalRain.OutsidePushAround;
			}
			if (dangerType == DangerType.Rain || dangerType == DangerType.FloodAndRain)
			{
				return globalRain.OutsidePushAround * room.roomSettings.RainIntensity;
			}
			return 0f;
		}
	}

	public float FloodLevel
	{
		get
		{
			if (room == null || room.waterObject == null)
			{
				return -100f;
			}
			if (dangerType != DangerType.Flood && dangerType != DangerType.FloodAndRain)
			{
				return room.waterObject.originalWaterLevel;
			}
			float min = ((ModManager.MSC && room.waterInverted) ? (-80f) : (-5000f));
			float num = room.waterObject.originalWaterLevel + globalRain.flood;
			if (ModManager.MSC)
			{
				num = room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index).y + room.waterObject.originalWaterLevel;
			}
			if (room.waterFlux != null)
			{
				bool flag = globalRain.flood == 0f;
				if (ModManager.MSC)
				{
					flag = globalRain.drainWorldFlood == 0f;
				}
				if (room.world.rainCycle.TimeUntilRain > 0 && flag)
				{
					return room.waterFlux.fluxWaterLevel;
				}
				float b = room.waterObject.originalWaterLevel + globalRain.flood;
				if (ModManager.MSC)
				{
					num = room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index).y;
					b = globalRain.flood - num;
				}
				if (ModManager.MSC && room.waterInverted)
				{
					num = room.world.RoomToWorldPos(new Vector2(0f, 0f), room.abstractRoom.index).y;
					b = 0f - Mathf.Max(globalRain.flood - num, 0f);
					return Mathf.Clamp(room.waterFlux.fluxWaterLevel + b, min, room.PixelHeight + 500f);
				}
				return Mathf.Clamp(Mathf.Max(room.waterFlux.fluxWaterLevel, b), min, room.PixelHeight + 500f);
			}
			if (ModManager.MSC)
			{
				float num2 = 0f;
				if (room.abstractRoom.shelter)
				{
					num2 = room.game.globalRain.drainWorldFlood;
				}
				if (room.world.rainCycle.TimeUntilRain > 0 && room.world.game.globalRain.drainWorldFlood == 0f)
				{
					return room.waterObject.originalWaterLevel;
				}
				return Mathf.Clamp(room.waterObject.originalWaterLevel + Mathf.Max(0f, globalRain.flood - num2 - num), min, room.PixelHeight + 500f);
			}
			return room.waterObject.originalWaterLevel + globalRain.flood;
		}
	}

	public bool ArenaMode => !globalRain.game.IsStorySession;

	public RoomRain(GlobalRain globalRain, Room rm)
	{
		this.globalRain = globalRain;
		if (rm.waterObject != null && (!ModManager.MSC || rm.roomSettings.DangerType == DangerType.Flood || rm.roomSettings.DangerType == DangerType.FloodAndRain))
		{
			rm.waterObject.fWaterLevel = rm.waterObject.originalWaterLevel + globalRain.flood;
		}
		else if (rm.waterObject != null)
		{
			rm.waterObject.fWaterLevel = rm.waterObject.originalWaterLevel;
		}
		dangerType = rm.roomSettings.DangerType;
		if (rm.abstractRoom.shelter)
		{
			dangerType = DangerType.Flood;
		}
		splashTiles = new List<IntVector2>();
		rainReach = new int[rm.TileWidth];
		shelterTex = new Texture2D(rm.TileWidth, rm.TileHeight);
		for (int i = 0; i < rm.TileWidth; i++)
		{
			bool flag = true;
			for (int num = rm.TileHeight - 1; num >= 0; num--)
			{
				if (flag && rm.GetTile(i, num).Solid)
				{
					flag = false;
					if (num < rm.TileHeight - 1)
					{
						splashTiles.Add(new IntVector2(i, num));
					}
					rainReach[i] = num;
				}
				shelterTex.SetPixel(i, num, flag ? new Color(1f, 0f, 0f) : new Color(0f, 0f, 0f));
			}
		}
		if (rm.water)
		{
			for (int j = 0; j < rm.TileWidth; j++)
			{
				if (!rm.GetTile(j, rm.defaultWaterLevel).Solid)
				{
					shelterTex.SetPixel(j, rm.defaultWaterLevel, (shelterTex.GetPixel(j, rm.defaultWaterLevel).r > 0.5f) ? new Color(1f, 0f, 1f) : new Color(0f, 0f, 1f));
					for (int k = rm.defaultWaterLevel; k < rm.TileHeight && (float)k < (float)rm.defaultWaterLevel + 20f && !rm.GetTile(j, k).Solid; k++)
					{
						shelterTex.SetPixel(j, k + 1, (shelterTex.GetPixel(j, k + 1).r > 0.5f) ? new Color(1f, 0f, 1f) : new Color(0f, 0f, 1f));
					}
				}
			}
		}
		else
		{
			for (int l = 0; l < rm.TileWidth; l++)
			{
				if (!rm.GetTile(l, 0).Solid)
				{
					shelterTex.SetPixel(l, 0, (shelterTex.GetPixel(l, 0).r > 0.5f) ? new Color(1f, 0f, 1f) : new Color(0f, 0f, 1f));
				}
			}
		}
		shelterTex.wrapMode = TextureWrapMode.Clamp;
		HeavyTexturesCache.LoadAndCacheAtlasFromTexture("RainMask_" + rm.abstractRoom.name, shelterTex, textureFromAsset: false);
		shelterTex.Apply();
		splashes = splashTiles.Count * 2 / rm.cameraPositions.Length;
		if (intensity > 0f)
		{
			lastIntensity = 0f;
		}
		else
		{
			lastIntensity = 1f;
		}
		bulletDrips = new List<BulletDrip>();
		if (dangerType != DangerType.Flood)
		{
			normalRainSound = new DisembodiedDynamicSoundLoop(this);
			normalRainSound.sound = SoundID.Normal_Rain_LOOP;
			normalRainSound.VolumeGroup = 3;
			heavyRainSound = new DisembodiedDynamicSoundLoop(this);
			heavyRainSound.sound = SoundID.Heavy_Rain_LOOP;
			heavyRainSound.VolumeGroup = 3;
		}
		deathRainSound = new DisembodiedDynamicSoundLoop(this);
		deathRainSound.sound = ((dangerType != DangerType.Flood) ? SoundID.Death_Rain_LOOP : SoundID.Death_Rain_Heard_From_Underground_LOOP);
		deathRainSound.VolumeGroup = 3;
		rumbleSound = new DisembodiedDynamicSoundLoop(this);
		rumbleSound.sound = SoundID.Death_Rain_Rumble_LOOP;
		rumbleSound.VolumeGroup = 3;
		if (dangerType != DangerType.Rain && dangerType != DangerType.AerieBlizzard && (!ModManager.MSC || dangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard))
		{
			floodingSound = new DisembodiedDynamicSoundLoop(this);
			floodingSound.sound = SoundID.Flash_Flood_LOOP;
			floodingSound.VolumeGroup = ((dangerType == DangerType.Flood) ? 3 : 0);
		}
		distantDeathRainSound = new DisembodiedDynamicSoundLoop(this);
		distantDeathRainSound.sound = ((dangerType != DangerType.Flood) ? SoundID.Death_Rain_Approaching_LOOP : SoundID.Death_Rain_Approaching_Heard_From_Underground_LOOP);
		SCREENSHAKESOUND = new DisembodiedDynamicSoundLoop(this);
		SCREENSHAKESOUND.sound = SoundID.Screen_Shake_LOOP;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (ModManager.MSC && (dangerType == DangerType.FloodAndRain || dangerType == DangerType.Flood))
		{
			intensity = Mathf.Lerp(intensity, globalRain.Intensity * RoomRainFloodShake(room, globalRain.flood), 0.2f);
		}
		else if (dangerType == DangerType.Rain || dangerType == DangerType.FloodAndRain || dangerType == DangerType.AerieBlizzard)
		{
			intensity = Mathf.Lerp(intensity, globalRain.Intensity, 0.2f);
		}
		if (dangerType == DangerType.AerieBlizzard)
		{
			intensity = Mathf.Min(intensity, 1f);
		}
		else
		{
			intensity = Mathf.Min(intensity, room.roomSettings.RainIntensity);
		}
		if (ModManager.MSC && dangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard)
		{
			intensity = 0f;
		}
		lastIntensity = intensity;
		if (globalRain.AnyPushAround)
		{
			ThrowAroundObjects();
		}
		if (bulletDrips.Count < (int)((float)room.TileWidth * globalRain.bulletRainDensity * room.roomSettings.RainIntensity))
		{
			bulletDrips.Add(new BulletDrip(this));
			room.AddObject(bulletDrips[bulletDrips.Count - 1]);
		}
		else if (bulletDrips.Count > (int)((float)room.TileWidth * globalRain.bulletRainDensity * room.roomSettings.RainIntensity))
		{
			bulletDrips[0].Destroy();
			bulletDrips.RemoveAt(0);
		}
		bool flag = globalRain.flood > 0f;
		if (ModManager.MSC)
		{
			flag = globalRain.drainWorldFlood > 0f;
		}
		if ((room.roomSettings.DangerType == DangerType.Flood || room.roomSettings.DangerType == DangerType.FloodAndRain) && (room.world.rainCycle.TimeUntilRain <= 0 || flag || room.defaultWaterLevel > -1 || (ModManager.MSC && room.waterInverted) || room.waterFlux != null))
		{
			if (room.waterObject != null)
			{
				room.waterObject.fWaterLevel = Mathf.Lerp(room.waterObject.fWaterLevel, FloodLevel, ModManager.MSC ? globalRain.floodLerpSpeed : 0.2f);
				if (!ModManager.MMF || room.roomSettings.RumbleIntensity > 0f)
				{
					room.waterObject.GeneralUpsetSurface(Mathf.InverseLerp(0f, 0.5f, globalRain.Intensity) * 4f);
				}
			}
			else if (globalRain.deathRain != null || room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterFluxMaxLevel) > 0f)
			{
				room.AddWater();
				if (ModManager.MSC)
				{
					room.waterObject.fWaterLevel = FloodLevel;
				}
			}
		}
		if (dangerType != DangerType.Flood)
		{
			normalRainSound.Volume = ((intensity > 0f) ? (0.1f + 0.9f * Mathf.Pow(Mathf.Clamp01(Mathf.Sin(Mathf.InverseLerp(0.001f, 0.7f, intensity) * (float)Math.PI)), 1.5f)) : 0f);
			normalRainSound.Update();
			heavyRainSound.Volume = Mathf.Pow(Mathf.InverseLerp(0.12f, 0.5f, intensity), 0.85f) * Mathf.Pow(1f - deathRainSound.Volume, 0.3f);
			heavyRainSound.Update();
		}
		deathRainSound.Volume = Mathf.Pow(Mathf.InverseLerp(0.35f, 0.75f, intensity), 0.8f);
		deathRainSound.Update();
		rumbleSound.Volume = globalRain.RumbleSound * room.roomSettings.RumbleIntensity;
		rumbleSound.Update();
		distantDeathRainSound.Volume = Mathf.InverseLerp(1400f, 0f, room.world.rainCycle.TimeUntilRain) * room.roomSettings.RainIntensity;
		distantDeathRainSound.Update();
		if (dangerType != DangerType.Rain && dangerType != DangerType.AerieBlizzard && (!ModManager.MSC || dangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard) && room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidSea) == 0f)
		{
			floodingSound.Volume = Mathf.InverseLerp(0.01f, 0.5f, globalRain.floodSpeed);
			floodingSound.Update();
		}
		if (room.game.cameras[0].room == room)
		{
			SCREENSHAKESOUND.Volume = room.game.cameras[0].ScreenShake * (1f - rumbleSound.Volume);
		}
		else
		{
			SCREENSHAKESOUND.Volume = 0f;
		}
		SCREENSHAKESOUND.Update();
	}

	public void Unloaded()
	{
		UnityEngine.Object.Destroy(shelterTex);
		shelterTex = null;
	}

	private void ThrowAroundObjects()
	{
		if ((room.roomSettings.DangerType != DangerType.AerieBlizzard && ((ModManager.MMF && room.roomSettings.RainIntensity < 0.02f) || (ModManager.MSC && room.game.IsStorySession && room.world.region != null && room.world.region.name == "OE" && room.roomSettings.RainIntensity <= 0.2f))) || room.roomSettings.RainIntensity == 0f)
		{
			return;
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				for (int k = 0; k < room.physicalObjects[i][j].bodyChunks.Length; k++)
				{
					BodyChunk bodyChunk = room.physicalObjects[i][j].bodyChunks[k];
					IntVector2 tilePosition = room.GetTilePosition(bodyChunk.pos + new Vector2(Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value)));
					float num = InsidePushAround;
					bool flag = false;
					if (rainReach[Custom.IntClamp(tilePosition.x, 0, room.TileWidth - 1)] < tilePosition.y)
					{
						flag = true;
						num = Mathf.Max(OutsidePushAround, InsidePushAround);
					}
					if (room.water)
					{
						num *= Mathf.InverseLerp(room.FloatWaterLevel(bodyChunk.pos.x) - 100f, room.FloatWaterLevel(bodyChunk.pos.x), bodyChunk.pos.y);
					}
					if (!(num > 0f))
					{
						continue;
					}
					if (bodyChunk.ContactPoint.y < 0)
					{
						int num2 = 0;
						if (rainReach[Custom.IntClamp(tilePosition.x - 1, 0, room.TileWidth - 1)] >= tilePosition.y && !room.GetTile(tilePosition + new IntVector2(-1, 0)).Solid)
						{
							num2--;
						}
						if (rainReach[Custom.IntClamp(tilePosition.x + 1, 0, room.TileWidth - 1)] >= tilePosition.y && !room.GetTile(tilePosition + new IntVector2(1, 0)).Solid)
						{
							num2++;
						}
						bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(-30f, 30f, UnityEngine.Random.value) + (float)(num2 * 16)) * UnityEngine.Random.value * (flag ? 9f : 4f) * num / bodyChunk.mass;
					}
					else
					{
						bodyChunk.vel.y -= Mathf.Pow(UnityEngine.Random.value, 5f) * 16.5f * num / bodyChunk.mass;
					}
					if (bodyChunk.owner is Creature)
					{
						if (Mathf.Pow(UnityEngine.Random.value, 1.2f) * 2f * (float)bodyChunk.owner.bodyChunks.Length < num)
						{
							(bodyChunk.owner as Creature).Stun(UnityEngine.Random.Range(1, 1 + (int)(9f * num)));
						}
						if (bodyChunk == (bodyChunk.owner as Creature).mainBodyChunk)
						{
							(bodyChunk.owner as Creature).rainDeath += num / 20f;
						}
						if (num > 0.5f && (bodyChunk.owner as Creature).rainDeath > 1f && UnityEngine.Random.value < 0.025f)
						{
							(bodyChunk.owner as Creature).Die();
						}
					}
					bodyChunk.vel += Custom.DegToVec(Mathf.Lerp(90f, 270f, UnityEngine.Random.value)) * UnityEngine.Random.value * 5f * InsidePushAround;
				}
			}
		}
	}

	public void CreatureSmashedInGround(Creature crit, float speed)
	{
		if (speed < 2.5f)
		{
			return;
		}
		float a = InsidePushAround;
		if (crit.bodyChunks.Length != 0)
		{
			BodyChunk bodyChunk = crit.bodyChunks[UnityEngine.Random.Range(0, crit.bodyChunks.Length)];
			IntVector2 tilePosition = room.GetTilePosition(bodyChunk.pos + new Vector2(Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value)));
			if (rainReach[Custom.IntClamp(tilePosition.x, 0, room.TileWidth - 1)] < tilePosition.y)
			{
				a = Mathf.Max(OutsidePushAround, InsidePushAround);
			}
			crit.rainDeath += Mathf.InverseLerp(-2.5f, -15f, speed) * Mathf.Lerp(a, 1f, 0.5f) * 0.65f / (float)bodyChunk.owner.bodyChunks.Length;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1 + splashes];
		sLeaser.sprites[0] = new FSprite("RainMask_" + rCam.room.abstractRoom.name);
		sLeaser.sprites[0].scaleX = room.game.rainWorld.options.ScreenSize.x / (float)shelterTex.width;
		sLeaser.sprites[0].scaleY = 768f / (float)shelterTex.height;
		sLeaser.sprites[0].anchorX = 0f;
		sLeaser.sprites[0].anchorY = 0f;
		sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["DeathRain"];
		for (int i = 1; i < splashes + 1; i++)
		{
			sLeaser.sprites[i] = new FSprite((i < splashes / 2) ? "90DegreeSplash" : "TallSplash");
			sLeaser.sprites[i].anchorY = ((i < splashes / 2) ? 0.2f : 0.1f);
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Shader.SetGlobalFloat(RainWorld.ShadPropRainDirection, Mathf.Lerp(globalRain.lastRainDirection, globalRain.rainDirection, timeStacker));
		Shader.SetGlobalVector(RainWorld.ShadPropRainSpriteRect, new Vector4(camPos.x / (20f * (float)room.TileWidth), camPos.y / (20f * (float)room.TileHeight), (ModManager.MMF ? rCam.sSize.x : 1366f) / (20f * (float)room.TileWidth), 768f / (20f * (float)room.TileHeight)));
		Shader.SetGlobalFloat(RainWorld.ShadPropRainIntensity, Mathf.InverseLerp(0f, 0.5f, intensity));
		if (dangerType == DangerType.Rain && !ArenaMode)
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropRainEverywhere, Mathf.Max(0.1f, RainUnderCeilings));
		}
		else
		{
			Shader.SetGlobalFloat(RainWorld.ShadPropRainEverywhere, RainUnderCeilings);
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].isVisible = intensity > 0f;
		}
		sLeaser.sprites[0].shader = ((intensity > 0f) ? room.game.rainWorld.Shaders["DeathRain"] : room.game.rainWorld.Shaders["Basic"]);
		if (intensity == 0f)
		{
			return;
		}
		for (int j = 1; j < splashes; j++)
		{
			sLeaser.sprites[j].isVisible = false;
			if (splashTiles.Count > 0)
			{
				for (int k = 0; k < 5; k++)
				{
					Vector2 testPos = room.MiddleOfTile(splashTiles[UnityEngine.Random.Range(0, splashTiles.Count)]);
					if (testPos.y > room.FloatWaterLevel(testPos.x) && rCam.IsVisibleAtCameraPosition(rCam.currentCameraPosition, testPos))
					{
						sLeaser.sprites[j].y = testPos.y + 10f - camPos.y;
						sLeaser.sprites[j].x = testPos.x + Mathf.Lerp(-10f, 10f, UnityEngine.Random.value) - camPos.x;
						sLeaser.sprites[j].isVisible = true;
						break;
					}
				}
			}
			sLeaser.sprites[j].scaleY = SplashSize;
			if (j < splashes / 2)
			{
				sLeaser.sprites[j].rotation = Mathf.Lerp(-45f, 45f, UnityEngine.Random.value);
				sLeaser.sprites[j].scaleX = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * SplashSize;
			}
			else
			{
				sLeaser.sprites[j].rotation = Mathf.Lerp(-25f, 25f, UnityEngine.Random.value);
				sLeaser.sprites[j].scaleX = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * SplashSize;
			}
			sLeaser.sprites[j].color = Color.Lerp(pal.fogColor, new Color(1f, 1f, 1f), UnityEngine.Random.value * intensity);
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		pal = palette;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		newContatiner = rCam.ReturnFContainer("Items");
		for (int i = 1; i < splashes; i++)
		{
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}

	public static float RoomRainFloodShake(Room room, float globalFloodLevel)
	{
		float y = room.world.RoomToWorldPos(new Vector2(0f, (float)room.abstractRoom.size.y * 20f), room.abstractRoom.index).y;
		float num = 1f - Mathf.InverseLerp(y, y + 800f, globalFloodLevel);
		return 0.75f + num * 0.25f;
	}
}
