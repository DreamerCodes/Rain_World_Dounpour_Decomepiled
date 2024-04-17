using System;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class SporesSmoke : SmokeSystem
{
	public class SporesParticle : SpriteSmoke
	{
		public Color color;

		public float moveDir;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			rad = Mathf.Lerp(1f, 3f, UnityEngine.Random.value);
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
				1 => 1.5f * Mathf.Lerp(2f, rad, 1f - useLife), 
				_ => Mathf.Lerp(4f, rad, 1f - useLife), 
			};
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].shader = room.game.rainWorld.Shaders["Spores"];
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!resting)
			{
				sLeaser.sprites[0].color = color;
				sLeaser.sprites[1].color = color;
				sLeaser.sprites[0].alpha = Mathf.Pow(life, 0.5f) * (1f - stretched);
				sLeaser.sprites[1].alpha = Mathf.Sin(life * (float)Math.PI) * (1f - stretched);
			}
		}
	}

	public SporesSmoke(Room room)
		: base(SmokeType.Spores, room, 2, 0f)
	{
	}

	protected override SmokeSystemParticle CreateParticle()
	{
		return new SporesParticle();
	}

	public void EmitSmoke(Vector2 pos, Vector2 vel, Color color)
	{
		if (AddParticle(pos, vel, Mathf.Lerp(20f, 30f, UnityEngine.Random.value)) is SporesParticle sporesParticle)
		{
			sporesParticle.color = color;
		}
	}
}
