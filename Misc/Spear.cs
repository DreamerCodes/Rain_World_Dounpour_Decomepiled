using System;
using MoreSlugcats;
using RWCustom;
using Smoke;
using UnityEngine;

public class Spear : Weapon
{
	public class Umbilical : CosmeticSprite
	{
		public Vector2[,] points;

		private Spear maggot;

		private Player spider;

		private Color fogColor;

		private Color threadCol;

		private Color blackColor;

		public Umbilical(Room room, Spear maggot, Player spider, Vector2 shootVel)
		{
			base.room = room;
			this.maggot = maggot;
			this.spider = spider;
			points = new Vector2[UnityEngine.Random.Range(10, 20), 4];
			for (int i = 0; i < points.GetLength(0); i++)
			{
				points[i, 0] = (spider.graphicsModule as PlayerGraphics).tail[0].pos + Custom.RNV();
				points[i, 1] = (spider.graphicsModule as PlayerGraphics).tail[0].pos;
				points[i, 2] = shootVel * 0.3f * UnityEngine.Random.value + Custom.RNV() * UnityEngine.Random.value * 1.5f;
				points[i, 3] = new Vector2(2f, Mathf.Lerp(150f, 200f, Mathf.Pow(UnityEngine.Random.value, 0.3f)));
			}
		}

		private float LifeOfSegment(int i)
		{
			if (i > 0)
			{
				return Mathf.Min(points[i, 3].x, points[i - 1, 3].x);
			}
			return points[i, 3].x;
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
					SharedPhysics.TerrainCollisionData cd = new SharedPhysics.TerrainCollisionData(points[i, 0], points[i, 1], points[i, 2], 1f, default(IntVector2), goThroughFloors: true);
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
				points[0, 0] = (spider.graphicsModule as PlayerGraphics).tail[0].pos;
				points[0, 2] *= 0f;
				points[1, 2] += Custom.DirVec((spider.graphicsModule as PlayerGraphics).tail[0].pos, (spider.graphicsModule as PlayerGraphics).tail[0].pos) * 3f;
				points[3, 2] += Custom.DirVec((spider.graphicsModule as PlayerGraphics).tail[0].pos, (spider.graphicsModule as PlayerGraphics).tail[0].pos) * 1.5f;
				if (LifeOfSegment(1) <= 0f || spider.enteringShortCut.HasValue || spider.room != room)
				{
					points[0, 3].x -= 1f;
				}
			}
			if (LifeOfSegment(points.GetLength(0) - 1) > 0f)
			{
				Vector2 rotation = maggot.rotation;
				rotation.Scale(new Vector2(25f, 25f));
				points[points.GetLength(0) - 1, 0] = maggot.bodyChunks[0].pos - rotation;
				points[points.GetLength(0) - 1, 2] *= 0f;
				points[points.GetLength(0) - 2, 2] += Custom.DirVec(maggot.bodyChunks[0].pos, maggot.bodyChunks[0].pos - rotation) * 6f;
				points[points.GetLength(0) - 3, 2] += Custom.DirVec(maggot.bodyChunks[0].pos, maggot.bodyChunks[0].pos - rotation) * 1.5f;
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
			Vector2 vector = ((!(LifeOfSegment(0) > 0f) || spider == null) ? Vector2.Lerp(points[0, 1], points[0, 0], timeStacker) : Vector2.Lerp((spider.graphicsModule as PlayerGraphics).tail[0].lastPos, (spider.graphicsModule as PlayerGraphics).tail[0].pos, timeStacker));
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
					Vector2 rotation = maggot.rotation;
					rotation.Scale(new Vector2(25f, 25f));
					vector2 = Vector2.Lerp(maggot.bodyChunks[0].pos - rotation, maggot.bodyChunks[0].pos - rotation, timeStacker);
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

	private bool spinning;

	protected bool pivotAtTip;

	protected bool lastPivotAtTip;

	public PhysicalObject stuckInObject;

	private int stuckInChunkIndex;

	public Appendage.Pos stuckInAppendage;

	public float stuckRotation;

	public Vector2? stuckInWall;

	private int stuckBodyPart;

	public bool alwaysStickInWalls;

	public int pinToWallCounter;

	private bool addPoles;

	public float spearDamageBonus = 1f;

	private int stillCounter;

	public Color? jollyCustomColor;

	public bool[] wasHorizontalBeam;

	public bool hasHorizontalBeamState;

	public int deerCounter;

	private bool spearmasterNeedle;

	private int spearmasterNeedleType;

	private bool spearmasterNeedle_hasConnection;

	private int spearmasterNeedle_fadecounter;

	private int spearmasterNeedle_fadecounter_max;

	public FireSmoke bugSmoke;

	public DebugLabel stuckIns;

	public AbstractSpear abstractSpear => abstractPhysicalObject as AbstractSpear;

	public BodyChunk stuckInChunk => stuckInObject.bodyChunks[stuckInChunkIndex];

	public override bool HeavyWeapon => true;

	public bool IsNeedle
	{
		get
		{
			if (ModManager.MSC)
			{
				return spearmasterNeedle;
			}
			return false;
		}
	}

	public bool bugSpear
	{
		get
		{
			if (ModManager.MSC)
			{
				return abstractSpear.hue != 0f;
			}
			return false;
		}
	}

	public bool onPlayerBack => base.mode == Mode.OnBack;

	public Spear(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 5f, 0.07f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.4f;
		surfaceFriction = 0.4f;
		collisionLayer = 2;
		base.waterFriction = 0.98f;
		base.buoyancy = 0.4f;
		pivotAtTip = false;
		lastPivotAtTip = false;
		stuckBodyPart = -1;
		base.firstChunk.loudness = 7f;
		tailPos = base.firstChunk.pos;
		soundLoop = new ChunkDynamicSoundLoop(base.firstChunk);
		wasHorizontalBeam = new bool[3];
		spearmasterNeedle = false;
		spearmasterNeedle_hasConnection = false;
		spearmasterNeedle_fadecounter_max = 400;
		spearmasterNeedle_fadecounter = spearmasterNeedle_fadecounter_max;
		spearmasterNeedleType = UnityEngine.Random.Range(0, 3);
		jollyCustomColor = null;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (abstractSpear.stuckInWall)
		{
			stuckInWall = placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile);
			ChangeMode(Mode.StuckInWall);
		}
	}

	public override void ChangeMode(Mode newMode)
	{
		if (base.mode == Mode.StuckInCreature)
		{
			if (room != null)
			{
				room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
			}
			PulledOutOfStuckObject();
			ChangeOverlap(newOverlap: true);
		}
		else if (newMode == Mode.StuckInCreature)
		{
			ChangeOverlap(newOverlap: false);
		}
		if (newMode != Mode.Thrown)
		{
			spearDamageBonus = 1f;
		}
		if (newMode == Mode.StuckInWall)
		{
			if (abstractSpear.stuckInWallCycles == 0)
			{
				abstractSpear.stuckInWallCycles = UnityEngine.Random.Range(3, 7) * ((throwDir.y == 0) ? 1 : (-1));
			}
			for (int i = -1; i < 2; i += 2)
			{
				if ((abstractSpear.stuckInWallCycles >= 0 && !room.GetTile(stuckInWall.Value + new Vector2(20f * (float)i, 0f)).Solid) || (abstractSpear.stuckInWallCycles < 0 && !room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)i)).Solid))
				{
					setRotation = ((abstractSpear.stuckInWallCycles >= 0) ? new Vector2(-i, 0f) : new Vector2(0f, -i));
					if (!(this is ExplosiveSpear))
					{
						addPoles = true;
					}
					break;
				}
			}
			if (setRotation.HasValue)
			{
				stuckInWall = room.MiddleOfTile(stuckInWall.Value) - setRotation.Value * 5f;
			}
			if (this is ExplosiveSpear)
			{
				abstractSpear.stuckInWallCycles = 0;
			}
			rotationSpeed = 0f;
		}
		if (newMode != Mode.Free)
		{
			spinning = false;
		}
		if (newMode != Mode.StuckInWall && newMode != Mode.StuckInCreature)
		{
			stuckInWall = null;
		}
		base.ChangeMode(newMode);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!spearmasterNeedle_hasConnection && spearmasterNeedle_fadecounter > 0)
		{
			spearmasterNeedle_fadecounter--;
		}
		soundLoop.sound = SoundID.None;
		if (base.firstChunk.vel.magnitude > 5f)
		{
			if (base.mode == Mode.Thrown)
			{
				soundLoop.sound = SoundID.Spear_Thrown_Through_Air_LOOP;
			}
			else if (base.mode == Mode.Free)
			{
				soundLoop.sound = SoundID.Spear_Spinning_Through_Air_LOOP;
			}
			soundLoop.Volume = Mathf.InverseLerp(5f, 15f, base.firstChunk.vel.magnitude);
		}
		soundLoop.Update();
		if (ModManager.MMF && base.mode == Mode.StuckInCreature && stuckInObject != null && (stuckInObject is Deer || (ModManager.MSC && stuckInObject is StowawayBug)))
		{
			deerCounter++;
			if (deerCounter > 40)
			{
				float ang = 0f;
				for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
				{
					if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject)
					{
						ang = (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).angle;
						break;
					}
				}
				ChangeMode(Mode.Free);
				if (room.BeingViewed)
				{
					base.firstChunk.vel = Custom.DegToVec(ang) * -10f;
					SetRandomSpin();
					if (stuckInObject is Deer)
					{
						room.AddObject(new SporeCloud(base.firstChunk.pos, Vector2.zero, Color.Lerp(new Color(0.9f, 1f, 0.8f), new Color(0.02f, 0.1f, 0.08f), 0.85f), 1f, null, 0, null));
					}
					for (int j = 0; j < 4; j++)
					{
						room.AddObject(new WaterDrip(base.firstChunk.pos, base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
					}
				}
				deerCounter = 0;
			}
		}
		lastPivotAtTip = pivotAtTip;
		pivotAtTip = base.mode == Mode.Thrown || base.mode == Mode.StuckInCreature;
		if (addPoles && room.readyForAI)
		{
			if (abstractSpear.stuckInWallCycles >= 0)
			{
				wasHorizontalBeam[1] = room.GetTile(stuckInWall.Value).horizontalBeam;
				room.GetTile(stuckInWall.Value).horizontalBeam = true;
				for (int k = -1; k < 2; k += 2)
				{
					wasHorizontalBeam[k + 1] = room.GetTile(stuckInWall.Value + new Vector2(20f * (float)k, 0f)).horizontalBeam;
					if (!room.GetTile(stuckInWall.Value + new Vector2(20f * (float)k, 0f)).Solid)
					{
						room.GetTile(stuckInWall.Value + new Vector2(20f * (float)k, 0f)).horizontalBeam = true;
					}
				}
			}
			else
			{
				wasHorizontalBeam[1] = room.GetTile(stuckInWall.Value).verticalBeam;
				room.GetTile(stuckInWall.Value).verticalBeam = true;
				for (int l = -1; l < 2; l += 2)
				{
					wasHorizontalBeam[l + 1] = room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)l)).verticalBeam;
					if (!room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)l)).Solid)
					{
						room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)l)).verticalBeam = true;
					}
				}
			}
			addPoles = false;
			hasHorizontalBeamState = true;
		}
		if (base.mode == Mode.Free)
		{
			if (ModManager.MSC)
			{
				Spear_NeedleDisconnect();
			}
			if (spinning)
			{
				if (Custom.DistLess(base.firstChunk.pos, base.firstChunk.lastPos, 4f * room.gravity))
				{
					stillCounter++;
				}
				else
				{
					stillCounter = 0;
				}
				if (base.firstChunk.ContactPoint.y < 0 || stillCounter > 20)
				{
					spinning = false;
					rotationSpeed = 0f;
					rotation = Custom.DegToVec(Mathf.Lerp(-50f, 50f, UnityEngine.Random.value) + 180f);
					base.firstChunk.vel *= 0f;
					room.PlaySound(SoundID.Spear_Stick_In_Ground, base.firstChunk);
				}
			}
			else if (!Custom.DistLess(base.firstChunk.lastPos, base.firstChunk.pos, 6f))
			{
				SetRandomSpin();
			}
		}
		else if (base.mode == Mode.Thrown)
		{
			base.firstChunk.vel.y += 0.45f;
			if (ModManager.MSC)
			{
				if (bugSpear && bugSmoke == null)
				{
					bugSmoke = new FireSmoke(room);
				}
				if (bugSmoke != null)
				{
					bugSmoke.Update(eu);
					if (room.ViewedByAnyCamera(base.firstChunk.pos, 300f))
					{
						bugSmoke.EmitSmoke(base.firstChunk.pos, Custom.RNV(), Custom.HSL2RGB(Custom.Decimal(abstractSpear.hue + EggBugGraphics.HUE_OFF), 1f, 0.5f), 25);
					}
					if (bugSmoke.Dead)
					{
						bugSmoke = null;
					}
				}
			}
			if (Custom.DistLess(thrownPos, base.firstChunk.pos, 560f * Mathf.Max(1f, spearDamageBonus)) && base.firstChunk.ContactPoint == throwDir && room.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Air && room.GetTile(base.firstChunk.pos + throwDir.ToVector2() * 20f).Terrain == Room.Tile.TerrainType.Solid && (UnityEngine.Random.value < ((this is ExplosiveSpear) ? 0.8f : 0.33f) || Custom.DistLess(thrownPos, base.firstChunk.pos, 140f) || alwaysStickInWalls))
			{
				bool flag = true;
				foreach (AbstractWorldEntity entity in room.abstractRoom.entities)
				{
					if (entity is AbstractSpear && (entity as AbstractSpear).realizedObject != null && ((entity as AbstractSpear).realizedObject as Weapon).mode == Mode.StuckInWall && entity.pos.Tile == abstractPhysicalObject.pos.Tile)
					{
						flag = false;
						break;
					}
				}
				bool flag2 = false;
				if (flag && !(this is ExplosiveSpear))
				{
					if (abstractPhysicalObject.pos.Tile.y <= 0 || abstractPhysicalObject.pos.Tile.y >= abstractPhysicalObject.Room.size.y - 1 || abstractPhysicalObject.pos.Tile.x <= 0 || abstractPhysicalObject.pos.Tile.x >= abstractPhysicalObject.Room.size.x - 1)
					{
						flag = false;
						flag2 = true;
						Custom.Log("Spear prevented from stabbing the edge of a room");
					}
					else
					{
						for (int m = 0; m < room.roomSettings.placedObjects.Count; m++)
						{
							if (room.roomSettings.placedObjects[m].type == PlacedObject.Type.NoSpearStickZone && Custom.DistLess(room.MiddleOfTile(base.firstChunk.pos), room.roomSettings.placedObjects[m].pos, (room.roomSettings.placedObjects[m].data as PlacedObject.ResizableObjectData).Rad))
							{
								flag = false;
								flag2 = true;
								break;
							}
						}
					}
				}
				if (flag && room.abstractRoom.shelter && room.shelterDoor != null && (room.shelterDoor.IsClosing || room.shelterDoor.IsOpening))
				{
					flag = false;
				}
				if (ModManager.MMF && base.firstChunk.vel.magnitude < 10f && !alwaysStickInWalls)
				{
					flag = false;
				}
				if (flag)
				{
					stuckInWall = room.MiddleOfTile(base.firstChunk.pos);
					vibrate = 10;
					ChangeMode(Mode.StuckInWall);
					room.PlaySound(SoundID.Spear_Stick_In_Wall, base.firstChunk);
					base.firstChunk.collideWithTerrain = false;
				}
				else if (ModManager.MMF && flag2)
				{
					vibrate = 10;
					room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk);
					for (int num = 17; num > 0; num--)
					{
						room.AddObject(new Spark(base.firstChunk.pos, Custom.RNV(), Color.white, null, 10, 20));
					}
				}
			}
		}
		else if (base.mode == Mode.StuckInCreature)
		{
			if (!stuckInWall.HasValue)
			{
				if (stuckInAppendage != null)
				{
					setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage)));
					base.firstChunk.pos = stuckInAppendage.appendage.OnAppendagePosition(stuckInAppendage);
				}
				else
				{
					base.firstChunk.vel = stuckInChunk.vel;
					if (stuckBodyPart == -1 || !room.BeingViewed || (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart) == null)
					{
						setRotation = Custom.DegToVec(stuckRotation + Custom.VecToDeg(stuckInChunk.Rotation));
						base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
					}
					else
					{
						setRotation = Custom.DegToVec(stuckRotation + Custom.AimFromOneVectorToAnother(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos));
						base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, Vector2.Lerp(stuckInChunk.pos, (stuckInChunk.owner as Creature).BodyPartByIndex(stuckBodyPart).pos, 0.5f) - stuckInChunk.pos);
					}
				}
			}
			else
			{
				if (pinToWallCounter > 0)
				{
					pinToWallCounter--;
				}
				if (stuckInChunk.vel.magnitude * stuckInChunk.mass > Custom.LerpMap(pinToWallCounter, 160f, 0f, 7f, 2f))
				{
					setRotation = (Custom.DegToVec(stuckRotation) + Vector2.ClampMagnitude(stuckInChunk.vel * stuckInChunk.mass * 0.005f, 0.1f)).normalized;
				}
				else
				{
					setRotation = Custom.DegToVec(stuckRotation);
				}
				base.firstChunk.vel *= 0f;
				base.firstChunk.pos = stuckInWall.Value;
				bool flag3 = false;
				bool flag4 = ModManager.MMF && !MMF.cfgVanillaExploits.Value;
				if (stuckInChunk.owner is Creature && (stuckInChunk.owner as Creature).enteringShortCut.HasValue)
				{
					Custom.Log("Chunk dislodged by shortcut");
					flag3 = true;
				}
				if (stuckInChunk.owner is Creature && (stuckInChunk.owner as Creature).inShortcut && flag4)
				{
					Custom.Log("Chunk dislodged by shortcut");
					flag3 = true;
				}
				if (pinToWallCounter < 160 && UnityEngine.Random.value < 0.025f)
				{
					if (stuckInChunk.vel.magnitude > Custom.LerpMap(pinToWallCounter, 160f, 0f, 140f, 30f / (1f + stuckInChunk.owner.TotalMass * 0.2f)))
					{
						Custom.Log("Chunk dislodged by velocity");
						flag3 = true;
					}
					else if (ModManager.MMF && flag4 && Vector2.Distance(stuckInChunk.pos, stuckInWall.Value) > stuckInChunk.rad * 3f)
					{
						Custom.Log("Chunk dislodged by chunk distance");
						flag3 = true;
					}
				}
				else if (ModManager.MMF && flag4 && Vector2.Distance(stuckInChunk.pos, stuckInWall.Value) > stuckInChunk.rad * 8f)
				{
					Custom.Log("Chunk dislodged by distance backup");
					flag3 = true;
				}
				if (flag3)
				{
					stuckRotation = Custom.Angle(setRotation.Value, stuckInChunk.Rotation);
					stuckInWall = null;
				}
				else
				{
					stuckInChunk.MoveFromOutsideMyUpdate(eu, stuckInWall.Value);
					stuckInChunk.vel *= 0f;
				}
			}
			if (stuckInChunk.owner.slatedForDeletetion)
			{
				ChangeMode(Mode.Free);
			}
		}
		else if (base.mode == Mode.StuckInWall)
		{
			if (ModManager.MSC)
			{
				Spear_NeedleDisconnect();
			}
			base.firstChunk.pos = stuckInWall.Value;
			base.firstChunk.vel *= 0f;
		}
		for (int num2 = abstractPhysicalObject.stuckObjects.Count - 1; num2 >= 0; num2--)
		{
			if (abstractPhysicalObject.stuckObjects[num2] is AbstractPhysicalObject.ImpaledOnSpearStick)
			{
				if (abstractPhysicalObject.stuckObjects[num2].B.realizedObject != null && (abstractPhysicalObject.stuckObjects[num2].B.realizedObject.slatedForDeletetion || abstractPhysicalObject.stuckObjects[num2].B.realizedObject.grabbedBy.Count > 0))
				{
					abstractPhysicalObject.stuckObjects[num2].Deactivate();
				}
				else if (abstractPhysicalObject.stuckObjects[num2].B.realizedObject != null && abstractPhysicalObject.stuckObjects[num2].B.realizedObject.room == room)
				{
					abstractPhysicalObject.stuckObjects[num2].B.realizedObject.firstChunk.MoveFromOutsideMyUpdate(eu, base.firstChunk.pos + rotation * Custom.LerpMap((abstractPhysicalObject.stuckObjects[num2] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition, 0f, 4f, 15f, -15f));
					abstractPhysicalObject.stuckObjects[num2].B.realizedObject.firstChunk.vel *= 0f;
				}
			}
		}
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		if (bugSpear)
		{
			_ = UnityEngine.Random.value;
			room?.PlaySound(MoreSlugcatsEnums.MSCSoundID.Throw_FireSpear, base.firstChunk.pos, 1f, UnityEngine.Random.Range(0.8f, 1.2f));
		}
		else
		{
			room?.PlaySound(SoundID.Slugcat_Throw_Spear, base.firstChunk);
		}
		alwaysStickInWalls = false;
		if (bugSpear)
		{
			room?.AddObject(new Explosion.ExplosionLight(base.firstChunk.pos, 280f, 1f, 7, Color.white));
			room?.AddObject(new ExplosionSpikes(room, base.firstChunk.pos, 14, 15f, 9f, 5f, 90f, Custom.HSL2RGB(Custom.Decimal(abstractSpear.hue + EggBugGraphics.HUE_OFF), 1f, 0.5f)));
		}
		if (Spear_NeedleCanFeed())
		{
			room?.AddObject(new Umbilical(room, this, thrownBy as Player, base.firstChunk.vel));
		}
	}

	public override void PickedUp(Creature upPicker)
	{
		if (hasHorizontalBeamState)
		{
			resetHorizontalBeamState();
			stuckInWall = default(Vector2);
			vibrate = 20;
			base.firstChunk.collideWithTerrain = true;
			abstractSpear.stuckInWallCycles = 0;
		}
		ChangeMode(Mode.Carried);
		room.PlaySound(SoundID.Slugcat_Pick_Up_Spear, base.firstChunk);
	}

	private void LodgeInCreature(SharedPhysics.CollisionResult result, bool eu, bool isJellyFish = false)
	{
		try
		{
			stuckInObject = result.obj;
			ChangeMode(Mode.StuckInCreature);
			if (ModManager.MSC && this is ElectricSpear)
			{
				(this as ElectricSpear).Electrocute(result.obj);
			}
			if (ModManager.MSC && result.obj is SeedCob)
			{
				if (result.chunk == null)
				{
					result.chunk = result.obj.firstChunk;
				}
				stuckInChunkIndex = result.chunk.index;
				stuckRotation = Custom.Angle(throwDir.ToVector2(), stuckInChunk.Rotation);
				base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
				Custom.Log("Add spear to seedcob chunk", stuckInChunk.index.ToString());
				new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, result.obj.abstractPhysicalObject, stuckInChunkIndex, stuckBodyPart, stuckRotation);
			}
			else if (ModManager.MSC && result.obj is JellyFish)
			{
				if (result.chunk == null)
				{
					result.chunk = result.obj.firstChunk;
				}
				stuckInChunkIndex = result.chunk.index;
				stuckRotation = Custom.Angle(throwDir.ToVector2(), stuckInChunk.Rotation);
				base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
				Custom.Log("Add spear to Jellyfish chunk", stuckInChunk.index.ToString());
				new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, result.obj.abstractPhysicalObject, stuckInChunkIndex, stuckBodyPart, stuckRotation);
			}
			else if (result.chunk != null)
			{
				stuckInChunkIndex = result.chunk.index;
				if (spearDamageBonus > 0.9f && room.GetTile(room.GetTilePosition(stuckInChunk.pos) + throwDir).Terrain == Room.Tile.TerrainType.Solid && room.GetTile(stuckInChunk.pos).Terrain == Room.Tile.TerrainType.Air)
				{
					stuckInWall = room.MiddleOfTile(stuckInChunk.pos) + throwDir.ToVector2() * (10f - stuckInChunk.rad);
					stuckInChunk.MoveFromOutsideMyUpdate(eu, stuckInWall.Value);
					stuckRotation = Custom.VecToDeg(rotation);
					stuckBodyPart = -1;
					pinToWallCounter = 300;
				}
				else if (stuckBodyPart == -1)
				{
					stuckRotation = Custom.Angle(throwDir.ToVector2(), stuckInChunk.Rotation);
				}
				base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
				Custom.Log("Add spear to creature chunk", stuckInChunk.index.ToString());
				new AbstractPhysicalObject.AbstractSpearStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, stuckInChunkIndex, stuckBodyPart, stuckRotation);
			}
			else if (result.onAppendagePos != null)
			{
				stuckInChunkIndex = 0;
				stuckInAppendage = result.onAppendagePos;
				stuckRotation = Custom.VecToDeg(rotation) - Custom.VecToDeg(stuckInAppendage.appendage.OnAppendageDirection(stuckInAppendage));
				Custom.Log("Add spear to creature Appendage");
				Custom.Log($"Abstract creature is: {(result.obj as Creature).abstractCreature}");
				new AbstractPhysicalObject.AbstractSpearAppendageStick(abstractPhysicalObject, (result.obj as Creature).abstractCreature, result.onAppendagePos.appendage.appIndex, result.onAppendagePos.prevSegment, result.onAppendagePos.distanceToNext, stuckRotation);
			}
			if (room.BeingViewed)
			{
				for (int i = 0; i < 8; i++)
				{
					room.AddObject(new WaterDrip(result.collisionPoint, -base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
				}
			}
		}
		catch (NullReferenceException ex)
		{
			ChangeMode(Mode.Free);
			stuckInObject = null;
			Custom.LogWarning($"Spear lodge in creature failure. {ex} :: {abstractSpear.pos} :: {result.obj} :: {room.abstractRoom.name}");
		}
	}

	private void LodgeInCreature(SharedPhysics.CollisionResult result, bool eu)
	{
		LodgeInCreature(result, eu, isJellyFish: false);
	}

	public override void RecreateSticksFromAbstract()
	{
		for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
		{
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).LodgedIn.realizedObject != null)
			{
				AbstractPhysicalObject.AbstractSpearStick abstractSpearStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick;
				stuckInObject = abstractSpearStick.LodgedIn.realizedObject;
				stuckInChunkIndex = abstractSpearStick.chunk;
				stuckBodyPart = abstractSpearStick.bodyPart;
				stuckRotation = abstractSpearStick.angle;
				ChangeMode(Mode.StuckInCreature);
			}
			else if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).LodgedIn.realizedObject != null)
			{
				AbstractPhysicalObject.AbstractSpearAppendageStick abstractSpearAppendageStick = abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick;
				stuckInObject = abstractSpearAppendageStick.LodgedIn.realizedObject;
				stuckInAppendage = new Appendage.Pos(stuckInObject.appendages[abstractSpearAppendageStick.appendage], abstractSpearAppendageStick.prevSeg, abstractSpearAppendageStick.distanceToNext);
				stuckRotation = abstractSpearAppendageStick.angle;
				ChangeMode(Mode.StuckInCreature);
			}
		}
	}

	public void PulledOutOfStuckObject()
	{
		for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
		{
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearStick).Spear == abstractPhysicalObject)
			{
				abstractPhysicalObject.stuckObjects[i].Deactivate();
				break;
			}
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.AbstractSpearAppendageStick && (abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.AbstractSpearAppendageStick).Spear == abstractPhysicalObject)
			{
				abstractPhysicalObject.stuckObjects[i].Deactivate();
				break;
			}
		}
		stuckInObject = null;
		stuckInAppendage = null;
		stuckInChunkIndex = 0;
	}

	public override void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
	{
		bool flag = false;
		if (obj is Creature)
		{
			flag = (obj as Creature).dead;
		}
		base.HitSomethingWithoutStopping(obj, chunk, appendage);
		if (obj is Fly)
		{
			if (!flag && Spear_NeedleCanFeed())
			{
				if (room.game.IsStorySession)
				{
					if (room.game.GetStorySession.playerSessionRecords != null)
					{
						room.game.GetStorySession.playerSessionRecords[((thrownBy as Player).abstractCreature.state as PlayerState).playerNumber].AddEat(obj);
					}
					(thrownBy as Player).AddQuarterFood();
				}
				else
				{
					(thrownBy as Player).AddFood(1);
				}
			}
			TryImpaleSmallCreature(obj as Creature);
		}
		if (!Spear_NeedleCanFeed())
		{
			return;
		}
		if (obj is Mushroom)
		{
			(thrownBy as Player).mushroomCounter += 320;
			obj.Destroy();
		}
		if (obj is KarmaFlower)
		{
			if ((thrownBy as Player).room.game.session is StoryGameSession && !((thrownBy as Player).room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma)
			{
				((thrownBy as Player).room.game.session as StoryGameSession).saveState.deathPersistentSaveData.reinforcedKarma = true;
				for (int i = 0; i < (thrownBy as Player).room.game.cameras.Length; i++)
				{
					if ((thrownBy as Player).room.game.cameras[i].followAbstractCreature == (thrownBy as Player).abstractCreature || ModManager.CoopAvailable)
					{
						(thrownBy as Player).room.game.cameras[i].hud.karmaMeter.reinforceAnimation = 0;
						break;
					}
				}
			}
			obj.Destroy();
		}
		if (!(obj is OracleSwarmer))
		{
			return;
		}
		room.PlaySound(SoundID.Centipede_Shock, obj.firstChunk.pos, 1f, 1.5f + UnityEngine.Random.value);
		if (room.game.IsStorySession)
		{
			if (room.game.GetStorySession.playerSessionRecords != null)
			{
				room.game.GetStorySession.playerSessionRecords[((thrownBy as Player).abstractCreature.state as PlayerState).playerNumber].AddEat(obj);
			}
			(thrownBy as Player).AddQuarterFood();
			(thrownBy as Player).AddQuarterFood();
		}
		else
		{
			(thrownBy as Player).AddFood(1);
		}
		(thrownBy as Player).glowing = true;
		if (room.game.session is StoryGameSession)
		{
			(room.game.session as StoryGameSession).saveState.theGlow = true;
		}
		Color color = Color.white;
		if (obj is SSOracleSwarmer)
		{
			color = Custom.HSL2RGB(((obj as SSOracleSwarmer).color.x > 0.5f) ? Custom.LerpMap((obj as SSOracleSwarmer).color.x, 0.5f, 1f, 2f / 3f, 0.99722224f) : (2f / 3f), 1f, Mathf.Lerp(0.75f, 0.9f, (obj as SSOracleSwarmer).color.y));
		}
		room.AddObject(new Spark(obj.firstChunk.pos, Custom.RNV() * 60f * UnityEngine.Random.value, color, null, 20, 50));
		obj.Destroy();
		foreach (AbstractCreature creature in room.abstractRoom.creatures)
		{
			if (creature != null && creature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.Inspector && creature.realizedCreature != null && thrownBy != null && (creature.realizedCreature as Inspector).AI.VisualContact(thrownBy.firstChunk) && (creature.realizedCreature as Inspector).AI.VisualContact(base.firstChunk))
			{
				Custom.Log("Inspector saw neuron eaten!");
				(creature.realizedCreature as Inspector).AI.preyTracker.AddPrey((creature.realizedCreature as Inspector).AI.tracker.RepresentationForCreature(thrownBy.abstractCreature, addIfMissing: true));
			}
		}
	}

	public void ProvideRotationBodyPart(BodyChunk chunk, BodyPart bodyPart)
	{
		stuckBodyPart = bodyPart.bodyPartArrayIndex;
		stuckRotation = Custom.Angle(base.firstChunk.vel, (bodyPart.pos - chunk.pos).normalized);
		bodyPart.vel += base.firstChunk.vel;
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		bool flag = false;
		if (abstractPhysicalObject.world.game.IsArenaSession && abstractPhysicalObject.world.game.GetArenaGameSession.GameTypeSetup.spearHitScore != 0 && thrownBy != null && thrownBy is Player && result.obj is Creature)
		{
			flag = true;
			if ((result.obj as Creature).State is HealthState && ((result.obj as Creature).State as HealthState).health <= 0f)
			{
				flag = false;
			}
			else if (!((result.obj as Creature).State is HealthState) && (result.obj as Creature).State.dead)
			{
				flag = false;
			}
		}
		bool flag2 = true;
		if (result.obj is Creature)
		{
			flag2 = (result.obj as Creature).dead;
			if (result.obj is EggBug && Spear_NeedleCanFeed() && !(result.obj as EggBug).FireBug)
			{
				(result.obj as EggBug).dropEggs = false;
			}
			if (!ModManager.MSC || !(result.obj is Player) || (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), result.chunk, result.onAppendagePos, base.firstChunk.vel))
			{
				float num = spearDamageBonus;
				if (ModManager.MSC && result.obj is Player && (result.obj as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && UnityEngine.Random.value < 0.15f)
				{
					num /= 10f;
					Custom.Log("GOURMAND SAVE!");
				}
				if (bugSpear)
				{
					num *= 3f;
				}
				(result.obj as Creature).Violence(base.firstChunk, base.firstChunk.vel * base.firstChunk.mass * 2f, result.chunk, result.onAppendagePos, Creature.DamageType.Stab, num, 20f);
				if (ModManager.MSC && result.obj is Player)
				{
					Player player = result.obj as Player;
					player.playerState.permanentDamageTracking += num / player.Template.baseDamageResistance;
					if (player.playerState.permanentDamageTracking >= 1.0)
					{
						player.Die();
					}
				}
			}
		}
		else if (result.chunk != null)
		{
			result.chunk.vel += base.firstChunk.vel * base.firstChunk.mass / result.chunk.mass;
		}
		else if (result.onAppendagePos != null)
		{
			(result.obj as IHaveAppendages).ApplyForceOnAppendage(result.onAppendagePos, base.firstChunk.vel * base.firstChunk.mass);
		}
		if (result.obj is Creature && (result.obj as Creature).SpearStick(this, Mathf.Lerp(0.55f, 0.62f, UnityEngine.Random.value), result.chunk, result.onAppendagePos, base.firstChunk.vel))
		{
			Creature creature = result.obj as Creature;
			if (ModManager.MSC)
			{
				if (Spear_NeedleCanFeed() && !flag2)
				{
					if (!(creature is GarbageWorm) && !(creature is Deer))
					{
						if (creature is Cicada || creature is TubeWorm || creature is Snail || creature is Leech || creature is VultureGrub || creature is SmallNeedleWorm || creature is Hazer || (creature is Centipede && (creature as Centipede).Small) || (creature is Vulture && result.onAppendagePos != null))
						{
							for (int i = 0; i < 2; i++)
							{
								(thrownBy as Player).AddQuarterFood();
							}
						}
						else if (creature is DaddyLongLegs && result.onAppendagePos != null)
						{
							(thrownBy as Player).AddQuarterFood();
						}
						else if (creature is EggBug)
						{
							(thrownBy as Player).AddFood(3);
						}
						else if (creature is BigSpider)
						{
							if ((creature as BigSpider).borrowedTime == -1)
							{
								(thrownBy as Player).AddFood(1);
							}
						}
						else
						{
							(thrownBy as Player).AddFood(1);
						}
						if (room.game.IsStorySession && room.game.GetStorySession.playerSessionRecords != null)
						{
							room.game.GetStorySession.playerSessionRecords[((thrownBy as Player).abstractCreature.state as PlayerState).playerNumber].AddEat(result.obj);
						}
					}
					Spear_NeedleDisconnect();
				}
				else
				{
					Spear_NeedleDisconnect();
				}
			}
			room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
			LodgeInCreature(result, eu);
			if (flag)
			{
				abstractPhysicalObject.world.game.GetArenaGameSession.PlayerLandSpear(thrownBy as Player, stuckInObject as Creature);
			}
			return true;
		}
		if (Spear_NeedleCanFeed() && result.obj is SeedCob)
		{
			if (room.game.IsStorySession && room.game.GetStorySession.playerSessionRecords != null)
			{
				room.game.GetStorySession.playerSessionRecords[((thrownBy as Player).abstractCreature.state as PlayerState).playerNumber].AddEat(result.obj);
			}
			(thrownBy as Player).AddFood(5);
			(result.obj as SeedCob).Open();
			Spear_NeedleDisconnect();
			room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
			LodgeInCreature(result, eu);
			return true;
		}
		if (Spear_NeedleCanFeed() && result.obj is JellyFish)
		{
			if (!(result.obj as JellyFish).dead)
			{
				(thrownBy as Player).AddQuarterFood();
				(thrownBy as Player).AddQuarterFood();
				if (room.game.IsStorySession && room.game.GetStorySession.playerSessionRecords != null)
				{
					room.game.GetStorySession.playerSessionRecords[((thrownBy as Player).abstractCreature.state as PlayerState).playerNumber].AddEat(result.obj);
				}
			}
			(result.obj as JellyFish).dead = true;
			Spear_NeedleDisconnect();
			room.PlaySound(SoundID.Spear_Stick_In_Creature, base.firstChunk);
			LodgeInCreature(result, eu, isJellyFish: true);
			return true;
		}
		room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, base.firstChunk);
		vibrate = 20;
		ChangeMode(Mode.Free);
		base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
		SetRandomSpin();
		return false;
	}

	public override void SetRandomSpin()
	{
		if (room != null)
		{
			rotationSpeed = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(50f, 150f, UnityEngine.Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
		}
		spinning = true;
	}

	public virtual void TryImpaleSmallCreature(Creature smallCrit)
	{
		if (ModManager.MMF && !MMF.cfgVanillaExploits.Value)
		{
			bool flag = false;
			foreach (AbstractPhysicalObject.AbstractObjectStick stuckObject in abstractPhysicalObject.stuckObjects)
			{
				if (stuckObject.A == smallCrit.abstractCreature || stuckObject.B == smallCrit.abstractCreature)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return;
			}
			while (smallCrit.grabbedBy.Count > 0)
			{
				smallCrit.grabbedBy[0].Release();
			}
			smallCrit.abstractCreature.LoseAllStuckObjects();
		}
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < abstractPhysicalObject.stuckObjects.Count; i++)
		{
			if (abstractPhysicalObject.stuckObjects[i] is AbstractPhysicalObject.ImpaledOnSpearStick)
			{
				if ((abstractPhysicalObject.stuckObjects[i] as AbstractPhysicalObject.ImpaledOnSpearStick).onSpearPosition == num2)
				{
					num2++;
				}
				num++;
			}
		}
		if (num <= 5 && num2 < 5)
		{
			new AbstractPhysicalObject.ImpaledOnSpearStick(abstractPhysicalObject, smallCrit.abstractCreature, 0, num2);
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (stuckIns != null)
		{
			rCam.ReturnFContainer("HUD").AddChild(stuckIns.label);
		}
		if (IsNeedle)
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("BioSpear" + (spearmasterNeedleType % 3 + 1));
		}
		else if (bugSpear)
		{
			sLeaser.sprites = new FSprite[2];
			sLeaser.sprites[1] = new FSprite("FireBugSpear");
			sLeaser.sprites[0] = new FSprite("FireBugSpearColor");
		}
		else
		{
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("SmallSpear");
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (stuckIns != null && room != null)
		{
			if (room.game.devToolsActive && Input.GetKeyDown("l"))
			{
				sLeaser.RemoveAllSpritesFromContainer();
				InitiateSprites(sLeaser, rCam);
				ApplyPalette(sLeaser, rCam, rCam.currentPalette);
			}
			if (stuckIns.relativePos)
			{
				stuckIns.label.x = base.bodyChunks[0].pos.x + stuckIns.pos.x - camPos.x;
				stuckIns.label.y = base.bodyChunks[0].pos.y + stuckIns.pos.y - camPos.y;
			}
			else
			{
				stuckIns.label.x = stuckIns.pos.x;
				stuckIns.label.y = stuckIns.pos.y;
			}
		}
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		if (vibrate > 0)
		{
			vector += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f * UnityEngine.Random.value;
		}
		Vector3 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		for (int num = ((this is ExplosiveSpear || bugSpear) ? 1 : 0); num >= 0; num--)
		{
			sLeaser.sprites[num].x = vector.x - camPos.x;
			sLeaser.sprites[num].y = vector.y - camPos.y;
			sLeaser.sprites[num].anchorY = Mathf.Lerp(lastPivotAtTip ? 0.85f : 0.5f, pivotAtTip ? 0.85f : 0.5f, timeStacker);
			sLeaser.sprites[num].rotation = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), vector2);
		}
		if (bugSpear)
		{
			if (blink > 0 && UnityEngine.Random.value < 0.5f)
			{
				sLeaser.sprites[1].color = base.blinkColor;
			}
			else
			{
				sLeaser.sprites[1].color = color;
			}
		}
		else if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[0].color = base.blinkColor;
		}
		else if (IsNeedle)
		{
			float num2 = (float)spearmasterNeedle_fadecounter / (float)spearmasterNeedle_fadecounter_max;
			if (spearmasterNeedle_hasConnection)
			{
				num2 = 1f;
			}
			if (num2 < 0.01f)
			{
				num2 = 0.01f;
			}
			if (ModManager.CoopAvailable && jollyCustomColor.HasValue)
			{
				sLeaser.sprites[0].color = jollyCustomColor.Value;
			}
			else if (PlayerGraphics.CustomColorsEnabled())
			{
				sLeaser.sprites[0].color = Color.Lerp(PlayerGraphics.CustomColorSafety(2), color, 1f - num2);
			}
			else
			{
				sLeaser.sprites[0].color = Color.Lerp(new Color(1f, 1f, 1f, 1f), color, 1f - num2);
			}
			sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("BioSpear" + (spearmasterNeedleType % 3 + 1));
		}
		else
		{
			sLeaser.sprites[0].color = color;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = palette.blackColor;
		if (bugSpear)
		{
			sLeaser.sprites[1].color = color;
			sLeaser.sprites[0].color = Custom.HSL2RGB(Custom.Decimal(abstractSpear.hue + EggBugGraphics.HUE_OFF), 1f, 0.5f);
		}
		else
		{
			sLeaser.sprites[0].color = color;
		}
	}

	public bool Spear_NeedleCanFeed()
	{
		if (!ModManager.MSC)
		{
			return false;
		}
		bool flag = false;
		if (thrownBy != null && thrownBy is Player)
		{
			flag = (thrownBy as Player).SlugCatClass == MoreSlugcatsEnums.SlugcatStatsName.Spear;
		}
		if (flag && spearmasterNeedle)
		{
			return spearmasterNeedle_hasConnection;
		}
		return false;
	}

	public void Spear_NeedleDisconnect()
	{
		spearmasterNeedle_hasConnection = false;
	}

	public void Spear_makeNeedle(int type, bool active)
	{
		spearmasterNeedle = true;
		spearmasterNeedleType = type;
		spearmasterNeedle_hasConnection = active;
		if (!active)
		{
			spearmasterNeedle_fadecounter = 0;
		}
		abstractSpear.needle = true;
	}

	public void resetHorizontalBeamState()
	{
		if (!stuckInWall.HasValue)
		{
			hasHorizontalBeamState = false;
			abstractSpear.stuckInWallCycles = 0;
			return;
		}
		if (abstractSpear.stuckInWallCycles > 0)
		{
			room.GetTile(stuckInWall.Value).horizontalBeam = wasHorizontalBeam[1];
			for (int i = -1; i < 2; i += 2)
			{
				if (!room.GetTile(stuckInWall.Value + new Vector2(20f * (float)i, 0f)).Solid)
				{
					room.GetTile(stuckInWall.Value + new Vector2(20f * (float)i, 0f)).horizontalBeam = wasHorizontalBeam[i + 1];
				}
			}
		}
		else
		{
			room.GetTile(stuckInWall.Value).verticalBeam = wasHorizontalBeam[1];
			for (int j = -1; j < 2; j += 2)
			{
				if (!room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)j)).Solid)
				{
					room.GetTile(stuckInWall.Value + new Vector2(0f, 20f * (float)j)).verticalBeam = wasHorizontalBeam[j + 1];
				}
			}
		}
		hasHorizontalBeamState = false;
		abstractSpear.stuckInWallCycles = 0;
	}
}
