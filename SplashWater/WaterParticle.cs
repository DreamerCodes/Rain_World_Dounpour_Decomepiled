using RWCustom;
using UnityEngine;

namespace SplashWater;

public class WaterParticle : CosmeticSprite
{
	protected float amount;

	protected float rad;

	protected float lastRad;

	public float lastLife;

	public float life;

	public float lifeTime;

	protected int makeSoundCounter = 4;

	public virtual void Reset(Vector2 pos, Vector2 vel, float amount, float initRad)
	{
		base.pos = pos;
		base.vel = vel;
		this.amount = amount;
		rad = initRad;
		lastRad = 0f;
		lastPos = pos;
		life = 1f;
		lastLife = 1f;
		lifeTime = 600f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastLife = life;
		life -= 1f / lifeTime;
		makeSoundCounter--;
		vel *= 0.97f;
		vel.y -= 0.9f;
		lastRad = rad;
		rad += Mathf.InverseLerp(1f, 20f, vel.magnitude);
		if (rad < amount)
		{
			rad = Mathf.Lerp(rad, amount, 0.1f);
		}
		if (rad > amount * 1.5f)
		{
			life -= 0.05f;
		}
		if (rad > amount * 2f)
		{
			rad = amount * 2f;
		}
		if (room.GetTile(pos).Solid && !room.GetTile(lastPos).Solid)
		{
			IntVector2? intVector = SharedPhysics.RayTraceTilesForTerrainReturnFirstSolid(room, room.GetTilePosition(lastPos), room.GetTilePosition(pos));
			FloatRect floatRect = Custom.RectCollision(pos, lastPos, room.TileRect(intVector.Value).Grow(2f));
			pos = floatRect.GetCorner(FloatRect.CornerLabel.D);
			if (makeSoundCounter <= 0 && vel.magnitude > 4f)
			{
				room.PlaySound(SoundID.Splashing_Water_Into_Terrain, pos, Mathf.InverseLerp(4f, 16f, vel.magnitude), 1f);
				makeSoundCounter = 10;
			}
			if (floatRect.GetCorner(FloatRect.CornerLabel.B).x < 0f)
			{
				vel.x = Mathf.Abs(vel.x) * 0.4f;
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).x > 0f)
			{
				vel.x = (0f - Mathf.Abs(vel.x)) * 0.4f;
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y < 0f)
			{
				vel.y = Mathf.Abs(vel.y) * 0.4f;
			}
			else if (floatRect.GetCorner(FloatRect.CornerLabel.B).y > 0f)
			{
				vel.y = (0f - Mathf.Abs(vel.y)) * 0.4f;
			}
			room.AddObject(new WaterDrip(pos, vel + Custom.RNV() * vel.magnitude, waterColor: true));
			life -= 0.02f;
		}
	}
}
