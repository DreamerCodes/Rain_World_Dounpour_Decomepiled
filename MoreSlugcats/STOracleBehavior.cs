using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class STOracleBehavior : OracleBehavior
{
	public class Phase : ExtEnum<Phase>
	{
		public static readonly Phase Inactive = new Phase("Inactive", register: true);

		public static readonly Phase Activate = new Phase("Activate", register: true);

		public static readonly Phase PhaseDirectedLaser = new Phase("PhaseDirectedLaser", register: true);

		public static readonly Phase PhaseLaserFan = new Phase("PhaseLaserFan", register: true);

		public static readonly Phase PhaseKarmicGrid = new Phase("PhaseKarmicGrid", register: true);

		public static readonly Phase PhaseTalk = new Phase("PhaseTalk", register: true);

		public static readonly Phase PhaseDirectedKGrid = new Phase("PhaseDirectedKGrid", register: true);

		public static readonly Phase Ending = new Phase("Ending", register: true);

		public static readonly Phase PhaseCombo = new Phase("PhaseCombo", register: true);

		public static readonly Phase PhaseFinalCombo = new Phase("PhaseFinalCombo", register: true);

		public static readonly Phase PhaseCircleDanmaku = new Phase("PhaseCircleDanmaku", register: true);

		public static readonly Phase PhaseGravitron = new Phase("PhaseGravitron", register: true);

		public Phase(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class ExtraGraphics : UpdatableAndDeletable, IDrawable
	{
		public float disruptAlpha;

		public STOracleBehavior parent;

		public ExtraGraphics(STOracleBehavior parent)
		{
			this.parent = parent;
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["GravityDisruptor"];
			sLeaser.sprites[0].scale = 15f;
			sLeaser.sprites[0].alpha = disruptAlpha;
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = parent.oracle.firstChunk.pos - camPos;
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].alpha = disruptAlpha;
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);
		}
	}

	public class Laser : UpdatableAndDeletable, IDrawable
	{
		public class Type : ExtEnum<Type>
		{
			public static readonly Type EXPLODE = new Type("EXPLODE", register: true);

			public static readonly Type LIGHTNING = new Type("LIGHTNING", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public STOracleBehavior parent;

		public int life;

		public int lifeTime;

		public Vector2 startPos;

		public float dir;

		public int postLife;

		public Type type;

		public float width;

		public bool hideGuides;

		public float dirTick;

		public override void Update(bool eu)
		{
			base.Update(eu);
			life--;
			dir += dirTick;
			if (life > 0)
			{
				return;
			}
			life = 0;
			postLife++;
			if ((postLife < 5 && type == Type.EXPLODE) || type == Type.LIGHTNING)
			{
				Vector2 normalized = Custom.PerpendicularVector(Custom.DegToVec(dir)).normalized;
				Vector2 r = startPos + normalized * width * 1.25f;
				Vector2 r2 = startPos - normalized * width * 1.25f;
				Vector2 r3 = startPos + Custom.DegToVec(dir) * 400f - normalized * width * 1.25f;
				Vector2 r4 = startPos + Custom.DegToVec(dir) * 400f + normalized * width * 1.25f;
				if (type == Type.EXPLODE && (Custom.PointInPoly4(parent.player.firstChunk.pos, r, r2, r3, r4) || Custom.Dist(parent.player.firstChunk.pos, parent.oracle.firstChunk.pos) < 64f))
				{
					parent.player.firstChunk.vel = Custom.DegToVec(dir) * 16f;
				}
				if (type == Type.LIGHTNING && Custom.PointInPoly4(parent.player.firstChunk.pos, r, r2, r3, r4))
				{
					parent.player.firstChunk.vel = Custom.DegToVec(dir) * 16f;
					if (postLife >= 5)
					{
						parent.player.Die();
					}
				}
			}
			if (postLife == 5)
			{
				Impact();
			}
			if (postLife > 10)
			{
				base.slatedForDeletetion = true;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			for (int i = 0; i < 3; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].rotation = dir;
				sLeaser.sprites[i].anchorY = 0f;
				sLeaser.sprites[i].scaleX = 0f;
				sLeaser.sprites[i].scaleY = 600f / sLeaser.sprites[i].height;
				if (i > 0)
				{
					sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["Hologram"];
				}
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = startPos - camPos;
			float perc = (float)life / (float)lifeTime;
			float num = Custom.LerpBackEaseOut(0f, (!hideGuides) ? (width * 1.5f) : 0f, perc);
			float num2 = Custom.LerpBackEaseOut(width * 0.75f, 0f, perc);
			if (postLife > 0)
			{
				num2 = Custom.LerpElasticEaseOut(width * 0.75f, 0f, (float)postLife / 10f);
			}
			float num3 = Custom.LerpBackEaseOut(2f, 0f, perc);
			Vector2 normalized = Custom.PerpendicularVector(Custom.DegToVec(dir)).normalized;
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			sLeaser.sprites[0].scaleX = num2 / sLeaser.sprites[0].element.sourceRect.width;
			sLeaser.sprites[1].x = Mathf.Floor(vector.x + normalized.x * num);
			sLeaser.sprites[1].y = vector.y + normalized.y * num;
			sLeaser.sprites[1].scaleX = num3 / sLeaser.sprites[1].element.sourceRect.width;
			sLeaser.sprites[2].x = Mathf.Floor(vector.x - normalized.x * num);
			sLeaser.sprites[2].y = vector.y - normalized.y * num;
			sLeaser.sprites[2].scaleX = num3 / sLeaser.sprites[2].element.sourceRect.width;
			sLeaser.sprites[0].rotation = dir;
			sLeaser.sprites[1].rotation = dir;
			sLeaser.sprites[2].rotation = dir;
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[0]);
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[1]);
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[2]);
		}

		public void Impact()
		{
			Vector2 vector = startPos + Custom.DegToVec(dir) * 600f;
			Vector2 vector2 = Custom.DegToVec(Custom.AimFromOneVectorToAnother(startPos, vector));
			IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, startPos, vector);
			if (!intVector.HasValue)
			{
				return;
			}
			Vector2 vector3 = room.MiddleOfTile(intVector.Value) - vector2 * 7f;
			if (type == Type.EXPLODE)
			{
				room.AddObject(new Explosion(room, parent.oracle, vector3, 7, 200f, 6.2f, 2f, 280f, 0f, null, 0.3f, 160f, 1f));
				room.AddObject(new Explosion.ExplosionLight(vector3, 280f, 1f, 7, Color.white));
				room.AddObject(new Explosion.ExplosionLight(vector3, 230f, 1f, 3, new Color(1f, 1f, 1f)));
				room.AddObject(new ShockWave(vector3, 330f, 0.045f, 5));
				for (int i = 0; i < 5; i++)
				{
					Vector2 vector4 = Custom.RNV();
					if (room.GetTile(vector3 + vector4 * 20f).Solid)
					{
						if (!room.GetTile(vector3 - vector4 * 20f).Solid)
						{
							vector4 *= -1f;
						}
						else
						{
							vector4 = Custom.RNV();
						}
					}
					room.AddObject(new Explosion.FlashingSmoke(vector3 + vector4 * 40f * UnityEngine.Random.value, vector4 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), Color.white, UnityEngine.Random.Range(3, 11)));
				}
				room.PlaySound(SoundID.Bomb_Explode, vector3, 0.8f, 0.75f + UnityEngine.Random.value * 0.5f);
			}
			else if (type == Type.LIGHTNING)
			{
				LightningBolt lightningBolt = new LightningBolt(startPos, vector3, 0, width / 60f, 2f, 0f, 0.2f, light: false);
				lightningBolt.intensity = 1f;
				room.AddObject(lightningBolt);
				room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, vector3, 0.4f, 1.4f - UnityEngine.Random.value * 0.4f);
				room.AddObject(new LightningMachine.Impact(vector3, 16f, Color.white, circle: true));
			}
		}

		public Laser(STOracleBehavior parent, Vector2 startPos, float dir, float width, int lifeTime, Type type)
		{
			this.parent = parent;
			life = lifeTime;
			this.lifeTime = lifeTime;
			this.startPos = startPos;
			this.dir = dir;
			this.type = type;
			this.width = width;
		}
	}

	public class KarmicGrid : UpdatableAndDeletable, IDrawable
	{
		public int numIntersects;

		public int lifeTime;

		public int life;

		public Vector2[] intersects;

		public STOracleBehavior parent;

		public KarmicGrid(STOracleBehavior parent, int numIntersects, int lifeTime)
		{
			this.parent = parent;
			this.numIntersects = numIntersects;
			this.lifeTime = lifeTime;
			life = lifeTime;
			intersects = new Vector2[numIntersects];
			bool flag = false;
			while (!flag)
			{
				for (int i = 0; i < numIntersects; i++)
				{
					int num = UnityEngine.Random.Range(0, this.parent.gridPositions.GetLength(0));
					int num2 = UnityEngine.Random.Range(0, this.parent.gridPositions.GetLength(1));
					intersects[i] = new Vector2(num, num2);
				}
				int num3 = 0;
				for (int j = 0; j < numIntersects; j++)
				{
					for (int k = 0; k < numIntersects; k++)
					{
						if (j != k)
						{
							if (Mathf.Abs(intersects[j].x - intersects[k].x) < 7f)
							{
								num3 += 2;
								break;
							}
							if (Mathf.Abs(intersects[j].y - intersects[k].y) < 7f)
							{
								num3 += 2;
								break;
							}
						}
					}
				}
				if ((float)num3 <= Mathf.Pow(numIntersects, 2f) / 2f)
				{
					flag = true;
				}
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			life--;
			float diameter = 250f;
			if (life == 10)
			{
				for (int i = 0; i < intersects.Length; i++)
				{
					for (int j = 0; j < intersects.Length; j++)
					{
						parent.oracle.room.AddObject(new KarmicBomb(parent, new Vector2(parent.gridPositions[(int)intersects[i].x, (int)intersects[i].y].x, parent.gridPositions[(int)intersects[j].x, (int)intersects[j].y].y), diameter, 20, i == 0 && j == 0));
					}
				}
			}
			if ((float)life <= 0f)
			{
				base.slatedForDeletetion = true;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[numIntersects * 2];
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i] = new FSprite("Futile_White");
				sLeaser.sprites[i].scaleX = 1000f / sLeaser.sprites[i].height;
				sLeaser.sprites[i].scaleY = 1f / sLeaser.sprites[i].height;
				sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["Hologram"];
				if (i % 2 == 0)
				{
					sLeaser.sprites[i].rotation = 90f;
				}
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				if (i % 2 == 0)
				{
					sLeaser.sprites[i].x = parent.gridPositions[(int)intersects[i / 2].x, (int)intersects[i / 2].y].x - camPos.x;
					sLeaser.sprites[i].y = parent.midPoint.y - camPos.y;
				}
				else
				{
					sLeaser.sprites[i].x = parent.midPoint.x - camPos.x;
					sLeaser.sprites[i].y = parent.gridPositions[(int)intersects[i / 2].x, (int)intersects[i / 2].y].y - camPos.y;
				}
				sLeaser.sprites[i].color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
				sLeaser.sprites[i].alpha = 0.8f;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[i]);
			}
		}
	}

	public class KarmicBomb : UpdatableAndDeletable
	{
		public Vector2 pos;

		public bool wasPlayerAlreadyDead;

		public KarmaVectorX karmaSymbol;

		public STOracleBehavior parent;

		public int life;

		public int lifeTime;

		public float diameter;

		public bool simulMain;

		public KarmicBomb(STOracleBehavior parent, Vector2 position, float diameter, int lifeTime, bool simulMain)
		{
			pos = position;
			this.parent = parent;
			life = lifeTime;
			this.lifeTime = lifeTime;
			this.simulMain = simulMain;
			this.diameter = diameter;
			karmaSymbol = new KarmaVectorX(pos, diameter * 1.25f, 5f, 0f);
			this.parent.oracle.room.AddObject(karmaSymbol);
			wasPlayerAlreadyDead = this.parent.player.dead;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			float perc = (float)life / (float)lifeTime;
			karmaSymbol.alpha = Custom.LerpSinEaseInOut(1f, 0f, perc);
			karmaSymbol.diameter = Custom.LerpExpEaseInOut(diameter, diameter * 1.25f, perc);
			life--;
			if (life != -1 || base.slatedForDeletetion)
			{
				return;
			}
			base.slatedForDeletetion = true;
			karmaSymbol.slatedForDeletetion = true;
			for (int i = 0; i < 5; i++)
			{
				room.AddObject(new Spark(pos, Custom.RNV() * UnityEngine.Random.value * 40f, new Color(1f, 1f, 1f), null, 30, 120));
			}
			if (Custom.Dist(parent.player.firstChunk.pos, pos) <= diameter / 2f)
			{
				parent.player.Die();
				BodyChunk[] bodyChunks = parent.player.bodyChunks;
				for (int j = 0; j < bodyChunks.Length; j++)
				{
					bodyChunks[j].vel += Custom.RNV() * 36f;
				}
			}
			if (simulMain && !wasPlayerAlreadyDead)
			{
				if (parent.player.dead)
				{
					room.PlaySound(SoundID.Firecracker_Bang, pos, 0.5f, 0.75f + UnityEngine.Random.value);
					room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, pos, 0.5f, 0.5f + UnityEngine.Random.value * 0.5f);
				}
				else
				{
					room.PlaySound(SoundID.Snail_Pop, pos, 0.75f, 1.5f + UnityEngine.Random.value);
				}
			}
		}
	}

	public class SimpleDan : UpdatableAndDeletable, IDrawable
	{
		public class DestroyType : ExtEnum<DestroyType>
		{
			public static readonly DestroyType BORDER = new DestroyType("BORDER", register: true);

			public static readonly DestroyType MID_X = new DestroyType("MID_X", register: true);

			public DestroyType(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public float speed;

		public float radius;

		public float dir;

		public Vector2 pos;

		public STOracleBehavior parent;

		public float dirTicker;

		public float life;

		public DestroyType destroyType;

		public bool active;

		public bool lastActive;

		public SimpleDan(STOracleBehavior parent, Vector2 pos, float dir, float speed)
		{
			active = true;
			this.parent = parent;
			this.pos = pos;
			this.dir = dir;
			this.speed = speed;
			destroyType = DestroyType.BORDER;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastActive = active;
			life += 1f;
			dir += dirTicker;
			dir %= 360f;
			Vector2 vector = Custom.DegToVec(dir);
			pos += vector * Custom.LerpExpEaseOut(0f, speed, Mathf.Clamp(life / 80f, 0f, 1f));
			if (destroyType == DestroyType.BORDER && (pos.x < parent.boxBounds.xMin || pos.x > parent.boxBounds.xMax || pos.y > parent.boxBounds.yMax || pos.y < parent.boxBounds.yMin))
			{
				base.slatedForDeletetion = true;
			}
			if (destroyType == DestroyType.MID_X && ((pos.x <= parent.midPoint.x && vector.x <= 0f) || (pos.x > parent.midPoint.x && vector.x >= 0f)))
			{
				base.slatedForDeletetion = true;
			}
			if (Custom.Dist(parent.player.firstChunk.pos, pos) <= radius * 2f && active)
			{
				base.slatedForDeletetion = true;
				parent.player.Die();
				parent.player.firstChunk.vel = Custom.DegToVec(dir) * speed * 2f;
				room.PlaySound(SoundID.Death_Lightning_Spark_Spontaneous, parent.player.firstChunk.pos, 0.4f, 1.4f - UnityEngine.Random.value * 0.4f);
				room.AddObject(new LightningMachine.Impact(parent.player.firstChunk.pos, 16f, Color.white, circle: true));
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			if (active)
			{
				sLeaser.sprites[0] = new FSprite("CorruptGrad");
			}
			else
			{
				sLeaser.sprites[0] = new FSprite("LizardBubble7");
			}
			radius = sLeaser.sprites[0].width * 0.5f;
			AddToContainer(sLeaser, rCam, null);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = pos - camPos;
			sLeaser.sprites[0].x = vector.x;
			sLeaser.sprites[0].y = vector.y;
			if (active != lastActive)
			{
				if (active)
				{
					sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("CorruptGrad");
				}
				else
				{
					sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("LizardBubble6");
				}
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[0]);
		}
	}

	public class SineDan : SimpleDan
	{
		public float sin_rate;

		public float sin_mag;

		public float sin_off;

		public override void Update(bool eu)
		{
			base.Update(eu);
			pos += new Vector2(0f, Mathf.Sin((float)Math.PI * 2f / sin_rate * life + sin_off) * sin_mag);
		}

		public SineDan(STOracleBehavior parent, Vector2 pos, float dir, float speed, float sin_rate, float sin_mag, float sin_off)
			: base(parent, pos, dir, speed)
		{
			base.parent = parent;
			base.pos = pos;
			base.dir = dir;
			base.speed = speed;
			this.sin_rate = sin_rate;
			this.sin_mag = sin_mag;
			this.sin_off = sin_off;
		}
	}

	private Vector2 lastPos;

	private Vector2 nextPos;

	private Vector2 lastPosHandle;

	private Vector2 nextPosHandle;

	private float pathProgression;

	private Vector2 currentGetTo;

	public Phase curPhase;

	public int activateTimer;

	public LightningMachine activateLightning;

	public ExtraGraphics extraGraphics;

	public bool protection;

	public float dirTicker;

	public float dirTickAmount;

	public float accDirTicks;

	public Vector2[,] gridPositions;

	public Vector2 midPoint;

	public Laser activeDirectedLaser;

	public EnergySwirl energySwirl;

	public Rect boxBounds;

	public LightSource hintLight;

	public bool lastProtection;

	public List<Laser> gravitronLasers;

	public int activateBuffer;

	public float floatActivateTimer;

	public int activateIntroTimer;

	public Vector2 targetVector;

	public override Vector2 OracleGetToPos => ClampVectorInRoom(currentGetTo);

	public override Vector2 GetToDir
	{
		get
		{
			if (pathProgression > 0.8f)
			{
				return new Vector2(0f, 1f);
			}
			return Custom.DirVec(oracle.firstChunk.pos, OracleGetToPos);
		}
	}

	public override bool EyesClosed
	{
		get
		{
			if (!(oracle.health <= 0f) && (!(curPhase != Phase.Inactive) || !(curPhase != Phase.Activate) || protection))
			{
				return curPhase == Phase.PhaseKarmicGrid;
			}
			return true;
		}
	}

	public STOracleBehavior(Oracle oracle)
		: base(oracle)
	{
		base.oracle.health = 5f;
		currentGetTo = oracle.firstChunk.pos;
		lastPos = oracle.firstChunk.pos;
		nextPos = oracle.firstChunk.pos;
		pathProgression = 1f;
		curPhase = Phase.Inactive;
		if (curPhase != Phase.Inactive && curPhase != Phase.Activate)
		{
			extraGraphics = new ExtraGraphics(this);
			base.oracle.room.AddObject(extraGraphics);
			protection = true;
			lastProtection = true;
		}
		gridPositions = new Vector2[13, 13];
		boxBounds = new Rect(180f, 40f, 620f, 620f);
		for (int i = 0; i < gridPositions.GetLength(0); i++)
		{
			for (int j = 0; j < gridPositions.GetLength(1); j++)
			{
				gridPositions[i, j] = new Vector2(240f + (float)i * 40f, 590f - (float)j * 40f);
			}
		}
		midPoint = new Vector2(490f, 370f);
		gravitronLasers = new List<Laser>();
		SetNewDestination(midPoint);
	}

	public override void Update(bool eu)
	{
		if (protection != lastProtection)
		{
			if (protection)
			{
				oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_On, oracle.firstChunk.pos, 1f, 1f);
			}
			else
			{
				oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.Core_Off, oracle.firstChunk.pos, 1f, 1f);
			}
		}
		lastProtection = protection;
		base.Update(eu);
		if (!oracle.Consious)
		{
			return;
		}
		if (activeDirectedLaser != null && activeDirectedLaser.slatedForDeletetion)
		{
			activeDirectedLaser = null;
		}
		lookPoint = player.firstChunk.pos;
		pathProgression = Mathf.Min(1f, pathProgression + 1f / Mathf.Lerp(40f + pathProgression * 80f, Vector2.Distance(lastPos, nextPos) / 5f, 0.5f));
		currentGetTo = Custom.Bezier(lastPos, ClampVectorInRoom(lastPos + lastPosHandle), nextPos, ClampVectorInRoom(nextPos + nextPosHandle), pathProgression);
		Move();
		if (curPhase == Phase.Activate || curPhase == Phase.Ending)
		{
			activateTimer++;
			if (activateTimer == 1)
			{
				activateLightning = new LightningMachine(oracle.firstChunk.pos, new Vector2(oracle.firstChunk.pos.x, oracle.firstChunk.pos.y), new Vector2(oracle.firstChunk.pos.x, oracle.firstChunk.pos.y + 10f), 0f, permanent: false, radial: true, 0.3f, 1f, 1f);
				activateLightning.volume = 0.8f;
				activateLightning.impactType = 3;
				oracle.room.AddObject(activateLightning);
				if (extraGraphics == null)
				{
					extraGraphics = new ExtraGraphics(this);
					oracle.room.AddObject(extraGraphics);
				}
			}
			activateLightning.pos = oracle.firstChunk.pos;
			if (curPhase == Phase.Activate)
			{
				float num = (float)activateTimer / 150f;
				activateLightning.startPoint = new Vector2(Mathf.Lerp(oracle.firstChunk.pos.x, 150f, num * 2f - 2f), oracle.firstChunk.pos.y);
				activateLightning.endPoint = new Vector2(Mathf.Lerp(oracle.firstChunk.pos.x, 150f, num * 2f - 2f), oracle.firstChunk.pos.y + 10f);
				activateLightning.chance = Mathf.Lerp(0f, 0.7f, num);
				extraGraphics.disruptAlpha = Mathf.Lerp(0f, 0.5f, num);
				oracle.room.game.cameras[0].ScreenMovement(null, Vector2.zero, Mathf.Lerp(0f, 2f, num));
				if (activateTimer >= 200)
				{
					curPhase = Phase.PhaseDirectedLaser;
					protection = true;
					activateLightning.Destroy();
					activateTimer = 0;
				}
			}
			else
			{
				protection = false;
				activateLightning.startPoint = new Vector2(150f, oracle.firstChunk.pos.y);
				activateLightning.endPoint = new Vector2(150f, oracle.firstChunk.pos.y + 10f);
				activateLightning.chance = 0.7f;
				oracle.room.game.cameras[0].ScreenMovement(null, Vector2.zero, 2f);
				if (activateTimer % 180 == 0)
				{
					oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.ST_Talk2, oracle.firstChunk.pos, 1f, UnityEngine.Random.value * 0.25f + 0.65f);
				}
			}
		}
		if (curPhase == Phase.PhaseDirectedLaser)
		{
			if (activateTimer % 60 == 0 && protection)
			{
				activeDirectedLaser = new Laser(this, oracle.firstChunk.pos, Custom.VecToDeg(Custom.DirVec(oracle.firstChunk.pos, player.firstChunk.pos)), 12f, 35, Laser.Type.EXPLODE);
				oracle.room.AddObject(activeDirectedLaser);
			}
			if (activateTimer % 840 == 600)
			{
				protection = false;
			}
			if (activateTimer % 840 == 839)
			{
				protection = true;
				player.godTimer = player.maxGodTime;
			}
			activateTimer++;
		}
		if (curPhase == Phase.PhaseGravitron)
		{
			if (activateTimer == 0)
			{
				SetNewDestination(midPoint + new Vector2(0f, 200f));
			}
			float num2 = 2f;
			float sin_rate = 160f;
			float num3 = 800f;
			float num4 = 1200f;
			if (activateTimer % 15 == 0)
			{
				for (int i = 0; i < 5; i++)
				{
					float sin_mag = 0f;
					float sin_off = (float)activateTimer * ((float)Math.PI / 120f);
					float num5 = 0f;
					if ((float)activateTimer > num3)
					{
						sin_mag = Mathf.Lerp(0f, 2f, ((float)activateTimer - num3) / num4);
						num5 = Mathf.Cos((float)activateTimer * ((float)Math.PI / 240f)) * Mathf.Lerp(0f, 120f, ((float)activateTimer - num3) / num4);
					}
					SineDan sineDan = new SineDan(this, new Vector2(boxBounds.xMin + 32f, boxBounds.yMax - 32f - (float)i * 32f + num5), 90f, num2, sin_rate, sin_mag, sin_off);
					SineDan sineDan2 = new SineDan(this, new Vector2(boxBounds.xMin + 32f, boxBounds.yMin + 32f + (float)i * 32f + num5), 90f, num2, sin_rate, sin_mag, sin_off);
					SineDan sineDan3 = new SineDan(this, new Vector2(boxBounds.xMax - 32f, boxBounds.yMax - 32f - (float)i * 32f + num5), 270f, num2, sin_rate, sin_mag, sin_off);
					SineDan sineDan4 = new SineDan(this, new Vector2(boxBounds.xMax - 32f, boxBounds.yMin + 32f + (float)i * 32f + num5), 270f, num2, sin_rate, sin_mag, sin_off);
					sineDan.destroyType = SimpleDan.DestroyType.MID_X;
					sineDan2.destroyType = SimpleDan.DestroyType.MID_X;
					sineDan3.destroyType = SimpleDan.DestroyType.MID_X;
					sineDan4.destroyType = SimpleDan.DestroyType.MID_X;
					oracle.room.AddObject(sineDan);
					oracle.room.AddObject(sineDan2);
					oracle.room.AddObject(sineDan3);
					oracle.room.AddObject(sineDan4);
				}
			}
			if (activateTimer > 80 && activateTimer % 60 == 0)
			{
				Laser laser = ((!(UnityEngine.Random.value < 0.5f)) ? new Laser(this, new Vector2(boxBounds.xMax, boxBounds.yMin + 32f), 0f, 18f, UnityEngine.Random.Range(30, 175), Laser.Type.LIGHTNING) : new Laser(this, new Vector2(boxBounds.xMin, boxBounds.yMin + 32f), 0f, 18f, UnityEngine.Random.Range(30, 175), Laser.Type.LIGHTNING));
				gravitronLasers.Add(laser);
				oracle.room.AddObject(laser);
			}
			for (int j = 0; j < gravitronLasers.Count; j++)
			{
				if (gravitronLasers[j].slatedForDeletetion)
				{
					gravitronLasers.RemoveAt(j);
					break;
				}
				if (gravitronLasers[j].startPos.x < midPoint.x - 10f)
				{
					Laser laser2 = gravitronLasers[j];
					laser2.startPos.x = laser2.startPos.x + num2;
				}
				else if (gravitronLasers[j].startPos.x > midPoint.x + 10f)
				{
					Laser laser3 = gravitronLasers[j];
					laser3.startPos.x = laser3.startPos.x - num2;
				}
			}
			if ((float)activateTimer > num3 + num4 + 200f)
			{
				protection = false;
			}
			activateTimer++;
		}
		if (curPhase == Phase.PhaseCircleDanmaku)
		{
			if (activateIntroTimer == 0)
			{
				SetNewDestination(midPoint + new Vector2(0f, 200f));
				dirTicker = 0f;
				hintLight = new LightSource(midPoint, environmentalLight: false, Color.white, null);
				hintLight.setRad = 0f;
				hintLight.requireUpKeep = false;
				hintLight.setAlpha = 0f;
				targetVector = Vector2.zero;
				oracle.room.AddObject(hintLight);
			}
			float num6 = 240f;
			float num7 = 480f;
			float num8 = 200f;
			float num9 = Mathf.Lerp(250f, 100f, (floatActivateTimer - num6 - num7) / (950f + num8));
			float num10 = Mathf.Lerp(300f, 85f, (floatActivateTimer - num6 - num7 - num8) / 1250f);
			float t = Mathf.Lerp(0f, 0.1f, (floatActivateTimer - num6) / (num7 + 320f));
			float f = (float)Math.PI / 200f * (floatActivateTimer - num6 - num7 - num8) + 3.0925052f + (float)Math.PI / 64f;
			Vector2 vector = new Vector2(Mathf.Sin(f) * num10, (0f - Mathf.Sin(f)) * Mathf.Cos(f) * num10);
			targetVector = new Vector2(Mathf.Lerp(targetVector.x, vector.x, t), Mathf.Lerp(targetVector.y, vector.y, t));
			if ((float)activateIntroTimer < num6)
			{
				vector = Vector2.zero;
				num9 = Mathf.Lerp(400f, 250f, (float)activateIntroTimer / num6);
			}
			hintLight.setRad = num9;
			Vector2 vector2 = midPoint + targetVector;
			float t2 = (floatActivateTimer - num8) / 120f;
			if (floatActivateTimer < num8)
			{
				floatActivateTimer += 1f;
			}
			else
			{
				floatActivateTimer += Mathf.Lerp(0.1f, 1f, t2);
			}
			if ((float)activateTimer < num8)
			{
				vector2 = midPoint;
				hintLight.setAlpha = Mathf.Lerp(0f, 1f, (float)activateTimer / num8);
			}
			hintLight.setPos = new Vector2(Mathf.Lerp(hintLight.Pos.x, vector2.x, 0.15f), Mathf.Lerp(hintLight.Pos.y, vector2.y, 0.15f));
			if ((float)activateTimer % 10f == 0f)
			{
				dirTicker += 4f;
				for (int k = 0; k < 24; k++)
				{
					float num11 = (float)k * 15f + dirTicker % 15f;
					Vector2 vector3 = Custom.DegToVec(num11) * num9;
					SimpleDan simpleDan = new SimpleDan(this, vector2 + vector3, num11, 4f);
					simpleDan.dirTicker = 0.1f;
					if ((float)activateIntroTimer < num6)
					{
						simpleDan.active = false;
					}
					oracle.room.AddObject(simpleDan);
				}
			}
			if (floatActivateTimer - num6 - num7 - num8 > 1450f)
			{
				protection = false;
			}
			activateIntroTimer++;
			if ((float)activateIntroTimer == num6 - 10f)
			{
				oracle.room.AddObject(new ShockWave(midPoint, 500f, 0.4f, 30));
			}
			if ((float)activateIntroTimer == num6)
			{
				for (int l = 0; l < oracle.room.updateList.Count; l++)
				{
					if (oracle.room.updateList[l] is SimpleDan)
					{
						(oracle.room.updateList[l] as SimpleDan).active = true;
					}
				}
			}
			activateTimer++;
		}
		if (curPhase == Phase.PhaseKarmicGrid || curPhase == Phase.PhaseDirectedKGrid)
		{
			int num12 = 60;
			int num13 = 14;
			int num14 = 40;
			if (curPhase == Phase.PhaseDirectedKGrid)
			{
				num12 = 100;
				num13 = 10;
			}
			if (activateTimer % num12 == 0 && protection)
			{
				int num15 = 60;
				if (activateTimer == 0 && curPhase == Phase.PhaseKarmicGrid)
				{
					num15 += num14;
				}
				oracle.room.AddObject(new KarmicGrid(this, 2, num15));
			}
			if (activateTimer % 60 == 0 && activateTimer >= 60 && protection && curPhase == Phase.PhaseDirectedKGrid)
			{
				activeDirectedLaser = new Laser(this, oracle.firstChunk.pos, Custom.VecToDeg(Custom.DirVec(oracle.firstChunk.pos, player.firstChunk.pos)), 12f, 35, Laser.Type.EXPLODE);
				oracle.room.AddObject(activeDirectedLaser);
			}
			if (activateTimer % (num12 * (num13 + 3)) == num12 * num13 - 1)
			{
				protection = false;
			}
			if (activateTimer % (num12 * (num13 + 3)) == num12 * (num13 + 3) - 1)
			{
				protection = true;
				player.godTimer = player.maxGodTime;
			}
			if (activateTimer == 10 && activateBuffer < num14 && curPhase == Phase.PhaseKarmicGrid)
			{
				activateBuffer++;
			}
			else
			{
				activateTimer++;
			}
		}
		if (curPhase == Phase.PhaseLaserFan)
		{
			int num16 = 400;
			if (activateTimer % num16 == 0 && protection)
			{
				dirTicker = UnityEngine.Random.value * 360f;
				if (dirTicker == 0f)
				{
					dirTicker = 1f;
				}
				dirTickAmount = ((UnityEngine.Random.value < 0.5f) ? 10f : (-10f));
				accDirTicks = 0f;
			}
			if (activateTimer % num16 == num16 - 100 && protection)
			{
				SetNewDestination(new Vector2(midPoint.x - 100f + UnityEngine.Random.value * 200f, midPoint.y - 100f + UnityEngine.Random.value * 200f));
			}
			if (activateTimer % 3 == 0 && dirTicker != 0f && protection && accDirTicks < 580f)
			{
				dirTicker += dirTickAmount;
				accDirTicks += Mathf.Abs(dirTickAmount);
				Laser laser4 = new Laser(this, oracle.firstChunk.pos, dirTicker, 12f, 120, Laser.Type.LIGHTNING);
				laser4.hideGuides = true;
				laser4.dirTick = -0.2f;
				oracle.room.AddObject(laser4);
			}
			if (activateTimer % (num16 * 5) == num16 * 4 - 1)
			{
				protection = false;
				dirTicker = 0f;
			}
			if (activateTimer % (num16 * 5) == num16 * 5 - 1)
			{
				protection = true;
				player.godTimer = player.maxGodTime;
			}
			activateTimer++;
		}
		if (curPhase == Phase.PhaseTalk)
		{
			if (activateTimer == 100)
			{
				oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.ST_Talk1, oracle.firstChunk.pos, 1f, 1.15f);
			}
			if (activateTimer == 240)
			{
				protection = false;
				AdvancePhase();
				return;
			}
			activateTimer++;
		}
		if (curPhase != Phase.Inactive && curPhase != Phase.Activate && extraGraphics != null)
		{
			if (protection)
			{
				extraGraphics.disruptAlpha = Mathf.Lerp(extraGraphics.disruptAlpha, 0.5f, 0.2f);
			}
			else
			{
				extraGraphics.disruptAlpha = Mathf.Lerp(extraGraphics.disruptAlpha, 0f, 0.2f);
			}
		}
	}

	private void Move()
	{
	}

	private Vector2 ClampVectorInRoom(Vector2 v)
	{
		Vector2 result = v;
		result.x = Mathf.Clamp(result.x, oracle.arm.cornerPositions[0].x + 10f, oracle.arm.cornerPositions[1].x - 10f);
		result.y = Mathf.Clamp(result.y, oracle.arm.cornerPositions[2].y + 10f, oracle.arm.cornerPositions[1].y - 10f);
		return result;
	}

	private void SetNewDestination(Vector2 dst)
	{
		lastPos = currentGetTo;
		nextPos = dst;
		lastPosHandle = Custom.RNV() * Mathf.Lerp(0.3f, 0.65f, UnityEngine.Random.value) * Vector2.Distance(lastPos, nextPos);
		nextPosHandle = -GetToDir * Mathf.Lerp(0.3f, 0.65f, UnityEngine.Random.value) * Vector2.Distance(lastPos, nextPos);
		pathProgression = 0f;
	}

	public void AdvancePhase()
	{
		if (protection)
		{
			return;
		}
		if (curPhase == Phase.Inactive)
		{
			curPhase = Phase.Activate;
			oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.ST_Cry_Intro, 0f, 1f, 0.7f + UnityEngine.Random.value * 0.25f);
			return;
		}
		if (curPhase == Phase.Ending)
		{
			if (activateLightning != null)
			{
				activateLightning.Destroy();
			}
			if (energySwirl != null)
			{
				energySwirl.Destroy();
			}
			oracle.health = 0f;
			oracle.setGravity(0.9f);
			oracle.room.game.GetArenaGameSession.exitManager.challengeCompletedA = true;
			oracle.room.game.GetArenaGameSession.exitManager.challengeCompletedB = true;
			RoomSettings.RoomEffect effect = oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.ZeroG);
			RoomSettings.RoomEffect effect2 = oracle.room.roomSettings.GetEffect(RoomSettings.RoomEffect.Type.DarkenLights);
			if (effect != null)
			{
				effect.amount = 0f;
			}
			if (effect2 != null)
			{
				effect2.amount = 0.95f;
			}
			for (int i = 0; i < oracle.room.roomSettings.ambientSounds.Count; i++)
			{
				oracle.room.roomSettings.ambientSounds[i].volume = 0f;
			}
			return;
		}
		if (curPhase == Phase.PhaseDirectedLaser)
		{
			curPhase = Phase.PhaseKarmicGrid;
		}
		else if (curPhase == Phase.PhaseKarmicGrid)
		{
			curPhase = Phase.PhaseLaserFan;
		}
		else if (curPhase == Phase.PhaseLaserFan)
		{
			SetNewDestination(midPoint);
			curPhase = Phase.PhaseTalk;
		}
		else if (curPhase == Phase.PhaseTalk)
		{
			curPhase = Phase.PhaseDirectedKGrid;
		}
		else if (curPhase == Phase.PhaseCombo)
		{
			curPhase = Phase.PhaseDirectedKGrid;
		}
		else if (curPhase == Phase.PhaseDirectedKGrid)
		{
			energySwirl = new EnergySwirl(midPoint, Color.white, null);
			energySwirl.setRad = 800f;
			energySwirl.setDepth = 0.77f;
			energySwirl.effectColor = -1;
			oracle.room.AddObject(energySwirl);
			curPhase = Phase.PhaseCircleDanmaku;
		}
		else if (curPhase == Phase.PhaseCircleDanmaku)
		{
			if (hintLight != null)
			{
				hintLight.slatedForDeletetion = true;
			}
			curPhase = Phase.PhaseGravitron;
		}
		else if (curPhase == Phase.PhaseGravitron)
		{
			SetNewDestination(midPoint);
			for (int j = 0; j < oracle.room.updateList.Count; j++)
			{
				if (oracle.room.updateList[j] is SimpleDan)
				{
					SimpleDan obj = oracle.room.updateList[j] as SimpleDan;
					obj.dir *= -1f;
					obj.speed *= 3f;
					obj.destroyType = SimpleDan.DestroyType.BORDER;
				}
			}
			curPhase = Phase.Ending;
		}
		if (curPhase == Phase.Ending)
		{
			player.tongue.disableStick = false;
		}
		if (curPhase == Phase.PhaseCircleDanmaku)
		{
			player.tongue.disableStick = true;
		}
		protection = true;
		activateTimer = 0;
		floatActivateTimer = 0f;
		player.godTimer = player.maxGodTime;
		player.DeactivateAscension();
		_ = UnityEngine.Random.value;
		oracle.room.PlaySound(MoreSlugcatsEnums.MSCSoundID.ST_Cry, 0f, 1f, 0.7f + UnityEngine.Random.value * 0.25f);
	}

	public Vector2? HandDirection()
	{
		if ((curPhase != Phase.PhaseDirectedLaser && curPhase != Phase.PhaseDirectedKGrid) || !protection)
		{
			return null;
		}
		if (activeDirectedLaser == null)
		{
			return Custom.DirVec(oracle.firstChunk.pos, player.firstChunk.pos);
		}
		return Custom.DegToVec(activeDirectedLaser.dir);
	}
}
