using RWCustom;
using UnityEngine;

namespace SplashWater;

public class Splash : WaterParticle
{
	public Vector2 lingerPos;

	public Vector2 lastLingerPos;

	public Vector2 lingerPosVel;

	public override void Reset(Vector2 pos, Vector2 vel, float amount, float initRad)
	{
		base.Reset(pos, vel, amount, initRad);
		lingerPos = new Vector2(pos.x, room.FloatWaterLevel(pos.x) - 30f - initRad);
		lastLingerPos = lingerPos;
		lingerPosVel = new Vector2(0f, 0f);
		base.pos = pos + initRad * vel.normalized;
		lifeTime = 60f;
	}

	public override void Update(bool eu)
	{
		if (lastLife < 0f)
		{
			Destroy();
		}
		base.Update(eu);
		lastLingerPos = lingerPos;
		lingerPos += lingerPosVel;
		if (room.FloatWaterLevel(lingerPos.x) - 20f > lingerPos.y)
		{
			lingerPos.y = room.FloatWaterLevel(lingerPos.x) - 20f;
		}
		lingerPosVel.y -= 0.9f;
		if (Custom.DistLess(lingerPos, pos, rad))
		{
			lingerPos = pos + Custom.DirVec(pos, lingerPos) * rad;
		}
		if (room.PointSubmerged(pos))
		{
			life -= 0.06f;
			room.waterObject.WaterfallHitSurface(pos.x - 5f, pos.x + 5f, Mathf.InverseLerp(-2f, -8f, vel.y));
			vel.y = Mathf.Abs(vel.y) * 0.4f;
			if (makeSoundCounter <= 0 && vel.magnitude > 4f)
			{
				room.PlaySound(SoundID.Splashing_Water_Into_Water_Surface, pos, Mathf.InverseLerp(4f, 14f, vel.magnitude), 1f);
				makeSoundCounter = int.MaxValue;
			}
		}
		else
		{
			vel.y -= 0.9f;
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].anchorY = 0f;
			sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["WaterSplash"];
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[i].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
			sLeaser.sprites[i].scaleX = Mathf.Lerp(lastRad, rad, timeStacker) / 8f;
			sLeaser.sprites[i].scaleY = Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), Vector2.Lerp(lastLingerPos, lingerPos, timeStacker)) / 16f;
			sLeaser.sprites[i].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastPos, pos, timeStacker), Vector2.Lerp(lastLingerPos, lingerPos, timeStacker));
			sLeaser.sprites[i].alpha = 0.5f * Mathf.Lerp(lastLife, life, timeStacker);
			sLeaser.sprites[i].color = new Color(0f, 0f, i);
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Water").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
	}
}
