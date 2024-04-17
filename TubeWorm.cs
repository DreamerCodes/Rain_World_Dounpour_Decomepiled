using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class TubeWorm : Creature
{
	public class Tongue
	{
		public class Mode : ExtEnum<Mode>
		{
			public static readonly Mode Retracted = new Mode("Retracted", register: true);

			public static readonly Mode ShootingOut = new Mode("ShootingOut", register: true);

			public static readonly Mode AttachedToTerrain = new Mode("AttachedToTerrain", register: true);

			public static readonly Mode AttachedToObject = new Mode("AttachedToObject", register: true);

			public static readonly Mode Retracting = new Mode("Retracting", register: true);

			public Mode(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public BodyChunk baseChunk;

		public TubeWorm worm;

		public Room room;

		public int tongueNum;

		public Vector2 terrainStuckPos;

		public BodyChunk attachedChunk;

		public float myMass = 0.1f;

		public bool returning;

		public float requestedRopeLength;

		public float idealRopeLength;

		public float elastic;

		public float ropeExtendSpeed;

		public Rope rope;

		public Mode mode;

		private IntVector2[] _cachedTlsList = new IntVector2[100];

		public bool Free
		{
			get
			{
				if (!(mode == Mode.ShootingOut))
				{
					return mode == Mode.Retracting;
				}
				return true;
			}
		}

		public bool Attached
		{
			get
			{
				if (!(mode == Mode.AttachedToTerrain))
				{
					return mode == Mode.AttachedToObject;
				}
				return true;
			}
		}

		public Vector2 AttachedPos => terrainStuckPos;

		public Tongue(TubeWorm worm, int tongueNum)
		{
			this.worm = worm;
			this.tongueNum = tongueNum;
			mode = Mode.Retracted;
			baseChunk = worm.mainBodyChunk;
			idealRopeLength = 150f;
		}

		public void NewRoom(Room newRoom)
		{
			room = newRoom;
			rope = new Rope(room, baseChunk.pos, pos, 1f);
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			if (mode == Mode.Retracted)
			{
				requestedRopeLength = 0f;
				pos = worm.mainBodyChunk.pos;
				vel = worm.mainBodyChunk.vel;
				rope.Reset();
			}
			else if (mode == Mode.ShootingOut)
			{
				requestedRopeLength = Mathf.Max(0f, requestedRopeLength - 4f);
				bool flag = false;
				if (!Custom.DistLess(baseChunk.pos, pos, 60f))
				{
					Vector2 vector = pos + vel;
					SharedPhysics.CollisionResult collisionResult = SharedPhysics.TraceProjectileAgainstBodyChunks(null, room, pos, ref vector, 1f, 1, baseChunk.owner, hitAppendages: false);
					if (collisionResult.chunk != null)
					{
						AttachToChunk(collisionResult.chunk);
						flag = true;
					}
				}
				if (!flag)
				{
					if (worm.playerCheatAttachPos.HasValue && Custom.DistLess(pos, worm.playerCheatAttachPos.Value, 60f))
					{
						Custom.Log("attach to player cheat pos");
						AttachToTerrain(worm.playerCheatAttachPos.Value);
						worm.playerCheatAttachPos = null;
						flag = true;
					}
					else
					{
						IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, lastPos, pos);
						if (intVector.HasValue)
						{
							FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(1f));
							AttachToTerrain(new Vector2(floatRect.left, floatRect.bottom));
							flag = true;
						}
					}
					if (!flag)
					{
						vel.y -= 0.9f * Mathf.InverseLerp(0.8f, 0f, elastic);
						if (returning)
						{
							int num;
							for (num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedTlsList); num >= _cachedTlsList.Length; num = SharedPhysics.RayTracedTilesArray(lastPos, pos, _cachedTlsList))
							{
								Custom.LogWarning($"TubeWorm ray tracing limit exceeded, extending cache to {_cachedTlsList.Length + 100} and trying again!");
								Array.Resize(ref _cachedTlsList, _cachedTlsList.Length + 100);
							}
							for (int i = 0; i < num; i++)
							{
								if (room.GetTile(_cachedTlsList[i]).horizontalBeam)
								{
									AttachToTerrain(new Vector2(Mathf.Clamp(Custom.HorizontalCrossPoint(lastPos, pos, room.MiddleOfTile(_cachedTlsList[i]).y).x, room.MiddleOfTile(_cachedTlsList[i]).x - 10f, room.MiddleOfTile(_cachedTlsList[i]).x + 10f), room.MiddleOfTile(_cachedTlsList[i]).y));
									break;
								}
								if (room.GetTile(_cachedTlsList[i]).verticalBeam)
								{
									AttachToTerrain(new Vector2(room.MiddleOfTile(_cachedTlsList[i]).x, Mathf.Clamp(Custom.VerticalCrossPoint(lastPos, pos, room.MiddleOfTile(_cachedTlsList[i]).x).y, room.MiddleOfTile(_cachedTlsList[i]).y - 10f, room.MiddleOfTile(_cachedTlsList[i]).y + 10f)));
									break;
								}
							}
							if (Custom.DistLess(baseChunk.pos, pos, 40f))
							{
								mode = Mode.Retracted;
							}
						}
						else if (Vector2.Dot(Custom.DirVec(baseChunk.pos, pos), vel.normalized) < 0f)
						{
							returning = true;
						}
					}
				}
			}
			else if (mode == Mode.Retracting)
			{
				mode = Mode.Retracted;
			}
			else if (mode == Mode.AttachedToTerrain)
			{
				if (ModManager.MMF && worm.room != null)
				{
					for (int j = 0; j < worm.room.zapCoils.Count; j++)
					{
						ZapCoil zapCoil = worm.room.zapCoils[j];
						if (zapCoil.turnedOn > 0.5f && zapCoil.GetFloatRect.Vector2Inside(terrainStuckPos))
						{
							zapCoil.TriggerZap(terrainStuckPos, 4f);
							worm.mainBodyChunk.vel = Custom.DegToVec(Custom.AimFromOneVectorToAnother(terrainStuckPos, worm.mainBodyChunk.pos)).normalized * 8f;
							Release();
							worm.room.AddObject(new ZapCoil.ZapFlash(worm.mainBodyChunk.pos, 6f));
							worm.Die();
						}
					}
				}
				pos = terrainStuckPos;
				vel *= 0f;
				if (worm.noSpearStickZones.Count > 0 && !Custom.DistLess(pos, worm.mainBodyChunk.pos, 20f) && UnityEngine.Random.value < 0.1f)
				{
					for (int k = 0; k < worm.noSpearStickZones.Count; k++)
					{
						if (Custom.DistLess(pos, worm.noSpearStickZones[k].pos, (worm.noSpearStickZones[k].data as PlacedObject.ResizableObjectData).Rad))
						{
							Release();
							break;
						}
					}
				}
			}
			else if (mode == Mode.AttachedToObject)
			{
				if (attachedChunk != null)
				{
					pos = attachedChunk.pos;
					vel = attachedChunk.vel;
					if (attachedChunk.owner.room != room)
					{
						attachedChunk = null;
						mode = Mode.Retracting;
					}
				}
				else
				{
					mode = Mode.Retracting;
				}
			}
			rope.Update(baseChunk.pos, pos);
			if (mode != Mode.Retracted)
			{
				Elasticity();
			}
			if (Attached)
			{
				elastic = Mathf.Max(0f, elastic - 0.05f);
				if (elastic <= 0f)
				{
					ropeExtendSpeed = Mathf.Min(ropeExtendSpeed + 0.025f, 1f);
				}
				if (requestedRopeLength < idealRopeLength)
				{
					requestedRopeLength = Mathf.Min(requestedRopeLength + ropeExtendSpeed * 2f, idealRopeLength);
				}
				else if (requestedRopeLength > idealRopeLength)
				{
					requestedRopeLength = Mathf.Max(requestedRopeLength - (1f - elastic) * 2f, idealRopeLength);
				}
			}
			else
			{
				ropeExtendSpeed = 0f;
			}
		}

		public void Shoot(Vector2 dir)
		{
			worm.playerCheatAttachPos = null;
			if (Attached)
			{
				Release();
			}
			else if (!(mode != Mode.Retracted))
			{
				mode = Mode.ShootingOut;
				room.PlaySound(SoundID.Tube_Worm_Shoot_Tongue, baseChunk);
				dir = ((worm.grabbedBy.Count <= 0 || !(worm.grabbedBy[0].grabber is Player)) ? CheapAutoAim(dir) : ProperAutoAim(dir));
				pos = baseChunk.pos + dir * 5f;
				vel = dir * 70f;
				elastic = 1f;
				requestedRopeLength = 140f;
				returning = false;
			}
		}

		private Vector2 ProperAutoAim(Vector2 originalDir)
		{
			float num = 230f;
			float num2 = idealRopeLength;
			float num3 = Custom.VecToDeg(originalDir);
			int num4 = 0;
			if (originalDir.y < 0.9f && worm.grabbedBy.Count > 0 && worm.grabbedBy[0].grabber is Player)
			{
				num4 = (worm.grabbedBy[0].grabber as Player).input[0].x;
			}
			bool flag = worm.grabbedBy.Count > 0 && worm.grabbedBy[0].grabber is Player && (worm.grabbedBy[0].grabber as Player).input[0].y > 0;
			bool flag2 = false;
			if (!flag)
			{
				IntVector2 tilePosition = room.GetTilePosition(baseChunk.pos + baseChunk.vel * 3f);
				for (int i = 0; i < 10; i++)
				{
					tilePosition.y--;
					if (room.GetTile(tilePosition).Solid)
					{
						num2 = Mathf.Max(40f, Vector2.Distance(baseChunk.pos + baseChunk.vel * 3f, room.MiddleOfTile(tilePosition)) - 40f);
						flag2 = true;
						break;
					}
				}
			}
			Vector2 result = originalDir;
			float num5 = float.MaxValue;
			for (float num6 = 0f; num6 < 35f; num6 += 2.5f)
			{
				for (float num7 = -1f; num7 <= 1f; num7 += 2f)
				{
					Vector2? vector = SharedPhysics.ExactTerrainRayTracePos(room, baseChunk.pos, baseChunk.pos + Custom.DegToVec(num3 + num6 * num7) * num);
					if (!vector.HasValue)
					{
						continue;
					}
					float num8 = num6 * 1.5f;
					if (!flag)
					{
						num8 += Mathf.Abs(num2 - Vector2.Distance(baseChunk.pos + baseChunk.vel * 3f, vector.Value));
						if (num4 != 0)
						{
							num8 += Mathf.Abs((float)num4 * 90f - (num3 + num6 * num7)) * 0.9f;
						}
						if (flag2)
						{
							for (int j = -1; j < 2; j++)
							{
								if (!room.VisualContact(vector.Value, vector.Value - new Vector2(40f * (float)j, Vector2.Distance(baseChunk.pos, vector.Value) + 20f)))
								{
									num8 += 1000f;
									break;
								}
							}
						}
					}
					if (num8 < num5)
					{
						num5 = num8;
						result = Custom.DegToVec(num3 + num6 * num7);
						worm.playerCheatAttachPos = vector.Value + Custom.DirVec(vector.Value, baseChunk.pos) * 2f;
					}
				}
			}
			return result;
		}

		private Vector2 CheapAutoAim(Vector2 originalDir)
		{
			float num = 230f;
			if (!SharedPhysics.RayTraceTilesForTerrain(room, baseChunk.pos, baseChunk.pos + originalDir * num))
			{
				return originalDir;
			}
			float num2 = Custom.VecToDeg(originalDir);
			for (float num3 = 5f; num3 < 30f; num3 += 5f)
			{
				for (float num4 = -1f; num4 <= 1f; num4 += 2f)
				{
					if (!SharedPhysics.RayTraceTilesForTerrain(room, baseChunk.pos, baseChunk.pos + Custom.DegToVec(num2 + num3 * num4) * num))
					{
						return Custom.DegToVec(num2 + num3 * num4);
					}
				}
			}
			return originalDir;
		}

		public void Release()
		{
			if (mode == Mode.AttachedToObject && attachedChunk != null)
			{
				room.PlaySound(SoundID.Tube_Worm_Detatch_Tongue_Creature, pos);
			}
			else if (mode == Mode.AttachedToTerrain)
			{
				room.PlaySound(SoundID.Tube_Worm_Detach_Tongue_Terrain, pos);
			}
			mode = Mode.Retracting;
			attachedChunk = null;
		}

		private void AttachToTerrain(Vector2 attPos)
		{
			terrainStuckPos = attPos;
			mode = Mode.AttachedToTerrain;
			pos = terrainStuckPos;
			Attatch();
			room.PlaySound(SoundID.Tube_Worm_Tongue_Hit_Terrain, pos);
		}

		private void AttachToChunk(BodyChunk chunk)
		{
			attachedChunk = chunk;
			pos = chunk.pos;
			mode = Mode.AttachedToObject;
			Attatch();
			room.PlaySound(SoundID.Tube_Worm_Tongue_Hit_Creature, pos);
		}

		private void Attatch()
		{
			vel *= 0f;
			elastic = 1f;
			requestedRopeLength = Vector2.Distance(baseChunk.pos, pos);
		}

		private void Elasticity()
		{
			float num = 0f;
			if (mode == Mode.AttachedToTerrain)
			{
				num = 1f;
			}
			else if (mode == Mode.AttachedToObject)
			{
				num = attachedChunk.mass / (attachedChunk.mass + baseChunk.mass);
			}
			Vector2 vector = Custom.DirVec(baseChunk.pos, rope.AConnect);
			float totalLength = rope.totalLength;
			float a = 0.7f;
			if (worm.tongues[0].Attached && worm.tongues[1].Attached)
			{
				a = Custom.LerpMap(Mathf.Abs(0.5f - worm.onRopePos), 0.5f, 0.4f, 1.1f, 0.7f);
			}
			float num2 = worm.RequestRope(tongueNum) * Mathf.Lerp(a, 1f, elastic);
			float num3 = Mathf.Lerp(0.85f, 0.25f, elastic);
			if (totalLength > num2)
			{
				baseChunk.vel += vector * (totalLength - num2) * num3 * num;
				baseChunk.pos += vector * (totalLength - num2) * num3 * num * Mathf.Lerp(1f, (mode == Mode.AttachedToTerrain) ? 1f : 0.5f, elastic);
				vector = Custom.DirVec(pos, rope.BConnect);
				if (Free)
				{
					vel += vector * (totalLength - num2) * num3 * (1f - num);
					pos += vector * (totalLength - num2) * num3 * (1f - num) * Mathf.Lerp(1f, 0.5f, elastic);
				}
				else if (mode == Mode.AttachedToObject)
				{
					attachedChunk.vel += vector * (totalLength - num2) * num3 * (1f - num);
					attachedChunk.pos += vector * (totalLength - num2) * num3 * (1f - num) * Mathf.Lerp(1f, 0.5f, elastic);
				}
			}
		}
	}

	public float lungs;

	public Tongue[] tongues;

	private bool lastGrabbed;

	public int step;

	public bool sleeping;

	public int sleepCounter;

	public float goalOnRopePos;

	private float crawlSpeed = 1f;

	public float walkCycle;

	public float lastWalk;

	public float onRopePos;

	public Vector2? playerCheatAttachPos;

	public float maxTotalRope = 200f;

	public List<PlacedObject> noSpearStickZones = new List<PlacedObject>();

	private int triedMove;

	private PathFinder.PathingCell lastAccessibleCell;

	public IntVector2? grappleToPos;

	public IntVector2? nextGrapplePos;

	private int grapplingCounter;

	private int shootTries;

	private IntVector2[] _cachedRays1 = new IntVector2[100];

	private IntVector2[] _cachedRays2 = new IntVector2[100];

	private IntVector2[] _cachedRays3 = new IntVector2[100];

	private bool useBool;

	public float totalRope
	{
		get
		{
			if (tongues[0].Attached && tongues[1].Attached)
			{
				return Mathf.Min(maxTotalRope, Vector2.Distance(tongues[0].pos, tongues[1].pos));
			}
			return maxTotalRope;
		}
	}

	public float FreeRope => Mathf.Max(0f, totalRope - RequestRope(0) - RequestRope(1));

	public float RopeStretchFac
	{
		get
		{
			float num = Mathf.Lerp(maxTotalRope, RequestRope(0) + RequestRope(1), 0.5f) / (tongues[0].rope.totalLength + tongues[1].rope.totalLength + 80f);
			num = Mathf.Pow(num, (num < 1f) ? 1.6f : 0.4f);
			if (tongues[0].mode == Tongue.Mode.AttachedToTerrain && tongues[1].mode == Tongue.Mode.AttachedToTerrain)
			{
				num = Mathf.Lerp(num, 1f, 0.5f);
			}
			return num;
		}
	}

	public float RequestRope(int tongue)
	{
		if (WeightedRopeRequest(0) + WeightedRopeRequest(1) < totalRope)
		{
			return WeightedRopeRequest(tongue);
		}
		float num = WeightedRopeRequest(tongue) / (WeightedRopeRequest(tongue) + WeightedRopeRequest(1 - tongue));
		return totalRope * num;
	}

	private float WeightedRopeRequest(int tongue)
	{
		return Mathf.Min(tongues[tongue].requestedRopeLength, ((tongue == 0) ? onRopePos : (1f - onRopePos)) * totalRope);
	}

	public TubeWorm(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		float num = 0.2f;
		base.bodyChunks = new BodyChunk[2];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, num / 2f);
		base.bodyChunks[1] = new BodyChunk(this, 1, new Vector2(0f, 0f), 6f, num / 2f);
		base.bodyChunks[0].loudness = 0.01f;
		base.bodyChunks[1].loudness = 0.01f;
		bodyChunkConnections = new BodyChunkConnection[1];
		bodyChunkConnections[0] = new BodyChunkConnection(base.bodyChunks[0], base.bodyChunks[1], 7f, BodyChunkConnection.Type.Normal, 1f, 0.5f);
		base.airFriction = 0.999f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.4f;
		collisionLayer = 1;
		base.waterFriction = 0.96f;
		base.buoyancy = 0.95f;
		walkCycle = UnityEngine.Random.value;
		lastWalk = walkCycle;
		tongues = new Tongue[2];
		for (int i = 0; i < 2; i++)
		{
			tongues[i] = new Tongue(this, i);
		}
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		for (int i = 0; i < 2; i++)
		{
			tongues[i].NewRoom(newRoom);
		}
		noSpearStickZones.Clear();
		for (int j = 0; j < newRoom.roomSettings.placedObjects.Count; j++)
		{
			if (newRoom.roomSettings.placedObjects[j].type == PlacedObject.Type.NoSpearStickZone)
			{
				noSpearStickZones.Add(newRoom.roomSettings.placedObjects[j]);
			}
		}
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new TubeWormGraphics(this);
		}
	}

	public override void Update(bool eu)
	{
		lastWalk = walkCycle;
		base.CollideWithTerrain = grabbedBy.Count == 0;
		if (sleeping)
		{
			crawlSpeed = Mathf.Max(0.4f, crawlSpeed - 0.0025f);
		}
		else
		{
			crawlSpeed = Mathf.Min(1.4f, crawlSpeed + 0.005f);
		}
		canBeHitByWeapons = grabbedBy.Count < 1;
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		if (room.game.devToolsActive && Input.GetKey("b") && room.game.cameras[0].room == room)
		{
			base.bodyChunks[0].vel += Custom.DirVec(base.bodyChunks[0].pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 14f;
			Stun(12);
		}
		bool flag = false;
		for (int i = 0; i < grabbedBy.Count; i++)
		{
			if (grabbedBy[i].grabber is Player)
			{
				flag = true;
				break;
			}
		}
		tongues[0].baseChunk = base.mainBodyChunk;
		tongues[1].baseChunk = base.bodyChunks[1];
		if (flag)
		{
			GrabbedByPlayer();
		}
		else if (base.Consious)
		{
			if (sleeping && !base.safariControlled)
			{
				Sleep();
			}
			else
			{
				Act();
			}
		}
		lastGrabbed = flag;
		bool flag2 = true;
		for (int j = 0; j < 2; j++)
		{
			tongues[j].Update();
			if (!tongues[j].Attached)
			{
				flag2 = false;
			}
		}
		if (flag2)
		{
			bodyChunkConnections[0].Update();
		}
		if (!base.dead && base.mainBodyChunk.submersion > 0.5f)
		{
			lungs = Mathf.Max(lungs - 1f / 180f, 0f);
			if (lungs == 0f)
			{
				Die();
			}
		}
		else
		{
			lungs = Mathf.Min(lungs + 0.02f, 1f);
		}
		if ((UnityEngine.Random.value < 1f / 30f && tongues[0].rope.totalLength + tongues[1].rope.totalLength > maxTotalRope * 2f) || tongues[0].rope.totalLength + tongues[1].rope.totalLength > maxTotalRope * 5f)
		{
			tongues[UnityEngine.Random.Range(0, 2)].Release();
		}
	}

	private void Sleep()
	{
		sleepCounter--;
		if (sleepCounter < 1)
		{
			sleeping = false;
		}
		for (int i = 0; i < 2; i++)
		{
			if (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0)
			{
				sleeping = false;
			}
		}
		if (UnityEngine.Random.value < 0.0020833334f)
		{
			goalOnRopePos = UnityEngine.Random.value;
		}
		WalkAlongRope();
	}

	private void Act()
	{
		if (base.safariControlled && inputWithDiagonals.HasValue && lastInputWithDiagonals.HasValue)
		{
			sleeping = false;
			nextGrapplePos = null;
			triedMove = 0;
			grapplingCounter = 0;
			shootTries = 0;
			Vector2 vector = default(Vector2);
			if (inputWithDiagonals.Value.AnyDirectionalInput)
			{
				vector.x = inputWithDiagonals.Value.x;
				vector.y = inputWithDiagonals.Value.y;
			}
			if (inputWithDiagonals.Value.jmp && !lastInputWithDiagonals.Value.jmp)
			{
				if (!tongues[0].Attached && !tongues[1].Attached)
				{
					tongues[0].Shoot(Vector2.Lerp(Custom.DirVec(base.bodyChunks[1].pos, base.firstChunk.pos), vector, 0.3f));
					tongues[1].Shoot(Vector2.Lerp(Custom.DirVec(base.firstChunk.pos, base.bodyChunks[1].pos), vector, 0.3f));
				}
				else
				{
					if (tongues[0].Attached)
					{
						tongues[0].Release();
					}
					if (tongues[1].Attached)
					{
						tongues[1].Release();
					}
				}
			}
			else
			{
				if (inputWithDiagonals.Value.thrw && !lastInputWithDiagonals.Value.thrw)
				{
					if (tongues[0].Attached)
					{
						tongues[0].Release();
					}
					else
					{
						tongues[0].Shoot(Vector2.Lerp(Custom.DirVec(base.bodyChunks[1].pos, base.firstChunk.pos), vector, 0.3f));
					}
				}
				if (inputWithDiagonals.Value.pckp && !lastInputWithDiagonals.Value.pckp)
				{
					if (tongues[1].Attached)
					{
						tongues[1].Release();
					}
					else
					{
						tongues[1].Shoot(Vector2.Lerp(Custom.DirVec(base.firstChunk.pos, base.bodyChunks[1].pos), vector, 0.3f));
					}
				}
			}
			if (tongues[0].Attached && tongues[1].Attached)
			{
				if (vector.x > 0f || vector.y > 0f)
				{
					goalOnRopePos = onRopePos + 0.05f;
				}
				else if (vector.x < 0f || vector.y < 0f)
				{
					goalOnRopePos = onRopePos - 0.05f;
				}
				goalOnRopePos = Mathf.Clamp(goalOnRopePos, 0f, 1f);
				WalkAlongRope();
			}
			else if ((tongues[0].Attached || tongues[1].Attached) && inputWithDiagonals.Value.AnyDirectionalInput && !lastInputWithDiagonals.Value.AnyDirectionalInput)
			{
				base.firstChunk.vel += vector * 5f;
			}
		}
		else if (tongues[0].mode == Tongue.Mode.AttachedToObject || tongues[1].mode == Tongue.Mode.AttachedToObject)
		{
			HoldOnToObject();
			return;
		}
		if (room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[step].pos), base.abstractCreature.creatureTemplate))
		{
			lastAccessibleCell = base.abstractCreature.abstractAI.RealAI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(base.bodyChunks[step].pos));
		}
		if (!base.safariControlled)
		{
			base.abstractCreature.abstractAI.RealAI.Update();
			triedMove++;
			grapplingCounter++;
			bool flag = room.aimap.TileAccessibleToCreature(room.GetTilePosition(base.bodyChunks[step].pos), base.abstractCreature.creatureTemplate);
			if (UnityEngine.Random.value < 0.025f && tongues[0].Attached && tongues[1].Attached && room.aimap.getTerrainProximity(base.mainBodyChunk.pos) > 2 && (base.abstractCreature.abstractAI.RealAI as TubeWormAI).SleepAllowed)
			{
				sleeping = true;
				sleepCounter = UnityEngine.Random.Range(60, 1260);
				goalOnRopePos = UnityEngine.Random.value;
				return;
			}
			if (grappleToPos.HasValue && tongues[1 - step].Attached && (grapplingCounter > 40 || !flag) && !room.aimap.getAItile(base.bodyChunks[step].pos).narrowSpace)
			{
				GrappleToPosition();
				return;
			}
			if (flag)
			{
				FindInaccessibleGrapplingPos();
			}
		}
		FlopAlong();
	}

	private void FindInaccessibleGrapplingPos()
	{
		Vector2 b = base.bodyChunks[step].pos + Custom.RNV() * 220f;
		int num;
		for (num = SharedPhysics.RayTracedTilesArray(base.bodyChunks[step].pos, b, _cachedRays1); num >= _cachedRays1.Length; num = SharedPhysics.RayTracedTilesArray(base.bodyChunks[step].pos, b, _cachedRays1))
		{
			Custom.LogWarning($"FindInaccessibleGrapplingPos ray tracing limit exceeded, extending cache to {_cachedRays1.Length + 100} and trying again!");
			Array.Resize(ref _cachedRays1, _cachedRays1.Length + 100);
		}
		IntVector2? intVector = null;
		for (int i = 0; i < num - 1; i++)
		{
			if (room.GetTile(_cachedRays1[i + 1]).Solid)
			{
				intVector = _cachedRays1[i];
				break;
			}
		}
		if (!intVector.HasValue || room.aimap.TileAccessibleToCreature(intVector.Value, base.Template) || intVector.Value == base.abstractCreature.pos.Tile)
		{
			return;
		}
		Vector2 vector = room.MiddleOfTile(intVector.Value);
		bool flag = false;
		float num2 = 45f * UnityEngine.Random.value;
		for (float num3 = 0f; num3 < 360f; num3 += 45f)
		{
			if (flag)
			{
				break;
			}
			int num4;
			for (num4 = SharedPhysics.RayTracedTilesArray(vector, vector + Custom.DegToVec(num3 + num2) * 220f, _cachedRays2); num4 >= _cachedRays2.Length; num4 = SharedPhysics.RayTracedTilesArray(vector, vector + Custom.DegToVec(num3 + num2) * 220f, _cachedRays2))
			{
				Custom.LogWarning($"FindInaccessibleGrapplingPos ray tracing limit exceeded, extending cache to {_cachedRays2.Length + 100} and trying again!");
				Array.Resize(ref _cachedRays2, _cachedRays2.Length + 100);
			}
			for (int j = 6; j < num4 - 1; j++)
			{
				if (flag)
				{
					break;
				}
				if (room.GetTile(_cachedRays2[j + 1]).Solid)
				{
					if (ComparePathingCells(base.abstractCreature.abstractAI.RealAI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(_cachedRays2[j])), lastAccessibleCell))
					{
						flag = true;
					}
					break;
				}
			}
		}
		if (flag)
		{
			grappleToPos = intVector.Value;
		}
	}

	private void FindAccessibleGrapplingPos()
	{
		Vector2 b = room.MiddleOfTile(grappleToPos.Value) + Custom.RNV() * 220f;
		int num;
		for (num = SharedPhysics.RayTracedTilesArray(room.MiddleOfTile(grappleToPos.Value), b, _cachedRays3); num >= _cachedRays3.Length; num = SharedPhysics.RayTracedTilesArray(room.MiddleOfTile(grappleToPos.Value), b, _cachedRays3))
		{
			Custom.LogWarning($"FindAccessibleGrapplingPos ray tracing limit exceeded, extending cache to {_cachedRays3.Length + 100} and trying again!");
			Array.Resize(ref _cachedRays3, _cachedRays3.Length + 100);
		}
		IntVector2? intVector = null;
		for (int i = 0; i < num - 1; i++)
		{
			if (room.GetTile(_cachedRays3[i + 1]).Solid)
			{
				intVector = _cachedRays3[i];
				break;
			}
		}
		if (intVector.HasValue && room.aimap.TileAccessibleToCreature(intVector.Value, base.Template) && !(intVector.Value == base.abstractCreature.pos.Tile) && (!nextGrapplePos.HasValue || ComparePathingCells(base.abstractCreature.abstractAI.RealAI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(intVector.Value)), base.abstractCreature.abstractAI.RealAI.pathFinder.PathingCellAtWorldCoordinate(room.GetWorldCoordinate(nextGrapplePos.Value)))))
		{
			nextGrapplePos = intVector;
		}
	}

	private bool ComparePathingCells(PathFinder.PathingCell A, PathFinder.PathingCell B)
	{
		if (A == null || A == base.abstractCreature.abstractAI.RealAI.pathFinder.fallbackPathingCell)
		{
			return false;
		}
		if (B == null || B == base.abstractCreature.abstractAI.RealAI.pathFinder.fallbackPathingCell)
		{
			return true;
		}
		if (A.generation > B.generation)
		{
			return true;
		}
		if (A.generation < B.generation)
		{
			return false;
		}
		return A.costToGoal < B.costToGoal;
	}

	private void HoldOnToObject()
	{
		bool flag = true;
		for (int i = 0; i < 2; i++)
		{
			if (UnityEngine.Random.value < 0.00027777778f)
			{
				tongues[i].Release();
			}
			if (!tongues[i].Attached)
			{
				flag = false;
			}
			if (tongues[i].mode == Tongue.Mode.Retracted && (base.bodyChunks[i].ContactPoint.x != 0 || base.bodyChunks[i].ContactPoint.y != 0))
			{
				tongues[i].Shoot(base.bodyChunks[i].ContactPoint.ToVector2());
			}
		}
		if (flag)
		{
			if (UnityEngine.Random.value < 0.0038461538f)
			{
				goalOnRopePos = UnityEngine.Random.value;
			}
			WalkAlongRope();
		}
	}

	private void FlopAlong()
	{
		if (triedMove > 200)
		{
			for (int i = 0; i < 2; i++)
			{
				tongues[i].Release();
				base.bodyChunks[i].vel += Custom.RNV() * UnityEngine.Random.value * 12f;
			}
			triedMove = 0;
		}
		if (tongues[1 - step].Attached)
		{
			if (tongues[step].Attached && (!base.safariControlled || (base.safariControlled && tongues[0].Attached != tongues[1].Attached)))
			{
				if (Custom.DistLess(base.bodyChunks[step].pos, tongues[step].pos, 100f))
				{
					tongues[1 - step].Release();
					step = 1 - step;
				}
				else
				{
					grappleToPos = room.GetTilePosition(tongues[step].pos + Custom.DirVec(tongues[step].pos, base.bodyChunks[step].pos) * 5f);
				}
				shootTries = 0;
				return;
			}
			if (!base.safariControlled)
			{
				if (step == 1)
				{
					onRopePos = Mathf.Max(0f, onRopePos - 1f / 60f);
				}
				else
				{
					onRopePos = Mathf.Min(1f, onRopePos + 1f / 60f);
				}
				onRopePos = Mathf.Lerp(onRopePos, 0.5f, Mathf.InverseLerp(100f, 200f, triedMove));
			}
			MovementConnection movementConnection = default(MovementConnection);
			if (base.safariControlled && (movementConnection == default(MovementConnection) || !AllowableControlledAIOverride(movementConnection.type)))
			{
				movementConnection = default(MovementConnection);
				if (inputWithDiagonals.HasValue)
				{
					MovementConnection.MovementType type = MovementConnection.MovementType.Standard;
					if (room.GetTile(base.mainBodyChunk.pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance)
					{
						type = MovementConnection.MovementType.ShortCut;
					}
					if (inputWithDiagonals.Value.AnyDirectionalInput)
					{
						movementConnection = new MovementConnection(type, room.GetWorldCoordinate(base.mainBodyChunk.pos), room.GetWorldCoordinate(base.mainBodyChunk.pos + new Vector2(inputWithDiagonals.Value.x, inputWithDiagonals.Value.y) * 40f), 2);
					}
				}
			}
			for (int j = 0; j < 5; j++)
			{
				if (!(movementConnection == default(MovementConnection)))
				{
					break;
				}
				movementConnection = (base.abstractCreature.abstractAI.RealAI.pathFinder as StandardPather).FollowPath(room.GetWorldCoordinate(base.mainBodyChunk.pos) + Custom.fourDirectionsAndZero[j], actuallyFollowingThisPath: true);
			}
			if (movementConnection != default(MovementConnection))
			{
				if (triedMove > 40)
				{
					base.bodyChunks[step].vel += Custom.DirVec(base.bodyChunks[step].pos, room.MiddleOfTile(movementConnection.DestTile));
				}
				base.GoThroughFloors = movementConnection.DestTile.y < movementConnection.StartTile.y;
				if (movementConnection.type == MovementConnection.MovementType.ShortCut || movementConnection.type == MovementConnection.MovementType.NPCTransportation)
				{
					enteringShortCut = movementConnection.StartTile;
					if (base.safariControlled)
					{
						bool flag = false;
						List<IntVector2> list = new List<IntVector2>();
						ShortcutData[] shortcuts = room.shortcuts;
						for (int k = 0; k < shortcuts.Length; k++)
						{
							ShortcutData shortcutData = shortcuts[k];
							if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile != movementConnection.StartTile)
							{
								list.Add(shortcutData.StartTile);
							}
							if (shortcutData.shortCutType == ShortcutData.Type.NPCTransportation && shortcutData.StartTile == movementConnection.StartTile)
							{
								flag = true;
							}
						}
						if (flag)
						{
							if (list.Count > 0)
							{
								list.Shuffle();
								NPCTransportationDestination = room.GetWorldCoordinate(list[0]);
							}
							else
							{
								NPCTransportationDestination = movementConnection.destinationCoord;
							}
						}
					}
					else if (movementConnection.type == MovementConnection.MovementType.NPCTransportation)
					{
						NPCTransportationDestination = movementConnection.destinationCoord;
					}
					for (int l = 0; l < 2; l++)
					{
						tongues[l].Release();
						if (!ModManager.MMF)
						{
							tongues[l].rope.Reset();
						}
					}
				}
			}
			if (!Custom.DistLess(base.bodyChunks[1 - step].pos, tongues[1 - step].pos, 20f))
			{
				return;
			}
			float num = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			for (int m = 0; m < 4; m++)
			{
				if (room.GetTile(base.bodyChunks[1 - step].pos + new Vector2((float)Custom.fourDirections[m].x * 20f * num, (float)Custom.fourDirections[m].y * 20f)).Solid)
				{
					base.bodyChunks[step].vel -= new Vector2((float)Custom.fourDirections[m].x * num, Custom.fourDirections[m].y) * Custom.LerpMap(triedMove, 20f, 100f, 1.2f, 1.5f);
				}
			}
			base.bodyChunks[1 - step].vel += Custom.DirVec(base.bodyChunks[1 - step].pos, tongues[1 - step].pos) * 0.6f;
			if (!(tongues[step].mode == Tongue.Mode.Retracted) || !(movementConnection != default(MovementConnection)))
			{
				return;
			}
			Vector2 p = room.MiddleOfTile(movementConnection.DestTile);
			for (int num2 = 3; num2 >= 0; num2--)
			{
				if (room.GetTile(movementConnection.DestTile + Custom.fourDirections[num2]).Solid)
				{
					p = room.MiddleOfTile(movementConnection.DestTile) + Custom.fourDirections[num2].ToVector2() * 11f;
					break;
				}
			}
			Vector2 vector = Custom.DirVec(base.bodyChunks[step].pos, p);
			vector = Vector3.Slerp(vector, Custom.RNV(), Mathf.InverseLerp(2f, 8f, shootTries));
			base.bodyChunks[step].vel += vector * Custom.LerpMap(triedMove, 0f, 40f, 0.6f, 1.7f);
			base.bodyChunks[1 - step].vel -= vector * Custom.LerpMap(triedMove, 0f, 40f, 0.6f, 1.7f);
			if (Vector2.Dot((base.bodyChunks[step].pos - base.bodyChunks[1 - step].pos).normalized, vector) > Custom.LerpMap(triedMove, 0f, 100f, 1f, -1f))
			{
				tongues[step].Shoot(vector);
				triedMove = 0;
				shootTries++;
			}
		}
		else if (base.bodyChunks[1 - step].ContactPoint.y < 0)
		{
			tongues[1 - step].Shoot(new Vector2(0f, -1f));
		}
	}

	private void GrappleToPosition()
	{
		if (!room.aimap.TileAccessibleToCreature(grappleToPos.Value, base.Template))
		{
			FindAccessibleGrapplingPos();
		}
		if (tongues[step].Attached)
		{
			goalOnRopePos = step;
			WalkAlongRope();
			if (onRopePos == (float)step)
			{
				tongues[1 - step].Release();
				step = 1 - step;
				grappleToPos = nextGrapplePos;
				nextGrapplePos = null;
				triedMove = 0;
				grapplingCounter = 0;
				shootTries = 0;
			}
		}
		else if (Custom.DistLess(base.bodyChunks[step].pos, room.MiddleOfTile(grappleToPos.Value), 220f) && room.VisualContact(base.bodyChunks[step].pos, room.MiddleOfTile(grappleToPos.Value)))
		{
			Vector2 vector = Custom.DirVec(base.bodyChunks[step].pos, room.MiddleOfTile(grappleToPos.Value));
			vector = Vector3.Slerp(vector, Custom.RNV(), Mathf.InverseLerp(2f, 8f, shootTries));
			base.bodyChunks[step].vel += vector * Custom.LerpMap(triedMove, 0f, 40f, 0.6f, 1.7f);
			base.bodyChunks[1 - step].vel -= vector * Custom.LerpMap(triedMove, 0f, 40f, 0.6f, 1.7f);
			if (Vector2.Dot((base.bodyChunks[step].pos - base.bodyChunks[1 - step].pos).normalized, vector) > Custom.LerpMap(triedMove, 0f, 100f, 1.5f, -1f))
			{
				tongues[step].Shoot(vector);
				shootTries++;
			}
		}
		else
		{
			grappleToPos = null;
		}
	}

	private void WalkAlongRope()
	{
		float num = crawlSpeed / totalRope;
		float num2 = onRopePos;
		if (onRopePos > goalOnRopePos)
		{
			onRopePos = Mathf.Max(goalOnRopePos, onRopePos - num);
		}
		else
		{
			onRopePos = Mathf.Min(goalOnRopePos, onRopePos + num);
		}
		walkCycle += (num2 - onRopePos) * totalRope * 0.021f;
	}

	private void GrabbedByPlayer()
	{
		if (!lastGrabbed)
		{
			tongues[0].elastic = 1f;
			tongues[1].elastic = 1f;
		}
		sleeping = false;
		Player player = null;
		for (int i = 0; i < grabbedBy.Count; i++)
		{
			if (grabbedBy[i].grabber is Player)
			{
				player = grabbedBy[i].grabber as Player;
				break;
			}
		}
		tongues[0].baseChunk = player.mainBodyChunk;
		if (tongues[1].Attached)
		{
			tongues[1].Release();
		}
		onRopePos = Mathf.Min(1f, onRopePos + 0.05f);
		Vector2 vector = new Vector2(player.flipDirection, 0.7f).normalized;
		if (player.input[0].y > 0)
		{
			vector = new Vector2(0f, 1f);
		}
		player.tubeWorm = this;
		base.mainBodyChunk.vel += vector * 3f;
		base.bodyChunks[1].vel -= vector * 3f;
		if (player.enteringShortCut.HasValue)
		{
			for (int j = 0; j < 2; j++)
			{
				if (tongues[j].Attached)
				{
					tongues[j].Release();
				}
			}
		}
		else if (ModManager.MMF && player.animation == Player.AnimationIndex.VineGrab)
		{
			if (!player.input[0].jmp || player.input[1].jmp)
			{
				return;
			}
			for (int k = 0; k < 2; k++)
			{
				if (tongues[k].Attached)
				{
					tongues[k].Release();
					useBool = false;
				}
			}
			if (useBool && !base.dead)
			{
				tongues[0].Shoot(vector);
				useBool = false;
			}
		}
		else
		{
			if (useBool && !base.dead)
			{
				tongues[0].Shoot(vector);
			}
			useBool = false;
		}
	}

	public bool JumpButton(Player plr)
	{
		if (base.dead || (!ModManager.MMF && base.stun > 12) || (ModManager.MMF && room != null && MMF.cfgOldTongue.Value))
		{
			return true;
		}
		if (plr.canJump < 1 && plr.bodyMode == Player.BodyModeIndex.Default)
		{
			useBool = true;
			return false;
		}
		if (tongues[0].Attached)
		{
			useBool = true;
			return false;
		}
		return true;
	}

	public void Use()
	{
		useBool = true;
	}

	public override void Stun(int time)
	{
		base.Stun(time);
		if (!ModManager.MMF)
		{
			return;
		}
		for (int i = 0; i < tongues.Length; i++)
		{
			if (tongues[i].Attached)
			{
				tongues[i].Release();
			}
		}
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
		sleeping = false;
	}

	public override void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
	}

	public override void Violence(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, Appendage.Pos onAppendagePos, DamageType type, float damage, float stunBonus)
	{
		for (int i = 0; i < 2; i++)
		{
			if (UnityEngine.Random.value < 0.5f)
			{
				tongues[i].Release();
			}
		}
		base.Violence(source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		for (int i = 0; i < base.bodyChunks.Length; i++)
		{
			base.bodyChunks[i].pos = newRoom.MiddleOfTile(pos) - vector * (-1.5f + (float)i) * 15f;
			base.bodyChunks[i].lastPos = newRoom.MiddleOfTile(pos);
			base.bodyChunks[i].vel = vector * 8f;
		}
		if (newRoom.ShorcutEntranceHoleDirection(pos).y > 0 && grabbedBy.Count == 0)
		{
			base.GoThroughFloors = false;
		}
		for (int j = 0; j < 2; j++)
		{
			if (tongues[j] != null)
			{
				tongues[j].mode = Tongue.Mode.Retracted;
				if (tongues[j].rope != null)
				{
					tongues[j].pos = base.mainBodyChunk.pos;
					tongues[j].lastPos = tongues[j].pos;
					tongues[j].rope.Reset(tongues[j].pos);
				}
			}
		}
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void Die()
	{
		base.Die();
		for (int i = 0; i < grabbedBy.Count; i++)
		{
			if (grabbedBy[i].grabber is Player)
			{
				(grabbedBy[i].grabber as Player).ReleaseGrasp(grabbedBy[i].graspUsed);
				break;
			}
		}
		for (int j = 0; j < tongues.Length; j++)
		{
			if (tongues[j].Attached)
			{
				tongues[j].Release();
			}
		}
	}
}
