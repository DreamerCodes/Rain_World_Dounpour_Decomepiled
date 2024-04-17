using System;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class VultureSmoke : SmokeSystem
{
	public class VultureSmokeParticle : SpriteSmoke
	{
		public float color;

		public float lastColor;

		public float moveDir;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			color = 0f;
			lastColor = 0f;
			rad = Mathf.Lerp(28f, 46f, UnityEngine.Random.value);
			moveDir = UnityEngine.Random.value * 360f;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (resting)
			{
				return;
			}
			vel *= 0.5f + 0.5f / Mathf.Pow(vel.magnitude, 0.5f);
			moveDir += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 50f;
			vel += Custom.DegToVec(moveDir) * 0.3f * Mathf.Lerp(vel.magnitude, 1f, 0.6f);
			if (room.PointSubmerged(pos))
			{
				pos.y = room.FloatWaterLevel(pos.x);
			}
			lastColor = color;
			color = Mathf.Min(1f, color + 0.05f);
			if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
			{
				IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
				FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
				pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
				if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
				{
					vel.x = Mathf.Abs(vel.x);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
				{
					vel.x = 0f - Mathf.Abs(vel.x);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
				{
					vel.y = Mathf.Abs(vel.y);
				}
				else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
				{
					vel.y = 0f - Mathf.Abs(vel.y);
				}
			}
		}

		public override float Rad(int type, float useLife, float useStretched, float timeStacker)
		{
			return type switch
			{
				0 => Mathf.Lerp(4f, rad, 1f - useLife + useStretched), 
				1 => Mathf.Lerp(3f, rad * 2f, 1f - useLife), 
				_ => Mathf.Lerp(4f, rad, 1f - useLife), 
			};
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].shader = room.game.rainWorld.Shaders["NewVultureSmoke"];
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!resting)
			{
				sLeaser.sprites[0].color = VultureSmokeColor(Mathf.Lerp(lastColor, color, timeStacker));
				sLeaser.sprites[1].color = VultureSmokeColor(Mathf.Lerp(lastColor, color, timeStacker));
				sLeaser.sprites[0].alpha = Mathf.InverseLerp(0f, 0.8f, Mathf.Lerp(lastLife, life, timeStacker)) * (1f - stretched);
				sLeaser.sprites[1].alpha = Mathf.Min(sLeaser.sprites[0].alpha, 0.6f * Mathf.Sin(Mathf.Lerp(lastLife, life, timeStacker) * (float)Math.PI));
			}
		}

		public Color VultureSmokeColor(float x)
		{
			Color rgb = HSLColor.Lerp(new HSLColor(0f, 0.5f, 0.8f), new HSLColor(0.9f, 0.5f, 0.15f), x).rgb;
			Color b = Color.Lerp(new HSLColor(0f, 0.5f, 0.8f).rgb, new HSLColor(0.9f, 0.5f, 0.15f).rgb, x);
			return Color.Lerp(rgb, b, 0.3f);
		}
	}

	private Vulture vulture;

	public override bool ObjectAffectWind(PhysicalObject obj)
	{
		return obj != vulture;
	}

	public VultureSmoke(Room room, Vulture vulture)
		: base(SmokeType.VultureSmoke, room, 2, 0f)
	{
		this.vulture = vulture;
	}

	protected override SmokeSystemParticle CreateParticle()
	{
		return new VultureSmokeParticle();
	}

	public void EmitSmoke(Vector2 pos, Vector2 vel)
	{
		AddParticle(pos, vel, Mathf.Lerp(20f, 60f, UnityEngine.Random.value));
	}
}
