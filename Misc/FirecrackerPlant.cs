using System;
using System.Collections.Generic;
using MoreSlugcats;
using Noise;
using RWCustom;
using UnityEngine;

public class FirecrackerPlant : Weapon, IProvideWarmth
{
	public class Part
	{
		public FirecrackerPlant owner;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 vel;

		public float rad;

		private SharedPhysics.TerrainCollisionData scratchTerrainCollisionData;

		public Part(FirecrackerPlant owner)
		{
			this.owner = owner;
			pos = owner.firstChunk.pos;
			lastPos = owner.firstChunk.pos;
			vel *= 0f;
		}

		public void Update()
		{
			lastPos = pos;
			pos += vel;
			if (owner.room.PointSubmerged(pos))
			{
				vel *= 0.7f;
			}
			else
			{
				vel *= 0.95f;
			}
			if (!owner.growPos.HasValue)
			{
				SharedPhysics.TerrainCollisionData cd = scratchTerrainCollisionData.Set(pos, lastPos, vel, rad, new IntVector2(0, 0), owner.firstChunk.goThroughFloors);
				cd = SharedPhysics.VerticalCollision(owner.room, cd);
				cd = SharedPhysics.HorizontalCollision(owner.room, cd);
				pos = cd.pos;
				vel = cd.vel;
			}
		}

		public void Reset()
		{
			pos = owner.firstChunk.pos + Custom.RNV() * UnityEngine.Random.value;
			lastPos = pos;
			vel *= 0f;
		}
	}

	public class ScareObject : UpdatableAndDeletable
	{
		public int lifeTime;

		public Vector2 pos;

		public List<ThreatTracker.ThreatPoint> threatPoints;

		private bool init;

		public bool fearScavs;

		public float fearRange;

		public ScareObject(Vector2 pos)
		{
			this.pos = pos;
			threatPoints = new List<ThreatTracker.ThreatPoint>();
			fearRange = 2000f;
			fearScavs = false;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lifeTime++;
			WorldCoordinate worldCoordinate = room.GetWorldCoordinate(pos);
			if (!init)
			{
				init = true;
				for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
				{
					if (room.abstractRoom.creatures[i].realizedCreature != null && !room.abstractRoom.creatures[i].realizedCreature.dead && (fearScavs || room.abstractRoom.creatures[i].creatureTemplate.type != CreatureTemplate.Type.Scavenger) && Custom.DistLess(room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, pos, fearRange))
					{
						if (room.abstractRoom.creatures[i].abstractAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker != null)
						{
							threatPoints.Add(room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker.AddThreatPoint(null, worldCoordinate, 1f));
							MakeCreatureLeaveRoom(room.abstractRoom.creatures[i].abstractAI.RealAI);
						}
						if (room.abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.GarbageWorm)
						{
							(room.abstractRoom.creatures[i].realizedCreature as GarbageWorm).AI.stress = 1f;
							(room.abstractRoom.creatures[i].realizedCreature as GarbageWorm).Retract();
						}
					}
				}
			}
			for (int j = 0; j < threatPoints.Count; j++)
			{
				threatPoints[j].severity = Mathf.InverseLerp(700f, 500f, lifeTime);
				threatPoints[j].pos = worldCoordinate;
			}
			if (lifeTime > 700)
			{
				Destroy();
			}
		}

		private void MakeCreatureLeaveRoom(ArtificialIntelligence AI)
		{
			if (AI.creature.abstractAI.destination.room != room.abstractRoom.index)
			{
				return;
			}
			int num = AI.threatTracker.FindMostAttractiveExit();
			if (num > -1 && num < room.abstractRoom.nodes.Length && room.abstractRoom.nodes[num].type == AbstractRoomNode.Type.Exit)
			{
				int num2 = room.world.GetAbstractRoom(room.abstractRoom.connections[num]).ExitIndex(room.abstractRoom.index);
				if (num2 > -1)
				{
					Custom.Log("migrate");
					AI.creature.abstractAI.MigrateTo(new WorldCoordinate(room.abstractRoom.connections[num], -1, -1, num2));
				}
			}
		}

		public override void Destroy()
		{
			for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
			{
				if (room.abstractRoom.creatures[i].realizedCreature != null && !room.abstractRoom.creatures[i].realizedCreature.dead && room.abstractRoom.creatures[i].abstractAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker != null)
				{
					for (int j = 0; j < threatPoints.Count; j++)
					{
						room.abstractRoom.creatures[i].abstractAI.RealAI.threatTracker.RemoveThreatPoint(threatPoints[j]);
					}
				}
			}
			base.Destroy();
		}
	}

	public Part[] stalk;

	public Part[] lumps;

	private Vector2? growPos;

	public Color explodeColor = new Color(1f, 0.4f, 0.3f);

	public int[] lumpConnections;

	public Vector2[] lumpDirs;

	public float[] lumpDetailRotations;

	public bool[] lumpsPopped;

	public float swallowed;

	public int fuseCounter;

	public ScareObject scareObj;

	public int StalkSprite => 0;

	public int TotalSprites => 1 + lumps.Length * 2;

	public AbstractConsumable AbstrConsumable => abstractPhysicalObject as AbstractConsumable;

	float IProvideWarmth.warmth => RainWorldGame.DefaultHeatSourceWarmth * 0.1f;

	Room IProvideWarmth.loadedRoom => room;

	float IProvideWarmth.range => 30f;

	public int LumpSprite(int l, int p)
	{
		return 1 + p * lumps.Length + l;
	}

	public FirecrackerPlant(AbstractPhysicalObject abstractPhysicalObject, World world)
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
		base.firstChunk.loudness = 2f;
		stalk = new Part[8];
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i] = new Part(this);
		}
		lumps = new Part[UnityEngine.Random.Range(6, 13)];
		lumpConnections = new int[lumps.Length];
		lumpDirs = new Vector2[lumps.Length];
		lumpDetailRotations = new float[lumps.Length];
		lumpsPopped = new bool[lumps.Length];
		float num = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
		for (int j = 0; j < lumps.Length; j++)
		{
			lumpConnections[j] = j / 2;
			lumpDirs[j] = (new Vector2((j % 2 == 0) ? (0f - num) : num, 0f) + Custom.RNV() * 0.5f).normalized;
			lumpDetailRotations[j] = UnityEngine.Random.value * 360f;
			lumps[j] = new Part(this);
			lumps[j].rad = Custom.LerpMap(lumps.Length, 6f, 12f, 1.6f, 1.2f) + Mathf.Lerp(0.75f, 1.25f, UnityEngine.Random.value) * (float)j * Custom.LerpMap(lumps.Length, 6f, 12f, 0.4f, 0.1f, 0.8f) * ((j == lumps.Length - 1) ? 0.5f : 1f);
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
		if (ModManager.MMF && room.game.IsArenaSession && (MMF.cfgSandboxItemStems.Value || room.game.GetArenaGameSession.chMeta != null) && room.game.GetArenaGameSession.counter < 10)
		{
			bool flag = false;
			for (int i = 1; i < 5; i++)
			{
				if (room.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -i)).Solid)
				{
					if (!room.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 2)).Solid)
					{
						IntVector2 pos = abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 2);
						growPos = room.MiddleOfTile(pos) + new Vector2(0f, -30f);
						base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos) + new Vector2(0f, -10f));
						flag = true;
						break;
					}
					if (!room.GetTile(abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 1)).Solid)
					{
						_ = abstractPhysicalObject.pos.Tile + new IntVector2(0, -i + 1);
						base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
						flag = true;
						break;
					}
				}
			}
			if (!flag)
			{
				base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
			}
		}
		else if (!AbstrConsumable.isConsumed && AbstrConsumable.placedObjectIndex >= 0 && AbstrConsumable.placedObjectIndex < placeRoom.roomSettings.placedObjects.Count)
		{
			IntVector2 tilePosition = room.GetTilePosition(placeRoom.roomSettings.placedObjects[AbstrConsumable.placedObjectIndex].pos);
			growPos = room.MiddleOfTile(tilePosition) + new Vector2(0f, -10f);
			base.firstChunk.HardSetPosition(growPos.Value + new Vector2(0f, 30f));
		}
		else
		{
			base.firstChunk.HardSetPosition(placeRoom.MiddleOfTile(abstractPhysicalObject.pos));
		}
		rotationSpeed = 0f;
	}

	public override void NewRoom(Room newRoom)
	{
		base.NewRoom(newRoom);
		ResetParts();
	}

	public void ResetParts()
	{
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i].Reset();
		}
		for (int j = 0; j < lumps.Length; j++)
		{
			lumps[j].Reset();
		}
	}

	public override void ChangeMode(Mode newMode)
	{
		if (fuseCounter == 0 && newMode == Mode.Thrown)
		{
			Ignite();
		}
		base.ChangeMode(newMode);
	}

	public override void HitByWeapon(Weapon weapon)
	{
		base.HitByWeapon(weapon);
		if (!AbstrConsumable.isConsumed)
		{
			AbstrConsumable.Consume();
		}
		if (growPos.HasValue)
		{
			growPos = null;
		}
	}

	public override void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
		base.HitByExplosion(hitFac, explosion, hitChunk);
		if (UnityEngine.Random.value < hitFac)
		{
			Ignite();
		}
	}

	public void Ignite()
	{
		if (fuseCounter <= 0)
		{
			fuseCounter = 60;
			room.PlaySound(SoundID.Firecracker_Burn, base.firstChunk);
			if (!AbstrConsumable.isConsumed)
			{
				AbstrConsumable.Consume();
			}
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (base.Submersion >= 0.2f && room.waterObject.WaterIsLethal && fuseCounter == 0)
		{
			Ignite();
		}
		if (fuseCounter > 0)
		{
			fuseCounter--;
			if (stalk.Length != 0)
			{
				room.AddObject(new Spark(stalk[UnityEngine.Random.Range(0, stalk.Length)].pos, Custom.RNV() * Mathf.Lerp(5f, 11f, UnityEngine.Random.value), explodeColor, null, 7, 17));
			}
			room.MakeBackgroundNoise(0.9f);
			for (int i = 0; i < stalk.Length; i++)
			{
				stalk[i].vel += Custom.RNV() * UnityEngine.Random.value * 4f;
			}
			if (fuseCounter < 45 && UnityEngine.Random.value < 0.05f)
			{
				int num = UnityEngine.Random.Range(0, lumps.Length);
				if (num < lumpsPopped.Length && !lumpsPopped[num])
				{
					PopLump(num);
					fuseCounter = Math.Max(fuseCounter, 2);
				}
			}
			int num2 = 0;
			for (int j = 0; j < lumpsPopped.Length; j++)
			{
				if (!lumpsPopped[j])
				{
					num2++;
				}
			}
			if (fuseCounter < 1)
			{
				for (int num3 = lumps.Length - 1; num3 >= 0; num3--)
				{
					if (!lumpsPopped[num3])
					{
						PopLump(num3);
						num2--;
						break;
					}
				}
				fuseCounter = 3;
			}
			if (num2 < 1)
			{
				Explode();
			}
		}
		if (growPos.HasValue && !Custom.DistLess(base.firstChunk.pos, growPos.Value, 100f))
		{
			growPos = null;
		}
		if (grabbedBy.Count > 0)
		{
			rotation = Custom.PerpendicularVector(Custom.DirVec(base.firstChunk.pos, grabbedBy[0].grabber.mainBodyChunk.pos));
			rotation.y = Mathf.Abs(rotation.y);
			if (!AbstrConsumable.isConsumed)
			{
				AbstrConsumable.Consume();
			}
			if (growPos.HasValue)
			{
				growPos = null;
			}
		}
		else if (!growPos.HasValue && base.firstChunk.ContactPoint.y == 0 && base.firstChunk.ContactPoint.x == 0)
		{
			rotation += base.firstChunk.pos - stalk[2].pos;
			rotation.Normalize();
		}
		if (setRotation.HasValue)
		{
			rotation = setRotation.Value;
			setRotation = null;
		}
		for (int k = 0; k < stalk.Length; k++)
		{
			stalk[k].Update();
			if (!growPos.HasValue)
			{
				stalk[k].vel.y -= Mathf.InverseLerp(0f, stalk.Length - 1, k) * 0.4f;
			}
		}
		for (int l = 0; l < lumps.Length; l++)
		{
			lumps[l].Update();
			Vector2 vector = ((lumpConnections[l] != 0) ? Custom.RotateAroundOrigo(lumpDirs[l], Custom.AimFromOneVectorToAnother(stalk[lumpConnections[l]].pos, stalk[lumpConnections[l] - 1].pos)) : Custom.RotateAroundOrigo(lumpDirs[l], Custom.AimFromOneVectorToAnother(stalk[lumpConnections[l] + 1].pos, stalk[lumpConnections[l]].pos)));
			lumps[l].vel += vector;
			stalk[lumpConnections[l]].vel -= vector;
			if (!growPos.HasValue)
			{
				lumps[l].vel.y -= 0.9f;
			}
		}
		for (int m = 0; m < stalk.Length; m++)
		{
			ConnectStalkSegment(m);
		}
		for (int num4 = stalk.Length - 1; num4 >= 0; num4--)
		{
			ConnectStalkSegment(num4);
		}
		for (int n = 0; n < lumps.Length; n++)
		{
			ConnectLump(n);
		}
		for (int num5 = 0; num5 < stalk.Length; num5++)
		{
			if (num5 > 1)
			{
				Vector2 vector2 = Custom.DirVec(stalk[num5].pos, stalk[num5 - 2].pos);
				stalk[num5].vel -= vector2 * 8.5f;
				stalk[num5 - 2].vel += vector2 * 8.5f;
			}
		}
		for (int num6 = 0; num6 < stalk.Length; num6++)
		{
			ConnectStalkSegment(num6);
		}
		for (int num7 = stalk.Length - 1; num7 >= 0; num7--)
		{
			ConnectStalkSegment(num7);
		}
		for (int num8 = 0; num8 < lumps.Length; num8++)
		{
			ConnectLump(num8);
		}
		if (growPos.HasValue)
		{
			stalk[stalk.Length - 1].pos = growPos.Value + new Vector2(0f, -7f);
			stalk[stalk.Length - 1].vel *= 0f;
			base.firstChunk.vel.y += base.gravity;
			base.firstChunk.vel += (growPos.Value + new Vector2(0f, 30f) - base.firstChunk.pos) / 100f;
			if (!Custom.DistLess(base.firstChunk.pos, growPos.Value, 50f))
			{
				base.firstChunk.pos = growPos.Value + Custom.DirVec(growPos.Value, base.firstChunk.pos) * 50f;
			}
			if (grabbedBy.Count > 0)
			{
				growPos = null;
			}
		}
		bool flag = false;
		if (base.mode == Mode.Carried)
		{
			stalk[3].vel += base.firstChunk.pos - stalk[3].pos;
			stalk[3].pos = base.firstChunk.pos;
			if (grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (grabbedBy[0].grabber as Player).swallowAndRegurgitateCounter > 50 && (grabbedBy[0].grabber as Player).objectInStomach == null && (grabbedBy[0].grabber as Player).input[0].pckp)
			{
				int num9 = -1;
				for (int num10 = 0; num10 < 2; num10++)
				{
					if ((grabbedBy[0].grabber as Player).grasps[num10] != null && (grabbedBy[0].grabber as Player).CanBeSwallowed((grabbedBy[0].grabber as Player).grasps[num10].grabbed))
					{
						num9 = num10;
						break;
					}
				}
				if (num9 > -1 && (grabbedBy[0].grabber as Player).grasps[num9] != null && (grabbedBy[0].grabber as Player).grasps[num9].grabbed == this)
				{
					flag = true;
				}
			}
		}
		swallowed = Custom.LerpAndTick(swallowed, flag ? 1f : 0f, 0.05f, 0.05f);
		if (scareObj != null)
		{
			scareObj.pos = base.firstChunk.pos;
		}
	}

	private void PopLump(int lmp)
	{
		lumpsPopped[lmp] = true;
		for (int i = 0; i < stalk.Length; i++)
		{
			stalk[i].vel += Custom.RNV() * UnityEngine.Random.value * 14f;
		}
		for (int j = 0; j < lumps.Length; j++)
		{
			lumps[j].vel += Custom.RNV() * UnityEngine.Random.value * 14f;
		}
		base.firstChunk.vel += Custom.RNV() * UnityEngine.Random.value * 14f;
		for (int num = UnityEngine.Random.Range(1, 6); num >= 0; num--)
		{
			room.AddObject(new Spark(lumps[lmp].pos, Custom.RNV() * Mathf.Lerp(15f, 30f, UnityEngine.Random.value), explodeColor, null, 7, 17));
		}
		room.AddObject(new Explosion.FlashingSmoke(lumps[lmp].pos, Custom.RNV() * 5f * UnityEngine.Random.value, 1f, new Color(1f, 1f, 1f), explodeColor, 5));
		Explosion.ExplosionLight obj = new Explosion.ExplosionLight(lumps[lmp].pos, Mathf.Lerp(50f, 150f, UnityEngine.Random.value), 0.5f, 4, explodeColor);
		room.AddObject(obj);
		room.PlaySound(SoundID.Firecracker_Bang, lumps[lmp].pos);
		for (int k = 0; k < room.abstractRoom.creatures.Count; k++)
		{
			if (room.abstractRoom.creatures[k].realizedCreature != null && room.abstractRoom.creatures[k].realizedCreature.room == room && !room.abstractRoom.creatures[k].realizedCreature.dead)
			{
				room.abstractRoom.creatures[k].realizedCreature.Deafen((int)Custom.LerpMap(Vector2.Distance(lumps[lmp].pos, room.abstractRoom.creatures[k].realizedCreature.mainBodyChunk.pos), 40f, 80f, 110f, 0f));
				room.abstractRoom.creatures[k].realizedCreature.Stun((int)Custom.LerpMap(Vector2.Distance(lumps[lmp].pos, room.abstractRoom.creatures[k].realizedCreature.mainBodyChunk.pos), 40f, 80f, 10f, 0f));
			}
		}
		if (scareObj == null)
		{
			scareObj = new ScareObject(base.firstChunk.pos);
			room.AddObject(scareObj);
		}
	}

	private void Explode()
	{
		Explosion obj = new Explosion(room, this, base.firstChunk.pos, 6, 60f, 5f, 0.1f, 20f, 0.5f, null, 1f, 0f, 1f);
		room.AddObject(obj);
		Explosion.ExplosionLight obj2 = new Explosion.ExplosionLight(base.firstChunk.pos, 200f, 0.9f, 8, explodeColor);
		room.AddObject(obj2);
		for (int i = 0; i < 6; i++)
		{
			room.AddObject(new Explosion.FlashingSmoke(base.firstChunk.pos, Custom.RNV() * 3f * UnityEngine.Random.value, 1f, new Color(1f, 1f, 1f), explodeColor, 11));
		}
		room.PlaySound(SoundID.Firecracker_Disintegrate, base.firstChunk.pos);
		room.InGameNoise(new InGameNoise(base.firstChunk.pos, 8000f, this, 1f));
		Destroy();
	}

	private void ConnectStalkSegment(int i)
	{
		float num = 5f * (1f - swallowed);
		if (i == 3)
		{
			Vector2 vector = Custom.DirVec(stalk[i].pos, base.firstChunk.pos);
			float num2 = Vector2.Distance(stalk[i].pos, base.firstChunk.pos);
			stalk[i].pos -= (num - num2) * vector * 0.9f;
			stalk[i].vel -= (num - num2) * vector * 0.9f;
			base.firstChunk.pos += (num - num2) * vector * 0.1f;
			base.firstChunk.vel += (num - num2) * vector * 0.1f;
		}
		if (i > 0)
		{
			Vector2 vector2 = Custom.DirVec(stalk[i].pos, stalk[i - 1].pos);
			float num3 = Vector2.Distance(stalk[i].pos, stalk[i - 1].pos);
			stalk[i].pos -= (num - num3) * vector2 * 0.5f;
			stalk[i].vel -= (num - num3) * vector2 * 0.5f;
			stalk[i - 1].pos += (num - num3) * vector2 * 0.5f;
			stalk[i - 1].vel += (num - num3) * vector2 * 0.5f;
		}
	}

	private void ConnectLump(int i)
	{
		int num = lumpConnections[i];
		float num2 = (lumps[i].rad * 1.1f + 0.2f) * (1f - swallowed);
		Vector2 vector = Custom.DirVec(lumps[i].pos, stalk[num].pos);
		float num3 = Vector2.Distance(lumps[i].pos, stalk[num].pos);
		lumps[i].pos -= (num2 - num3) * vector * 0.5f;
		lumps[i].vel -= (num2 - num3) * vector * 0.5f;
		stalk[num].pos += (num2 - num3) * vector * 0.5f;
		stalk[num].vel += (num2 - num3) * vector * 0.5f;
		num2 = (6f + (float)Mathf.Abs(lumpConnections[i] - 3) * 5f) * (1f - swallowed);
		num3 = Vector2.Distance(lumps[i].pos, base.firstChunk.pos);
		if (num3 > num2)
		{
			vector = Custom.DirVec(lumps[i].pos, base.firstChunk.pos);
			lumps[i].pos -= (num2 - num3) * vector * 0.9f;
			lumps[i].vel -= (num2 - num3) * vector * 0.9f;
			base.firstChunk.pos += (num2 - num3) * vector * 0.1f;
			base.firstChunk.vel += (num2 - num3) * vector * 0.1f;
		}
	}

	public override bool HitSomething(SharedPhysics.CollisionResult result, bool eu)
	{
		if (result.obj == null)
		{
			return false;
		}
		vibrate = 20;
		ChangeMode(Mode.Free);
		base.firstChunk.vel = base.firstChunk.vel * -0.5f + Custom.DegToVec(UnityEngine.Random.value * 360f) * Mathf.Lerp(0.05f, 0.2f, UnityEngine.Random.value) * base.firstChunk.vel.magnitude;
		SetRandomSpin();
		return true;
	}

	public override void Thrown(Creature thrownBy, Vector2 thrownPos, Vector2? firstFrameTraceFromPos, IntVector2 throwDir, float frc, bool eu)
	{
		base.Thrown(thrownBy, thrownPos, firstFrameTraceFromPos, throwDir, frc, eu);
		room?.PlaySound(SoundID.Slugcat_Throw_Rock, base.firstChunk);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[TotalSprites];
		sLeaser.sprites[StalkSprite] = TriangleMesh.MakeLongMesh(stalk.Length, pointyTip: false, customColor: true);
		for (int i = 0; i < lumps.Length; i++)
		{
			sLeaser.sprites[LumpSprite(i, 0)] = new FSprite("Circle20");
			sLeaser.sprites[LumpSprite(i, 0)].scaleX = lumps[i].rad / 9f;
			sLeaser.sprites[LumpSprite(i, 0)].scaleY = lumps[i].rad / 7f;
			sLeaser.sprites[LumpSprite(i, 1)] = new FSprite("ScavengerHandA");
			sLeaser.sprites[LumpSprite(i, 1)].scale = lumps[i].rad / 12f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < lumps.Length; i++)
		{
			if (lumpsPopped[i])
			{
				sLeaser.sprites[LumpSprite(i, 0)].isVisible = false;
				sLeaser.sprites[LumpSprite(i, 1)].isVisible = false;
				continue;
			}
			sLeaser.sprites[LumpSprite(i, 0)].isVisible = true;
			sLeaser.sprites[LumpSprite(i, 1)].isVisible = true;
			Vector2 vector = Vector2.Lerp(lumps[i].lastPos, lumps[i].pos, timeStacker);
			Vector2 vector2 = Vector2.Lerp(stalk[lumpConnections[i]].lastPos, stalk[lumpConnections[i]].pos, timeStacker);
			sLeaser.sprites[LumpSprite(i, 0)].x = vector.x - camPos.x;
			sLeaser.sprites[LumpSprite(i, 0)].y = vector.y - camPos.y;
			sLeaser.sprites[LumpSprite(i, 0)].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
			vector += Custom.DirVec(vector2, vector) * lumps[i].rad * 0.5f;
			sLeaser.sprites[LumpSprite(i, 1)].x = vector.x - camPos.x;
			sLeaser.sprites[LumpSprite(i, 1)].y = vector.y - camPos.y;
			sLeaser.sprites[LumpSprite(i, 1)].rotation = Custom.AimFromOneVectorToAnother(vector, vector2) + lumpDetailRotations[i];
		}
		Vector2 vector3 = Vector2.Lerp(stalk[0].lastPos, stalk[0].pos, timeStacker);
		vector3 += Custom.DirVec(Vector2.Lerp(stalk[1].lastPos, stalk[1].pos, timeStacker), vector3) * 4f;
		float num = 1.2f;
		for (int j = 0; j < stalk.Length; j++)
		{
			Vector2 vector4 = Vector2.Lerp(stalk[j].lastPos, stalk[j].pos, timeStacker);
			Vector2 normalized = (vector4 - vector3).normalized;
			Vector2 vector5 = Custom.PerpendicularVector(normalized);
			float num2 = Vector2.Distance(vector4, vector3) / 5f;
			if (j == 0)
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * num - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * num - camPos);
			}
			else
			{
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4, vector3 - vector5 * num + normalized * num2 - camPos);
				(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 1, vector3 + vector5 * num + normalized * num2 - camPos);
			}
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 2, vector4 - vector5 * num - normalized * num2 - camPos);
			(sLeaser.sprites[StalkSprite] as TriangleMesh).MoveVertice(j * 4 + 3, vector4 + vector5 * num - normalized * num2 - camPos);
			vector3 = vector4;
		}
		if (blink > 0)
		{
			Color color = base.color;
			if (blink > 1 && UnityEngine.Random.value < 0.5f)
			{
				color = base.blinkColor;
			}
			sLeaser.sprites[StalkSprite].color = color;
			for (int k = 0; k < lumps.Length; k++)
			{
				sLeaser.sprites[LumpSprite(k, 0)].color = color;
			}
		}
		else if (sLeaser.sprites[StalkSprite].color != base.color)
		{
			sLeaser.sprites[StalkSprite].color = base.color;
			for (int l = 0; l < lumps.Length; l++)
			{
				sLeaser.sprites[LumpSprite(l, 0)].color = base.color;
			}
		}
		for (int m = 0; m < lumps.Length; m++)
		{
			sLeaser.sprites[LumpSprite(m, 1)].color = Color.Lerp(explodeColor, new Color(1f, 0f, 0f), Mathf.Lerp(0.25f, 0.75f, UnityEngine.Random.value));
		}
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		color = palette.blackColor;
		sLeaser.sprites[StalkSprite].color = color;
		for (int i = 0; i < lumps.Length; i++)
		{
			sLeaser.sprites[LumpSprite(i, 0)].color = color;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
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

	Vector2 IProvideWarmth.Position()
	{
		return base.firstChunk.pos;
	}
}
