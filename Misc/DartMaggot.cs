using System;
using RWCustom;
using UnityEngine;

public class DartMaggot : PhysicalObject, IDrawable
{
	public class Mode : ExtEnum<Mode>
	{
		public static readonly Mode Free = new Mode("Free", register: true);

		public static readonly Mode Shot = new Mode("Shot", register: true);

		public static readonly Mode StuckInChunk = new Mode("StuckInChunk", register: true);

		public static readonly Mode StuckInTerrain = new Mode("StuckInTerrain", register: true);

		public Mode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class DartMaggotStick : AbstractPhysicalObject.AbstractObjectStick
	{
		public DartMaggotStick(AbstractPhysicalObject maggot, AbstractPhysicalObject stickIn)
			: base(maggot, stickIn)
		{
		}

		public override string SaveToString(int roomIndex)
		{
			return roomIndex + "<stkA>gripStk<stkA>" + A.ID.ToString() + "<stkA>" + B.ID.ToString();
		}
	}

	public class Umbilical : CosmeticSprite
	{
		public Vector2[,] points;

		private DartMaggot maggot;

		private BigSpider spider;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		private Color fogColor;

		private Color threadCol;

		private Color blackColor;

		private float LifeOfSegment(int i)
		{
			if (i > 0)
			{
				return Mathf.Min(points[i, 3].x, points[i - 1, 3].x);
			}
			return points[i, 3].x;
		}

		public Umbilical(Room room, DartMaggot maggot, BigSpider spider, Vector2 shootVel)
		{
			base.room = room;
			this.maggot = maggot;
			this.spider = spider;
			points = new Vector2[UnityEngine.Random.Range(10, 20), 4];
			for (int i = 0; i < points.GetLength(0); i++)
			{
				points[i, 0] = spider.mainBodyChunk.pos + Custom.RNV();
				points[i, 1] = spider.mainBodyChunk.pos;
				points[i, 2] = shootVel * 0.3f * UnityEngine.Random.value + Custom.RNV() * UnityEngine.Random.value * 1.5f;
				points[i, 3] = new Vector2(1f, (UnityEngine.Random.value < 1f / 60f) ? 0f : Mathf.Lerp(50f, 200f, Mathf.Pow(UnityEngine.Random.value, 0.3f)));
			}
			if (UnityEngine.Random.value < 0.5f)
			{
				points[UnityEngine.Random.Range(0, points.GetLength(0)), 3].x = 0f;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			bool flag = true;
			for (int i = 0; i < points.GetLength(0); i++)
			{
				points[i, 1] = points[i, 0];
				points[i, 0] += points[i, 2];
				points[i, 2] *= Custom.LerpMap(points[i, 2].magnitude, 1f, 30f, 0.99f, 0.8f);
				points[i, 2].y -= Mathf.Lerp(0.1f, 0.6f, LifeOfSegment(i));
				if (!(LifeOfSegment(i) > 0f))
				{
					continue;
				}
				if (room.GetTile(points[i, 0]).Solid)
				{
					SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(points[i, 0], points[i, 1], points[i, 2], 1f, default(IntVector2), goThroughFloors: true);
					cd = SharedPhysics.HorizontalCollision(room, cd);
					cd = SharedPhysics.VerticalCollision(room, cd);
					points[i, 0] = cd.pos;
					points[i, 2] = cd.vel;
				}
				if (i > 0)
				{
					if (!Custom.DistLess(points[i, 0], points[i - 1, 0], 6f))
					{
						Vector2 vector = Custom.DirVec(points[i, 0], points[i - 1, 0]) * (Vector2.Distance(points[i, 0], points[i - 1, 0]) - 6f);
						points[i, 0] += vector * 0.15f;
						points[i, 2] += vector * 0.25f;
						points[i - 1, 0] -= vector * 0.15f;
						points[i - 1, 2] -= vector * 0.25f;
					}
					if (!room.VisualContact(points[i, 0], points[i - 1, 0]))
					{
						points[i, 3].x -= 0.2f;
					}
				}
				if (i > 1 && LifeOfSegment(i - 1) > 0f)
				{
					points[i, 2] += Custom.DirVec(points[i - 2, 0], points[i, 0]) * 0.6f;
					points[i - 2, 2] -= Custom.DirVec(points[i - 2, 0], points[i, 0]) * 0.6f;
				}
				points[i, 3].x -= 1f / points[i, 3].y;
				if (points[i, 3].x > 0f)
				{
					flag = false;
				}
			}
			if (LifeOfSegment(0) > 0f)
			{
				points[0, 0] = spider.mainBodyChunk.pos;
				points[0, 2] *= 0f;
				points[1, 2] += Custom.DirVec(spider.bodyChunks[1].pos, spider.mainBodyChunk.pos) * 3f;
				points[3, 2] += Custom.DirVec(spider.bodyChunks[1].pos, spider.mainBodyChunk.pos) * 1.5f;
				if (LifeOfSegment(1) <= 0f || spider.enteringShortCut.HasValue || spider.room != room)
				{
					points[0, 3].x -= 1f;
				}
			}
			if (LifeOfSegment(points.GetLength(0) - 1) > 0f)
			{
				points[points.GetLength(0) - 1, 0] = maggot.body[1, 0];
				points[points.GetLength(0) - 1, 2] *= 0f;
				points[points.GetLength(0) - 2, 2] += Custom.DirVec(maggot.body[0, 0], maggot.body[1, 0]) * 3f;
				points[points.GetLength(0) - 3, 2] += Custom.DirVec(maggot.body[0, 0], maggot.body[1, 0]) * 1.5f;
				if (maggot.slatedForDeletetion || maggot.room != room)
				{
					points[points.GetLength(0) - 1, 3].x -= 1f;
				}
			}
			if (flag)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(points.GetLength(0), pointyTip: false, customColor: true);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = ((!(LifeOfSegment(0) > 0f) || spider == null) ? Vector2.Lerp(points[0, 1], points[0, 0], timeStacker) : Vector2.Lerp(spider.mainBodyChunk.lastPos, spider.mainBodyChunk.pos, timeStacker));
			float num = 0f;
			float b = 1f;
			bool flag = rCam.room.Darkness(pos) > 0.2f;
			for (int i = 0; i < points.GetLength(0); i++)
			{
				float f = Mathf.InverseLerp(0f, points.GetLength(0) - 1, i);
				float num2 = LifeOfSegment(i);
				if (i < points.GetLength(0) - 1)
				{
					num2 = Mathf.Min(num2, LifeOfSegment(i + 1));
				}
				float num3 = 0.5f * Mathf.InverseLerp(0f, 0.3f, num2);
				Vector2 vector2 = Vector2.Lerp(points[i, 1], points[i, 0], timeStacker);
				if (i == 0 && LifeOfSegment(0) > 0f)
				{
					vector2 = vector;
				}
				else if (i == points.GetLength(0) - 1 && LifeOfSegment(i) > 0f)
				{
					vector2 = Vector2.Lerp(maggot.body[1, 1], maggot.body[1, 0], timeStacker);
				}
				Vector2 vector3 = Custom.PerpendicularVector(vector2, vector);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, (vector + vector2) / 2f - vector3 * (num3 + num) * 0.5f - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, (vector + vector2) / 2f + vector3 * (num3 + num) * 0.5f - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * num3 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * num3 - camPos);
				Color color = Color.Lerp(fogColor, Color.Lerp(new Color(1f, 0f, 0f), Color.Lerp(threadCol, new Color(1f, 1f, 1f), rCam.room.WaterShinyness(vector2, timeStacker)), 0.1f + 0.9f * Mathf.Pow(f, 0.25f + num2)), Mathf.Min(num2, b));
				if (flag && num2 > 0f)
				{
					color = Color.Lerp(color, blackColor, rCam.room.DarknessOfPoint(rCam, vector2));
				}
				for (int j = 0; j < 4; j++)
				{
					(sLeaser.sprites[0] as TriangleMesh).verticeColors[i * 4 + j] = color;
				}
				vector = vector2;
				num = num3;
				b = num2;
			}
			for (int k = 0; k < 4; k++)
			{
				(sLeaser.sprites[0] as TriangleMesh).verticeColors[k] = Color.Lerp(fogColor, blackColor, LifeOfSegment(0));
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			blackColor = palette.blackColor;
			fogColor = palette.fogColor;
			threadCol = Color.Lerp(new Color(0.95f, 0.8f, 0.55f), palette.fogColor, 0.2f);
		}
	}

	private int meshSegs = 9;

	public Vector2[,] body;

	public Creature shotBy;

	private Vector2 wiggleDir;

	private Vector2 wiggleGetToDir;

	private float[,] squirm;

	private float squirmOffset;

	private float squirmAdd;

	private float squirmAddGetTo;

	private float squirmWidth;

	private float squirmWidthGetTo;

	private float squirmAmp;

	private float squirmAmpGetTo;

	public Vector2 stuckPos;

	public Vector2 stuckDir;

	public Mode mode;

	public Vector2 needleDir;

	public Vector2 lastNeedleDir;

	private BodyChunk stuckInChunk;

	public float age;

	public float lifeTime;

	private int sleepCounter;

	private float bloat;

	private float squeeze = -0.1f;

	private float sizeFac;

	private float newAndPink;

	private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

	public static int UntilSleepDelay = 340;

	public float darkness;

	public float lastDarkness;

	private Color yellow;

	public bool Stuck
	{
		get
		{
			if (!(mode == Mode.StuckInTerrain))
			{
				return mode == Mode.StuckInChunk;
			}
			return true;
		}
	}

	public float Life => Mathf.InverseLerp(0.65f, 0.55f, age);

	public float Rot => Mathf.Max(Mathf.InverseLerp(0.65f, 0.75f, age), squeeze);

	public float Dissapear => Mathf.InverseLerp(0.75f, 1f, age);

	public Vector2 StuckInChunkPos(BodyChunk chunk)
	{
		if (chunk.owner is Player && chunk.owner.graphicsModule != null)
		{
			return (chunk.owner.graphicsModule as PlayerGraphics).drawPositions[chunk.index, 0];
		}
		return chunk.pos;
	}

	public DartMaggot(AbstractPhysicalObject abstrObj)
		: base(abstrObj)
	{
		UnityEngine.Random.State state = UnityEngine.Random.state;
		UnityEngine.Random.InitState(abstractPhysicalObject.ID.RandomSeed);
		sizeFac = Custom.ClampedRandomVariation(0.8f, 0.2f, 0.5f);
		UnityEngine.Random.state = state;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, default(Vector2), 2.6f * sizeFac, 0.1f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 0;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		body = new Vector2[2, 3];
		ResetBody(Custom.RNV());
		wiggleGetToDir = Custom.RNV() * UnityEngine.Random.value;
		wiggleDir = wiggleGetToDir;
		mode = Mode.Free;
		needleDir = Custom.RNV();
		lastNeedleDir = needleDir;
		squirm = new float[meshSegs, 3];
		lifeTime = Mathf.Lerp(1100f, 2100f, UnityEngine.Random.value);
	}

	public void ResetBody(Vector2 dr)
	{
		for (int i = 0; i < body.GetLength(0); i++)
		{
			body[i, 0] = base.firstChunk.pos - dr * i;
			body[i, 1] = body[i, 0];
			body[i, 2] *= 0f;
		}
	}

	public void Shoot(Vector2 pos, Vector2 dir, Creature shotBy)
	{
		base.firstChunk.HardSetPosition(pos);
		base.firstChunk.vel = dir * 70f;
		ResetBody(dir);
		ChangeMode(Mode.Shot);
		this.shotBy = shotBy;
		needleDir = dir;
		lastNeedleDir = dir;
		sleepCounter = UntilSleepDelay;
		newAndPink = 1f;
		room.PlaySound(SoundID.Dart_Maggot_Whizz_By, base.firstChunk);
		if (shotBy is BigSpider)
		{
			room.AddObject(new Umbilical(room, this, shotBy as BigSpider, base.firstChunk.vel));
		}
		for (int num = UnityEngine.Random.Range(0, 5); num >= 0; num--)
		{
			room.AddObject(new WaterDrip(pos, dir * UnityEngine.Random.value * 15f + Custom.RNV() * UnityEngine.Random.value * 5f, waterColor: false));
		}
	}

	public void ChangeMode(Mode newMode)
	{
		base.CollideWithTerrain = newMode == Mode.Free;
		if (newMode == Mode.StuckInChunk && stuckInChunk != null && stuckInChunk.owner is Creature && shotBy != null && shotBy is BigSpider && (shotBy as BigSpider).spitter)
		{
			(shotBy as BigSpider).AI.spitModule.CreatureHitByDart((stuckInChunk.owner as Creature).abstractCreature);
		}
		mode = newMode;
	}

	public override void Update(bool eu)
	{
		lastNeedleDir = needleDir;
		canBeHitByWeapons = mode != Mode.StuckInChunk;
		if (mode != Mode.Shot)
		{
			base.Update(eu);
			NormalUpdate();
		}
		if (mode == Mode.Shot)
		{
			ShotUpdate();
		}
		else if (mode == Mode.StuckInChunk)
		{
			needleDir = Custom.RotateAroundOrigo(stuckDir, Custom.VecToDeg(stuckInChunk.Rotation));
			base.firstChunk.pos = StuckInChunkPos(stuckInChunk) + Custom.RotateAroundOrigo(stuckPos, Custom.VecToDeg(stuckInChunk.Rotation));
			base.firstChunk.vel *= 0f;
			if (sleepCounter > 0)
			{
				if (sleepCounter < 260)
				{
					float t = Mathf.InverseLerp(260f, 30f, sleepCounter);
					bloat = Mathf.Min(1f, bloat + 1f / 60f);
					squeeze = Mathf.Min(1f, squeeze + 0.0038461538f);
					if (stuckInChunk.owner is Creature && UnityEngine.Random.value < 1f / Mathf.Lerp(40f, 20f, t))
					{
						(stuckInChunk.owner as Creature).stun = Math.Max((stuckInChunk.owner as Creature).stun, (int)(UnityEngine.Random.value * Mathf.Lerp(8f, 22f, t)));
					}
					if (stuckInChunk.owner is Player && UnityEngine.Random.value < 1f / Mathf.Lerp(60f, 10f, t))
					{
						(stuckInChunk.owner as Player).slowMovementStun = Math.Max((stuckInChunk.owner as Player).slowMovementStun, (int)(UnityEngine.Random.value * 20f));
					}
				}
				sleepCounter--;
				if (sleepCounter == 0)
				{
					int num = 0;
					for (int i = 0; i < stuckInChunk.owner.abstractPhysicalObject.stuckObjects.Count; i++)
					{
						if (stuckInChunk.owner.abstractPhysicalObject.stuckObjects[i] is DartMaggotStick)
						{
							num++;
						}
					}
					if (stuckInChunk.owner is Creature)
					{
						(stuckInChunk.owner as Creature).stun = Math.Max((stuckInChunk.owner as Creature).stun, 40 * (2 + 3 * Custom.IntClamp(num, 1, 4)));
					}
				}
			}
			else if (UnityEngine.Random.value < 1f / 30f || abstractPhysicalObject.stuckObjects.Count < 1)
			{
				Unstuck();
				age = Mathf.Max(age, 0.75f);
			}
		}
		else if (mode == Mode.StuckInTerrain)
		{
			needleDir = stuckDir;
			base.firstChunk.pos = stuckPos;
			base.firstChunk.vel *= 0f;
		}
		if (!Stuck && (base.firstChunk.pos.x < -100f || base.firstChunk.pos.x > room.PixelWidth + 100f || base.firstChunk.pos.y < -100f))
		{
			Destroy();
		}
		newAndPink = Mathf.Max(0f, newAndPink - 0.025f);
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (mode == Mode.Shot)
		{
			ChangeMode(Mode.Free);
			base.firstChunk.vel = Vector2.Lerp(base.firstChunk.vel, weapon.firstChunk.vel, 0.6f);
		}
	}

	public void ShotUpdate()
	{
		base.firstChunk.lastLastPos = base.firstChunk.lastPos;
		base.firstChunk.lastPos = base.firstChunk.pos;
		for (int i = 0; i < body.GetLength(0); i++)
		{
			body[i, 1] = body[i, 0];
			body[i, 2] = base.firstChunk.vel * 0.5f;
		}
		body[0, 0] = Vector2.Lerp(body[0, 0], (base.firstChunk.lastPos + base.firstChunk.pos) / 2f, Mathf.InverseLerp(0f, 50f, Vector2.Distance((base.firstChunk.lastPos + base.firstChunk.pos) / 2f, body[0, 0])));
		body[1, 0] = Vector2.Lerp(body[1, 0], (base.firstChunk.lastLastPos + base.firstChunk.lastPos) / 2f, Mathf.InverseLerp(0f, 50f, Vector2.Distance((base.firstChunk.lastLastPos + base.firstChunk.lastPos) / 2f, body[1, 0])));
		Vector2 pos = base.firstChunk.pos;
		Vector2 vector = base.firstChunk.pos + base.firstChunk.vel;
		needleDir = Custom.DirVec(pos, vector);
		FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, pos, vector);
		Vector2 vector2 = default(Vector2);
		if (floatRect.HasValue)
		{
			vector2 = new Vector2(floatRect.Value.left, floatRect.Value.bottom);
		}
		Vector2 pos2 = vector;
		SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, pos, ref pos2, 1f, 1, shotBy, hitAppendages: false);
		if (floatRect.HasValue && collisionResult.chunk != null)
		{
			if (Vector2.Distance(pos, vector2) < Vector2.Distance(pos, collisionResult.collisionPoint))
			{
				collisionResult.chunk = null;
			}
			else
			{
				floatRect = null;
			}
		}
		if (floatRect.HasValue)
		{
			if (Vector2.Dot(Custom.DirVec(pos, vector), new Vector2(floatRect.Value.right, floatRect.Value.top)) > 0.5f)
			{
				stuckPos = vector2 + Custom.DirVec(vector, pos) * 15f;
				stuckDir = Custom.DirVec(pos, vector);
				base.firstChunk.pos = stuckPos;
				room.PlaySound(SoundID.Dart_Maggot_Stick_In_Wall, base.firstChunk);
				ChangeMode(Mode.StuckInTerrain);
			}
			else
			{
				if (floatRect.Value.right != 0f)
				{
					base.firstChunk.vel.x = Mathf.Abs(base.firstChunk.vel.x) * Mathf.Sign(floatRect.Value.right) * -1f;
				}
				if (floatRect.Value.top != 0f)
				{
					base.firstChunk.vel.y = Mathf.Abs(base.firstChunk.vel.y) * Mathf.Sign(floatRect.Value.top) * -1f;
				}
				base.firstChunk.pos = vector2 + Custom.DirVec(vector, pos) * 15f;
				room.PlaySound(SoundID.Dart_Maggot_Bounce_Off_Wall, base.firstChunk);
				ChangeMode(Mode.Free);
			}
		}
		else if (collisionResult.chunk != null && (!ModManager.MMF || !(collisionResult.chunk.owner is TempleGuard)))
		{
			base.firstChunk.pos = collisionResult.collisionPoint + Custom.DirVec(vector, pos) * 11f;
			stuckPos = Custom.RotateAroundOrigo(base.firstChunk.pos - StuckInChunkPos(collisionResult.chunk), 0f - Custom.VecToDeg(collisionResult.chunk.Rotation));
			stuckDir = Custom.RotateAroundOrigo(Custom.DirVec(pos, vector), 0f - Custom.VecToDeg(collisionResult.chunk.Rotation));
			stuckInChunk = collisionResult.chunk;
			if (stuckInChunk.owner is Creature)
			{
				(stuckInChunk.owner as Creature).Violence(base.firstChunk, Custom.DirVec(pos, vector) * 3f, stuckInChunk, null, Creature.DamageType.Stab, 0.07f, 3f);
			}
			else
			{
				stuckInChunk.vel += Custom.DirVec(pos, vector) * 3f / stuckInChunk.mass;
			}
			new DartMaggotStick(abstractPhysicalObject, stuckInChunk.owner.abstractPhysicalObject);
			room.PlaySound(SoundID.Dart_Maggot_Stick_In_Creature, base.firstChunk);
			ChangeMode(Mode.StuckInChunk);
		}
		else
		{
			base.firstChunk.pos += base.firstChunk.vel;
		}
		base.firstChunk.vel.y -= base.gravity;
		if (base.firstChunk.vel.magnitude < 30f)
		{
			ChangeMode(Mode.Free);
		}
	}

	public void NormalUpdate()
	{
		for (int i = 0; i < body.GetLength(0); i++)
		{
			body[i, 1] = body[i, 0];
			body[i, 0] += body[i, 2];
			body[i, 2] *= base.airFriction;
			body[i, 2].y -= base.gravity;
		}
		for (int j = 0; j < body.GetLength(0); j++)
		{
			SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(body[j, 0], body[j, 1], body[j, 2], (2.5f - (float)j * 0.5f) * sizeFac, default(IntVector2), base.firstChunk.goThroughFloors);
			cd = SharedPhysics.VerticalCollision(room, cd);
			cd = SharedPhysics.HorizontalCollision(room, cd);
			cd = SharedPhysics.SlopesVertically(room, cd);
			body[j, 0] = cd.pos;
			body[j, 2] = cd.vel;
			if (cd.contactPoint.y < 0)
			{
				body[j, 2].x *= 0.4f;
			}
			if (j == 0)
			{
				Vector2 vector = Custom.DirVec(body[j, 0], base.firstChunk.pos) * (Vector2.Distance(body[j, 0], base.firstChunk.pos) - 5f * sizeFac);
				body[j, 0] += vector * 0.5f;
				body[j, 2] += vector * 0.5f;
				base.firstChunk.pos -= vector * 0.5f;
				base.firstChunk.vel -= vector * 0.5f;
			}
			else
			{
				Vector2 vector = Custom.DirVec(body[j, 0], body[j - 1, 0]) * (Vector2.Distance(body[j, 0], body[j - 1, 0]) - 5f * sizeFac);
				body[j, 0] += vector * 0.5f;
				body[j, 2] += vector * 0.5f;
				body[j - 1, 0] -= vector * 0.5f;
				body[j - 1, 2] -= vector * 0.5f;
			}
		}
		float num = Mathf.Pow(Mathf.InverseLerp(0.25f, -0.75f, Vector2.Dot((base.firstChunk.pos - body[0, 0]).normalized, (body[0, 0] - body[1, 0]).normalized)), 2f);
		base.firstChunk.vel += Custom.DirVec(body[1, 0], base.firstChunk.pos) * num * 1.5f * sizeFac;
		base.firstChunk.pos += Custom.DirVec(body[1, 0], base.firstChunk.pos) * num * 1.5f * sizeFac;
		body[1, 2] -= Custom.DirVec(body[1, 0], base.firstChunk.pos) * num * 1.5f * sizeFac;
		body[1, 0] -= Custom.DirVec(body[1, 0], base.firstChunk.pos) * num * 1.5f * sizeFac;
		if (UnityEngine.Random.value < Life / 20f)
		{
			wiggleGetToDir = Custom.RNV() * UnityEngine.Random.value;
		}
		wiggleGetToDir += Custom.RNV() * UnityEngine.Random.value * 0.05f;
		wiggleDir = Custom.MoveTowards(wiggleDir, wiggleGetToDir, 0.02f);
		if (Stuck)
		{
			body[0, 2] -= (wiggleDir * 2.3f * Life + needleDir * 1.2f) * sizeFac;
			body[1, 2] += wiggleDir * 4.6f * Life * sizeFac;
		}
		else
		{
			base.firstChunk.vel += wiggleDir * 2.3f * Life * sizeFac;
			body[0, 2] -= wiggleDir * 4.6f * Life * sizeFac;
			body[1, 2] += wiggleDir * 2.3f * Life * sizeFac;
			needleDir = (needleDir + Custom.DirVec(body[0, 0], base.firstChunk.pos) * 0.2f).normalized;
		}
		if (Life > 0f)
		{
			squirmOffset += squirmAdd * 0.2f * Life;
			if (UnityEngine.Random.value < Life / 50f)
			{
				squirmAddGetTo = Mathf.Lerp(-1f, 1f, UnityEngine.Random.value);
			}
			squirmAdd = Custom.LerpAndTick(squirmAdd, squirmAddGetTo, 0.03f, 0.0125f);
			if (UnityEngine.Random.value < Life / 50f)
			{
				squirmWidthGetTo = UnityEngine.Random.value;
			}
			squirmWidth = Custom.LerpAndTick(squirmWidth, squirmWidthGetTo, 0.03f, 0.0125f);
			if (UnityEngine.Random.value < Life / 50f)
			{
				squirmAmpGetTo = UnityEngine.Random.value;
			}
			squirmAmp = Custom.LerpAndTick(squirmAmp, squirmAmpGetTo, 0.03f, 0.0125f);
		}
		for (int k = 0; k < squirm.GetLength(0); k++)
		{
			squirm[k, 1] = squirm[k, 0];
			squirm[k, 0] = Mathf.Sin(squirmOffset + (float)k * Mathf.Lerp(0.5f, 2f, squirmWidth)) * squirmAmp * Life * (1f - bloat);
		}
		if (age < 0.65f)
		{
			age += 1f / lifeTime;
		}
		else
		{
			age += 0.0014285714f;
		}
		if (Stuck && stuckInChunk == null && UnityEngine.Random.value < 1f / Mathf.Lerp(240f, 80f, 0.5f * Rot + 0.5f * Dissapear) && UnityEngine.Random.value < Rot)
		{
			Unstuck();
		}
		if (age > 1f)
		{
			Destroy();
		}
	}

	public void Unstuck()
	{
		if (stuckInChunk != null)
		{
			base.firstChunk.vel = stuckInChunk.vel + Custom.RNV() * UnityEngine.Random.value * 2f;
			stuckInChunk = null;
		}
		else
		{
			base.firstChunk.vel = Custom.RNV() * UnityEngine.Random.value * 2f;
		}
		for (int num = abstractPhysicalObject.stuckObjects.Count - 1; num >= 0; num--)
		{
			if (abstractPhysicalObject.stuckObjects[num] is DartMaggotStick && abstractPhysicalObject.stuckObjects[num].A == abstractPhysicalObject)
			{
				abstractPhysicalObject.stuckObjects[num].Deactivate();
			}
		}
		ChangeMode(Mode.Free);
	}

	public override void Destroy()
	{
		base.Destroy();
		for (int num = abstractPhysicalObject.stuckObjects.Count - 1; num >= 0; num--)
		{
			abstractPhysicalObject.stuckObjects[num].Deactivate();
		}
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (speed > 6f && firstContact)
		{
			room.PlaySound(SoundID.Dart_Maggot_Bounce_Off_Wall, base.firstChunk, loop: false, Mathf.InverseLerp(4f, 20f, speed), 1f);
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		ResetBody(Custom.RNV());
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetBody(Custom.RNV());
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(meshSegs, pointyTip: false, customColor: true);
		sLeaser.sprites[1] = TriangleMesh.MakeLongMesh(meshSegs - 3, pointyTip: false, customColor: true);
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector2.Lerp(body[0, 1], body[0, 0], timeStacker);
		Vector2 vector3 = Vector2.Lerp(body[1, 1], body[1, 0], timeStacker);
		Vector2 vector4 = -Vector3.Slerp(lastNeedleDir, needleDir, timeStacker);
		Vector2 vector5 = Custom.DirVec(vector, vector2);
		Vector2 vector6 = Custom.DirVec(vector2, vector3);
		if (room != null)
		{
			lastDarkness = darkness;
			darkness = room.DarknessOfPoint(rCam, vector2);
			if (darkness != lastDarkness)
			{
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
		}
		vector -= vector4 * 7f * sizeFac;
		vector3 += vector5 * (3f * (1f - bloat)) * sizeFac;
		Vector2 vector7 = vector - vector4 * 18f;
		Vector2 v = vector7;
		float num = 0f;
		float num2 = Custom.LerpMap(Vector2.Distance(vector, vector2) + Vector2.Distance(vector2, vector3), 20f, 140f, 1f, 0.3f, 2f);
		float num3 = 1f;
		if (Rot > 0f || newAndPink > 0f)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			if (Dissapear > 0f)
			{
				num3 *= Mathf.Pow(1f - Dissapear, 0.2f);
			}
		}
		Vector2 vector8 = Custom.DegToVec(-45f);
		for (int i = 0; i < meshSegs; i++)
		{
			float num4 = Mathf.InverseLerp(1f, meshSegs - 1, i);
			float num5 = ((i < 2) ? (0.5f + (float)i) : (Custom.LerpMap(num4, 0.5f, 1f, Mathf.Lerp(3f, 2.5f, num4), 1f, 3f) * num2)) * num3;
			if (bloat > 0f && i > 1)
			{
				num5 = Mathf.Lerp(num5 * (1.2f + 0.65f * Mathf.Sin((float)Math.PI * num4) * bloat), 1f, (0.5f + 0.5f * squeeze) * Mathf.InverseLerp(1f - squeeze - 0.1f, 1f - squeeze + 0.1f, num4));
			}
			num5 *= sizeFac;
			Vector2 vector9 = ((i == 0) ? (vector - vector4 * 4f) : ((!(num4 < 0.5f)) ? Custom.Bezier(vector2, vector2 + vector5 * 4f, vector3, vector3 - vector6 * 2f, Mathf.InverseLerp(0.5f, 1f, num4)) : Custom.Bezier(vector, vector + vector4 * 2f, vector2, vector2 - vector5 * 4f, Mathf.InverseLerp(0f, 0.5f, num4))));
			Vector2 vector10 = vector9;
			Vector2 vector11 = Custom.PerpendicularVector(vector10, v);
			vector9 += vector11 * Mathf.Lerp(squirm[i, 1], squirm[i, 0], timeStacker) * num5 * (num4 * 0.3f + Mathf.Sin(num4 * (float)Math.PI));
			Vector2 vector12 = Custom.PerpendicularVector(vector9, vector7);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4, (vector7 + vector9) / 2f - vector12 * (num5 + num) * 0.5f - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 1, (vector7 + vector9) / 2f + vector12 * (num5 + num) * 0.5f - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 2, vector9 - vector12 * num5 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(i * 4 + 3, vector9 + vector12 * num5 - camPos);
			if (i > 1 && i < meshSegs - 1)
			{
				float num6 = Mathf.Lerp(0.2f, 0.5f, Mathf.Sin((float)Math.PI * Mathf.Pow(Mathf.InverseLerp(2f, meshSegs - 2, i), 0.5f)));
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice((i - 2) * 4, (vector7 + vector8 * num * num6 + vector9 + vector8 * num5 * num6) / 2f - vector12 * (num5 + num) * 0.5f * num6 - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice((i - 2) * 4 + 1, (vector7 + vector8 * num * num6 + vector9 + vector8 * num5 * num6) / 2f + vector12 * (num5 + num) * 0.5f * num6 - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice((i - 2) * 4 + 2, vector9 + vector8 * num5 * num6 - vector12 * num5 * num6 - camPos);
				(sLeaser.sprites[1] as TriangleMesh).MoveVertice((i - 2) * 4 + 3, vector9 + vector8 * num5 * num6 + vector12 * num5 * num6 - camPos);
			}
			vector7 = vector9;
			v = vector10;
			num = num5;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		yellow = Color.Lerp(new Color(0.95f, 0.8f, 0.55f), palette.fogColor, 0.2f);
		Color a = new Color(1f, 0f, 0f);
		Color color = new Color(1f, 1f, 1f);
		if (Rot > 0f)
		{
			yellow = Color.Lerp(yellow, new Color(0.6f, 0.4f, 0.2f), Rot);
			a = Color.Lerp(a, new Color(0.7f, 0.2f, 0.2f), Rot);
			color = Color.Lerp(color, new Color(0.9f, 0.5f, 0.5f), Rot * 0.5f);
		}
		if (Dissapear > 0f)
		{
			yellow = Color.Lerp(yellow, Color.Lerp(palette.blackColor, palette.fogColor, 0.4f * Mathf.Pow(Dissapear, 3f)), Dissapear);
			a = Color.Lerp(a, Color.Lerp(palette.blackColor, palette.fogColor, 0.4f * Mathf.Pow(Dissapear, 3f)), Dissapear);
			color = Color.Lerp(color, Color.Lerp(palette.blackColor, palette.fogColor, 0.4f * Mathf.Pow(Dissapear, 3f)), Dissapear);
		}
		if (darkness > 0f)
		{
			yellow = Color.Lerp(yellow, palette.blackColor, darkness);
			a = Color.Lerp(a, palette.blackColor, darkness);
			color = Color.Lerp(color, palette.blackColor, Mathf.Pow(darkness, 1.5f));
		}
		for (int i = 0; i < (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length; i++)
		{
			float value = Mathf.InverseLerp(0f, (sLeaser.sprites[0] as TriangleMesh).verticeColors.Length - 1, i);
			(sLeaser.sprites[0] as TriangleMesh).verticeColors[i] = Color.Lerp(a, yellow, 0.25f + Mathf.InverseLerp(0f, 0.2f + 0.8f * newAndPink, value) * 0.75f);
		}
		(sLeaser.sprites[0] as TriangleMesh).verticeColors[0] = color;
		(sLeaser.sprites[0] as TriangleMesh).verticeColors[1] = color;
		for (int j = 0; j < (sLeaser.sprites[1] as TriangleMesh).verticeColors.Length; j++)
		{
			float value2 = Mathf.InverseLerp(0f, (sLeaser.sprites[1] as TriangleMesh).verticeColors.Length - 1, j);
			(sLeaser.sprites[1] as TriangleMesh).verticeColors[j] = Custom.RGB2RGBA(color, Mathf.InverseLerp(0f, 0.2f, value2) * (1f - 0.75f * Rot - 0.25f * Dissapear));
		}
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[i]);
		}
	}
}
