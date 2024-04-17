using RWCustom;
using UnityEngine;

namespace Smoke;

public abstract class SpriteSmoke : SmokeSystem.SmokeSystemParticle
{
	public float rad;

	public float stretched;

	public float lastStretched;

	public virtual float ToMidSpeed => 0.7f;

	public SpriteSmoke()
	{
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!resting)
		{
			if (base.nextParticle != null)
			{
				Vector2 b = Vector2.Lerp(vel, base.nextParticle.vel, 0.5f);
				vel = Vector2.Lerp(vel, b, ToMidSpeed);
				base.nextParticle.vel = Vector2.Lerp(base.nextParticle.vel, b, 0.7f);
				lingerPos = base.nextParticle.pos;
			}
			lastStretched = stretched;
			stretched = Mathf.InverseLerp(60f, 200f, Vector2.Distance(pos, lingerPos));
		}
	}

	public override void Reset(SmokeSystem newOwner, Vector2 pos, Vector2 vel, float newLifeTime)
	{
		base.Reset(newOwner, pos, vel, newLifeTime);
		stretched = 0f;
		lastStretched = 0f;
	}

	public virtual float Rad(int type, float useLife, float useStretched, float timeStacker)
	{
		return 1f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[1] = new FSprite("Futile_White");
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("Background"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		if (!resting)
		{
			float useLife = Mathf.Lerp(lastLife, life, timeStacker);
			float useStretched = Mathf.Lerp(lastStretched, stretched, timeStacker);
			sLeaser.sprites[0].scaleX = Rad(0, useLife, useStretched, timeStacker) / 16f;
			sLeaser.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[1].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[1].scale = Rad(1, useLife, useStretched, timeStacker) / 16f;
			Vector2 vector = Vector2.Lerp(lastLingerPos, lingerPos, timeStacker);
			if (base.nextParticle != null)
			{
				vector = (lingerPos = Vector2.Lerp(base.nextParticle.lastPos, base.nextParticle.pos, timeStacker));
			}
			sLeaser.sprites[0].scaleY = (Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), vector) + Rad(2, useLife, useStretched, timeStacker)) / 16f;
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastPos, pos, timeStacker), vector);
			sLeaser.sprites[0].x = Mathf.Lerp(Mathf.Lerp(lastPos.x, pos.x, timeStacker), vector.x, 0.5f) - camPos.x;
			sLeaser.sprites[0].y = Mathf.Lerp(Mathf.Lerp(lastPos.y, pos.y, timeStacker), vector.y, 0.5f) - camPos.y;
		}
	}
}
