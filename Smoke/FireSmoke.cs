using System;
using RWCustom;
using UnityEngine;

namespace Smoke;

public class FireSmoke : SmokeSystem
{
	public class FireSmokeParticle : SpriteSmoke
	{
		public Color effectColor;

		public Color colorA;

		public float col;

		public float lastCol;

		public int colorFadeTime;

		public float moveDir;

		public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float lifeTime)
		{
			base.Reset(newOwner, pos, vel, lifeTime);
			col = 0f;
			lastCol = 0f;
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
			vel *= 0.7f + 0.3f / Mathf.Pow(vel.magnitude, 0.5f);
			moveDir += Mathf.Lerp(-1f, 1f, UnityEngine.Random.value) * 50f;
			vel += Custom.DegToVec(moveDir) * 0.6f * Mathf.Lerp(vel.magnitude, 1f, 0.6f);
			if (room.PointSubmerged(pos))
			{
				pos.y = room.FloatWaterLevel(pos.x);
			}
			lastCol = col;
			col += 1f;
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
				0 => Mathf.Lerp(4f, rad, Mathf.Pow(1f - useLife, 0.6f) + useStretched), 
				1 => 1.5f * Mathf.Lerp(2f, rad, Mathf.Pow(1f - useLife, 0.6f)), 
				_ => Mathf.Lerp(4f, rad, Mathf.Pow(1f - useLife, 0.6f)), 
			};
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			base.InitiateSprites(sLeaser, rCam);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].shader = room.game.rainWorld.Shaders["FireSmoke"];
			}
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			if (!resting)
			{
				float num = Mathf.Lerp(lastLife, life, timeStacker);
				float t = Mathf.InverseLerp(colorFadeTime, 0.5f, Mathf.Lerp(lastCol, col, timeStacker));
				sLeaser.sprites[0].color = Color.Lerp(colorA, effectColor, t);
				sLeaser.sprites[1].color = Color.Lerp(colorA, effectColor, t);
				sLeaser.sprites[0].alpha = Mathf.Pow(num, 0.25f) * (1f - stretched);
				sLeaser.sprites[1].alpha = 0.3f + Mathf.Pow(Mathf.Sin(num * (float)Math.PI), 0.7f) * 0.65f * (1f - stretched);
			}
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			colorA = Color.Lerp(palette.blackColor, palette.fogColor, 0.2f);
		}
	}

	public FireSmoke(Room room)
		: base(SmokeType.FireSmoke, room, 2, 0f)
	{
	}

	protected override SmokeSystemParticle CreateParticle()
	{
		return new FireSmokeParticle();
	}

	public void EmitSmoke(Vector2 pos, Vector2 vel, Color effectColor, int colorFadeTime)
	{
		if (AddParticle(pos, vel, Mathf.Lerp(10f, 40f, UnityEngine.Random.value)) is FireSmokeParticle fireSmokeParticle)
		{
			fireSmokeParticle.effectColor = effectColor;
			fireSmokeParticle.colorFadeTime = colorFadeTime;
		}
	}
}
