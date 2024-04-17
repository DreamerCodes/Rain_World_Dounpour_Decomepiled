using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Noise;
using RWCustom;
using UnityEngine;

public abstract class PhysicalObject : UpdatableAndDeletable
{
	public class BodyChunkConnection
	{
		public class Type : ExtEnum<Type>
		{
			public static readonly Type Normal = new Type("Normal", register: true);

			public static readonly Type Pull = new Type("Pull", register: true);

			public static readonly Type Push = new Type("Push", register: true);

			public Type(string value, bool register = false)
				: base(value, register)
			{
			}
		}

		public BodyChunk chunk1;

		public BodyChunk chunk2;

		public float distance;

		public float elasticity;

		public float weightSymmetry;

		public bool active;

		public Type type;

		public BodyChunkConnection(BodyChunk chunk1, BodyChunk chunk2, float distance, Type type, float elasticity, float weightSymmetry)
		{
			this.chunk1 = chunk1;
			this.chunk2 = chunk2;
			this.distance = distance;
			this.type = type;
			this.elasticity = elasticity;
			if (weightSymmetry == -1f)
			{
				this.weightSymmetry = chunk2.mass / (chunk1.mass + chunk2.mass);
			}
			else
			{
				this.weightSymmetry = weightSymmetry;
			}
			active = true;
			chunk1.rotationChunk = chunk2;
			chunk2.rotationChunk = chunk1;
		}

		public void Update()
		{
			if (active)
			{
				float num = Vector2.Distance(chunk1.pos, chunk2.pos);
				if (type == Type.Normal || (type == Type.Pull && num > distance) || (type == Type.Push && num < distance))
				{
					Vector2 vector = Custom.DirVec(chunk1.pos, chunk2.pos);
					chunk1.pos -= (distance - num) * vector * weightSymmetry * elasticity;
					chunk1.vel -= (distance - num) * vector * weightSymmetry * elasticity;
					chunk2.pos += (distance - num) * vector * (1f - weightSymmetry) * elasticity;
					chunk2.vel += (distance - num) * vector * (1f - weightSymmetry) * elasticity;
				}
			}
		}
	}

	public class Appendage
	{
		public class Pos
		{
			public Appendage appendage;

			public int prevSegment;

			public float distanceToNext;

			public Pos(Appendage appendage, int prevSegment, float distanceToNext)
			{
				this.appendage = appendage;
				this.prevSegment = prevSegment;
				this.distanceToNext = distanceToNext;
			}
		}

		public PhysicalObject owner;

		public IHaveAppendages ownerApps;

		public Vector2[] segments;

		public int appIndex;

		public float totalLength;

		public bool canBeHit = true;

		public Appendage(PhysicalObject owner, int appIndex, int totSegs)
		{
			this.owner = owner;
			this.appIndex = appIndex;
			ownerApps = owner as IHaveAppendages;
			segments = new Vector2[totSegs];
			for (int i = 0; i < totSegs; i++)
			{
				segments[i] = owner.FirstChunk().pos;
			}
		}

		public void Update()
		{
			totalLength = 0f;
			for (int i = 0; i < segments.Length; i++)
			{
				segments[i] = ownerApps.AppendagePosition(appIndex, i);
				if (i > 0)
				{
					totalLength += Vector2.Distance(segments[i - 1], segments[i]);
				}
			}
		}

		public Vector2 OnAppendagePosition(Pos pos)
		{
			return Vector2.Lerp(segments[pos.prevSegment], segments[pos.prevSegment + 1], pos.distanceToNext);
		}

		public Vector2 OnAppendageDirection(Pos pos)
		{
			return Vector3.Slerp(b: (pos.prevSegment >= segments.Length - 2) ? Custom.DirVec(segments[pos.prevSegment], segments[pos.prevSegment + 1]) : Custom.DirVec(segments[pos.prevSegment + 1], segments[pos.prevSegment + 2]), a: Custom.DirVec(segments[pos.prevSegment], segments[pos.prevSegment + 1]), t: pos.distanceToNext);
		}

		public bool LineCross(Vector2 A, Vector2 B)
		{
			for (int i = 1; i < segments.Length; i++)
			{
				Vector2 p = Custom.LineIntersection(A, B, segments[i - 1], segments[i]);
				if (Custom.DistLess(p, A, Vector2.Distance(A, B)) && Custom.DistLess(p, B, Vector2.Distance(A, B)) && Custom.DistLess(p, segments[i - 1], Vector2.Distance(segments[i - 1], segments[i])) && Custom.DistLess(p, segments[i], Vector2.Distance(segments[i - 1], segments[i])))
				{
					return true;
				}
			}
			return false;
		}
	}

	public interface IHaveAppendages
	{
		Vector2 AppendagePosition(int appendage, int segment);

		void ApplyForceOnAppendage(Appendage.Pos pos, Vector2 momentum);
	}

	public BodyChunkConnection[] bodyChunkConnections;

	public List<Creature.Grasp> grabbedBy;

	public float collisionRange;

	public int collisionLayer;

	public AbstractPhysicalObject abstractPhysicalObject;

	private float g;

	public float surfaceFriction;

	public float bounce;

	public float impactTreshhold = 1f;

	public float waterRetardationImmunity;

	public bool sticksRespawned;

	public List<Appendage> appendages;

	public bool canBeHitByWeapons = true;

	public int jollyBeingPointedCounter;

	public BodyChunk[] bodyChunks { get; protected set; }

	public GraphicsModule graphicsModule { get; protected set; }

	public BodyChunk firstChunk => bodyChunks[0];

	public float TotalMass
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				num += bodyChunks[i].mass;
			}
			return num;
		}
	}

	public BodyChunk RandomChunk
	{
		get
		{
			if (bodyChunks.Length != 0)
			{
				return bodyChunks[Random.Range(0, bodyChunks.Length)];
			}
			return null;
		}
	}

	public float gravity
	{
		get
		{
			return g * room.gravity;
		}
		protected set
		{
			g = value;
		}
	}

	public float airFriction { get; protected set; }

	public float waterFriction { get; protected set; }

	public float buoyancy { get; protected set; }

	public virtual float VisibilityBonus => 0f;

	public bool GoThroughFloors
	{
		get
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				if (!bodyChunks[i].goThroughFloors)
				{
					return false;
				}
			}
			return true;
		}
		set
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				bodyChunks[i].goThroughFloors = value;
			}
		}
	}

	public bool CollideWithTerrain
	{
		get
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				if (bodyChunks[i].collideWithTerrain)
				{
					return true;
				}
			}
			return false;
		}
		set
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				bodyChunks[i].collideWithTerrain = value;
			}
		}
	}

	public bool CollideWithSlopes
	{
		get
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				if (bodyChunks[i].collideWithSlopes)
				{
					return true;
				}
			}
			return false;
		}
		protected set
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				bodyChunks[i].collideWithSlopes = value;
			}
		}
	}

	public bool CollideWithObjects
	{
		get
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				if (bodyChunks[i].collideWithObjects && !bodyChunks[i].actAsTrigger)
				{
					return true;
				}
			}
			return false;
		}
		set
		{
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				bodyChunks[i].collideWithObjects = value;
			}
		}
	}

	public float Submersion
	{
		get
		{
			float num = 0f;
			for (int i = 0; i < bodyChunks.Length; i++)
			{
				num += bodyChunks[i].submersion;
			}
			return num / (float)bodyChunks.Length;
		}
	}

	public virtual float EffectiveRoomGravity
	{
		get
		{
			if (room != null)
			{
				return room.gravity;
			}
			return 0f;
		}
	}

	public Player LickedByPlayer
	{
		get
		{
			if (room.game.Players.Count == 0)
			{
				return null;
			}
			foreach (AbstractCreature player in room.game.Players)
			{
				if (player.realizedCreature != null && (player.realizedCreature as Player).tongue != null && (player.realizedCreature as Player).tongue.attachedChunk != null && (player.realizedCreature as Player).tongue.attachedChunk.owner == this)
				{
					return player.realizedCreature as Player;
				}
			}
			return null;
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public BodyChunk FirstChunk()
	{
		return bodyChunks[0];
	}

	public PhysicalObject(AbstractPhysicalObject abstractPhysicalObject)
	{
		this.abstractPhysicalObject = abstractPhysicalObject;
		if (abstractPhysicalObject != null)
		{
			abstractPhysicalObject.realizedObject = this;
		}
		grabbedBy = new List<Creature.Grasp>();
		collisionRange = 50f;
	}

	public virtual void NewRoom(Room newRoom)
	{
	}

	public override void Update(bool eu)
	{
		WeatherInertia();
		for (int i = 0; i < bodyChunks.Length; i++)
		{
			bodyChunks[i].Update();
		}
		abstractPhysicalObject.pos.Tile = room.GetTilePosition(FirstChunk().pos);
		for (int j = 0; j < bodyChunkConnections.Length; j++)
		{
			bodyChunkConnections[j].Update();
		}
		if (grabbedBy.Count > 0)
		{
			for (int num = grabbedBy.Count - 1; num >= 0; num--)
			{
				if (grabbedBy[num].discontinued || grabbedBy[num].grabber.grasps[grabbedBy[num].graspUsed] != grabbedBy[num])
				{
					grabbedBy.RemoveAt(num);
				}
			}
		}
		if (room.abstractRoom.index != abstractPhysicalObject.pos.room)
		{
			if (abstractPhysicalObject.world != null)
			{
				Custom.LogWarning($"ROOM MISMATCH FOR PHYSICAL OBJECT {abstractPhysicalObject.type}");
				if (abstractPhysicalObject is AbstractCreature)
				{
					Custom.LogWarning("critter name:", (abstractPhysicalObject as AbstractCreature).creatureTemplate.name);
				}
			}
			abstractPhysicalObject.pos.room = room.abstractRoom.index;
		}
		if (!sticksRespawned)
		{
			RecreateSticksFromAbstract();
			sticksRespawned = true;
		}
		base.Update(eu);
		if (appendages != null)
		{
			for (int k = 0; k < appendages.Count; k++)
			{
				appendages[k].Update();
			}
		}
	}

	public virtual void PlaceInRoom(Room placeRoom)
	{
		placeRoom.AddObject(this);
	}

	public virtual void PushOutOf(Vector2 pos, float rad, int exceptedChunk)
	{
		BodyChunk[] array = bodyChunks;
		foreach (BodyChunk bodyChunk in array)
		{
			if (bodyChunk.index != exceptedChunk && Custom.DistLess(bodyChunk.pos, pos, rad + bodyChunk.rad))
			{
				float num = Vector2.Distance(bodyChunk.pos, pos);
				Vector2 vector = Custom.DirVec(bodyChunk.pos, pos);
				bodyChunk.pos -= (rad + bodyChunk.rad - num) * vector;
				bodyChunk.vel -= (rad + bodyChunk.rad - num) * vector;
			}
		}
		if (graphicsModule != null)
		{
			graphicsModule.PushOutOf(pos, rad);
		}
	}

	public void ChangeCollisionLayer(int newCollisionLayer)
	{
		if (room == null)
		{
			collisionLayer = newCollisionLayer;
		}
		else
		{
			room.ChangeCollisionLayerForObject(this, newCollisionLayer);
		}
	}

	public virtual void Collide(PhysicalObject otherObject, int myChunk, int otherChunk)
	{
	}

	public virtual void HitByWeapon(Weapon weapon)
	{
	}

	public bool IsTileSolid(int bChunk, int relativeX, int relativeY)
	{
		switch (room.GetTile(room.GetTilePosition(bodyChunks[bChunk].pos) + new IntVector2(relativeX, relativeY)).Terrain)
		{
		case Room.Tile.TerrainType.Solid:
			return true;
		case Room.Tile.TerrainType.Floor:
			if (relativeY < 0 && !bodyChunks[bChunk].goThroughFloors)
			{
				return true;
			}
			break;
		}
		return false;
	}

	public void AllGraspsLetGoOfThisObject(bool evenNonExlusive)
	{
		for (int num = grabbedBy.Count - 1; num >= 0; num--)
		{
			grabbedBy[num].grabber.ReleaseGrasp(grabbedBy[num].graspUsed);
		}
	}

	public virtual void Grabbed(Creature.Grasp grasp)
	{
		GoThroughFloors = true;
		grabbedBy.Add(grasp);
	}

	public virtual void TerrainImpact(int chunk, IntVector2 direction, float speed, bool firstContact)
	{
		if (firstContact)
		{
			if (speed * bodyChunks[chunk].mass > 7f)
			{
				room.ScreenMovement(bodyChunks[chunk].pos, Custom.IntVector2ToVector2(direction) * speed * bodyChunks[chunk].mass * 0.1f, Mathf.Max((speed * bodyChunks[chunk].mass - 30f) / 50f, 0f));
			}
			if (speed > 4f && speed * bodyChunks[chunk].loudness * Mathf.Lerp(bodyChunks[chunk].mass, 1f, 0.5f) > 0.5f)
			{
				room.InGameNoise(new InGameNoise(bodyChunks[chunk].pos + IntVector2.ToVector2(direction) * bodyChunks[chunk].rad * 0.9f, Mathf.Lerp(350f, Mathf.Lerp(100f, 1500f, Mathf.InverseLerp(0.5f, 20f, speed * bodyChunks[chunk].loudness * Mathf.Lerp(bodyChunks[chunk].mass, 1f, 0.5f))), 0.5f), this, (this is Spear || this is Rock) ? 4f : 1f));
			}
		}
	}

	public virtual void GraphicsModuleUpdated(bool actuallyViewed, bool eu)
	{
	}

	public virtual void InitiateGraphicsModule()
	{
	}

	public virtual void RemoveGraphicsModule()
	{
		for (int i = 0; i < abstractPhysicalObject.world.game.cameras.Length; i++)
		{
			if (abstractPhysicalObject.world.game.cameras[i].followAbstractCreature == abstractPhysicalObject)
			{
				return;
			}
		}
		Custom.Log("REMOVE GRAPHICS MODULE!");
		graphicsModule = null;
	}

	public void WeightedPush(int A, int B, Vector2 dir, float frc)
	{
		float num = bodyChunks[B].mass / (bodyChunks[A].mass + bodyChunks[B].mass);
		bodyChunks[A].vel += dir * frc * num;
		bodyChunks[B].vel -= dir * frc * (1f - num);
	}

	public virtual void RecreateSticksFromAbstract()
	{
	}

	public virtual void HitByExplosion(float hitFac, Explosion explosion, int hitChunk)
	{
	}

	public void SetLocalGravity(float g)
	{
		gravity = g;
	}

	public void SetLocalAirFriction(float frict)
	{
		airFriction = frict;
	}

	public void WeatherInertia()
	{
		if (room != null && room.blizzard && Random.value < 0.1f && room.blizzardGraphics != null && room.blizzardGraphics.WindStrength > 0.5f)
		{
			Vector2 pos = FirstChunk().pos;
			Color blizzardPixel = room.blizzardGraphics.GetBlizzardPixel((int)(pos.x / 20f), (int)(pos.y / 20f));
			BodyChunk[] array = bodyChunks;
			foreach (BodyChunk obj in array)
			{
				Vector2 vector = new Vector2(0f - room.blizzardGraphics.WindAngle, 0.1f);
				vector *= blizzardPixel.g * (5f * room.blizzardGraphics.WindStrength);
				obj.vel += Vector2.Lerp(vector, vector * 0.08f, Submersion) * Mathf.InverseLerp(40f, 1f, TotalMass);
			}
		}
	}

	public virtual void DisposeGraphicsModule()
	{
		graphicsModule.dispose = true;
		graphicsModule = null;
	}
}
