using System;
using System.Globalization;
using Noise;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class FireEgg : PlayerCarryableItem, IDrawable, IPlayerEdible
{
	public class AbstractBugEgg : AbstractPhysicalObject
	{
		public float hue;

		public bool stuckInWall;

		public AbstractBugEgg(World world, PhysicalObject obj, WorldCoordinate pos, EntityID ID, float hue)
			: base(world, MoreSlugcatsEnums.AbstractObjectType.FireEgg, obj, pos, ID)
		{
			this.hue = hue;
		}

		public override string ToString()
		{
			string baseString = string.Format(CultureInfo.InvariantCulture, "{0}<oA>{1}<oA>{2}<oA>{3}<oA>{4}", ID.ToString(), type.ToString(), pos.SaveToString(), hue, stuckInWall ? "1" : "0");
			baseString = SaveState.SetCustomData(this, baseString);
			return SaveUtils.AppendUnrecognizedStringAttrs(baseString, "<oA>", unrecognizedAttributes);
		}
	}

	public Vector2 rotation;

	public Vector2 lastRotation;

	public Vector2? setRotation;

	public float darkness;

	public float lastDarkness;

	private Vector2[,] segments;

	public float swell;

	public float liquid;

	public float liquidDeplete;

	public float rotVel;

	private Color[] eggColors;

	private Color blackColor;

	public int bites;

	public int activeCounter;

	public Creature thrownBy;

	public static int explodeDuration = 180;

	public PhysicalObject stuckInObject;

	private int stuckInChunkIndex;

	public Vector2? stuckInWall;

	public Weapon.Mode mode;

	public float SwellFac => 1f + 0.15f * swell;

	public AbstractBugEgg abstractBugEgg => abstractPhysicalObject as AbstractBugEgg;

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public BodyChunk stuckInChunk => stuckInObject.bodyChunks[stuckInChunkIndex];

	public FireEgg(AbstractPhysicalObject abstractPhysicalObject)
		: base(abstractPhysicalObject)
	{
		swell = 1f;
		bites = 2;
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 4f * SwellFac, 0.2f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.2f;
		surfaceFriction = 0.7f;
		collisionLayer = 0;
		base.waterFriction = 0.95f;
		base.buoyancy = 1.1f;
		liquidDeplete = Mathf.Lerp(40f, 80f, UnityEngine.Random.value);
		segments = new Vector2[3, 3];
		ChangeMode(Weapon.Mode.Free);
		stuckInWall = null;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		base.bodyChunks[0].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile));
		rotation = Custom.RNV();
		lastRotation = rotation;
		ResetSegments();
		if (abstractBugEgg.stuckInWall)
		{
			stuckInWall = placeRoom.MiddleOfTile(abstractPhysicalObject.pos.Tile);
			ChangeMode(Weapon.Mode.StuckInWall);
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetSegments();
	}

	private void ResetSegments()
	{
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			segments[i, 0] = base.firstChunk.pos + rotation * i;
			segments[i, 1] = segments[i, 0];
			segments[i, 2] *= 0f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (mode == Weapon.Mode.Free && collisionLayer != 1)
		{
			ChangeCollisionLayer(0);
		}
		else if (mode != Weapon.Mode.Free && collisionLayer != 2)
		{
			ChangeCollisionLayer(2);
		}
		if (activeCounter > 0)
		{
			activeCounter++;
		}
		if (activeCounter == explodeDuration)
		{
			Explode();
		}
		base.firstChunk.rad = 4f * SwellFac;
		if (room.game.devToolsActive && Input.GetKey("b"))
		{
			base.firstChunk.vel += Custom.DirVec(base.firstChunk.pos, Input.mousePosition) * 3f;
		}
		lastRotation = rotation;
		if (activeCounter > 0)
		{
			swell = Custom.LerpAndTick(swell, 1f, 0.01f, UnityEngine.Random.value / 80f);
		}
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		if (!stuckInWall.HasValue)
		{
			stuckInWall = null;
		}
		if (base.firstChunk.ContactPoint.y < 0 && mode == Weapon.Mode.Free)
		{
			rotVel = Mathf.Lerp(rotVel, 0.12f * base.firstChunk.vel.x, 0.8f);
			BodyChunk bodyChunk = base.firstChunk;
			bodyChunk.vel.x = bodyChunk.vel.x * 0.8f;
		}
		if (rotVel > 0f)
		{
			rotation = (rotation - Custom.PerpendicularVector(rotation) * rotVel).normalized;
		}
		for (int i = 0; i < segments.GetLength(0); i++)
		{
			float value = (float)i / (float)(segments.GetLength(0) - 1);
			segments[i, 1] = segments[i, 0];
			segments[i, 0] += segments[i, 2] * 0.5f;
			segments[i, 2] *= 0.995f;
			segments[i, 2].y -= 0.9f * Mathf.InverseLerp(0.5f, 1f, value);
			segments[i, 2] += rotation * 1f * Mathf.InverseLerp(0.5f, 0f, value);
			if (i > 1)
			{
				segments[i, 2] += Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
				segments[i - 2, 2] -= Custom.DirVec(segments[i - 2, 0], segments[i, 0]);
			}
			ConnectSegment(i);
		}
		for (int num = segments.GetLength(0) - 1; num >= 0; num--)
		{
			ConnectSegment(num);
		}
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			ConnectSegment(j);
		}
		if (liquid > 0f && eggColors != null)
		{
			liquid -= 1f / liquidDeplete;
			if (UnityEngine.Random.value < 0.25f && UnityEngine.Random.value < Mathf.InverseLerp(0f, 0.5f, liquid))
			{
				if (bites < 2)
				{
					room.AddObject(new EggBugEgg.LiquidDrip(base.firstChunk.pos, rotation * 3f + Custom.RNV() * UnityEngine.Random.value * 3f, Color.Lerp(eggColors[1], blackColor, 0.4f)));
					return;
				}
				Vector2 vel = Custom.DirVec(segments[segments.GetLength(0) - 2, 0], segments[segments.GetLength(0) - 1, 0]) * Mathf.Lerp(8f, 1f, liquid);
				vel += segments[segments.GetLength(0) - 1, 2];
				vel += Custom.RNV() * 4f * UnityEngine.Random.value;
				room.AddObject(new EggBugEgg.LiquidDrip(Vector2.Lerp(segments[segments.GetLength(0) - 1, 1], segments[segments.GetLength(0) - 1, 0], UnityEngine.Random.value), vel, Color.Lerp(eggColors[1], blackColor, 0.4f)));
			}
		}
		if (mode == Weapon.Mode.Free && grabbedBy.Count == 0 && activeCounter > 0)
		{
			base.firstChunk.lastLastPos = base.firstChunk.lastPos;
			base.firstChunk.lastPos = base.firstChunk.pos;
			Vector2 pos = base.firstChunk.pos;
			Vector2 vector = base.firstChunk.pos + base.firstChunk.vel;
			FloatRect? floatRect = SharedPhysics.ExactTerrainRayTrace(room, pos, vector);
			Vector2 vector2 = default(Vector2);
			if (floatRect.HasValue)
			{
				vector2 = new Vector2(floatRect.Value.left, floatRect.Value.bottom);
			}
			Vector2 pos2 = vector;
			SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, pos, ref pos2, 1f, 1, thrownBy, hitAppendages: false);
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
					stuckInWall = vector2 + Custom.DirVec(vector, pos) * 15f;
					ChangeMode(Weapon.Mode.StuckInWall);
					if (room.BeingViewed)
					{
						for (int k = 0; k < 8; k++)
						{
							room.AddObject(new WaterDrip(stuckInWall.Value, -base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
						}
					}
				}
			}
			else if (collisionResult.chunk != null && collisionResult.chunk.owner is Creature && collisionResult.chunk.owner != thrownBy)
			{
				stuckInObject = collisionResult.chunk.owner;
				ChangeMode(Weapon.Mode.StuckInCreature);
				stuckInChunkIndex = collisionResult.chunk.index;
				if (room.BeingViewed)
				{
					for (int l = 0; l < 8; l++)
					{
						room.AddObject(new WaterDrip(collisionResult.chunk.owner.bodyChunks[stuckInChunkIndex].pos, -base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
					}
				}
			}
		}
		if (mode == Weapon.Mode.StuckInCreature)
		{
			rotVel = 0f;
			if (stuckInChunk.owner is Creature && (stuckInChunk.owner as Creature).enteringShortCut.HasValue)
			{
				stuckInWall = null;
				ChangeMode(Weapon.Mode.Free);
				return;
			}
			if (!stuckInWall.HasValue)
			{
				base.firstChunk.vel = stuckInChunk.vel;
				base.firstChunk.MoveWithOtherObject(eu, stuckInChunk, new Vector2(0f, 0f));
				setRotation = Custom.DegToVec(Custom.VecToDeg(stuckInChunk.Rotation));
			}
			else
			{
				base.firstChunk.vel *= 0f;
				base.firstChunk.pos = stuckInWall.Value;
				stuckInChunk.MoveFromOutsideMyUpdate(eu, stuckInWall.Value);
				stuckInChunk.vel *= 0f;
			}
			if (stuckInChunk.owner.slatedForDeletetion)
			{
				ChangeMode(Weapon.Mode.Free);
			}
		}
		else if (mode == Weapon.Mode.StuckInWall)
		{
			base.firstChunk.pos = stuckInWall.Value;
			base.firstChunk.vel *= 0f;
			rotVel = 0f;
		}
	}

	private void ConnectSegment(int i)
	{
		if (i == 0)
		{
			Vector2 vector = base.firstChunk.pos + rotation * 7f * SwellFac;
			Vector2 vector2 = Custom.DirVec(segments[i, 0], vector);
			float num = Vector2.Distance(segments[i, 0], vector);
			segments[i, 0] -= vector2 * (2f - num);
			segments[i, 2] -= vector2 * (2f - num);
		}
		else
		{
			Vector2 vector3 = Custom.DirVec(segments[i, 0], segments[i - 1, 0]);
			float num2 = Vector2.Distance(segments[i, 0], segments[i - 1, 0]);
			segments[i, 0] -= vector3 * (2f - num2) * 0.5f;
			segments[i, 2] -= vector3 * (2f - num2) * 0.5f;
			segments[i - 1, 0] += vector3 * (2f - num2) * 0.5f;
			segments[i - 1, 2] += vector3 * (2f - num2) * 0.5f;
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[4];
		sLeaser.sprites[0] = TriangleMesh.MakeLongMesh(segments.GetLength(0), pointyTip: false, customColor: false);
		sLeaser.sprites[1] = new FSprite("LegsAPole");
		sLeaser.sprites[2] = new FSprite("JetFishEyeB");
		sLeaser.sprites[3] = new FSprite("Futile_White");
		sLeaser.sprites[3].shader = rCam.game.rainWorld.Shaders["WaterNut"];
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = Vector2.Lerp(base.firstChunk.lastPos, base.firstChunk.pos, timeStacker);
		Vector2 vector2 = Vector3.Slerp(lastRotation, rotation, timeStacker);
		vector -= vector2 * 3f * SwellFac;
		lastDarkness = darkness;
		darkness = rCam.room.Darkness(vector) * (1f - rCam.room.LightSourceExposure(vector));
		if (darkness != lastDarkness)
		{
			ApplyPalette(sLeaser, rCam, rCam.currentPalette);
		}
		float num = Custom.LerpQuadEaseIn(0f, 1f, (float)activeCounter / (float)explodeDuration);
		for (int i = 0; i < 3; i++)
		{
			sLeaser.sprites[i + 1].x = vector.x - camPos.x;
			sLeaser.sprites[i + 1].y = vector.y - camPos.y;
			sLeaser.sprites[i + 1].rotation = Custom.VecToDeg(vector2) + Mathf.Sin((float)activeCounter / 27f * (float)Math.PI) * num * 80f;
		}
		sLeaser.sprites[2].x += vector2.x * 5f;
		sLeaser.sprites[2].y += vector2.y * 5f;
		float num2 = 1f;
		sLeaser.sprites[2].scaleX = num2 * SwellFac;
		sLeaser.sprites[2].scaleY = num2 * SwellFac;
		sLeaser.sprites[1].scaleX = num2 * SwellFac + Mathf.Abs(Mathf.Sin((float)activeCounter / 40f * (float)Math.PI) * num * 0.95f);
		sLeaser.sprites[1].scaleY = num2 * SwellFac + Mathf.Abs(Mathf.Cos((float)activeCounter / 40f * (float)Math.PI) * num * 0.95f);
		sLeaser.sprites[3].scale = 0.85f * num2 * SwellFac + 0.6f * num;
		if (bites < 2)
		{
			sLeaser.sprites[0].isVisible = false;
		}
		if (blink > 0 && UnityEngine.Random.value < 0.5f)
		{
			sLeaser.sprites[1].color = new Color(1f, 1f, 1f);
		}
		else if (activeCounter > 0)
		{
			float num3 = Mathf.Abs(Mathf.Sin((float)activeCounter / Mathf.Max(1f, 80f * (1f - num * 0.8f)) * (float)Math.PI)) * 0.45f;
			sLeaser.sprites[1].color = new Color(eggColors[1].r + (1f - eggColors[1].r) * num3, eggColors[1].g + (1f - eggColors[1].g) * num3, eggColors[1].b + (1f - eggColors[1].b) * num3);
		}
		else
		{
			sLeaser.sprites[1].color = eggColors[1];
		}
		Vector2 vector3 = vector + vector2 * 5f * SwellFac;
		float num4 = 1f;
		for (int j = 0; j < segments.GetLength(0); j++)
		{
			float f = (float)j / (float)(segments.GetLength(0) - 1);
			Vector2 vector4 = Vector2.Lerp(segments[j, 1], segments[j, 0], timeStacker);
			Vector2 normalized = (vector4 - vector3).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num5 = Mathf.Lerp(1f, 0.5f, Mathf.Pow(f, 0.25f));
			float num6 = Vector2.Distance(vector4, vector3) / 5f;
			if (j == 0)
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * num5 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * num5 - camPos);
			}
			else
			{
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * (num5 + num4) * 0.5f + normalized * num6 - camPos);
				(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * (num5 + num4) * 0.5f + normalized * num6 - camPos);
			}
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - vector5 * num5 - normalized * num6 - camPos);
			(sLeaser.sprites[0] as TriangleMesh).MoveVertice(j * 4 + 3, vector4 + vector5 * num5 - normalized * num6 - camPos);
			vector3 = vector4;
			num4 = num5;
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		blackColor = palette.blackColor;
		eggColors = EggBugGraphics.FireEggColors(palette, abstractBugEgg.hue, darkness);
		sLeaser.sprites[0].color = eggColors[0];
		sLeaser.sprites[1].color = eggColors[1];
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

	public void BitByPlayer(Creature.Grasp grasp, bool eu)
	{
		bites--;
		room.PlaySound((bites != 0) ? SoundID.Slugcat_Bite_Dangle_Fruit : SoundID.Slugcat_Eat_Dangle_Fruit, base.firstChunk.pos);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		liquid = 1f;
		if (bites < 1)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			Destroy();
		}
	}

	public void ThrowByPlayer()
	{
	}

	public void Explode()
	{
		if (base.slatedForDeletetion)
		{
			return;
		}
		Vector2 vector = Vector2.Lerp(base.firstChunk.pos, base.firstChunk.lastPos, 0.35f);
		Color color = Custom.HSL2RGB(abstractBugEgg.hue, 1f, 0.75f);
		room.AddObject(new Explosion(room, this, vector, 7, 225f, 4.2f, 50f, 280f, 0.25f, thrownBy, 0.7f, 160f, 1f));
		room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, color));
		room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
		room.AddObject(new ExplosionSpikes(room, vector, 14, 30f, 9f, 7f, 170f, color));
		room.AddObject(new ShockWave(vector, 240f, 0.045f, 5));
		for (int i = 0; i < 10; i++)
		{
			Vector2 vector2 = Custom.RNV();
			if (room.GetTile(vector + vector2 * 20f).Solid)
			{
				if (!room.GetTile(vector - vector2 * 20f).Solid)
				{
					vector2 *= -1f;
				}
				else
				{
					vector2 = Custom.RNV();
				}
			}
			for (int j = 0; j < 3; j++)
			{
				room.AddObject(new Spark(vector + vector2 * Mathf.Lerp(30f, 60f, UnityEngine.Random.value), vector2 * Mathf.Lerp(7f, 38f, UnityEngine.Random.value) + Custom.RNV() * 20f * UnityEngine.Random.value, Color.Lerp(color, new Color(1f, 1f, 1f), UnityEngine.Random.value), null, 11, 28));
			}
			room.AddObject(new Explosion.FlashingSmoke(vector + vector2 * 40f * UnityEngine.Random.value, vector2 * Mathf.Lerp(4f, 20f, Mathf.Pow(UnityEngine.Random.value, 2f)), 1f + 0.05f * UnityEngine.Random.value, new Color(1f, 1f, 1f), color, UnityEngine.Random.Range(3, 11)));
		}
		for (int k = 0; k < abstractPhysicalObject.stuckObjects.Count; k++)
		{
			abstractPhysicalObject.stuckObjects[k].Deactivate();
		}
		room.PlaySound(SoundID.Bomb_Explode, vector);
		room.InGameNoise(new InGameNoise(vector, 9000f, this, 1f));
		Destroy();
	}

	public void Tossed(Creature tosser)
	{
		thrownBy = tosser;
		if (activeCounter == 0)
		{
			activeCounter = 1;
		}
	}

	public void ChangeMode(Weapon.Mode newMode)
	{
		if (newMode == mode)
		{
			return;
		}
		if (mode == Weapon.Mode.StuckInCreature)
		{
			if (room != null)
			{
				room.PlaySound(SoundID.Spear_Dislodged_From_Creature, base.firstChunk);
			}
			PulledOutOfStuckObject();
		}
		if (newMode == Weapon.Mode.StuckInWall)
		{
			abstractBugEgg.stuckInWall = true;
			stuckInWall = room.MiddleOfTile(stuckInWall.Value);
		}
		if (newMode != Weapon.Mode.StuckInWall && newMode != Weapon.Mode.StuckInCreature)
		{
			stuckInWall = null;
		}
		if (newMode == Weapon.Mode.Thrown || newMode == Weapon.Mode.StuckInWall)
		{
			ChangeCollisionLayer(0);
		}
		else
		{
			ChangeCollisionLayer(1);
		}
		mode = newMode;
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
				ChangeMode(Weapon.Mode.StuckInCreature);
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
		}
		stuckInObject = null;
		stuckInChunkIndex = 0;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		base.TerrainImpact(chunk, direction, speed, firstContact);
		if (!firstContact || !(mode == Weapon.Mode.Free) || activeCounter <= 0 || grabbedBy.Count != 0)
		{
			return;
		}
		stuckInWall = base.firstChunk.pos;
		ChangeMode(Weapon.Mode.StuckInWall);
		if (room.BeingViewed)
		{
			for (int i = 0; i < 8; i++)
			{
				room.AddObject(new WaterDrip(stuckInWall.Value, -base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		if (!(otherObject is Creature) || !(mode == Weapon.Mode.Free) || grabbedBy.Count != 0 || activeCounter <= 0 || otherObject == thrownBy)
		{
			return;
		}
		stuckInObject = otherObject;
		ChangeMode(Weapon.Mode.StuckInCreature);
		stuckInChunkIndex = otherChunk;
		if (room.BeingViewed)
		{
			for (int i = 0; i < 8; i++)
			{
				room.AddObject(new WaterDrip(otherObject.bodyChunks[otherChunk].pos, -base.firstChunk.vel * UnityEngine.Random.value * 0.5f + Custom.DegToVec(360f * UnityEngine.Random.value) * base.firstChunk.vel.magnitude * UnityEngine.Random.value * 0.5f, waterColor: false));
			}
		}
	}

	public override void Grabbed(Creature.Grasp grasp)
	{
		base.Grabbed(grasp);
		ChangeMode(Weapon.Mode.Free);
	}
}
