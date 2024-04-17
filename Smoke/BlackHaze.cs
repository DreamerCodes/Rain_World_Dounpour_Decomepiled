using System;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class BlackHaze : PositionedSmokeEmitter
{
	public class BlackHazeSegment : HyrbidSmokeSegment
	{
		private Vector2 driftDir;

		public int age;

		public float power;

		private Color fogCol;

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
			return BlackHazeColor(Mathf.InverseLerp(1f, 5f + 15f * power, (float)age + timeStacker));
		}

		public Color BlackHazeColor(float x)
		{
			if (x < 0.8f)
			{
				return Color.Lerp(new Color(0.1f, 0.1f, 0.2f), new Color(0.05f, 0.05f, 0.1f), Mathf.InverseLerp(0f, 0.5f, x));
			}
			return Color.Lerp(new Color(0.05f, 0.05f, 0.1f), fogCol, Mathf.InverseLerp(0.8f, 1f, x) * 0.1f);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
			fogCol = Color.Lerp(palette.blackColor, palette.fogColor, 0.5f);
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

	public class BigBlackHaze : CosmeticSprite
	{
		public class BlackHazeVisionObscurer : VisionObscurer
		{
			private BigBlackHaze bigBlackHaze;

			public BlackHazeVisionObscurer(Vector2 pos, BigBlackHaze bigBlackHaze)
				: base(pos, bigBlackHaze.rad, 280f, 1f)
			{
				this.bigBlackHaze = bigBlackHaze;
			}

			public override void Update(bool eu)
			{
				base.Update(eu);
				obscureFac = Mathf.InverseLerp(0.1f, 0.8f, bigBlackHaze.alpha);
				rad = bigBlackHaze.Rad(1f) * 0.55f;
				pos = bigBlackHaze.pos;
				if (UnityEngine.Random.value < bigBlackHaze.alpha && room.abstractRoom.creatures.Count > 0)
				{
					AbstractCreature abstractCreature = room.abstractRoom.creatures[UnityEngine.Random.Range(0, room.abstractRoom.creatures.Count)];
					if (abstractCreature.realizedCreature != null && Custom.DistLess(abstractCreature.realizedCreature.mainBodyChunk.pos, pos, rad * Mathf.Pow(bigBlackHaze.alpha, 0.6f) / 2f))
					{
						abstractCreature.realizedCreature.Blind((int)(bigBlackHaze.alpha * (float)room.abstractRoom.creatures.Count * 10f));
					}
				}
				if (bigBlackHaze.slatedForDeletetion)
				{
					Destroy();
				}
			}
		}

		private float rad;

		private float lastRad;

		private BlackHaze smoke;

		private float independent;

		private float alpha;

		private float lastAlpha;

		private float size;

		private int age;

		private BlackHazeVisionObscurer visObsc;

		public float Rad(float timeStacker)
		{
			return Mathf.Lerp(lastRad, rad, timeStacker) * Mathf.Lerp(500f, 600f, size);
		}

		public BigBlackHaze(Vector2 pos, BlackHaze smoke, float size)
		{
			base.pos = pos;
			lastPos = pos;
			this.smoke = smoke;
			this.size = size;
			rad = 0f;
			lastRad = 0f;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastRad = rad;
			rad = Custom.LerpAndTick(rad, 1f, 0.01f, 1f / Mathf.Lerp(75f, 620f, rad));
			lastAlpha = alpha;
			independent = Mathf.Min(1f, independent + 1f / 90f);
			alpha = Mathf.Pow(rad, 0.5f) * Mathf.InverseLerp(Mathf.Lerp(1600f, 800f, size), 250f, age);
			Vector2 b = pos;
			float num = 1f;
			for (int i = 0; i < smoke.particles.Count; i++)
			{
				float num2 = Mathf.Sin(smoke.particles[i].life * (float)Math.PI);
				b += smoke.particles[i].pos * num2;
				num += num2;
			}
			b /= num;
			age++;
			pos = Vector2.Lerp(pos, b, 0.15f * Mathf.InverseLerp(1f, 0.5f, independent) * Mathf.InverseLerp(150f, 0f, Vector2.Distance(pos, b)));
			if (visObsc == null)
			{
				visObsc = new BlackHazeVisionObscurer(pos, this);
				room.AddObject(visObsc);
			}
			if (alpha == 0f && lastAlpha == 0f)
			{
				Destroy();
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			sLeaser.sprites = new FSprite[1];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["HazerHaze"];
			AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Water"));
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[0].alpha = Mathf.Lerp(lastAlpha, alpha, timeStacker);
			sLeaser.sprites[0].color = new Color(Mathf.InverseLerp(Mathf.Lerp(1600f, 800f, size), 250f, (float)age + timeStacker), 0f, 0f);
			sLeaser.sprites[0].scaleX = Rad(timeStacker) * 1.35f / 16f;
			sLeaser.sprites[0].scaleY = Rad(timeStacker) / 16f;
		}
	}

	public BlackHaze(Room room, Vector2 pos)
		: base(SmokeType.BlackHaze, room, pos, 2, 0f, autoSpawn: false, -1f, -1)
	{
	}

	protected override SmokeSystemParticle CreateParticle()
	{
		return new BlackHazeSegment();
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

	public float PushPow(int i)
	{
		return Mathf.InverseLerp(0.65f, 0.85f, particles[i].life) * (particles[i] as BlackHazeSegment).power;
	}

	public void EmitSmoke(Vector2 vel, float power)
	{
		if (AddParticle(pos, vel * power, Custom.LerpMap(power, 0.3f, 0f, Mathf.Lerp(20f, 60f, UnityEngine.Random.value), Mathf.Lerp(60f, 100f, UnityEngine.Random.value))) is BlackHazeSegment blackHazeSegment)
		{
			blackHazeSegment.power = power;
		}
	}

	public void EmitBigSmoke(float size)
	{
		room.AddObject(new BigBlackHaze(pos, this, size));
	}
}
