using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class Fly : Creature, IPlayerEdible
{
	public class MovementMode : ExtEnum<MovementMode>
	{
		public static readonly MovementMode BatFlight = new MovementMode("BatFlight", register: true);

		public static readonly MovementMode SwarmFlight = new MovementMode("SwarmFlight", register: true);

		public static readonly MovementMode Passive = new MovementMode("Passive", register: true);

		public static readonly MovementMode Burrow = new MovementMode("Burrow", register: true);

		public static readonly MovementMode Panic = new MovementMode("Panic", register: true);

		public static readonly MovementMode Hang = new MovementMode("Hang", register: true);

		public MovementMode(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public float flap;

	public float flapSpeed;

	public float flapDepth;

	public float lastFlapDepth;

	private ChunkSoundEmitter hoverSound;

	public float drown;

	public MovementMode movMode;

	public int eaten;

	public Vector2? burrowOrHangSpot;

	public Vector2 dir;

	public float wallAvoidance;

	public int chainBehaviorVariation;

	public FlyAI AI;

	public bool everBeenCaughtByPlayer;

	public int bites;

	public Vector2 localGoal => AI.localGoal;

	public FlyAI.Behavior CurrentBehavior => AI.behavior;

	public bool PlayerAutoGrabable
	{
		get
		{
			if (base.Consious && (grabbedBy.Count == 0 || grabbedBy[0].grabber is Fly))
			{
				return shortcutDelay < 1;
			}
			return false;
		}
	}

	public int BitesLeft => bites;

	public int FoodPoints => 1;

	public bool Edible => true;

	public bool AutomaticPickUp => true;

	public Fly(AbstractCreature abstractCreature, World world)
		: base(abstractCreature, world)
	{
		base.bodyChunks = new BodyChunk[1];
		base.bodyChunks[0] = new BodyChunk(this, 0, new Vector2(0f, 0f), 6f, 0.05f);
		bodyChunkConnections = new BodyChunkConnection[0];
		base.GoThroughFloors = true;
		flap = UnityEngine.Random.value;
		flapSpeed = UnityEngine.Random.value + 0.1f;
		AI = new FlyAI(this, world);
		base.airFriction = 0.98f;
		base.gravity = 0.9f;
		bounce = 0.1f;
		surfaceFriction = 0.5f;
		collisionLayer = 0;
		base.waterFriction = 0.9f;
		base.buoyancy = 0.94f;
		bites = 3;
	}

	public override void InitiateGraphicsModule()
	{
		if (base.graphicsModule == null)
		{
			base.graphicsModule = new FlyGraphics(this);
		}
	}

	public override void SpitOutOfShortCut(IntVector2 pos, Room newRoom, bool spitOutAllSticks)
	{
		base.SpitOutOfShortCut(pos, newRoom, spitOutAllSticks);
		shortcutDelay = 40;
		Vector2 vector = Custom.IntVector2ToVector2(newRoom.ShorcutEntranceHoleDirection(pos));
		base.mainBodyChunk.pos = newRoom.MiddleOfTile(pos) - vector * 5f;
		base.mainBodyChunk.lastPos = base.mainBodyChunk.pos;
		base.mainBodyChunk.vel = vector * 15f;
		if (base.graphicsModule != null)
		{
			base.graphicsModule.Reset();
		}
	}

	public override void PlaceInRoom(Room placeRoom)
	{
		base.PlaceInRoom(placeRoom);
	}

	public override void NewRoom(Room room)
	{
		base.NewRoom(room);
		ReportToFliesRoomAI(room);
		AI.NewRoom();
		ChangeCollisionLayer(0);
	}

	private void ReportToFliesRoomAI(Room newRoom)
	{
		if (newRoom.fliesRoomAi == null)
		{
			newRoom.fliesRoomAi = new FliesRoomAI(newRoom);
		}
		newRoom.fliesRoomAi.AddFly(this);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (room == null)
		{
			return;
		}
		lastFlapDepth = flapDepth;
		base.CollideWithTerrain = (grabbedBy.Count == 0 || grabbedBy[0].grabber is Fly) && (!base.Consious || movMode != MovementMode.Burrow);
		if (!base.dead)
		{
			drown = Mathf.Clamp(drown + 0.0125f * ((base.mainBodyChunk.submersion == 1f) ? 1f : (-1f)), 0f, 1f);
			if (drown == 1f)
			{
				Die();
			}
			if (UnityEngine.Random.value < 1f / 160f && grabbedBy.Count > 0 && grabbedBy[0].grabber is Player && (!ModManager.MSC || (grabbedBy[0].grabber as Player).SlugCatClass != MoreSlugcatsEnums.SlugcatStatsName.Saint))
			{
				Die();
			}
		}
		if (room.game.devToolsActive && Input.GetKey("f"))
		{
			base.mainBodyChunk.vel += Custom.DirVec(base.mainBodyChunk.pos, (Vector2)Futile.mousePosition + room.game.cameras[0].pos) * 3f;
		}
		if (base.Consious && !base.inShortcut)
		{
			if (base.mainBodyChunk.submersion == 0f)
			{
				Act(eu);
			}
			else
			{
				WaterBehavior();
			}
			if (room == null)
			{
				return;
			}
			if (shortcutDelay == 0 && room.GetTile(base.bodyChunks[0].pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(room.GetTilePosition(base.bodyChunks[0].pos)).shortCutType != ShortcutData.Type.DeadEnd)
			{
				enteringShortCut = room.GetTilePosition(base.bodyChunks[0].pos);
				LoseAllGrasps();
			}
		}
		else
		{
			flapDepth = 1f;
		}
		if (base.grasps[0] != null)
		{
			float num = Vector2.Distance(base.mainBodyChunk.pos, base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos);
			if (num > 8f)
			{
				Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos);
				base.mainBodyChunk.pos -= (8f - num) * vector * 0.5f;
				base.mainBodyChunk.vel -= (8f - num) * vector * 0.5f;
				Fly fly = FirstInChain();
				base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed].vel += (8f - num) * vector * 0.25f;
				if (fly.movMode != MovementMode.Hang || !fly.burrowOrHangSpot.HasValue)
				{
					base.grasps[0].grabbed.bodyChunks[base.grasps[0].chunkGrabbed].pos += (8f - num) * vector * 0.25f;
				}
			}
		}
		if (eaten > 0)
		{
			eaten--;
			if (eaten == 0)
			{
				Destroy();
			}
		}
	}

	public void Act(bool eu)
	{
		AI.Update();
		if (AI.Stuck)
		{
			base.mainBodyChunk.vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 2f;
		}
		if (movMode == MovementMode.BatFlight)
		{
			BatFlight(panic: false);
		}
		else if (movMode == MovementMode.SwarmFlight)
		{
			SwarmFlight();
		}
		else if (movMode == MovementMode.Burrow)
		{
			if (burrowOrHangSpot.HasValue)
			{
				base.mainBodyChunk.pos = burrowOrHangSpot.Value + new Vector2(Mathf.Lerp(-2f, 2f, UnityEngine.Random.value), 0f);
				burrowOrHangSpot = burrowOrHangSpot.Value + new Vector2(0f, -0.4f);
				if (room.GetTile(burrowOrHangSpot.Value + new Vector2(0f, 5f)).Terrain == Room.Tile.TerrainType.Solid)
				{
					Burrowed();
				}
				_ = room;
			}
			else
			{
				burrowOrHangSpot = null;
			}
		}
		else if (movMode == MovementMode.Panic)
		{
			BatFlight(panic: true);
		}
		else if (movMode == MovementMode.Hang)
		{
			Hanging();
		}
	}

	private void Burrowed()
	{
		AI.Burrowed();
		room.PlaySound(SoundID.Bat_Dive_Into_Grass, base.mainBodyChunk.pos);
		if (room.fliesRoomAi != null)
		{
			room.fliesRoomAi.MoveFlyToHive(this);
		}
	}

	private void WaterBehavior()
	{
		if (base.mainBodyChunk.submersion == 1f)
		{
			flap = Mathf.Clamp(flap + Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 0.1f, 0f, 1f);
			base.mainBodyChunk.vel += Custom.DegToVec(-45f + 90f * UnityEngine.Random.value) * 0.75f;
		}
		else if (grabbedBy.Count == 0 || base.grasps[0] == null)
		{
			base.mainBodyChunk.vel.y += 7f;
		}
	}

	private void BatFlight(bool panic)
	{
		if (!panic)
		{
			dir = Custom.DirVec(base.mainBodyChunk.pos, localGoal);
			if (room.GetTile(base.mainBodyChunk.pos + base.mainBodyChunk.vel * 3f).Terrain == Room.Tile.TerrainType.Solid)
			{
				dir = Vector2.Lerp(dir, -base.mainBodyChunk.vel.normalized, 0.3f);
			}
			bool flag = false;
			if (!AI.Stuck && room.aimap.getTerrainProximity(localGoal) > 1)
			{
				int terrainProximity = room.aimap.getTerrainProximity(base.mainBodyChunk.pos);
				Vector2 vector = new Vector2(0f, 0f);
				if (terrainProximity < 5)
				{
					for (int i = 0; i < 8; i++)
					{
						if (room.aimap.getTerrainProximity(room.GetTilePosition(base.mainBodyChunk.pos) + Custom.eightDirections[i]) < terrainProximity)
						{
							vector += Custom.DirVec(base.mainBodyChunk.pos, room.MiddleOfTile(room.GetTilePosition(base.mainBodyChunk.pos) - Custom.eightDirections[i]));
							flag = true;
						}
					}
				}
				if (flag)
				{
					dir = Vector3.Slerp(dir, vector.normalized, wallAvoidance * 0.3f);
				}
			}
			if (flag)
			{
				wallAvoidance -= 0.05f;
			}
			else
			{
				wallAvoidance += 0.025f;
			}
			wallAvoidance = Mathf.Clamp(wallAvoidance, 0f, 1f);
		}
		base.mainBodyChunk.vel *= 0.92f;
		base.mainBodyChunk.vel.y += base.gravity * 0.45f;
		base.mainBodyChunk.vel.x += dir.x * 0.6f;
		if (UnityEngine.Random.value < 0.025f)
		{
			room.PlaySound((UnityEngine.Random.value < AI.afraid) ? SoundID.Bat_Afraid_Flying_Sounds : SoundID.Bat_Idle_Flying_Sounds, base.mainBodyChunk);
		}
		if (panic)
		{
			base.mainBodyChunk.vel += dir * 0.2f;
		}
		flapDepth = Mathf.InverseLerp(-1f, 1f, dir.y);
		flap += flapSpeed * (panic ? 1.2f : 1f);
		if (flapSpeed > 0f)
		{
			base.mainBodyChunk.vel.y += Mathf.Pow(1f + flapSpeed, 1.5f) * Mathf.Lerp(1.1f, 1.7f, flapDepth);
			if (flap > 1f)
			{
				flap = 1f;
				flapSpeed = -0.04f - Mathf.InverseLerp(-1f, 1f, dir.y) * 0.15f;
			}
			if (base.mainBodyChunk.ContactPoint.x != 0)
			{
				base.mainBodyChunk.vel.x = (float)base.mainBodyChunk.ContactPoint.x * -6f;
			}
		}
		else if (flapSpeed < 0f && flap < 0f)
		{
			flap = 0f;
			flapSpeed = 0.2f + Mathf.InverseLerp(-1f, 1f, dir.y) * 0.8f;
			room.PlaySound(SoundID.Fly_Wing_Flap, base.mainBodyChunk);
		}
	}

	private void SwarmFlight()
	{
		float num = AI.flockBehavior.sinCycle + localGoal.y / AI.flockBehavior.swarmFlightVerticalDisplace;
		num = Mathf.Sin(num * 2f * (float)Math.PI) * AI.flockBehavior.swarmFlightSinAmpl;
		dir = Custom.DirVec(base.mainBodyChunk.pos, localGoal + new Vector2(num, 0f));
		base.mainBodyChunk.vel *= 0.89f;
		base.mainBodyChunk.vel.y += base.gravity * 0.9f;
		base.mainBodyChunk.vel += dir * 0.6f;
		if (hoverSound == null || hoverSound.slatedForDeletetion)
		{
			hoverSound = room.PlaySound(SoundID.Bat_Hover_Fly_LOOP, base.mainBodyChunk, loop: true, 1f, 1f);
			hoverSound.requireActiveUpkeep = true;
		}
		else
		{
			hoverSound.alive = true;
			hoverSound.pitch = 0.9f + 0.2f * Mathf.InverseLerp(-4f, 4f, base.mainBodyChunk.vel.y);
		}
		flap = 1f;
		flapDepth = 0.5f;
		flapSpeed = 1f;
		if (base.mainBodyChunk.ContactPoint.x != 0)
		{
			base.mainBodyChunk.vel.x = (float)base.mainBodyChunk.ContactPoint.x * -6f;
		}
	}

	private void Hanging()
	{
		if (base.grasps[0] == null && burrowOrHangSpot.HasValue)
		{
			float num = Vector2.Distance(base.mainBodyChunk.pos, burrowOrHangSpot.Value);
			if (num > 8f)
			{
				Vector2 vector = Custom.DirVec(base.mainBodyChunk.pos, burrowOrHangSpot.Value);
				base.mainBodyChunk.pos -= (8f - num) * vector;
				base.mainBodyChunk.vel -= (8f - num) * vector;
			}
		}
		flapDepth = 1f;
		if (chainBehaviorVariation == 0 && UnityEngine.Random.value < 0.00066666666f)
		{
			chainBehaviorVariation = UnityEngine.Random.Range(1, 5);
		}
		else if (UnityEngine.Random.value < 1f / 160f)
		{
			chainBehaviorVariation = 0;
		}
		switch (chainBehaviorVariation)
		{
		case 1:
			base.mainBodyChunk.vel.x += Mathf.Sin(AI.flockBehavior.sinCycle * 2f * (float)Math.PI);
			flapDepth = 0.1f;
			flap = UnityEngine.Random.value;
			break;
		case 2:
			flapDepth = 0.3f;
			flap = Mathf.Clamp(flap + UnityEngine.Random.value * UnityEngine.Random.value * 0.1f, 0f, 1f);
			break;
		case 3:
			base.mainBodyChunk.vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * 0.1f;
			flapDepth = 0.3f;
			flap = 1f;
			break;
		case 4:
			base.mainBodyChunk.vel += Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value * UnityEngine.Random.value * 0.2f;
			break;
		}
	}

	public Fly FirstInChain()
	{
		Fly fly = this;
		while (fly.grasps[0] != null && fly.grasps[0].grabbed is Fly)
		{
			fly = fly.grasps[0].grabbed as Fly;
		}
		return fly;
	}

	public Fly LastInChain()
	{
		Fly fly = this;
		while (fly.NextInChain() != null)
		{
			fly = fly.NextInChain();
		}
		return fly;
	}

	public Fly NextInChain()
	{
		for (int i = 0; i < grabbedBy.Count; i++)
		{
			if (grabbedBy[i].grabber is Fly)
			{
				if (grabbedBy[i].grabber.grasps[0] != null && grabbedBy[i].grabber.grasps[0].grabbed == this)
				{
					return grabbedBy[i].grabber as Fly;
				}
				return null;
			}
		}
		return null;
	}

	public bool CheckChainForLoops()
	{
		Fly fly = NextInChain();
		while (fly != null)
		{
			fly = fly.NextInChain();
			if (fly == this)
			{
				NextInChain().ReleaseGrasp(0);
				return true;
			}
		}
		return false;
	}

	public override void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
		base.Collide(otherObject, myChunk, otherChunk);
	}

	public override void Die()
	{
		surfaceFriction = 0.3f;
		base.Die();
	}

	public override void Grabbed(Grasp grasp)
	{
		if (grasp.grabber is Player)
		{
			for (int i = 0; i < room.game.cameras.Length; i++)
			{
				room.game.cameras[i].MoveObjectToContainer(base.graphicsModule, null);
			}
		}
		base.Grabbed(grasp);
	}

	public void BitByPlayer(Grasp grasp, bool eu)
	{
		bites--;
		if (!base.dead)
		{
			Die();
		}
		room.PlaySound((bites == 0) ? SoundID.Slugcat_Final_Bite_Fly : SoundID.Slugcat_Bite_Fly, base.mainBodyChunk.pos);
		base.mainBodyChunk.MoveFromOutsideMyUpdate(eu, grasp.grabber.mainBodyChunk.pos);
		if (bites < 1 && eaten == 0)
		{
			(grasp.grabber as Player).ObjectEaten(this);
			grasp.Release();
			eaten = 3;
		}
	}

	public void ThrowByPlayer()
	{
	}
}
