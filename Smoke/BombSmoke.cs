using RWCustom;
using UnityEngine;

namespace Smoke;

public class BombSmoke : PositionedSmokeEmitter
{
	public class ThickSmokeSegment : SmokeSegment
	{
		private Color colorB;

		public int colorCounter;

		public bool stationaryMode;

		public float startConDist;

		public float radFac;

		public float alphaFac;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			colorCounter = 0;
			radFac = 0.5f + Random.value;
			alphaFac = 0.6f + 0.4f * Mathf.Pow(Random.value, 0.6f);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			vel.y += 0.1f * Mathf.InverseLerp(0.9f, 1f, life);
			colorCounter++;
		}

		public override float ConDist(float timeStacker)
		{
			return Mathf.Lerp(16f, startConDist, Mathf.Lerp(lastLife, life, timeStacker));
		}

		public override void WindAndDrag(Room rm, ref Vector2 v, Vector2 p)
		{
			v *= Custom.LerpMap(v.magnitude, 5f, 20f, 0.97f, 0.4f);
			v.y += 0.02f;
			v += SmokeSystem.PerlinWind(p, rm);
			if (!rm.readyForAI || rm.aimap.getTerrainProximity(p) >= 3)
			{
				return;
			}
			int terrainProximity = rm.aimap.getTerrainProximity(p);
			Vector2 vector = default(Vector2);
			for (int i = 0; i < 8; i++)
			{
				if (rm.aimap.getTerrainProximity(p + Custom.eightDirections[i].ToVector2() * 20f) > terrainProximity)
				{
					vector += Custom.eightDirections[i].ToVector2();
				}
			}
			v += Vector2.ClampMagnitude(vector, 1f) * 0.015f;
		}

		private float MaxDist(float timeStacker)
		{
			return Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.9f, 60f, 20f);
		}

		public override float MyRad(float timeStacker)
		{
			return (6f + Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.9f, 0f, 5f) + Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.3f, 0f, 20f)) * Custom.LerpMap(Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), NextPos(timeStacker)), ConDist(timeStacker) * 2f, ConDist(timeStacker) * MaxDist(timeStacker), 1f, 0.1f, 3f) * (stationaryMode ? 1f : (2f - MyOpactiy(timeStacker))) * radFac;
		}

		public override float MyOpactiy(float timeStacker)
		{
			if (resting)
			{
				return 0f;
			}
			return Mathf.InverseLerp(0f, 0.9f, Mathf.Lerp(lastLife, life, timeStacker)) * (stationaryMode ? (Mathf.InverseLerp(9f, 24f, (float)colorCounter + timeStacker) * 0.8f) : 1f) * Mathf.Lerp(Custom.LerpMap(Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), NextPos(timeStacker)), ConDist(timeStacker) * 1.2f, ConDist(timeStacker) * MaxDist(timeStacker), 1f, 0f, 0.75f), 1f, Mathf.InverseLerp(0.85f, 1f, Mathf.Lerp(lastLife, life, timeStacker)) * 0.9f) * alphaFac;
		}

		public override Color MyColor(float timeStacker)
		{
			return Color.Lerp((owner as BombSmoke).fireColor, colorB, Mathf.InverseLerp(2f, 5f, (float)colorCounter + timeStacker));
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
			colorB = Color.Lerp(palette.blackColor, palette.fogColor, 0.2f * (1f - palette.darkness));
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["SmokeTrail"];
		}
	}

	public BodyChunk chunk;

	public Color fireColor;

	public bool stationary;

	public int life;

	public float fadeIn;

	public override float ParticleLifeTime => (stationary ? 2f : 1f) * Mathf.Lerp(70f, 120f, Random.value);

	public override bool ObjectAffectWind(PhysicalObject obj)
	{
		if (chunk == null)
		{
			return true;
		}
		return obj != chunk.owner;
	}

	public BombSmoke(Room room, Vector2 pos, BodyChunk chunk, Color fireColor)
		: base(SmokeType.BombSmoke, room, pos, 2, 6f, autoSpawn: true, 15f, 15)
	{
		this.chunk = chunk;
		objectWind = 1f;
		this.fireColor = fireColor;
		life = Random.Range(200, 400);
	}

	public override void Update(bool eu)
	{
		if (chunk != null)
		{
			pos = chunk.pos;
			if (chunk.owner.room != room)
			{
				Destroy();
			}
		}
		fadeIn = Mathf.Min(1f, fadeIn + 1f / (stationary ? 40f : 6f));
		base.Update(eu);
		if (stationary)
		{
			life--;
		}
		if (life < 1)
		{
			Destroy();
		}
	}

	protected override SmokeSystemParticle AddParticle(Vector2 emissionPoint, Vector2 emissionForce, float lifeTime)
	{
		if (room.PointSubmerged(emissionPoint))
		{
			if (Random.value < 0.1f)
			{
				room.AddObject(new Bubble(emissionPoint, emissionForce, bottomBubble: false, fakeWaterBubble: false));
			}
			life--;
			return null;
		}
		ThickSmokeSegment thickSmokeSegment = base.AddParticle(emissionPoint, emissionForce, lifeTime) as ThickSmokeSegment;
		if (thickSmokeSegment != null)
		{
			thickSmokeSegment.stationaryMode = stationary;
			if (thickSmokeSegment.nextParticle == null)
			{
				thickSmokeSegment.startConDist = 5f;
			}
			else
			{
				thickSmokeSegment.startConDist = Vector2.Distance(thickSmokeSegment.pos, thickSmokeSegment.nextParticle.pos) * 0.8f;
			}
			thickSmokeSegment.life = fadeIn * Mathf.InverseLerp(0f, 120f, life);
			thickSmokeSegment.lastLife = thickSmokeSegment.life;
		}
		return thickSmokeSegment;
	}

	protected override SmokeSystemParticle CreateParticle()
	{
		return new ThickSmokeSegment();
	}
}
