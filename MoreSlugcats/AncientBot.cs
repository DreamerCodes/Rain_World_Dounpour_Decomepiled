using System;
using OverseerHolograms;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class AncientBot : UpdatableAndDeletable, IDrawable
{
	public class Animation : ExtEnum<Animation>
	{
		public static readonly Animation Idle = new Animation("Idle", register: true);

		public static readonly Animation Vanish = new Animation("Vanish", register: true);

		public static readonly Animation Teleport = new Animation("Teleport", register: true);

		public static readonly Animation Reappear = new Animation("Reappear", register: true);

		public static readonly Animation TurnOn = new Animation("TurnOn", register: true);

		public static readonly Animation IdleOffline = new Animation("IdleOffline", register: true);

		public Animation(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class FollowMode : ExtEnum<FollowMode>
	{
		public static readonly FollowMode MoveTowards = new FollowMode("MoveTowards", register: true);

		public static readonly FollowMode Orbit = new FollowMode("Orbit", register: true);

		public static readonly FollowMode TargetRandom = new FollowMode("TargetRandom", register: true);

		public static readonly FollowMode Teleport = new FollowMode("Teleport", register: true);

		public static readonly FollowMode None = new FollowMode("None", register: true);

		public static readonly FollowMode Offline = new FollowMode("Offline", register: true);

		public FollowMode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public Vector2? setPos;

	private Vector2 pos;

	private Vector2 lastPos;

	public float? setRad;

	public float rad;

	public float lastRad;

	public float? setAlpha;

	public float alpha;

	public float lastAlpha;

	public float affectedByPaletteDarkness;

	private Color c;

	private float colorAlpha;

	public int effectColor;

	public bool fadeWithSun;

	private bool shaderDirty;

	private bool f;

	public float waterSurfaceLevel;

	private bool environmentalLight;

	public Creature tiedToObject;

	public bool stayAlive;

	private int flickerWait;

	private int flicker;

	private float sin;

	private float sin2;

	private float sin3;

	private float[] xoffs;

	private float[] yoffs;

	private float[] baseXScales;

	private float[] baseYScales;

	private float antAngOff;

	private float gDrawYOff;

	public float gXScaleFactor;

	public float gYScaleFactor;

	private float offlineFactor;

	public float vel;

	private bool lightOn;

	private int turnOnTimer;

	private int updateTimer;

	private int movePatternTimer;

	private int movePointTimer;

	private int nextMovePatternChange;

	private int nextMovePointChange;

	private Vector2 movePointTarget;

	public Animation myAnimation;

	public FollowMode myMovement;

	public Vector2? lockTarget;

	private OverseerHologram hologram;

	public Vector2 Pos => pos;

	public float Rad => rad;

	public float Alpha
	{
		get
		{
			if (fadeWithSun)
			{
				return alpha * Mathf.Pow(Mathf.InverseLerp(-0.5f, 0f, room.world.rainCycle.ShaderLight), 1.2f);
			}
			return alpha;
		}
	}

	public float Lightness => (color.r + color.g + color.b) / 3f * colorAlpha * alpha;

	public Color color
	{
		get
		{
			return c;
		}
		set
		{
			c = value;
			colorAlpha = 0f;
			if (c.r > colorAlpha)
			{
				colorAlpha = c.r;
			}
			if (c.g > colorAlpha)
			{
				colorAlpha = c.g;
			}
			if (c.b > colorAlpha)
			{
				colorAlpha = c.b;
			}
			c /= colorAlpha;
		}
	}

	public bool flat
	{
		get
		{
			return f;
		}
		set
		{
			if (f != value)
			{
				f = value;
				shaderDirty = true;
			}
		}
	}

	public virtual string ElementName => "Futile_White";

	public virtual string LayerName => "ForegroundLights";

	public int BodyIndex => 0;

	public int LeftAntIndex => BodyIndex + 5;

	public int RightAntIndex => LeftAntIndex + 2;

	public int HeadIndex => RightAntIndex + 2;

	public int LightBaseIndex => HeadIndex + 1;

	public int LightIndex => LightBaseIndex + 1;

	public int AfterLightIndex => LightIndex + 2;

	public int TotalSprites => AfterLightIndex;

	public void HardSetRad(float r)
	{
		lastRad = r;
		rad = r;
		setRad = null;
	}

	public void HardSetAlpha(float a)
	{
		lastAlpha = a;
		alpha = a;
		setAlpha = null;
	}

	public void HardSetPos(Vector2 p)
	{
		lastPos = p;
		pos = p;
		setPos = null;
	}

	public AncientBot(Vector2 initPos, Color color, Creature tiedToObject, bool online)
	{
		affectedByPaletteDarkness = 1f;
		effectColor = -1;
		stayAlive = true;
		pos = initPos;
		sin = UnityEngine.Random.value;
		flickerWait = UnityEngine.Random.Range(0, 700);
		lastPos = initPos;
		this.color = color;
		this.tiedToObject = tiedToObject;
		affectedByPaletteDarkness = 0f;
		gXScaleFactor = 1f;
		gYScaleFactor = 1f;
		myAnimation = Animation.Idle;
		myMovement = FollowMode.MoveTowards;
		if (online)
		{
			lightOn = true;
			offlineFactor = 0f;
			gXScaleFactor = 0f;
			gYScaleFactor = 0f;
			myAnimation = Animation.Vanish;
			myMovement = FollowMode.None;
		}
		else
		{
			lightOn = false;
			offlineFactor = 1f;
			myAnimation = Animation.IdleOffline;
			myMovement = FollowMode.Offline;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		updateTimer++;
		colorAlpha = 1f;
		flickerWait--;
		sin += 1f / Mathf.Lerp(60f, 80f, UnityEngine.Random.value);
		if (lightOn)
		{
			setRad = Mathf.Lerp(290f, 310f, 0.5f + Mathf.Sin(sin * (float)Math.PI * 2f) * 0.5f) * 0.16f * gYScaleFactor;
			if (tiedToObject != null && tiedToObject.dead && updateTimer % 10 < 5)
			{
				setRad = 0f;
			}
		}
		else
		{
			setRad = 0f;
		}
		if (flickerWait < 1)
		{
			flickerWait = UnityEngine.Random.Range(0, 700);
			flicker = UnityEngine.Random.Range(1, 15);
		}
		if (flicker > 0)
		{
			flicker--;
			if (UnityEngine.Random.value < 1f / 3f)
			{
				float num = Mathf.Pow(UnityEngine.Random.value, 0.5f);
				setAlpha = num * 1f;
			}
		}
		else
		{
			setAlpha = Mathf.Lerp(0.9f, 1f, 0.5f + Mathf.Sin(sin * (float)Math.PI * 2f) * 0.5f * UnityEngine.Random.value) * 1f;
		}
		if (myMovement == FollowMode.Offline && lightOn)
		{
			turnOnTimer++;
			if (turnOnTimer > 120)
			{
				myMovement = FollowMode.MoveTowards;
			}
		}
		lastPos = pos;
		ProcessMovement();
		if (setPos.HasValue)
		{
			pos = setPos.Value;
			setPos = null;
		}
		if (pos.y < 0f)
		{
			pos.y = 0f;
		}
		lastRad = rad;
		if (setRad.HasValue)
		{
			rad = setRad.Value;
			setRad = null;
		}
		if (tiedToObject != null && (tiedToObject.slatedForDeletetion || (tiedToObject.room != room && tiedToObject.room != null)))
		{
			if (alpha == 0f)
			{
				Destroy();
			}
			else
			{
				setAlpha = 0f;
				setRad = 0f;
			}
		}
		lastAlpha = Alpha;
		if (setAlpha.HasValue)
		{
			alpha = setAlpha.Value;
			setAlpha = null;
		}
		ProcessAnimation();
		waterSurfaceLevel = room.FloatWaterLevel(pos.x);
		if (hologram != null && hologram.slatedForDeletetion)
		{
			hologram = null;
		}
		if (tiedToObject == null || !(tiedToObject is Scavenger) || !(tiedToObject as Scavenger).kingWaiting)
		{
			if (hologram != null)
			{
				hologram.stillRelevant = false;
				lockTarget = null;
			}
			return;
		}
		Player player = null;
		for (int i = 0; i < room.game.Players.Count; i++)
		{
			if (room.game.Players[i].realizedCreature != null && room.game.Players[i].Room.index == room.abstractRoom.index && Custom.Dist(tiedToObject.firstChunk.pos, room.game.Players[i].realizedCreature.firstChunk.pos) < 700f)
			{
				player = room.game.Players[i].realizedCreature as Player;
				break;
			}
		}
		if (hologram == null && player != null)
		{
			hologram = new OverseerHologram.CreaturePointer(this, OverseerHologram.Message.DangerousCreature, tiedToObject, 1f);
			room.AddObject(hologram);
		}
		if (hologram == null)
		{
			lockTarget = null;
			return;
		}
		hologram.stillRelevant = player != null;
		if (player == null)
		{
			lockTarget = null;
			return;
		}
		(hologram as OverseerHologram.CreaturePointer).pointAtCreature = player;
		if (Custom.Dist(tiedToObject.firstChunk.pos, player.firstChunk.pos) > 450f)
		{
			lockTarget = new Vector2(player.firstChunk.pos.x + (tiedToObject.firstChunk.pos.x - player.firstChunk.pos.x) * 0.65f, player.firstChunk.pos.y + (tiedToObject.firstChunk.pos.y - player.firstChunk.pos.y) * 0.65f);
		}
		else
		{
			lockTarget = null;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		xoffs = new float[TotalSprites];
		yoffs = new float[TotalSprites];
		baseXScales = new float[TotalSprites];
		baseYScales = new float[TotalSprites];
		for (int i = 0; i < baseXScales.Length; i++)
		{
			baseXScales[i] = 1f;
			baseYScales[i] = 1f;
		}
		sLeaser.sprites = new FSprite[TotalSprites];
		float num = 0.5f;
		for (int j = BodyIndex; j < LightIndex; j++)
		{
			if (j == HeadIndex || j == LightBaseIndex)
			{
				sLeaser.sprites[j] = new FSprite("Circle20");
			}
			else
			{
				sLeaser.sprites[j] = new FSprite("pixel");
			}
			sLeaser.sprites[j].anchorX = 0.5f;
			sLeaser.sprites[j].anchorY = 0.5f;
		}
		if (tiedToObject != null && tiedToObject is Scavenger)
		{
			baseXScales[BodyIndex + 4] = 10f * num;
			baseYScales[BodyIndex + 4] = 16f * num;
			baseXScales[BodyIndex] = 6f * num;
			baseYScales[BodyIndex] = 8f * num;
			for (int k = 1; k <= 3; k++)
			{
				baseXScales[BodyIndex + k] = 12f * num;
				baseYScales[BodyIndex + k] = 3f * num;
			}
			baseYScales[BodyIndex + 3] = 6f * num;
			baseXScales[HeadIndex] = 0.8f * num;
			baseYScales[HeadIndex] = 1f * num;
			baseXScales[LeftAntIndex + 1] = 8f * num;
			baseYScales[LeftAntIndex + 1] = 8f * num;
			sLeaser.sprites[LeftAntIndex + 1].anchorY = 0.9f;
			baseXScales[LeftAntIndex] = 4f * num;
			baseYScales[LeftAntIndex] = 8f * num;
			sLeaser.sprites[LeftAntIndex].anchorY = 0.9f;
			baseXScales[RightAntIndex + 1] = 8f * num;
			baseYScales[RightAntIndex + 1] = 8f * num;
			sLeaser.sprites[RightAntIndex + 1].anchorY = 0.9f;
			baseXScales[RightAntIndex] = 4f * num;
			baseYScales[RightAntIndex] = 8f * num;
			sLeaser.sprites[RightAntIndex].anchorY = 0.9f;
			sLeaser.sprites[LightBaseIndex] = new FSprite("Circle20");
			baseXScales[LightBaseIndex] = 0.5f * num;
			baseYScales[LightBaseIndex] = 0.5f * num;
		}
		else
		{
			baseXScales[BodyIndex + 4] = 10f * num;
			baseYScales[BodyIndex + 4] = 13f * num;
			baseXScales[BodyIndex] = 6f * num;
			baseYScales[BodyIndex] = 12f * num;
			for (int l = 1; l <= 3; l++)
			{
				baseXScales[BodyIndex + l] = 12f * num;
				baseYScales[BodyIndex + l] = 3f * num;
			}
			baseYScales[BodyIndex + 3] = 5f * num;
			baseXScales[HeadIndex] = 0.8f * num;
			baseYScales[HeadIndex] = 0.8f * num;
			baseXScales[LeftAntIndex + 1] = 6f * num;
			baseYScales[LeftAntIndex + 1] = 8f * num;
			sLeaser.sprites[LeftAntIndex + 1].anchorY = 0.9f;
			baseXScales[LeftAntIndex] = 2f * num;
			baseYScales[LeftAntIndex] = 12f * num;
			sLeaser.sprites[LeftAntIndex].anchorY = 0.9f;
			baseXScales[RightAntIndex + 1] = 6f * num;
			baseYScales[RightAntIndex + 1] = 8f * num;
			sLeaser.sprites[RightAntIndex + 1].anchorY = 0.9f;
			baseXScales[RightAntIndex] = 2f * num;
			baseYScales[RightAntIndex] = 12f * num;
			sLeaser.sprites[RightAntIndex].anchorY = 0.9f;
			sLeaser.sprites[LightBaseIndex] = new FSprite("Circle20");
			baseXScales[LightBaseIndex] = 0.5f * num;
			baseYScales[LightBaseIndex] = 0.5f * num;
		}
		sLeaser.sprites[LightIndex] = new FSprite(ElementName);
		sLeaser.sprites[LightIndex].shader = rCam.room.game.rainWorld.Shaders[flat ? "FlatLight" : "LightSource"];
		sLeaser.sprites[LightIndex].color = color;
		sLeaser.sprites[LightIndex + 1] = new FSprite("Futile_White");
		sLeaser.sprites[LightIndex + 1].shader = rCam.room.game.rainWorld.Shaders["UnderWaterLight"];
		sLeaser.sprites[LightIndex + 1].color = color;
		for (int m = 0; m < baseXScales.Length; m++)
		{
			sLeaser.sprites[m].scaleX = baseXScales[m];
			sLeaser.sprites[m].scaleY = baseYScales[m];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public float EffXOff(RoomCamera.SpriteLeaser sLeaser, int index, float sfactor)
	{
		return xoffs[index] * sLeaser.sprites[index].scaleX / sfactor;
	}

	public float EffYOff(RoomCamera.SpriteLeaser sLeaser, int index, float sfactor)
	{
		return yoffs[index] * sLeaser.sprites[index].scaleY / sfactor;
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = Mathf.Lerp(lastPos.x, pos.x, timeStacker);
		float num2 = Mathf.Lerp(lastPos.y, pos.y, timeStacker) + gDrawYOff;
		float num3 = Mathf.Floor(num - camPos.x);
		float num4 = Mathf.Floor(num2 - camPos.y);
		for (int i = 0; i < baseXScales.Length; i++)
		{
			sLeaser.sprites[i].scaleX = baseXScales[i] * gXScaleFactor;
			sLeaser.sprites[i].scaleY = baseYScales[i] * gYScaleFactor;
		}
		sLeaser.sprites[HeadIndex].scaleY *= 1f + 0.3f * offlineFactor;
		sLeaser.sprites[HeadIndex].scaleX *= 1f + 0.3f * offlineFactor;
		sLeaser.sprites[LightBaseIndex].scaleY *= 1f + 0.2f * offlineFactor;
		sLeaser.sprites[LightBaseIndex].scaleX *= 1f + 0.2f * offlineFactor;
		sLeaser.sprites[BodyIndex].scaleY *= (1f - offlineFactor) * 0.5f + 0.5f;
		sLeaser.sprites[BodyIndex + 1].scaleX *= 1f - offlineFactor;
		sLeaser.sprites[BodyIndex + 2].scaleX *= 1f - offlineFactor;
		sLeaser.sprites[BodyIndex + 3].scaleX *= 1f - offlineFactor;
		sLeaser.sprites[BodyIndex + 4].scaleY *= 1f - offlineFactor;
		sLeaser.sprites[LeftAntIndex].scaleY *= (1f - offlineFactor) * 0.5f + 0.5f;
		sLeaser.sprites[LeftAntIndex + 1].scaleY *= (1f - offlineFactor) * 0.5f + 0.5f;
		sLeaser.sprites[RightAntIndex].scaleY *= (1f - offlineFactor) * 0.5f + 0.5f;
		sLeaser.sprites[RightAntIndex + 1].scaleY *= (1f - offlineFactor) * 0.5f + 0.5f;
		if (!lightOn)
		{
			sLeaser.sprites[LightBaseIndex].color = new Color(0.01f, 0.01f, 0.01f);
		}
		else
		{
			sLeaser.sprites[LightBaseIndex].color = color;
			if (tiedToObject != null && tiedToObject.dead && updateTimer % 10 < 5)
			{
				sLeaser.sprites[LightBaseIndex].color = new Color(0.01f, 0.01f, 0.01f);
			}
			if (lockTarget.HasValue && !(tiedToObject is Scavenger))
			{
				sLeaser.sprites[LightBaseIndex].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
			}
		}
		sLeaser.sprites[BodyIndex + 4].x = num3 + EffXOff(sLeaser, BodyIndex + 4, 1f);
		sLeaser.sprites[BodyIndex + 4].y = num4 + EffYOff(sLeaser, BodyIndex + 4, 1f);
		sLeaser.sprites[BodyIndex].x = num3 + EffXOff(sLeaser, BodyIndex, 1f);
		sLeaser.sprites[BodyIndex].y = num4 - sLeaser.sprites[BodyIndex + 4].scaleY * 0.5f + EffYOff(sLeaser, BodyIndex, 1f);
		sLeaser.sprites[BodyIndex + 1].x = num3 - sLeaser.sprites[BodyIndex + 1].scaleX * 0.15f + EffXOff(sLeaser, BodyIndex + 1, 1f);
		sLeaser.sprites[BodyIndex + 1].y = num4 + sLeaser.sprites[BodyIndex + 1].scaleY + EffYOff(sLeaser, BodyIndex + 1, 1f);
		sLeaser.sprites[BodyIndex + 2].x = num3 - sLeaser.sprites[BodyIndex + 2].scaleX * 0.15f + EffXOff(sLeaser, BodyIndex + 2, 1f);
		sLeaser.sprites[BodyIndex + 2].y = num4 - sLeaser.sprites[BodyIndex + 2].scaleY + EffYOff(sLeaser, BodyIndex + 2, 1f);
		sLeaser.sprites[BodyIndex + 3].x = num3 + sLeaser.sprites[BodyIndex + 3].scaleX * 0.15f + EffXOff(sLeaser, BodyIndex + 3, 1f);
		sLeaser.sprites[BodyIndex + 3].y = num4 + EffYOff(sLeaser, BodyIndex + 3, 1f);
		sLeaser.sprites[HeadIndex].x = num3 + EffXOff(sLeaser, HeadIndex, 20f);
		sLeaser.sprites[HeadIndex].y = num4 + sLeaser.sprites[BodyIndex + 4].scaleY * 0.5f + sLeaser.sprites[HeadIndex].scaleY * 20f * 0.35f + EffYOff(sLeaser, HeadIndex, 20f);
		sLeaser.sprites[LightBaseIndex].x = sLeaser.sprites[HeadIndex].x + EffXOff(sLeaser, LightBaseIndex, 20f);
		sLeaser.sprites[LightBaseIndex].y = sLeaser.sprites[HeadIndex].y + EffYOff(sLeaser, LightBaseIndex, 20f);
		sLeaser.sprites[LeftAntIndex + 1].x = sLeaser.sprites[HeadIndex].x - sLeaser.sprites[HeadIndex].scaleX * 20f * 0.3f;
		sLeaser.sprites[LeftAntIndex + 1].y = sLeaser.sprites[HeadIndex].y + sLeaser.sprites[HeadIndex].scaleY * 20f * 0.3f;
		Vector2 vector = Custom.DegToVec(-30f - antAngOff) * sLeaser.sprites[LeftAntIndex + 1].scaleY * 0.8f;
		Vector2 vector2 = Custom.DegToVec(30f + antAngOff) * sLeaser.sprites[RightAntIndex + 1].scaleY * 0.8f;
		if (tiedToObject != null && tiedToObject is Scavenger)
		{
			vector = Custom.DegToVec(-90f - antAngOff) * sLeaser.sprites[LeftAntIndex + 1].scaleY * 0.8f;
			vector2 = Custom.DegToVec(90f + antAngOff) * sLeaser.sprites[RightAntIndex + 1].scaleY * 0.8f;
		}
		sLeaser.sprites[LeftAntIndex].x = sLeaser.sprites[LeftAntIndex + 1].x + vector.x;
		sLeaser.sprites[LeftAntIndex].y = sLeaser.sprites[LeftAntIndex + 1].y + vector.y;
		sLeaser.sprites[RightAntIndex + 1].x = sLeaser.sprites[HeadIndex].x + sLeaser.sprites[HeadIndex].scaleX * 20f * 0.3f;
		sLeaser.sprites[RightAntIndex + 1].y = sLeaser.sprites[HeadIndex].y + sLeaser.sprites[HeadIndex].scaleY * 20f * 0.3f;
		sLeaser.sprites[RightAntIndex].x = sLeaser.sprites[RightAntIndex + 1].x + vector2.x;
		sLeaser.sprites[RightAntIndex].y = sLeaser.sprites[RightAntIndex + 1].y + vector2.y;
		if (tiedToObject != null && tiedToObject is Scavenger)
		{
			sLeaser.sprites[LeftAntIndex + 1].rotation = 90f - antAngOff;
			sLeaser.sprites[LeftAntIndex].rotation = 90f - antAngOff;
			sLeaser.sprites[RightAntIndex + 1].rotation = 270f + antAngOff;
			sLeaser.sprites[RightAntIndex].rotation = 270f + antAngOff;
			sLeaser.sprites[RightAntIndex].y -= sLeaser.sprites[HeadIndex].scaleY * 20f * 0.4f;
			sLeaser.sprites[RightAntIndex + 1].y -= sLeaser.sprites[HeadIndex].scaleY * 20f * 0.4f;
			sLeaser.sprites[LeftAntIndex].y -= sLeaser.sprites[HeadIndex].scaleY * 20f * 0.4f;
			sLeaser.sprites[LeftAntIndex + 1].y -= sLeaser.sprites[HeadIndex].scaleY * 20f * 0.4f;
		}
		else
		{
			sLeaser.sprites[LeftAntIndex + 1].rotation = 150f - antAngOff;
			sLeaser.sprites[LeftAntIndex].rotation = 150f - antAngOff;
			sLeaser.sprites[RightAntIndex + 1].rotation = 210f + antAngOff;
			sLeaser.sprites[RightAntIndex].rotation = 210f + antAngOff;
		}
		if (rCam.room.water)
		{
			sLeaser.sprites[LightIndex + 1].isVisible = true;
		}
		else
		{
			sLeaser.sprites[LightIndex + 1].isVisible = false;
		}
		for (int j = LightIndex; j < AfterLightIndex; j++)
		{
			sLeaser.sprites[j].x = sLeaser.sprites[HeadIndex].x + 0.5f;
			sLeaser.sprites[j].color = color;
			if (j == LightIndex)
			{
				sLeaser.sprites[j].y = sLeaser.sprites[HeadIndex].y + 0.5f;
				sLeaser.sprites[j].scale = Mathf.Lerp(lastRad, rad, timeStacker) / 8f;
				sLeaser.sprites[j].alpha = Mathf.Lerp(lastAlpha, Alpha, timeStacker) * Mathf.Lerp(1f, rCam.room.Darkness(pos), affectedByPaletteDarkness) * colorAlpha;
			}
			else
			{
				float num5 = Mathf.InverseLerp(waterSurfaceLevel - rad * 0.25f, waterSurfaceLevel + rad * 0.25f, sLeaser.sprites[HeadIndex].y + camPos.y);
				float num6 = Mathf.Lerp(lastRad, rad, timeStacker) * 0.5f * Mathf.Pow(1f - num5, 0.5f);
				sLeaser.sprites[j].y = Mathf.Floor(Mathf.Min(sLeaser.sprites[HeadIndex].y + camPos.y, Mathf.Lerp(sLeaser.sprites[HeadIndex].y + camPos.y, waterSurfaceLevel - num6 * 0.5f, 0.5f)) - camPos.y) + 0.5f;
				sLeaser.sprites[j].scale = num6 / 8f;
				sLeaser.sprites[j].alpha = Mathf.Lerp(lastAlpha, Alpha, timeStacker) * Mathf.Lerp(1f, rCam.room.Darkness(pos), affectedByPaletteDarkness) * Mathf.Pow(1f - num5, 0.5f) * colorAlpha;
			}
		}
		if (shaderDirty)
		{
			sLeaser.sprites[LightIndex].shader = rCam.room.game.rainWorld.Shaders[flat ? "FlatLight" : "LightSource"];
			shaderDirty = false;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ProcessMovement()
	{
		Vector2 vector = pos;
		if (tiedToObject != null)
		{
			vector = tiedToObject.mainBodyChunk.pos;
		}
		if (lockTarget.HasValue)
		{
			vector = (movePointTarget = lockTarget.Value);
			myMovement = FollowMode.TargetRandom;
		}
		if (myMovement == FollowMode.Offline)
		{
			vel = 0f;
			return;
		}
		int num = 75;
		int num2 = 25;
		float num3 = Custom.Dist(pos, vector);
		if (num3 > (float)num)
		{
			myMovement = FollowMode.MoveTowards;
		}
		else if (myMovement == FollowMode.MoveTowards)
		{
			myMovement = FollowMode.None;
		}
		if (myMovement != FollowMode.MoveTowards)
		{
			movePointTimer++;
			movePatternTimer++;
			if (movePatternTimer >= nextMovePatternChange)
			{
				if ((double)UnityEngine.Random.value <= 0.5)
				{
					myMovement = FollowMode.Teleport;
				}
				else
				{
					myMovement = FollowMode.TargetRandom;
				}
				movePatternTimer = 0;
				nextMovePatternChange = (int)Mathf.Lerp(300f, 1200f, UnityEngine.Random.value);
			}
		}
		if (myMovement == FollowMode.MoveTowards)
		{
			float num4 = (num3 - (float)num) / 20f + 1f;
			vel = num4;
			setPos = pos + Custom.DirVec(pos, vector) * vel;
			movePatternTimer = nextMovePatternChange - (int)Mathf.Lerp(30f, 120f, UnityEngine.Random.value);
			nextMovePointChange = 0;
		}
		else if (myMovement == FollowMode.None || myMovement == FollowMode.Offline)
		{
			float num5 = 0f;
			vel += (num5 - vel) * 0.05f;
			setPos = pos + Custom.DirVec(pos, vector) * vel;
			if (myMovement != FollowMode.Offline)
			{
				movePointTimer = 0;
			}
			nextMovePointChange = 0;
		}
		else if (myMovement == FollowMode.TargetRandom || myMovement == FollowMode.Teleport)
		{
			if (movePointTimer <= nextMovePointChange)
			{
				_ = movePointTarget;
			}
			else
			{
				do
				{
					if (tiedToObject != null && tiedToObject is Scavenger && (tiedToObject as Scavenger).kingWaiting)
					{
						movePointTarget = new Vector2(vector.x + Mathf.Lerp(0f - (float)num, num, UnityEngine.Random.value), vector.y + Mathf.Lerp((float)num * 0.5f, num, UnityEngine.Random.value));
					}
					else
					{
						movePointTarget = new Vector2(vector.x + Mathf.Lerp(0f - (float)num, num, UnityEngine.Random.value), vector.y + Mathf.Lerp(0f - (float)num, num, UnityEngine.Random.value));
					}
				}
				while (Custom.Dist(vector, movePointTarget) < (float)num && Custom.Dist(vector, movePointTarget) > (float)num2);
				movePointTimer = 0;
				nextMovePointChange = (int)Mathf.Lerp(30f, 300f, UnityEngine.Random.value);
			}
			float num6 = (vel = Custom.Dist(pos, movePointTarget) / 20f);
			if (myMovement != FollowMode.Teleport || gYScaleFactor < 0.001f)
			{
				setPos = pos + Custom.DirVec(pos, movePointTarget) * vel;
			}
			if (myMovement == FollowMode.Teleport)
			{
				if (num6 > 0.2f)
				{
					myAnimation = Animation.Teleport;
				}
				else if (myAnimation == Animation.Teleport)
				{
					myAnimation = Animation.Reappear;
				}
			}
			if (myAnimation == Animation.Teleport)
			{
				movePointTimer = 0;
			}
		}
		if (myMovement != FollowMode.Teleport && myAnimation == Animation.Teleport)
		{
			myAnimation = Animation.Reappear;
		}
	}

	public void ProcessAnimation()
	{
		if (tiedToObject != null)
		{
			if (tiedToObject.room == null)
			{
				myAnimation = Animation.Vanish;
			}
			else if (myAnimation == Animation.Vanish)
			{
				myAnimation = Animation.Reappear;
			}
		}
		if (myAnimation == Animation.IdleOffline)
		{
			sin3 += 0.05f;
			gDrawYOff = 4f * Mathf.Sin(sin3);
		}
		else if (myAnimation == Animation.TurnOn)
		{
			if (!lightOn && turnOnTimer < 60)
			{
				turnOnTimer++;
				if (turnOnTimer == 1)
				{
					room.PlaySound(SoundID.Zapper_Zap, 0f, 1f, 3f);
					room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_On, 0f, 1f, 3f);
					room.PlaySound(SoundID.Broken_Anti_Gravity_Switch_On, 0f, 1f, 2.5f);
				}
				setPos = new Vector2(pos.x, pos.y + 0.5f);
			}
			else
			{
				offlineFactor = Mathf.Lerp(offlineFactor, 0f, 0.075f);
			}
			if (offlineFactor < 0.01f && !lightOn)
			{
				offlineFactor = 0f;
				myAnimation = Animation.Idle;
				lightOn = true;
				room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, 0f, 1f, 1.5f);
				room.AddObject(new ShockWave(pos, 100f, 0.07f, 6));
				for (int i = 0; i < 10; i++)
				{
					room.AddObject(new WaterDrip(pos, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(4f, 21f, UnityEngine.Random.value), waterColor: false));
				}
			}
		}
		else if (myAnimation == Animation.Vanish || myAnimation == Animation.Teleport)
		{
			gXScaleFactor = Mathf.Lerp(gXScaleFactor, 0f, 0.25f);
			if ((double)gXScaleFactor < 0.25)
			{
				gYScaleFactor = Mathf.Lerp(gYScaleFactor, 0f, 0.5f);
			}
		}
		else if (myAnimation == Animation.Reappear)
		{
			gYScaleFactor = Mathf.Lerp(gYScaleFactor, 1f, 0.25f);
			if ((double)gYScaleFactor < 0.5)
			{
				gXScaleFactor = Mathf.Lerp(gXScaleFactor, 1f, 0.01f);
			}
			else
			{
				gXScaleFactor = Mathf.Lerp(gXScaleFactor, 1f, 0.1f);
			}
			if (gXScaleFactor > 0.95f && gYScaleFactor > 0.95f)
			{
				myAnimation = Animation.Idle;
			}
		}
		if (!(myAnimation == Animation.Idle))
		{
			return;
		}
		gXScaleFactor = 1f;
		gYScaleFactor = 1f;
		sin2 += 1f / Mathf.Lerp(1f, 30f, UnityEngine.Random.value);
		if (xoffs != null && yoffs != null)
		{
			if (Mathf.Sin(sin2) > 0f)
			{
				xoffs[BodyIndex + 1] = 0.4f * Mathf.Sin(sin2);
				xoffs[BodyIndex + 2] = 0.4f * Mathf.Sin(sin2);
				xoffs[BodyIndex + 3] = -0.4f * Mathf.Sin(sin2);
			}
			else
			{
				xoffs[BodyIndex + 1] = 0.05f * Mathf.Sin(sin2);
				xoffs[BodyIndex + 2] = 0.05f * Mathf.Sin(sin2);
				xoffs[BodyIndex + 3] = -0.05f * Mathf.Sin(sin2);
			}
			yoffs[BodyIndex] = 0f - Mathf.Abs(0.2f * Mathf.Sin(sin2 + (float)Math.PI / 2f));
		}
		antAngOff = 5f * Mathf.Cos(sin2);
		sin3 += 0.05f;
		gDrawYOff = 4f * Mathf.Sin(sin3);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (effectColor >= 0)
		{
			color = palette.texture.GetPixel(30, 5 - effectColor * 2);
		}
		sLeaser.sprites[LightBaseIndex].color = color;
		if (tiedToObject != null && tiedToObject is Scavenger)
		{
			for (int i = BodyIndex; i < LightBaseIndex; i++)
			{
				if (i == LeftAntIndex + 1 || i == RightAntIndex + 1)
				{
					sLeaser.sprites[i].color = new Color(0.845f, 0.1765f, 0.07f);
				}
				else if (i == BodyIndex)
				{
					sLeaser.sprites[i].color = new Color(0.845f, 0.1765f, 0.07f);
				}
				else
				{
					sLeaser.sprites[i].color = new Color(0.28f, 0.053f, 0.12f);
				}
			}
			return;
		}
		for (int j = BodyIndex; j < LightBaseIndex; j++)
		{
			if (j == LeftAntIndex + 1 || j == RightAntIndex + 1)
			{
				sLeaser.sprites[j].color = new Color(1f, 0.6549f, 0.2863f);
			}
			else if (j == BodyIndex)
			{
				sLeaser.sprites[j].color = new Color(0.945f, 0.3765f, 0f);
			}
			else
			{
				sLeaser.sprites[j].color = new Color(1f, 0.8902f, 0.451f);
			}
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		for (int i = BodyIndex; i < LightIndex; i++)
		{
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[i]);
		}
		rCam.ReturnFContainer(LayerName).AddChild(sLeaser.sprites[LightIndex]);
		if (AfterLightIndex - LightIndex > 1)
		{
			rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[LightIndex + 1]);
		}
	}
}
