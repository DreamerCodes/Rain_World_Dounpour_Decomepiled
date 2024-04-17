using System;
using System.Collections.Generic;
using MoreSlugcats;
using Noise;
using RWCustom;
using SplashWater;
using UnityEngine;

public class Water : IDrawable, IAccessibilityModifier
{
	public class WaterSoundObject : UpdatableAndDeletable
	{
	}

	private class SurfacePoint
	{
		public Vector2 defaultPos;

		public Vector2 lastPos;

		public Vector2 pos;

		public float height;

		public float lastHeight;

		public float nextHeight;

		public Vector2 RoomPos => defaultPos + pos;

		public Vector2 LastRoomPos => defaultPos + lastPos;

		public SurfacePoint(Vector2 defaultPos)
		{
			this.defaultPos = defaultPos;
			lastPos = new Vector2(0f, 0f);
			pos = new Vector2(0f, 0f);
			height = 0f;
			lastHeight = 0f;
			nextHeight = 0f;
		}
	}

	private class BubbleEmitter
	{
		private Water water;

		public BodyChunk chunk;

		public float amount;

		public float decrease;

		public BubbleEmitter(Water water, BodyChunk chunk, float amount, float decrease)
		{
			this.water = water;
			this.chunk = chunk;
			this.amount = amount;
			this.decrease = decrease;
		}

		public void Update()
		{
			amount -= decrease;
			if (UnityEngine.Random.value * 40f < amount)
			{
				water.room.AddObject(new Bubble(chunk.pos + Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * chunk.rad, chunk.vel, bottomBubble: false, fakeWaterBubble: false));
			}
			if (ModManager.MMF && water.room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.VoidSea) > 0f)
			{
				amount = 0f;
			}
		}
	}

	private class RippleWave
	{
		public Water water;

		public Vector2 pos;

		public float rad;

		public float speed;

		public float width;

		public float lifeTime;

		public float intensity;

		public float life;

		public RippleWave(Water water, Vector2 pos, float initRad, float speed, float width, float lifeTime, float intensity)
		{
			rad = initRad;
			this.water = water;
			this.pos = pos;
			this.speed = speed;
			this.width = width;
			this.lifeTime = lifeTime;
			this.intensity = intensity;
			life = 1f;
		}

		public void Update()
		{
			rad += speed * Mathf.Pow(1f - water.viscosity, 5f);
			life -= 1f / lifeTime;
			water.Ripple(this);
		}
	}

	public Room room;

	public float triangleWidth = 20f;

	private bool slatedForDeletetion;

	public float leftMargin;

	public float rightMargin;

	private SurfacePoint[,] surface;

	private float sinCounter;

	private int pointsToRender;

	public float originalWaterLevel;

	public float fWaterLevel;

	private List<BubbleEmitter> bubbleEmitters;

	private List<RippleWave> rippleWaves;

	private float dx;

	private float dt;

	private float C;

	private float R;

	public float cosmeticSurfaceDisplace;

	private RoomPalette palette;

	public float cosmeticLowerBorder = -1f;

	public RectangularDynamicSoundLoop waterSounds;

	public RectangularDynamicSoundLoop upsetWaterSounds;

	public WaterSoundObject waterSoundObject;

	public float[,] camerasOutOfBreathFac;

	public float viscosity;

	public FSprite firstWaterSprite;

	public bool WaterIsLethal;

	private float waveAmplitude => Mathf.Lerp(1f, 40f, room.roomSettings.WaveAmplitude);

	private float waveSpeed => Mathf.Lerp(-1f / 30f, 1f / 30f, room.roomSettings.WaveSpeed);

	private float waveLength => Mathf.Lerp(50f, 750f, room.roomSettings.WaveLength);

	private float rollBackLength => Mathf.Lerp(2f, 0f, room.roomSettings.SecondWaveLength);

	private float rollBackAmp => room.roomSettings.SecondWaveAmplitude;

	public Water(Room room, int waterLevel)
	{
		this.room = room;
		originalWaterLevel = room.MiddleOfTile(new IntVector2(0, waterLevel)).y;
		if (ModManager.MSC && room.roomRain != null && room.roomRain.globalRain != null && (room.roomSettings.DangerType == RoomRain.DangerType.Flood || room.roomSettings.DangerType == RoomRain.DangerType.FloodAndRain))
		{
			fWaterLevel = originalWaterLevel + room.roomRain.globalRain.flood;
		}
		fWaterLevel = originalWaterLevel;
		dx = 0.0005f * triangleWidth;
		dt = 0.0045f;
		C = 1f;
		R = C * dt / dx;
		leftMargin = 220f;
		rightMargin = 220f;
		float num = 0f - leftMargin;
		float num2 = room.PixelWidth + rightMargin;
		camerasOutOfBreathFac = new float[room.game.cameras.Length, 4];
		int num3 = (int)((num2 - num) / triangleWidth) + 1;
		surface = new SurfacePoint[num3, 2];
		for (int i = 0; i < surface.GetLength(0); i++)
		{
			for (int j = 0; j < 2; j++)
			{
				surface[i, j] = new SurfacePoint(new Vector2(num + ((float)i + ((j == 0) ? 0f : 0.5f)) * triangleWidth, originalWaterLevel));
			}
		}
		pointsToRender = Custom.IntClamp((int)((room.game.rainWorld.options.ScreenSize.x + 60f) / triangleWidth) + 2, 0, surface.GetLength(0));
		bubbleEmitters = new List<BubbleEmitter>();
		rippleWaves = new List<RippleWave>();
		waterSoundObject = new WaterSoundObject();
		room.AddObject(waterSoundObject);
		if (ModManager.MSC && room.waterInverted)
		{
			waterSounds = new RectangularDynamicSoundLoop(waterSoundObject, new FloatRect(0f, room.PixelHeight - (float)(room.defaultWaterLevel - 1) * 20f, room.PixelWidth, room.PixelHeight - (float)room.defaultWaterLevel * 20f), room);
		}
		else
		{
			waterSounds = new RectangularDynamicSoundLoop(waterSoundObject, new FloatRect(0f, (float)(room.defaultWaterLevel - 1) * 20f, room.PixelWidth, (float)room.defaultWaterLevel * 20f), room);
		}
		waterSounds.sound = SoundID.Water_Surface_Calm_LOOP;
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.LethalWater) > 0f)
		{
			WaterIsLethal = true;
		}
	}

	public void Update()
	{
		waterSounds.Update();
		if (room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SilenceWater) > 0f)
		{
			waterSounds.Volume = 1f - room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.SilenceWater);
		}
		viscosity = room.roomSettings.GetEffectAmount(RoomSettings.RoomEffect.Type.WaterViscosity);
		for (int i = 0; i < camerasOutOfBreathFac.GetLength(0); i++)
		{
			camerasOutOfBreathFac[i, 1] = camerasOutOfBreathFac[i, 0];
			camerasOutOfBreathFac[i, 3] = camerasOutOfBreathFac[i, 2];
			if (room.game.cameras[i].followAbstractCreature != null && room.game.cameras[i].followAbstractCreature.realizedCreature != null && room.game.cameras[i].followAbstractCreature.realizedCreature is Player)
			{
				if (room.game.cameras[i].followAbstractCreature.realizedCreature.dead)
				{
					camerasOutOfBreathFac[i, 0] = Custom.LerpAndTick(camerasOutOfBreathFac[i, 0], 0f, 0.2f, 0.05f);
				}
				else
				{
					camerasOutOfBreathFac[i, 0] = Mathf.InverseLerp(0.5f, 0.2f, (room.game.cameras[i].followAbstractCreature.realizedCreature as Player).airInLungs);
				}
			}
			camerasOutOfBreathFac[i, 2] += 1f / Mathf.Lerp(40f, 4f, camerasOutOfBreathFac[i, 0]);
		}
		bool num = ModManager.MSC && room.waterInverted;
		IntVector2 intVector = new IntVector2(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(0, room.defaultWaterLevel));
		if (num)
		{
			intVector = new IntVector2(UnityEngine.Random.Range(0, room.TileWidth), UnityEngine.Random.Range(room.defaultWaterLevel, room.TileHeight * 20));
		}
		if (room.GetTile(intVector).Terrain == Room.Tile.TerrainType.Air && room.GetTile(intVector + new IntVector2(0, -1)).Terrain == Room.Tile.TerrainType.Solid)
		{
			room.AddObject(new Bubble(room.MiddleOfTile(intVector) + new Vector2(Mathf.Lerp(-10f, 10f, UnityEngine.Random.value), -10f), new Vector2(0f, 0f), bottomBubble: true, fakeWaterBubble: false));
		}
		sinCounter -= waveSpeed * Mathf.Pow(1f - viscosity, 2f);
		int num2 = 0;
		for (int j = 0; j < room.physicalObjects.Length; j++)
		{
			foreach (PhysicalObject item in room.physicalObjects[j])
			{
				BodyChunk[] bodyChunks = item.bodyChunks;
				foreach (BodyChunk bodyChunk in bodyChunks)
				{
					if (viscosity > 0f && bodyChunk.submersion > 0f)
					{
						if (bodyChunk.submersion < 1f)
						{
							bodyChunk.vel.x *= 1f - 0.75f * viscosity;
							if (bodyChunk.vel.y > 0f)
							{
								bodyChunk.vel.y *= 1f - 0.075f * viscosity;
							}
							else
							{
								bodyChunk.vel.y *= 1f - 0.15f * viscosity;
							}
						}
						else
						{
							bodyChunk.vel.x *= 1f - 0.225f * viscosity;
							if (bodyChunk.vel.y > 0f)
							{
								bodyChunk.vel.y *= 1f - 0.1f * viscosity;
							}
							else
							{
								bodyChunk.vel.y *= 1f - 0.15f * viscosity;
							}
						}
					}
					if ((bodyChunk.vel.y < -3f && bodyChunk.lastPos.y > DetailedWaterLevel(bodyChunk.pos.x) && bodyChunk.pos.y <= DetailedWaterLevel(bodyChunk.pos.x)) || (bodyChunk.vel.y > 3f && bodyChunk.lastPos.y < DetailedWaterLevel(bodyChunk.pos.x) && bodyChunk.pos.y >= DetailedWaterLevel(bodyChunk.pos.x)))
					{
						float num3 = Mathf.Lerp(bodyChunk.vel.y * bodyChunk.rad * Mathf.Lerp(bodyChunk.mass, 1f, 0.3f) / 3f, 10f, 0.5f);
						if (Mathf.Abs(num3) > Mathf.Abs(bodyChunk.vel.y))
						{
							num3 = bodyChunk.vel.y;
						}
						if (bodyChunk.splashStop == 0)
						{
							bodyChunk.splashStop = 10;
							int num4 = ClosestSurfacePoint(bodyChunk.pos.x);
							surface[num4, 0].height += num3 / 2f * (1f - viscosity);
							if (num4 > 0)
							{
								surface[num4 - 1, 0].height += num3 / 6f * (1f - viscosity);
							}
							if (num4 < surface.GetLength(0) - 1)
							{
								surface[num4 + 1, 0].height += num3 / 6f * (1f - viscosity);
							}
							ThroughSurfaceSound(bodyChunk);
						}
						if (Mathf.Abs(num3) > 5f)
						{
							Splash splash = new Splash();
							room.AddObject(splash);
							splash.Reset(new Vector2(bodyChunk.pos.x, room.FloatWaterLevel(bodyChunk.pos.x) + ((num3 > 0f) ? 20f : (-20f))), new Vector2(bodyChunk.vel.x * 0.5f, Custom.LerpMap(Mathf.Abs(num3), 10f, 40f, 4f, 14f)), Custom.LerpMap(Mathf.Abs(bodyChunk.vel.y) * bodyChunk.rad * bodyChunk.mass, 4f, 300f, 7f, 90f), bodyChunk.rad);
							num2++;
						}
						if (num2 >= 10)
						{
							continue;
						}
						for (int l = 0; l < (int)Mathf.Abs(num3 * ((num3 < 0f) ? 0.25f : 0.55f)); l++)
						{
							Splash splash = new Splash();
							room.AddObject(splash);
							splash.Reset(new Vector2(bodyChunk.pos.x + Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), room.FloatWaterLevel(bodyChunk.pos.x) + 5f), new Vector2(bodyChunk.vel.x, 0f) + Custom.DegToVec(-45f + 90f * UnityEngine.Random.value) * UnityEngine.Random.value * (Mathf.Abs(bodyChunk.vel.y) * ((num3 > 0f) ? 0.4f : 0.3f) + 10f), Custom.LerpMap(Mathf.Abs(bodyChunk.vel.y) * bodyChunk.rad * bodyChunk.mass, 7f, 200f, 2f, 10f), bodyChunk.rad);
							num2++;
						}
						if (bodyChunk.vel.y < 0f && bubbleEmitters.Count < 10)
						{
							bubbleEmitters.Add(new BubbleEmitter(this, bodyChunk, Mathf.Clamp((1f + bodyChunk.vel.magnitude) * Mathf.Lerp(Mathf.Abs(num3), 5f, 0.75f) * 0.2f, 3f, 25f), 0.4f));
							for (int m = 0; m < (int)Mathf.Abs(num3 * 0.75f); m++)
							{
								room.AddObject(new Bubble(new Vector2(bodyChunk.pos.x + Mathf.Lerp(0f - bodyChunk.rad, bodyChunk.rad, UnityEngine.Random.value), room.FloatWaterLevel(bodyChunk.pos.x) - 5f), Custom.DegToVec(135f + 90f * UnityEngine.Random.value) * UnityEngine.Random.value * (Mathf.Abs(num3 * 1.5f) + 10f), bottomBubble: false, fakeWaterBubble: false));
								num2++;
							}
						}
					}
					else if (bodyChunk.pos.y - bodyChunk.rad < fWaterLevel && bodyChunk.pos.y + bodyChunk.rad > fWaterLevel)
					{
						int num5 = PreviousSurfacePoint(bodyChunk.pos.x);
						int num6 = Custom.IntClamp(num5 + 1, 0, surface.GetLength(0) - 1);
						surface[num5, 0].height -= bodyChunk.vel.x * bodyChunk.rad * 0.03f * (1f - viscosity);
						surface[num6, 0].height += bodyChunk.vel.x * bodyChunk.rad * 0.03f * (1f - viscosity);
						if (Mathf.Abs(bodyChunk.vel.y) > 2f)
						{
							surface[num5, 0].height += bodyChunk.vel.y * bodyChunk.rad * 0.01f * (1f - viscosity);
							surface[num6, 0].height += bodyChunk.vel.y * bodyChunk.rad * 0.01f * (1f - viscosity);
						}
					}
				}
			}
		}
		for (int num7 = bubbleEmitters.Count - 1; num7 >= 0; num7--)
		{
			if (bubbleEmitters[num7].amount <= 0f)
			{
				bubbleEmitters.RemoveAt(num7);
			}
			else
			{
				bubbleEmitters[num7].Update();
			}
		}
		for (int num8 = rippleWaves.Count - 1; num8 >= 0; num8--)
		{
			if (rippleWaves[num8].life < 0f)
			{
				rippleWaves.RemoveAt(num8);
			}
			else
			{
				rippleWaves[num8].Update();
			}
		}
		float num9 = 0f;
		for (int n = 0; n < surface.GetLength(0); n++)
		{
			if (n == 0)
			{
				surface[n, 0].nextHeight = (2f * surface[n, 0].height + (R - 1f) * surface[n, 0].lastHeight + 2f * Mathf.Pow(R, 2f) * (surface[n + 1, 0].height - surface[n, 0].height)) / (1f + R);
			}
			else if (n == surface.GetLength(0) - 1)
			{
				surface[n, 0].nextHeight = (2f * surface[n, 0].height + (R - 1f) * surface[n, 0].lastHeight + 2f * Mathf.Pow(R, 2f) * (surface[n - 1, 0].height - surface[n, 0].height)) / (1f + R);
			}
			else
			{
				surface[n, 0].nextHeight = Mathf.Pow(R, 2f) * (surface[n - 1, 0].height + surface[n + 1, 0].height) + 2f * (1f - Mathf.Pow(R, 2f)) * surface[n, 0].height - surface[n, 0].lastHeight;
				if (room.GetTile(surface[n, 0].defaultPos + new Vector2(0f, surface[n, 0].height)).Terrain == Room.Tile.TerrainType.Solid)
				{
					surface[n, 0].nextHeight *= (room.waterInFrontOfTerrain ? 0.95f : 0.75f);
				}
			}
			surface[n, 0].nextHeight += Mathf.Lerp(0f - waveAmplitude, waveAmplitude, UnityEngine.Random.value) * 0.005f * (1f - viscosity);
			surface[n, 0].nextHeight *= 0.99f * (0.1f * (1f - viscosity) + 0.9f);
			if (room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MSC || room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard))
			{
				surface[n, 0].nextHeight += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * room.world.rainCycle.ScreenShake * room.roomSettings.RumbleIntensity * (1f - viscosity);
			}
			num9 += surface[n, 0].height;
			for (int num10 = 0; num10 < 2; num10++)
			{
				float num11 = (0f - (float)n) * triangleWidth / waveLength;
				surface[n, num10].lastPos = surface[n, num10].pos;
				surface[n, num10].defaultPos.y = fWaterLevel;
				float num12 = surface[n, num10].height * 3f;
				float num13 = 3f;
				for (int num14 = -1; num14 < 2; num14 += 2)
				{
					if (n + num14 * 2 > 0 && n + num14 * 2 < surface.GetLength(0) && Mathf.Abs(surface[n, num10].height - surface[n + num14, num10].height) > Mathf.Abs(surface[n, num10].height - surface[n + num14 * 2, num10].height))
					{
						num12 += surface[n + num14, num10].height;
						num13 += 1f;
					}
				}
				surface[n, num10].pos = new Vector2(0f, num12 / num13);
				surface[n, num10].pos += Custom.DegToVec((num11 + sinCounter * ((num10 == 1) ? 1f : (-1f))) * 360f) * waveAmplitude;
				if (room.roomSettings.DangerType != RoomRain.DangerType.None && (!ModManager.MSC || room.roomSettings.DangerType != MoreSlugcatsEnums.RoomRainDangerType.Blizzard))
				{
					surface[n, num10].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * room.world.rainCycle.MicroScreenShake * 4f * room.roomSettings.RumbleIntensity * Mathf.Pow(1f - viscosity, 3f);
				}
				surface[n, num10].pos += Custom.DegToVec((num11 + sinCounter * ((num10 == 1) ? (-1f) : 1f)) * 360f * rollBackLength) * waveAmplitude * rollBackAmp * Mathf.Pow(1f - viscosity, 3f);
			}
		}
		num9 /= (float)surface.GetLength(0) * 1.5f;
		for (int num15 = 0; num15 < surface.GetLength(0); num15++)
		{
			surface[num15, 0].lastHeight = surface[num15, 0].height;
			float num16 = surface[num15, 0].nextHeight - num9;
			if (num15 > 0 && num15 < surface.GetLength(0) - 1)
			{
				num16 = Mathf.Lerp(num16, Mathf.Lerp(surface[num15 - 1, 0].nextHeight, surface[num15 + 1, 0].nextHeight, 0.5f), 0.01f);
			}
			surface[num15, 0].height = Mathf.Clamp(num16, -40f, 40f);
		}
	}

	public void ThroughSurfaceSound(BodyChunk chunk)
	{
		float num = Mathf.Sin(Mathf.InverseLerp(-1f, -50f, chunk.vel.y));
		float num2 = Mathf.InverseLerp(-30f, -60f, chunk.vel.y);
		if (!(chunk.vel.y < -1f))
		{
			return;
		}
		if (chunk.mass < 0.1f)
		{
			room.PlaySound(SoundID.Small_Object_Into_Water_Fast, chunk.pos, num2, 1f);
			room.PlaySound(SoundID.Small_Object_Into_Water_Slow, chunk.pos, num, 1f);
			if (ModManager.MMF)
			{
				room.InGameNoise(new InGameNoise(chunk.pos, 250f, chunk.owner, num2));
			}
		}
		else if (chunk.mass < 0.8f)
		{
			room.PlaySound(SoundID.Medium_Object_Into_Water_Fast, chunk.pos, num2, 1f);
			room.PlaySound(SoundID.Medium_Object_Into_Water_Slow, chunk.pos, num, 1f);
			if (ModManager.MMF)
			{
				room.InGameNoise(new InGameNoise(chunk.pos, 450f, chunk.owner, num2 * 1.2f + num));
			}
		}
		else
		{
			room.PlaySound(SoundID.Large_Object_Into_Water_Fast, chunk.pos, num2, 1f);
			room.PlaySound(SoundID.Large_Object_Into_Water_Slow, chunk.pos, num, 1f);
			if (ModManager.MMF)
			{
				room.InGameNoise(new InGameNoise(chunk.pos, 850f, chunk.owner, num2 * 2f + num));
			}
		}
	}

	public Vector2 SurfaceLeftAndRightBoundries()
	{
		return new Vector2(surface[0, 0].defaultPos.x, surface[surface.GetLength(0) - 1, 0].defaultPos.x);
	}

	public int PreviousSurfacePoint(float horizontalPosition)
	{
		int num = Mathf.Clamp(Mathf.FloorToInt((horizontalPosition + 250f) / triangleWidth) + 2, 0, surface.GetLength(0) - 1);
		while (num > 0 && horizontalPosition < surface[num, 0].defaultPos.x + surface[num, 0].pos.x)
		{
			num--;
		}
		return num;
	}

	public int ClosestSurfacePoint(float horizontalPosition)
	{
		int num = Custom.IntClamp(PreviousSurfacePoint(horizontalPosition), 0, surface.GetLength(0) - 2);
		if (Mathf.Abs(surface[num, 0].defaultPos.x + surface[num, 0].pos.x - horizontalPosition) < Mathf.Abs(surface[num + 1, 0].defaultPos.x + surface[num + 1, 0].pos.x - horizontalPosition))
		{
			return num;
		}
		return num + 1;
	}

	public float DetailedWaterLevel(float horizontalPosition)
	{
		int num = PreviousSurfacePoint(horizontalPosition);
		int num2 = Custom.IntClamp(num + 1, 0, surface.GetLength(0) - 1);
		float t = Mathf.InverseLerp(surface[num, 0].defaultPos.x + surface[num, 0].pos.x, surface[num2, 0].defaultPos.x + surface[num2, 0].pos.x, horizontalPosition);
		return Mathf.Lerp(surface[num, 0].defaultPos.y + surface[num, 0].pos.y, surface[num2, 0].defaultPos.y + surface[num2, 0].pos.y, t);
	}

	public bool IsTileAccessible(IntVector2 tile, CreatureTemplate crit)
	{
		if (!WaterIsLethal)
		{
			return true;
		}
		bool flag = tile.y <= room.defaultWaterLevel + 1;
		if (ModManager.MSC && room.waterInverted)
		{
			flag = tile.y >= room.defaultWaterLevel - 1;
		}
		return !flag;
	}

	public Vector2 shiftWithInversion(Vector2 shift)
	{
		if (ModManager.MSC && room.waterInverted)
		{
			return new Vector2(shift.x, 0f - shift.y);
		}
		return shift;
	}

	private int FindFirstVisibleTriangle(RoomCamera rCam)
	{
		for (int i = 0; i < surface.GetLength(0); i++)
		{
			if (rCam.IsVisibleAtCameraPosition(rCam.currentCameraPosition, surface[i, 0].defaultPos) && i > 0)
			{
				return i - 1;
			}
		}
		return 0;
	}

	public void WaterfallHitSurface(float left, float right, float flow)
	{
		int num = PreviousSurfacePoint(left);
		int num2 = Custom.IntClamp(PreviousSurfacePoint(right) + 1, 0, surface.GetLength(0) - 1);
		for (int i = num; i <= num2; i++)
		{
			if (UnityEngine.Random.value < flow)
			{
				surface[i, 0].height += (1f - viscosity) * 1.9f * Mathf.Lerp(-1f, 0.5f, UnityEngine.Random.value) * ((i == num || i == num2) ? (-1f) : 1f) * Mathf.Lerp(0.6f, 1f, flow);
				surface[i, 0].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 5f * Mathf.Pow(1f - viscosity, 5f);
			}
		}
	}

	public void GeneralUpsetSurface(float intensity)
	{
		int num = UnityEngine.Random.Range(0, surface.GetLength(0));
		surface[num, 0].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * intensity * Mathf.Pow(1f - viscosity, 5f);
		surface[num, 0].height += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 4f * intensity * (1f - viscosity);
	}

	public void Explosion(Vector2 pos, float rad, float frc)
	{
		for (int i = 0; i < surface.GetLength(0); i++)
		{
			if (Custom.DistLess(surface[i, 0].RoomPos, pos, rad) && room.VisualContact(surface[i, 0].RoomPos, pos))
			{
				float num = Mathf.InverseLerp(rad, 0f, Vector2.Distance(surface[i, 0].RoomPos, pos));
				if (num > 0.8f)
				{
					num *= -2f;
				}
				num *= frc;
				surface[i, 0].height += num * ((pos.y > DetailedWaterLevel(pos.x)) ? (-1f) : 1f) * UnityEngine.Random.value * (1f - viscosity);
				surface[i, 0].pos += Custom.DegToVec(UnityEngine.Random.value) * num;
			}
		}
		rippleWaves.Add(new RippleWave(this, pos, rad / 4f, 17f, 70f, 20f, frc * 3f));
	}

	public void Explosion(Explosion explosion)
	{
		for (int i = 0; i < surface.GetLength(0); i++)
		{
			if (Custom.DistLess(surface[i, 0].RoomPos, explosion.pos, explosion.rad) && room.VisualContact(explosion.pos, surface[i, 0].RoomPos))
			{
				float num = Mathf.Pow(Mathf.InverseLerp(explosion.rad, 0f, Vector2.Distance(surface[i, 0].RoomPos, explosion.pos)), 2f);
				Vector2 vector = Custom.DirVec(explosion.pos, surface[i, 0].RoomPos);
				surface[i, 0].pos += vector * num * explosion.force * 5f;
				surface[i, 0].height += Mathf.Cos(num * (float)Math.PI * 2f) * num * explosion.force * 8f;
				num *= Mathf.InverseLerp(0f, 8f, explosion.force);
				if (UnityEngine.Random.value < Mathf.Pow(num, 0.4f))
				{
					Splash splash = new Splash();
					room.AddObject(splash);
					vector.y = Mathf.Abs(vector.y) * 3f;
					vector.x *= 0.15f;
					splash.Reset(Vector2.Lerp(surface[Math.Max(0, i - 1), 0].RoomPos, surface[Math.Min(surface.GetLength(0) - 1, i + 1), 0].RoomPos, UnityEngine.Random.value) - vector * 3f, vector * (1f + num) * 12f, 6f + 15f * num, 3f + num * 4f);
				}
			}
		}
		rippleWaves.Add(new RippleWave(this, explosion.pos, explosion.rad / 4f, 17f, 70f, 20f, explosion.force * 3f));
	}

	private void Ripple(RippleWave wave)
	{
		for (int i = 0; i < surface.GetLength(0); i++)
		{
			float num = Mathf.InverseLerp(wave.width, wave.width / 3f, Mathf.Abs(Vector2.Distance(surface[i, 0].RoomPos, wave.pos) - wave.rad));
			num *= Mathf.InverseLerp(0f, 0.5f, wave.life);
			surface[i, 0].pos += Custom.DegToVec(UnityEngine.Random.value * 360f) * num * wave.intensity * Mathf.Pow(1f - viscosity, 5f);
			surface[i, 0].height += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f * num * wave.intensity * (1f - viscosity);
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		TriangleMesh.Triangle[] array = new TriangleMesh.Triangle[pointsToRender * 2];
		for (int i = 0; i < pointsToRender; i++)
		{
			int num = i * 2;
			array[num] = new TriangleMesh.Triangle(num, num + 1, num + 2);
			array[num + 1] = new TriangleMesh.Triangle(num + 1, num + 2, num + 3);
		}
		sLeaser.sprites[0] = new WaterTriangleMesh("Futile_White", array, customColor: true);
		if ((ModManager.MSC && room.roomSettings.DangerType == MoreSlugcatsEnums.RoomRainDangerType.Blizzard) || (ModManager.MSC && room.world.region != null && room.world.region.name == "HR") || room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.LavaSurface) != null)
		{
			sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["WaterSlush"];
		}
		else
		{
			sLeaser.sprites[0].shader = room.game.rainWorld.Shaders["WaterSurface"];
		}
		TriangleMesh.Triangle[] array2 = new TriangleMesh.Triangle[pointsToRender * 2];
		for (int j = 0; j < pointsToRender; j++)
		{
			int num2 = j * 2;
			array2[num2] = new TriangleMesh.Triangle(num2, num2 + 1, num2 + 2);
			array2[num2 + 1] = new TriangleMesh.Triangle(num2 + 1, num2 + 2, num2 + 3);
		}
		sLeaser.sprites[1] = new WaterTriangleMesh("Futile_White", array2, customColor: false);
		sLeaser.sprites[1].color = new Color(0f, 0f, 0f);
		sLeaser.sprites[1].shader = room.game.rainWorld.Shaders["DeepWater"];
		firstWaterSprite = sLeaser.sprites[0];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[1].isVisible = !rCam.voidSeaMode;
		if (rCam.voidSeaMode)
		{
			return;
		}
		float y = -10f;
		if (cosmeticLowerBorder > -1f)
		{
			y = cosmeticLowerBorder - camPos.y;
		}
		if (ModManager.MSC && room.waterInverted)
		{
			y = (float)room.TileHeight * 22f;
		}
		int num = Custom.IntClamp(PreviousSurfacePoint(camPos.x - 30f), 0, surface.GetLength(0) - 1);
		int num2 = Custom.IntClamp(num + pointsToRender, 0, surface.GetLength(0) - 1);
		for (int i = num; i < num2; i++)
		{
			int num3 = (i - num) * 2;
			Vector2 v = surface[i, 0].defaultPos + Vector2.Lerp(surface[i, 0].lastPos, surface[i, 0].pos, timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			Vector2 v2 = surface[i, 1].defaultPos + Vector2.Lerp(surface[i, 1].lastPos, surface[i, 1].pos, timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			Vector2 v3 = surface[i + 1, 0].defaultPos + Vector2.Lerp(surface[i + 1, 0].lastPos, surface[i + 1, 0].pos, timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			Vector2 v4 = surface[i + 1, 1].defaultPos + Vector2.Lerp(surface[i + 1, 1].lastPos, surface[i + 1, 1].pos, timeStacker) - camPos + new Vector2(0f, cosmeticSurfaceDisplace);
			v = Custom.ApplyDepthOnVector(v, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), -10f);
			v2 = Custom.ApplyDepthOnVector(v2, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), 30f);
			v3 = Custom.ApplyDepthOnVector(v3, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), -10f);
			v4 = Custom.ApplyDepthOnVector(v4, new Vector2(rCam.sSize.x / 2f, rCam.sSize.y * (2f / 3f)), 30f);
			if (i == num)
			{
				v2.x -= 100f;
			}
			else if (i == num2 - 1)
			{
				v2.x += 100f;
			}
			Vector2 vector = Vector2.zero;
			if (ModManager.MSC && room.waterInverted)
			{
				vector = new Vector2(0f, -40f);
			}
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num3, v);
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num3 + 1, v2 + vector);
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num3 + 2, v3);
			(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(num3 + 3, ModManager.MSC ? (v4 + vector) : v3);
			float num4 = rCam.room.WaterShinyness(Vector2.Lerp(surface[i, 0].LastRoomPos, surface[i, 0].RoomPos, timeStacker), timeStacker);
			float num5 = Vector2.Dot((surface[i + 1, 0].RoomPos - surface[i, 0].RoomPos).normalized, Custom.DegToVec(60f));
			if (i > 0)
			{
				num5 = Mathf.Lerp(num5, Vector2.Dot((surface[i, 0].RoomPos - surface[i - 1, 0].RoomPos).normalized, Custom.DegToVec(60f)), 0.5f);
			}
			num4 = Mathf.Pow(num4, 0.1f) * Mathf.InverseLerp(0.9f - num4 * 0.1f, 0.98f - num4 * 0.05f, num5);
			Color color = Color.Lerp(palette.waterSurfaceColor1, palette.waterShineColor, num4);
			Vector2 vector2 = surface[i, 0].defaultPos + Vector2.Lerp(surface[i, 0].lastPos, surface[i, 0].pos, timeStacker);
			if (room.Darkness(vector2) > 0f)
			{
				for (int j = 0; j < room.lightSources.Count; j++)
				{
					float num6 = Mathf.InverseLerp(vector2.y + 500f, vector2.y, room.lightSources[j].Pos.y);
					if (room.lightSources[j].Pos.y < vector2.y)
					{
						num6 *= Mathf.InverseLerp(vector2.y - room.lightSources[j].Rad * 0.7f, vector2.y, room.lightSources[j].Pos.y);
					}
					color = Custom.Screen(color, room.lightSources[j].color * room.lightSources[j].Alpha * num6 * Mathf.InverseLerp(10f + 160f * num6, 10f, Mathf.Abs(Custom.DistanceToLine(room.lightSources[j].Pos, vector2, vector2 - Custom.PerpendicularVector((v - v3).normalized) + new Vector2(0f, 1f - num6)))) * room.Darkness(vector2));
				}
				for (int k = 0; k < room.cosmeticLightSources.Count; k++)
				{
					float num7 = Mathf.InverseLerp(vector2.y + 500f, vector2.y, room.cosmeticLightSources[k].Pos.y);
					if (room.cosmeticLightSources[k].Pos.y < vector2.y)
					{
						num7 *= Mathf.InverseLerp(vector2.y - room.cosmeticLightSources[k].Rad * 0.7f, vector2.y, room.cosmeticLightSources[k].Pos.y);
					}
					color = Custom.Screen(color, room.cosmeticLightSources[k].color * room.cosmeticLightSources[k].Alpha * num7 * Mathf.InverseLerp(10f + 160f * num7, 10f, Mathf.Abs(Custom.DistanceToLine(room.cosmeticLightSources[k].Pos, vector2, vector2 - Custom.PerpendicularVector((v - v3).normalized) + new Vector2(0f, 1f - num7)))) * room.Darkness(vector2));
				}
			}
			(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors[num3] = color;
			(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors[num3 + 1] = Color.Lerp(palette.waterSurfaceColor2, palette.waterShineColor, num4 * (1f - palette.fogAmount));
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3, new Vector2(v.x + v.y * 0.3f, y));
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3 + 1, v);
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3 + 2, new Vector2(v3.x + v3.y * 0.3f, y));
			(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(num3 + 3, v3);
		}
		if (ModManager.MSC)
		{
			for (int l = (num2 - num) * 2; l < (sLeaser.sprites[0] as WaterTriangleMesh).vertices.Length; l++)
			{
				(sLeaser.sprites[0] as WaterTriangleMesh).MoveVertice(l, new Vector2(3400f, fWaterLevel - camPos.y + cosmeticSurfaceDisplace));
				(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(l, new Vector2(3400f, fWaterLevel - camPos.y + cosmeticSurfaceDisplace));
			}
		}
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(0, new Vector2(-10f, y));
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice(1, new Vector2(-10f, fWaterLevel - camPos.y + cosmeticSurfaceDisplace));
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice((sLeaser.sprites[1] as WaterTriangleMesh).vertices.Length - 2, new Vector2(ModManager.MSC ? 3400f : 1400f, fWaterLevel - camPos.y + cosmeticSurfaceDisplace));
		(sLeaser.sprites[1] as WaterTriangleMesh).MoveVertice((sLeaser.sprites[1] as WaterTriangleMesh).vertices.Length - 1, new Vector2(ModManager.MSC ? 3400f : 1400f, y));
		float t = Mathf.Lerp(camerasOutOfBreathFac[rCam.cameraNumber, 1], camerasOutOfBreathFac[rCam.cameraNumber, 0], timeStacker);
		float b = Mathf.Lerp(1f, 0.75f + 0.25f * Mathf.Sin(Mathf.Lerp(camerasOutOfBreathFac[rCam.cameraNumber, 3], camerasOutOfBreathFac[rCam.cameraNumber, 2], timeStacker) * 2f), t);
		if (rCam.followAbstractCreature != null && rCam.followAbstractCreature.realizedCreature != null && rCam.followAbstractCreature.realizedCreature.room == room && !rCam.followAbstractCreature.realizedCreature.dead)
		{
			Vector2 vector3 = Vector2.Lerp(rCam.followAbstractCreature.realizedCreature.mainBodyChunk.lastPos, rCam.followAbstractCreature.realizedCreature.mainBodyChunk.pos, timeStacker) - camPos;
			(sLeaser.sprites[1] as WaterTriangleMesh).color = new Color(Mathf.InverseLerp(0f, rCam.sSize.x, vector3.x), Mathf.InverseLerp(0f, rCam.sSize.y, vector3.y), b);
		}
		else
		{
			(sLeaser.sprites[1] as WaterTriangleMesh).color = new Color(0f, 0f, 0f);
		}
		(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors[(sLeaser.sprites[0] as WaterTriangleMesh).verticeColors.Length - 2] = palette.waterSurfaceColor1;
		if (slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		this.palette = palette;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[1]);
	}

	public void Destroy()
	{
		slatedForDeletetion = true;
	}
}
