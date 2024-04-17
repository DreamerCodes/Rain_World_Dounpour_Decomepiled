using System;
using RWCustom;
using UnityEngine;

public abstract class Weapon : PlayerCarryableItem, IDrawable, SharedPhysics.IProjectileTracer
{
	public class Mode : ExtEnum<Mode>
	{
		public static readonly Mode Free = new Mode("Free", register: true);

		public static readonly Mode Frozen = new Mode("Frozen", register: true);

		public static readonly Mode Thrown = new Mode("Thrown", register: true);

		public static readonly Mode Carried = new Mode("Carried", register: true);

		public static readonly Mode StuckInCreature = new Mode("StuckInCreature", register: true);

		public static readonly Mode StuckInWall = new Mode("StuckInWall", register: true);

		public static readonly Mode OnBack = new Mode("OnBack", register: true);

		public Mode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public interface INotifyOfFlyingWeapons
	{
		void FlyingWeapon(Weapon weapon);
	}

	public Mode lastMode;

	protected Vector2 tailPos;

	public Creature thrownBy;

	protected IntVector2 throwDir;

	public int changeDirCounter;

	protected int vibrate;

	protected Vector2 lastRotation;

	public Vector2 rotation;

	public float rotationSpeed;

	public Vector2? setRotation;

	public Vector2 thrownPos;

	public Vector2? firstFrameTraceFromPos;

	public Creature thrownClosestToCreature;

	public float closestCritDist;

	public BodyChunk meleeHitChunk;

	private int inFrontOfObjects;

	public float exitThrownModeSpeed;

	public int throwModeFrames = -1;

	public int floorBounceFrames;

	protected DynamicSoundLoop soundLoop;

	public float overrideExitThrownSpeed;

	public bool doNotTumbleAtLowSpeed;

	public virtual int DefaultCollLayer => 2;

	public Mode mode { get; set; }

	public virtual bool HeavyWeapon => false;

	public Weapon(AbstractPhysicalObject abstractPhysicalObject, World world)
		: base(abstractPhysicalObject)
	{
		mode = Mode.Free;
		rotation = Custom.DegToVec(UnityEngine.Random.value * 360f);
		lastRotation = rotation;
		rotationSpeed = 0f;
		inFrontOfObjects = -1;
		exitThrownModeSpeed = 30f;
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
		NewRoom(placeRoom);
		SetRandomSpin();
		inFrontOfObjects = -1;
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		inFrontOfObjects = -1;
	}

	public void ChangeOverlap(bool newOverlap)
	{
		if (inFrontOfObjects != (newOverlap ? 1 : 0) && room != null)
		{
			for (int i = 0; i < room.game.cameras.Length; i++)
			{
				room.game.cameras[i].MoveObjectToContainer(this, room.game.cameras[i].ReturnFContainer(newOverlap ? "Items" : "Background"));
			}
			inFrontOfObjects = (newOverlap ? 1 : 0);
		}
	}

	public virtual void ChangeMode(Mode newMode)
	{
		if (!(newMode == mode))
		{
			if (newMode == Mode.Thrown || newMode == Mode.StuckInWall)
			{
				ChangeCollisionLayer(0);
			}
			else
			{
				ChangeCollisionLayer(DefaultCollLayer);
			}
			if (newMode != Mode.Thrown)
			{
				throwModeFrames = -1;
				firstFrameTraceFromPos = null;
			}
			base.firstChunk.collideWithObjects = newMode != Mode.Carried;
			base.firstChunk.collideWithTerrain = newMode == Mode.Free || newMode == Mode.Thrown;
			base.firstChunk.goThroughFloors = true;
			if (newMode == Mode.Free)
			{
				SetRandomSpin();
			}
			if (closestCritDist < 120f && mode == Mode.Thrown && thrownBy != null && thrownClosestToCreature != null)
			{
				room.socialEventRecognizer.WeaponAttack(this, thrownBy, thrownClosestToCreature, hit: false);
			}
			thrownClosestToCreature = null;
			closestCritDist = float.MaxValue;
			mode = newMode;
		}
	}

	public override void Update(bool eu)
	{
		lastMode = mode;
		float num = exitThrownModeSpeed;
		if (overrideExitThrownSpeed > 0f)
		{
			num = overrideExitThrownSpeed;
		}
		if (vibrate > 0)
		{
			vibrate--;
		}
		if (floorBounceFrames > 0 && throwDir.y == 0)
		{
			floorBounceFrames--;
			if (base.firstChunk.ContactPoint.y != 0 && Mathf.Abs(base.firstChunk.vel.x) > 5f)
			{
				base.firstChunk.vel.y = (Mathf.Abs(base.firstChunk.vel.y) + 1f) * (float)(-base.firstChunk.ContactPoint.y) + ((base.firstChunk.ContactPoint.y < 0) ? 4.5f : 0f);
				for (int i = 0; i < 4; i++)
				{
					room.AddObject(new Spark(new Vector2(base.firstChunk.pos.x, room.MiddleOfTile(base.firstChunk.pos).y + (float)base.firstChunk.ContactPoint.y * 10f), base.firstChunk.vel * UnityEngine.Random.value + Custom.RNV() * UnityEngine.Random.value * 4f - base.firstChunk.ContactPoint.ToVector2() * 4f * UnityEngine.Random.value, new Color(1f, 1f, 1f), null, 6, 18));
				}
				room.PlaySound(SoundID.Weapon_Skid, base.firstChunk.pos, 1f, 1f);
				base.firstChunk.vel.x = Mathf.Max(num + 1f, Mathf.Abs(base.firstChunk.vel.x)) * Mathf.Sign(base.firstChunk.vel.x);
				floorBounceFrames /= 2;
			}
		}
		lastRotation = rotation;
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		else
		{
			float num2 = Custom.AimFromOneVectorToAnother(new Vector2(0f, 0f), rotation);
			num2 += rotationSpeed;
			rotation = Custom.DegToVec(num2);
		}
		if (abstractPhysicalObject.stuckObjects.Count > 1)
		{
			int num3 = 0;
			int num4 = -1;
			for (int j = 0; j < abstractPhysicalObject.stuckObjects.Count; j++)
			{
				if (!(abstractPhysicalObject.stuckObjects[j] is AbstractPhysicalObject.ImpaledOnSpearStick))
				{
					num3++;
					if (abstractPhysicalObject.stuckObjects[j] is AbstractPhysicalObject.CreatureGripStick)
					{
						num4 = j;
					}
				}
			}
			if (num3 > 1)
			{
				int index = 1;
				if (num4 > -1)
				{
					if (abstractPhysicalObject.stuckObjects[num4].A.realizedObject == null)
					{
						index = num4;
					}
					else
					{
						bool flag = false;
						int num5 = 0;
						while (!flag && num5 < (abstractPhysicalObject.stuckObjects[num4].A.realizedObject as Creature).grasps.Length)
						{
							if ((abstractPhysicalObject.stuckObjects[num4].A.realizedObject as Creature).grasps[num5] != null && (abstractPhysicalObject.stuckObjects[num4].A.realizedObject as Creature).grasps[num5].grabbed == this)
							{
								flag = true;
							}
							num5++;
						}
						if (!flag)
						{
							index = num4;
						}
						else
						{
							switch (num4)
							{
							case 0:
								index = 1;
								break;
							case 1:
								index = 0;
								break;
							}
						}
					}
				}
				abstractPhysicalObject.stuckObjects[index].Deactivate();
				Custom.Log($"{abstractPhysicalObject.type} stuck in several things. Deactivate one");
			}
		}
		waterRetardationImmunity = ((mode == Mode.Thrown) ? 0.9f : 0f);
		tailPos = base.firstChunk.lastPos;
		if (mode == Mode.Carried)
		{
			if (grabbedBy.Count < 1)
			{
				ChangeMode(Mode.Free);
			}
		}
		else if (mode == Mode.Free)
		{
			if (base.firstChunk.submersion > 0f && Mathf.Abs(rotationSpeed) > 5f)
			{
				rotationSpeed *= 0.7f;
			}
		}
		else if (mode == Mode.Thrown)
		{
			if (thrownBy != null && Mathf.Abs(base.firstChunk.vel.x) > 0.5f)
			{
				for (int k = 0; k < room.abstractRoom.creatures.Count; k++)
				{
					if (room.abstractRoom.creatures[k].realizedCreature == null || room.abstractRoom.creatures[k].realizedCreature == thrownBy || room.abstractRoom.creatures[k].realizedCreature.room != room)
					{
						continue;
					}
					for (int l = 0; l < room.abstractRoom.creatures[k].realizedCreature.bodyChunks.Length; l++)
					{
						if (Custom.InRange(room.abstractRoom.creatures[k].realizedCreature.bodyChunks[l].pos.x, thrownPos.x - Mathf.Sign(base.firstChunk.vel.x) * (20f + room.abstractRoom.creatures[k].realizedCreature.bodyChunks[l].rad), thrownPos.x + Mathf.Sign(base.firstChunk.vel.x) * 2000f) && Custom.DistLess(base.firstChunk.pos, room.abstractRoom.creatures[k].realizedCreature.bodyChunks[l].pos, closestCritDist * (room.abstractRoom.creatures[k].creatureTemplate.quantified ? 2f : 1f)))
						{
							thrownClosestToCreature = room.abstractRoom.creatures[k].realizedCreature;
							closestCritDist = Vector2.Distance(base.firstChunk.pos, room.abstractRoom.creatures[k].realizedCreature.bodyChunks[l].pos) * (room.abstractRoom.creatures[k].creatureTemplate.quantified ? 2f : 1f);
						}
					}
					if (room.abstractRoom.creatures[k].realizedCreature is INotifyOfFlyingWeapons)
					{
						(room.abstractRoom.creatures[k].realizedCreature as INotifyOfFlyingWeapons).FlyingWeapon(this);
					}
				}
			}
			if (changeDirCounter > 0 && thrownBy is Player && (thrownBy as Player).ThrowDirection == -throwDir.x)
			{
				changeDirCounter = 0;
				throwDir = new IntVector2((thrownBy as Player).ThrowDirection, 0);
				base.firstChunk.vel.x = Mathf.Abs(base.firstChunk.vel.x) * (float)throwDir.x;
				rotation = (rotation + Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.1f).normalized;
				setRotation = Custom.DegToVec((base.firstChunk.vel.x < 0f) ? 270 : 90);
			}
			changeDirCounter--;
			if (throwModeFrames > 0)
			{
				throwModeFrames--;
			}
			if ((base.firstChunk.vel.magnitude < num && !doNotTumbleAtLowSpeed) || throwModeFrames == 0)
			{
				SetRandomSpin();
				ChangeMode(Mode.Free);
				base.forbiddenToPlayer = 10;
			}
			if (floorBounceFrames > 0 && throwDir.x != 0 && (base.firstChunk.ContactPoint.x != 0 || base.firstChunk.ContactPoint.y != 0) && room.GetTile(base.firstChunk.pos).Terrain == Room.Tile.TerrainType.Slope)
			{
				Custom.Log("vertical bounce");
				base.firstChunk.vel.x = 0.5f * (float)throwDir.x;
				throwDir = new IntVector2(0, (room.IdentifySlope(base.firstChunk.pos) == Room.SlopeDirection.UpLeft || room.IdentifySlope(base.firstChunk.pos) == Room.SlopeDirection.UpRight) ? 1 : (-1));
				base.firstChunk.vel.y = (float)throwDir.y * Mathf.Max(base.firstChunk.vel.magnitude, num + 1f + ((throwDir.y <= 0) ? 0f : ((this is Spear) ? 5f : 10f)));
				base.firstChunk.pos = room.MiddleOfTile(base.firstChunk.pos);
				floorBounceFrames = 0;
				ChangeMode(Mode.Thrown);
				thrownPos = base.firstChunk.pos;
				throwModeFrames = -1;
				setRotation = throwDir.ToVector2();
				rotationSpeed = 0f;
				for (int m = 0; m < 4; m++)
				{
					room.AddObject(new Spark(base.firstChunk.pos, base.firstChunk.vel * UnityEngine.Random.value + Custom.RNV() * UnityEngine.Random.value, new Color(1f, 1f, 1f), null, 6, 18));
				}
				room.PlaySound(SoundID.Weapon_Skid, base.firstChunk.pos, 1f, 1f);
			}
			else if (base.firstChunk.ContactPoint == throwDir)
			{
				HitWall();
				floorBounceFrames = 0;
			}
			else
			{
				Vector2 pos = base.firstChunk.pos + base.firstChunk.vel;
				SharedPhysics.CollisionResult result = SharedPhysics.TraceProjectileAgainstBodyChunks(this, room, firstFrameTraceFromPos.HasValue ? firstFrameTraceFromPos.Value : base.firstChunk.pos, ref pos, base.firstChunk.rad + ((thrownBy != null && thrownBy is Player) ? 5f : 0f), 1, thrownBy, hitAppendages: true);
				firstFrameTraceFromPos = null;
				if (meleeHitChunk != null)
				{
					result.obj = meleeHitChunk.owner;
					result.chunk = meleeHitChunk;
					result.hitSomething = true;
					result.collisionPoint = meleeHitChunk.pos + Custom.DirVec(meleeHitChunk.pos, base.firstChunk.pos) * meleeHitChunk.rad;
				}
				Vector2 pos2 = ((!firstFrameTraceFromPos.HasValue) ? base.firstChunk.pos : firstFrameTraceFromPos.Value);
				if (result.obj is Creature && !room.RayTraceTilesForTerrain(room.GetTilePosition(pos2).x, room.GetTilePosition(pos2).y, room.GetTilePosition(pos).x, room.GetTilePosition(pos).y) && Vector2.Distance(base.firstChunk.pos, result.collisionPoint) > Vector2.Distance(base.firstChunk.pos, base.firstChunk.pos + base.firstChunk.vel.normalized * 19f))
				{
					HitWall();
					floorBounceFrames = 0;
					return;
				}
				if (thrownBy != null && result.obj != null && result.obj is Creature)
				{
					thrownClosestToCreature = null;
					closestCritDist = float.MaxValue;
					(result.obj as Creature).SetKillTag(thrownBy.abstractCreature);
				}
				if (result.obj is Creature && HeavyWeapon && result.chunk != null && (result.obj as Creature).grasps != null && (thrownBy == null || !Custom.DistLess(thrownBy.mainBodyChunk.pos, base.firstChunk.pos, 100f)))
				{
					for (int n = 0; n < (result.obj as Creature).grasps.Length; n++)
					{
						if ((result.obj as Creature).grasps[n] != null && ((result.obj as Creature).grasps[n].grabbed is ScavengerBomb || (result.obj as Creature).grasps[n].grabbed is SporePlant) && Vector2.Distance(base.firstChunk.lastPos, (result.obj as Creature).grasps[n].grabbed.firstChunk.pos) < Vector2.Distance(base.firstChunk.lastPos, result.chunk.pos))
						{
							result.chunk = (result.obj as Creature).grasps[n].grabbed.firstChunk;
							result.obj = (result.obj as Creature).grasps[n].grabbed;
							break;
						}
					}
				}
				bool flag2 = HitSomething(result, eu);
				base.forbiddenToPlayer = 40;
				if (result.obj != null)
				{
					if (thrownBy != null && result.obj is Creature)
					{
						room.socialEventRecognizer.WeaponAttack(this, thrownBy, result.obj as Creature, flag2);
					}
					result.obj.HitByWeapon(this);
					if (!flag2 && this is ExplosiveSpear)
					{
						(this as ExplosiveSpear).Explode();
					}
					thrownBy = null;
				}
			}
			float num6 = 0f;
			for (int num7 = 0; num7 < room.physicalObjects[0].Count; num7++)
			{
				if (room.physicalObjects[0][num7] == this || !room.physicalObjects[0][num7].canBeHitByWeapons)
				{
					continue;
				}
				bool flag3 = false;
				for (int num8 = 0; num8 < room.physicalObjects[0][num7].grabbedBy.Count; num8++)
				{
					if (flag3)
					{
						break;
					}
					flag3 = room.physicalObjects[0][num7].grabbedBy[num8].grabber == thrownBy;
				}
				if (flag3)
				{
					continue;
				}
				for (int num9 = 0; num9 < room.physicalObjects[0][num7].bodyChunks.Length; num9++)
				{
					BodyChunk bodyChunk = room.physicalObjects[0][num7].bodyChunks[num9];
					float num10 = Custom.CirclesCollisionTime(base.firstChunk.lastPos.x, base.firstChunk.lastPos.y, bodyChunk.pos.x, bodyChunk.pos.y, base.firstChunk.pos.x - base.firstChunk.lastPos.x, base.firstChunk.pos.y - base.firstChunk.lastPos.y, base.firstChunk.rad + ((thrownBy != null && thrownBy is Player) ? 5f : 0f), bodyChunk.rad);
					if (!(num10 > 0f) || !(num10 < 1f))
					{
						continue;
					}
					if (room.physicalObjects[0][num7] is Weapon && mode == Mode.Thrown && (room.physicalObjects[0][num7] as Weapon).mode == Mode.Thrown && (room.physicalObjects[0][num7] as Weapon).HeavyWeapon)
					{
						if (HeavyWeapon)
						{
							HitAnotherThrownWeapon(room.physicalObjects[0][num7] as Weapon);
							num6 = float.MaxValue;
							break;
						}
						continue;
					}
					bodyChunk.vel += base.firstChunk.vel;
					HitSomethingWithoutStopping(room.physicalObjects[0][num7], bodyChunk, null);
					num6 += bodyChunk.mass;
					if (num6 > 0.6f)
					{
						base.firstChunk.pos = bodyChunk.pos;
						ChangeMode(Mode.Free);
						base.firstChunk.vel *= 0.5f;
						room.PlaySound(SoundID.Spear_Hit_Small_Creature, bodyChunk);
						break;
					}
				}
				if (num6 <= 0.6f && room.physicalObjects[0][num7].appendages != null)
				{
					for (int num11 = 0; num11 < room.physicalObjects[0][num7].appendages.Count; num11++)
					{
						if (room.physicalObjects[0][num7].appendages[num11].canBeHit && room.physicalObjects[0][num7].appendages[num11].LineCross(base.firstChunk.lastPos, base.firstChunk.pos))
						{
							(room.physicalObjects[0][num7].appendages[num11].owner as IHaveAppendages).ApplyForceOnAppendage(new Appendage.Pos(room.physicalObjects[0][num7].appendages[num11], 0, 0.5f), base.firstChunk.vel * base.firstChunk.mass);
							HitSomethingWithoutStopping(room.physicalObjects[0][num7], null, room.physicalObjects[0][num7].appendages[num11]);
						}
					}
				}
				if (num6 > 0.6f)
				{
					break;
				}
			}
		}
		base.Update(eu);
	}

	public virtual void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		this.thrownBy = thrownBy;
		this.thrownPos = thrownPos;
		this.throwDir = throwDir;
		this.firstFrameTraceFromPos = firstFrameTraceFromPos;
		changeDirCounter = 3;
		ChangeOverlap(newOverlap: true);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, thrownPos);
		if (throwDir.x != 0)
		{
			base.firstChunk.vel.y = thrownBy.mainBodyChunk.vel.y * 0.5f;
			base.firstChunk.vel.x = thrownBy.mainBodyChunk.vel.x * 0.2f;
			base.firstChunk.vel.x += (float)throwDir.x * 40f * frc;
			base.firstChunk.vel.y += ((this is Spear) ? 1.5f : 3f);
		}
		else
		{
			if (throwDir.y == 0)
			{
				ChangeMode(Mode.Free);
				return;
			}
			base.firstChunk.vel.x = thrownBy.mainBodyChunk.vel.x * 0.5f;
			base.firstChunk.vel.y = (float)throwDir.y * 40f * frc;
		}
		if (frc >= 1f)
		{
			overrideExitThrownSpeed = 0f;
		}
		else
		{
			overrideExitThrownSpeed = Mathf.Min(exitThrownModeSpeed, frc * 20f);
		}
		ChangeMode(Mode.Thrown);
		setRotation = throwDir.ToVector2();
		rotationSpeed = 0f;
		meleeHitChunk = null;
	}

	public override void Grabbed(Creature.Grasp grasp)
	{
		ChangeMode(Mode.Carried);
		base.Grabbed(grasp);
	}

	public virtual bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		vibrate = 20;
		ChangeMode(Mode.Free);
		base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.1f, 0.4f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
		SetRandomSpin();
		return true;
	}

	private void HitAnotherThrownWeapon(Weapon obj)
	{
		if (obj.firstChunk.pos.x - obj.firstChunk.lastPos.x < 0f != base.firstChunk.pos.x - base.firstChunk.lastPos.x < 0f)
		{
			if (abstractPhysicalObject.world.game.IsArenaSession && thrownBy != null && thrownBy is Player)
			{
				abstractPhysicalObject.world.game.GetArenaGameSession.arenaSitting.players[0].parries++;
			}
			Vector2 vector = Vector2.Lerp(obj.firstChunk.lastPos, base.firstChunk.lastPos, 0.5f);
			int num = 3;
			if (this is Spear)
			{
				num += 2;
			}
			if (obj is Spear)
			{
				num += 2;
			}
			for (int i = 0; i < num; i++)
			{
				room.AddObject(new Spark(vector + Custom.DegToVec(UnityEngine.Random.value * 360f) * 5f * UnityEngine.Random.value, Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(2f, 7f, UnityEngine.Random.value) * num, new Color(1f, 1f, 1f), null, 10, 170));
			}
			Vector2 vector2 = Custom.DegToVec(UnityEngine.Random.value * 360f);
			WeaponDeflect(vector, vector2, base.firstChunk.vel.magnitude);
			obj.WeaponDeflect(vector, -vector2, base.firstChunk.vel.magnitude);
			room.PlaySound(SoundID.Spear_Bounce_Off_Creauture_Shell, vector);
		}
	}

	public virtual void WeaponDeflect(Vector2 inbetweenPos, Vector2 deflectDir, float bounceSpeed)
	{
		base.firstChunk.pos = Vector2.Lerp(base.firstChunk.pos, inbetweenPos, 0.5f);
		vibrate = 20;
		ChangeMode(Mode.Free);
		base.firstChunk.vel = deflectDir * bounceSpeed * 0.5f;
		SetRandomSpin();
	}

	public virtual void HitSomethingWithoutStopping(PhysicalObject obj, BodyChunk chunk, Appendage appendage)
	{
		if (obj is Creature)
		{
			if (thrownBy != null)
			{
				(obj as Creature).SetKillTag(thrownBy.abstractCreature);
			}
			if (this is Spear)
			{
				(obj as Creature).Die();
			}
			else if (this is Rock && (obj as Creature).Template.smallCreature && UnityEngine.Random.value < ((obj is Fly) ? 0.8f : 0.2f))
			{
				(obj as Creature).Die();
			}
			else
			{
				(obj as Creature).Stun(80);
			}
		}
		obj.HitByWeapon(this);
	}

	public virtual void HitWall()
	{
		if (room.BeingViewed)
		{
			for (int i = 0; i < 7; i++)
			{
				room.AddObject(new Spark(base.firstChunk.pos + throwDir.ToVector2() * (base.firstChunk.rad - 1f), Custom.DegToVec(UnityEngine.Random.value * 360f) * 10f * UnityEngine.Random.value + -throwDir.ToVector2() * 10f, new Color(1f, 1f, 1f), null, 2, 4));
			}
		}
		room.ScreenMovement(base.firstChunk.pos, throwDir.ToVector2() * 1.5f, 0f);
		room.PlaySound((this is Spear) ? SoundID.Spear_Bounce_Off_Wall : SoundID.Rock_Hit_Wall, base.firstChunk);
		SetRandomSpin();
		ChangeMode(Mode.Free);
		base.forbiddenToPlayer = 10;
	}

	public virtual void SetRandomSpin()
	{
		rotationSpeed = Mathf.Lerp(-100f, 100f, UnityEngine.Random.value) * Mathf.Lerp(0.05f, 1f, room.gravity);
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Items");
		}
		for (int num = sLeaser.sprites.Length - 1; num >= 0; num--)
		{
			sLeaser.sprites[num].RemoveFromContainer();
			newContatiner.AddChild(sLeaser.sprites[num]);
		}
	}

	public bool HitThisObject(PhysicalObject obj)
	{
		bool num = obj is Player && this is Spear && thrownBy != null && thrownBy is Player;
		bool flag = ModManager.CoopAvailable && Custom.rainWorld.options.friendlyFire;
		bool flag2 = room.game.IsArenaSession && room.game.GetArenaGameSession.arenaSitting.gameTypeSetup.spearsHitPlayers;
		if (num)
		{
			return flag || flag2;
		}
		return true;
	}

	public bool HitThisChunk(BodyChunk chunk)
	{
		if (firstFrameTraceFromPos.HasValue)
		{
			return Vector2.Dot((chunk.pos - base.firstChunk.lastPos).normalized, throwDir.ToVector2().normalized) > 0.3f;
		}
		return true;
	}

	public void setPosAndTail(Vector2 pos)
	{
		base.firstChunk.setPos = pos;
		tailPos = base.firstChunk.pos;
	}

	public virtual void Shoot(Creature shotBy, Vector2 thrownPos, Vector2 throwDir, float force, bool eu)
	{
		thrownBy = shotBy;
		this.thrownPos = thrownPos;
		this.throwDir = new IntVector2(Math.Sign(throwDir.x), Math.Sign(throwDir.y));
		changeDirCounter = 3;
		ChangeOverlap(newOverlap: true);
		base.firstChunk.MoveFromOutsideMyUpdate(eu, thrownPos);
		base.firstChunk.vel = throwDir * 40f * force;
		ChangeMode(Mode.Thrown);
		setRotation = throwDir;
		rotationSpeed = 0f;
		meleeHitChunk = null;
	}
}
