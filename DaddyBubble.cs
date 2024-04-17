using System;
using RWCustom;
using UnityEngine;

public class DaddyBubble : CosmeticSprite
{
	public DaddyGraphics.DaddyBubbleOwner owner;

	public float life;

	private float lastLife;

	public int lifeTime;

	private float hollowNess;

	public bool hollow;

	public Vector2 originPoint;

	public bool stuckToOrigin;

	public bool freeFloating;

	public Vector2 liberatedOrigin;

	public Vector2 liberatedOriginVel;

	public Vector2 lastLiberatedOrigin;

	public Vector2 direction;

	public float lifeTimeWhenFree;

	public float stickiness;

	public int freeCounter;

	public DaddyBubble(DaddyGraphics.DaddyBubbleOwner owner, Vector2 dir, float intensity, float stickiness, float extraSpeed)
	{
		this.owner = owner;
		this.stickiness = stickiness;
		direction = dir.normalized;
		pos = owner.GetPosition();
		lastPos = pos;
		life = 1f;
		lifeTime = 10 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 200))));
		vel = dir + Custom.DegToVec(UnityEngine.Random.value * 360f) * (UnityEngine.Random.value * UnityEngine.Random.value * 12f * Mathf.Pow(intensity, 1.5f) + extraSpeed);
		hollowNess = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
		if (stickiness == 0f)
		{
			freeFloating = true;
		}
		else
		{
			stuckToOrigin = true;
		}
		liberatedOrigin = pos;
		lastLiberatedOrigin = pos;
		if (freeFloating)
		{
			freeCounter++;
		}
	}

	public override void Update(bool eu)
	{
		vel = Vector3.Slerp(vel, Custom.DegToVec(UnityEngine.Random.value * 360f), 0.2f);
		vel += direction * Mathf.Sin(life * (float)Math.PI);
		lastLife = life;
		life -= 1f / (float)lifeTime;
		if (life <= 0f)
		{
			Destroy();
		}
		if (freeFloating && UnityEngine.Random.value < 0.5f)
		{
			hollow = UnityEngine.Random.value < 0.5f;
		}
		if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid)
		{
			lifeTime = Math.Min(1, lifeTime - 5);
		}
		if ((!ModManager.MSC) ? (pos.y < room.FloatWaterLevel(pos.x)) : room.PointSubmerged(pos))
		{
			vel *= 0.9f;
			vel.y += 4f;
		}
		if (stuckToOrigin)
		{
			Vector2 position = owner.GetPosition();
			liberatedOriginVel = position - liberatedOrigin;
			liberatedOrigin = position;
			lastLiberatedOrigin = position;
			if (life < 0.5f || UnityEngine.Random.value < 1f / (10f + stickiness * 80f) || !Custom.DistLess(pos, position, 10f + 90f * stickiness))
			{
				stuckToOrigin = false;
			}
		}
		else if (!freeFloating)
		{
			lastLiberatedOrigin = liberatedOrigin;
			liberatedOriginVel = Vector2.Lerp(liberatedOriginVel, Custom.DirVec(liberatedOrigin, pos) * Mathf.Lerp(Vector2.Distance(liberatedOrigin, pos), 10f, 0.5f), 0.7f);
			liberatedOrigin += liberatedOriginVel;
			if (Custom.DistLess(liberatedOrigin, pos, 5f))
			{
				vel = Vector2.Lerp(vel, liberatedOriginVel, 0.3f);
				lifeTimeWhenFree = life;
				freeFloating = true;
			}
		}
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("LizardBubble0");
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float num = 0.625f * Mathf.Lerp(Mathf.Lerp(Mathf.Sin((float)Math.PI * lastLife), lastLife, 0.5f), Mathf.Lerp(Mathf.Sin((float)Math.PI * life), life, 0.5f), timeStacker);
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].color = Color.Lerp(owner.GetColor(), Color.blue, Mathf.InverseLerp(2f, 7f, (float)freeCounter + timeStacker));
		float num2 = (sLeaser.sprites[0].color.r + sLeaser.sprites[0].color.g + (1f - sLeaser.sprites[0].color.b)) / 3f;
		int num3 = 0;
		if (hollow)
		{
			num3 = Custom.IntClamp((int)(Mathf.Pow(Mathf.InverseLerp(lifeTimeWhenFree, 0f, life), hollowNess) * 7f), 1, 7);
		}
		sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("LizardBubble" + num3);
		if (stuckToOrigin || !freeFloating)
		{
			Vector2 vector = ((!stuckToOrigin) ? Vector2.Lerp(lastLiberatedOrigin, liberatedOrigin, timeStacker) : owner.GetPosition());
			float num4 = Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), vector) / 16f;
			sLeaser.sprites[0].scaleX = Mathf.Min(num, num / Mathf.Lerp(num4, 1f, 0.35f)) * (1f - 0.75f * num2);
			sLeaser.sprites[0].scaleY = Mathf.Max(num, num4 - 0.125f);
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastPos, pos, timeStacker), vector);
			sLeaser.sprites[0].anchorY = 0f;
		}
		else
		{
			sLeaser.sprites[0].scaleX = num;
			sLeaser.sprites[0].scaleY = num;
			sLeaser.sprites[0].rotation = 0f;
			sLeaser.sprites[0].anchorY = 0.5f;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Midground");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
	}
}
