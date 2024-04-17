using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class SmokeSystem : UpdatableAndDeletable, Explosion.IReactToExplosions
{
	public class SmokeType : ExtEnum<SmokeType>
	{
		public static readonly SmokeType VultureSmoke = new SmokeType("VultureSmoke", register: true);

		public static readonly SmokeType NewVultureSmoke = new SmokeType("NewVultureSmoke", register: true);

		public static readonly SmokeType Spores = new SmokeType("Spores", register: true);

		public static readonly SmokeType Steam = new SmokeType("Steam", register: true);

		public static readonly SmokeType FireSmoke = new SmokeType("FireSmoke", register: true);

		public static readonly SmokeType CyanLizardSmoke = new SmokeType("CyanLizardSmoke", register: true);

		public static readonly SmokeType Smolder = new SmokeType("Smolder", register: true);

		public static readonly SmokeType BombSmoke = new SmokeType("BombSmoke", register: true);

		public static readonly SmokeType BlackHaze = new SmokeType("BlackHaze", register: true);

		public SmokeType(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public class SmokeSystemParticle : CosmeticSprite
	{
		public SmokeSystem owner;

		public bool resting;

		public bool turnOnNextUpdate;

		public int killCounter;

		public float lastLife;

		public float life;

		public float lifeTime;

		public bool lastEmitted;

		protected SmokeSystemParticle np;

		protected SmokeSystemParticle pp;

		public Vector2 lingerPos;

		public Vector2 lastLingerPos;

		public SmokeSystemParticle nextParticle
		{
			get
			{
				if (np != null && np.resting)
				{
					if (!np.turnOnNextUpdate)
					{
						np = null;
					}
					return null;
				}
				return np;
			}
			set
			{
				np = value;
				if (np != null)
				{
					np.pp = this;
				}
			}
		}

		public SmokeSystemParticle prevParticle
		{
			get
			{
				if (pp != null && pp.resting)
				{
					if (!pp.turnOnNextUpdate)
					{
						pp = null;
					}
					return null;
				}
				return pp;
			}
			set
			{
				pp = value;
				if (pp != null)
				{
					pp.np = this;
				}
			}
		}

		public virtual float ObjectWind => 1f;

		public virtual void Reset(SmokeSystem newOwner, Vector2 newPos, Vector2 newVel, float newLifeTime)
		{
			owner = newOwner;
			vel = newVel;
			lifeTime = newLifeTime;
			pos = newPos;
			lastPos = newPos;
			lingerPos = newPos + Custom.RNV();
			lastLingerPos = lingerPos;
			life = 1f;
			lastLife = 1f;
			np = null;
			pp = null;
			turnOnNextUpdate = true;
			resting = true;
			lastEmitted = true;
		}

		public override void Update(bool eu)
		{
			lastLife = life;
			lastLingerPos = lingerPos;
			if (resting)
			{
				if (!turnOnNextUpdate)
				{
					killCounter++;
					if (killCounter >= SmokePool.KillCountToDestroyParticle)
					{
						Destroy();
					}
					return;
				}
				resting = false;
				turnOnNextUpdate = false;
			}
			base.Update(eu);
			life = Mathf.Max(0f, life - 1f / lifeTime);
			if (life <= 0f && lastLife <= 0f)
			{
				resting = true;
				turnOnNextUpdate = false;
				nextParticle = null;
				killCounter = 0;
			}
			if (lastEmitted && prevParticle != null)
			{
				lastEmitted = false;
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].isVisible = !resting;
			}
		}
	}

	public SmokeType smokeType;

	public SmokePool particlePool;

	public List<SmokeSystemParticle> particles;

	public int counter;

	public float minParticleDistance;

	public int connectParticlesTime;

	public int checkPhysObj;

	public float objectWind = 1f;

	public bool Dead
	{
		get
		{
			if (counter > connectParticlesTime + 100)
			{
				return particles.Count < 1;
			}
			return false;
		}
	}

	public virtual bool ObjectAffectWind(PhysicalObject obj)
	{
		return true;
	}

	public void DisconnectSmoke()
	{
		counter = connectParticlesTime;
	}

	public SmokeSystem(SmokeType smokeType, Room room, int connectParticlesTime, float minParticleDistance)
	{
		this.smokeType = smokeType;
		this.minParticleDistance = minParticleDistance;
		this.connectParticlesTime = connectParticlesTime;
		base.room = room;
		particles = new List<SmokeSystemParticle>();
		for (int i = 0; i < room.updateList.Count; i++)
		{
			if (!room.updateList[i].slatedForDeletetion && room.updateList[i] is SmokePool && room.updateList[i].room == room && (room.updateList[i] as SmokePool).type == smokeType)
			{
				particlePool = room.updateList[i] as SmokePool;
				break;
			}
		}
		if (particlePool == null)
		{
			particlePool = new SmokePool(smokeType);
			room.AddObject(particlePool);
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		counter++;
		for (int num = particles.Count - 1; num >= 0; num--)
		{
			if (particles[num].slatedForDeletetion)
			{
				particles.RemoveAt(num);
			}
			else if (particles[num].resting && !particles[num].turnOnNextUpdate)
			{
				particlePool.ParticleToRest(particles[num]);
				particles.RemoveAt(num);
			}
		}
		if (!(objectWind > 0f) || particles.Count <= 0)
		{
			return;
		}
		checkPhysObj++;
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			if (room.physicalObjects[i].Count > 0 && ObjectAffectWind(room.physicalObjects[i][checkPhysObj % room.physicalObjects[i].Count]))
			{
				PhysicalObjectAffectSmoke(room.physicalObjects[i][checkPhysObj % room.physicalObjects[i].Count], Mathf.Min(room.physicalObjects[i].Count, 16f) * objectWind);
			}
		}
	}

	protected virtual SmokeSystemParticle AddParticle(Vector2 emissionPoint, Vector2 emissionForce, float lifeTime)
	{
		if (base.slatedForDeletetion || room == null)
		{
			return null;
		}
		bool flag = counter < connectParticlesTime;
		counter = 0;
		if (minParticleDistance > 0f && particles.Count > 0 && Custom.DistLess(emissionPoint, particles[particles.Count - 1].pos, minParticleDistance))
		{
			return null;
		}
		if (!room.ViewedByAnyCamera(emissionPoint, 300f))
		{
			return null;
		}
		SmokeSystemParticle smokeSystemParticle = particlePool.GetParticle();
		if (smokeSystemParticle == null)
		{
			smokeSystemParticle = CreateParticle();
			room.AddObject(smokeSystemParticle);
		}
		particles.Add(smokeSystemParticle);
		smokeSystemParticle.Reset(this, emissionPoint, emissionForce, lifeTime);
		if (flag && particles.Count > 2)
		{
			smokeSystemParticle.nextParticle = particles[particles.Count - 2];
		}
		return smokeSystemParticle;
	}

	protected virtual SmokeSystemParticle CreateParticle()
	{
		return null;
	}

	public static Vector2 PerlinWind(Vector2 pos, Room room)
	{
		return new Vector2(-0.075f + 0.17f * Mathf.PerlinNoise(23413f + pos.x / 41.5f, 521f - pos.y / 41.5f + (float)room.game.clock / 25.344f), -0.075f + 0.17f * Mathf.PerlinNoise(2341.235f - pos.x / 41.5f, 88712.11f + pos.y / 41.5f + (float)room.game.clock / 25.344f));
	}

	public void PhysicalObjectAffectSmoke(PhysicalObject obj, float fac)
	{
		bool flag = false;
		for (int i = 0; i < obj.bodyChunks.Length; i++)
		{
			if (!Custom.DistLess(obj.bodyChunks[i].pos, obj.bodyChunks[i].lastPos, 1f))
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			return;
		}
		for (int j = 0; j < obj.bodyChunks.Length; j++)
		{
			float num = Vector2.Distance(obj.bodyChunks[j].lastPos, obj.bodyChunks[j].pos);
			float num2 = obj.bodyChunks[j].rad * (1f + Mathf.InverseLerp(1f, 250f, Mathf.Pow(num, 1.5f))) + 20f + 60f * obj.bodyChunks[j].mass;
			for (int k = 0; k < particles.Count; k++)
			{
				if (!(particles[k].ObjectWind > 0f) || !Custom.DistLess(obj.firstChunk.pos, particles[k].pos, num2 * 2f + num))
				{
					continue;
				}
				float num3 = Vector2.Distance(particles[k].pos, Custom.ClosestPointOnLineSegment(obj.bodyChunks[j].lastPos, obj.bodyChunks[j].pos, particles[k].pos));
				if (num3 < num2 * 2f)
				{
					float num4 = Custom.LerpMap(Mathf.Abs(Custom.DistanceToLine(particles[k].pos, obj.bodyChunks[j].lastPos, obj.bodyChunks[j].pos)), obj.bodyChunks[j].rad, num2 * 2f, 1f, -0.5f, 0.3f) * Mathf.Pow(Mathf.InverseLerp(obj.bodyChunks[j].rad, num2 * 2f, num3), 0.3f) * particles[k].ObjectWind;
					particles[k].vel += Vector2.ClampMagnitude((obj.bodyChunks[j].pos - obj.bodyChunks[j].lastPos) * fac * num4 * Mathf.Pow(num, 0.85f) / 140f, 5f);
					if (num4 < 0f)
					{
						particles[k].vel += Vector2.ClampMagnitude(Custom.DirVec(particles[k].pos, Custom.ClosestPointOnLine(obj.bodyChunks[j].lastPos, obj.bodyChunks[j].pos, particles[k].pos)) * Mathf.Abs(num4) * fac * Mathf.Pow(num, 0.75f) / 30f, 5f);
					}
				}
			}
		}
	}

	public void WindDrag(Vector2 pos, Vector2 vel, float size)
	{
		for (int i = 0; i < particles.Count; i++)
		{
			WindDrag(pos, vel, size, particles[i]);
		}
	}

	public void WindDrag(Vector2 pos, Vector2 vel, float size, SmokeSystemParticle B)
	{
		float num = Vector2.Dot(vel.normalized, (B.pos - pos).normalized);
		float num2 = Mathf.InverseLerp(size + vel.magnitude * (5f - num * 2f), size, Vector2.Distance(pos, B.pos));
		if (num < 0f)
		{
			pos -= vel * Mathf.Abs(num) * 2f;
			num2 *= Mathf.Lerp(1f, 0.5f, Mathf.Abs(num));
		}
		B.vel += Custom.DirVec(B.pos, pos) * num2 * vel.magnitude * (0f - num) * 0.8f;
	}

	public void WindPuff(Vector2 pos, float frc, float rad)
	{
		for (int i = 0; i < particles.Count; i++)
		{
			if (Custom.DistLess(pos, particles[i].pos, rad))
			{
				float num = Mathf.Pow(Mathf.InverseLerp(rad, 0f, Vector2.Distance(pos, particles[i].pos)), 0.2f);
				particles[i].vel += Custom.DirVec(pos, particles[i].pos) * frc * num;
			}
		}
	}

	public void Explosion(Explosion explosion)
	{
		float num = Mathf.Lerp(explosion.rad, Mathf.Min(explosion.rad, 90f), 0.5f);
		int num2 = 0;
		for (int i = 0; i < particles.Count; i++)
		{
			if (!Custom.DistLess(particles[i].pos, explosion.pos, num * 3f))
			{
				continue;
			}
			float num3 = Vector2.Distance(particles[i].pos, explosion.pos);
			if (num2 < 15)
			{
				if (!room.VisualContact(particles[i].pos, explosion.pos))
				{
					num3 /= 4f;
				}
				num2++;
			}
			particles[i].pos += Custom.DirVec(explosion.pos, particles[i].pos) * Mathf.Pow(Mathf.InverseLerp(num * 3f, num * 1.5f, num3), 0.3f) * (10f + explosion.force * 6f);
			particles[i].vel += Custom.DirVec(explosion.pos, particles[i].pos) * Mathf.Pow(Mathf.InverseLerp(num * 2f, num * 0.2f, num3), 2f) * explosion.force * 22.2f;
		}
	}
}
