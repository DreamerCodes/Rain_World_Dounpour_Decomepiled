using System;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class NewVultureSmoke : PositionedSmokeEmitter
{
	public class NewVultureSmokeSegment : HyrbidSmokeSegment
	{
		private Vector2 driftDir;

		public int age;

		public float power;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			driftDir = Custom.RNV();
			age = 0;
			power = 0f;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			age++;
			vel += driftDir * Mathf.Sin(Mathf.InverseLerp(0.55f, 0.75f, life) * (float)Math.PI) * 0.6f * power;
		}

		public override void WindAndDrag(Room rm, ref Vector2 v, Vector2 p)
		{
			if (v.magnitude > 0f)
			{
				v *= 0.5f + 0.5f / Mathf.Pow(v.magnitude, 0.5f);
			}
			v += SmokeSystem.PerlinWind(p, rm) * 2f;
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
			v += Vector2.ClampMagnitude(vector, 1f) * 0.035f;
		}

		public override float ConDist(float timeStacker)
		{
			return Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.7f, 4f, 20f, 3f) * power;
		}

		public override float MyRad(float timeStacker)
		{
			return Mathf.Min(Custom.LerpMap(Mathf.Lerp(lastLife, life, timeStacker), 1f, 0.7f, 4f, 20f, 3f) + Mathf.Sin(Mathf.InverseLerp(0.7f, 0f, Mathf.Lerp(lastLife, life, timeStacker)) * (float)Math.PI) * 8f, 5f + 25f * power) * (2f - MyOpactiy(timeStacker));
		}

		public override float MyOpactiy(float timeStacker)
		{
			if (resting)
			{
				return 0f;
			}
			return Mathf.InverseLerp(0f, 0.7f, Mathf.Lerp(lastLife, life, timeStacker)) * Mathf.Lerp(Custom.LerpMap(Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), NextPos(timeStacker)), 20f, 250f, 1f, 0f, 1.5f), 1f, Mathf.InverseLerp(0.9f, 1f, Mathf.Lerp(lastLife, life, timeStacker))) * (0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.4f, power));
		}

		public override Color MyColor(float timeStacker)
		{
			return VultureSmokeColor(Mathf.InverseLerp(1f, 5f + 15f * power, (float)age + timeStacker));
		}

		public Color VultureSmokeColor(float x)
		{
			Color rgb = HSLColor.Lerp(new HSLColor(0f, 0.5f, 0.8f), new HSLColor(0.9f, 0.5f, 0.15f), x).rgb;
			Color b = Color.Lerp(new HSLColor(0f, 0.5f, 0.8f).rgb, new HSLColor(0.9f, 0.5f, 0.15f).rgb, x);
			return Color.Lerp(rgb, b, 0.3f);
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["SmokeTrail"];
			sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["FireSmoke"];
			sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["FireSmoke"];
		}

		public override void HybridDraw(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos, Vector2 Apos, Vector2 Bpos, Color Acol, Color Bcol, float Arad, float Brad)
		{
			base.HybridDraw(sLeaser, rCam, timeStacker, camPos, Apos, Bpos, Acol, Bcol, Arad, Brad);
			sLeaser.sprites[1].scale = Arad * (2f - Acol.a) / 8f;
			sLeaser.sprites[1].alpha = Mathf.Pow(Acol.a, 0.6f) * (0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.4f, power));
			Acol.a = 1f;
			sLeaser.sprites[1].color = Acol;
			sLeaser.sprites[2].scale = Brad * (2f - Bcol.a) / 8f;
			sLeaser.sprites[2].alpha = Mathf.Pow(Bcol.a, 0.6f) * (0.5f + 0.5f * Mathf.InverseLerp(0.2f, 0.4f, power));
			Bcol.a = 1f;
			sLeaser.sprites[2].color = Bcol;
		}
	}

	public NewVultureSmoke(Room room, Vector2 pos, Vulture vulture)
		: base(SmokeType.NewVultureSmoke, room, pos, 2, 0f, autoSpawn: false, -1f, -1)
	{
	}

	protected override SmokeSystemParticle CreateParticle()
	{
		return new NewVultureSmokeSegment();
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		for (int i = 0; i < particles.Count; i++)
		{
			if (!(PushPow(i) > 0f))
			{
				continue;
			}
			for (int num = i - 1; num >= 0; num--)
			{
				if (Custom.DistLess(particles[i].pos, particles[num].pos, 60f))
				{
					float num2 = PushPow(num) / (PushPow(i) + PushPow(num));
					Vector2 b = (particles[i].vel + particles[num].vel) / 2f;
					float num3 = Mathf.InverseLerp(60f, 30f, Vector2.Distance(particles[i].pos, particles[num].pos));
					particles[i].vel = Vector2.Lerp(particles[i].vel, b, num2 * num3);
					particles[num].vel = Vector2.Lerp(particles[num].vel, b, (1f - num2) * num3);
				}
			}
		}
	}

	private float PushPow(int i)
	{
		return Mathf.InverseLerp(0.65f, 0.85f, particles[i].life) * (particles[i] as NewVultureSmokeSegment).power;
	}

	public void EmitSmoke(Vector2 vel, float power)
	{
		if (AddParticle(pos, vel * power, Custom.LerpMap(power, 0.3f, 0f, Mathf.Lerp(20f, 60f, UnityEngine.Random.value), Mathf.Lerp(60f, 100f, UnityEngine.Random.value))) is NewVultureSmokeSegment newVultureSmokeSegment)
		{
			newVultureSmokeSegment.power = power;
		}
	}
}
