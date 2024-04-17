using System;
using RWCustom;
using UnityEngine;

public class LizardBubble : CosmeticSprite
{
	public LizardGraphics lizardGraphics;

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

	public float lifeTimeWhenFree;

	public float stickiness;

	private string[] _lizardBubbleName = new string[8] { "LizardBubble0", "LizardBubble1", "LizardBubble2", "LizardBubble3", "LizardBubble4", "LizardBubble5", "LizardBubble6", "LizardBubble7" };

	public float Stickiness => Mathf.Max(stickiness, Mathf.InverseLerp(4f, 14f, lizardGraphics.head.vel.magnitude));

	public LizardBubble(LizardGraphics lizardGraphics, float intensity, float stickiness, float extraSpeed)
	{
		this.lizardGraphics = lizardGraphics;
		this.stickiness = stickiness;
		pos = Vector2.Lerp(lizardGraphics.lizard.mainBodyChunk.pos, lizardGraphics.head.pos, UnityEngine.Random.value) + Custom.DegToVec(UnityEngine.Random.value * 360f) * 5f * UnityEngine.Random.value;
		originPoint = Custom.RotateAroundOrigo(pos - lizardGraphics.head.pos, 0f - lizardGraphics.HeadRotation(1f));
		lastPos = pos;
		life = 1f;
		lifeTime = 10 + UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, UnityEngine.Random.Range(0, 200))));
		vel = lizardGraphics.head.vel + Custom.DegToVec(UnityEngine.Random.value * 360f) * (UnityEngine.Random.value * UnityEngine.Random.value * 12f * Mathf.Pow(intensity, 1.5f) + extraSpeed);
		hollowNess = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
		if (Stickiness == 0f)
		{
			freeFloating = true;
		}
		else
		{
			stuckToOrigin = true;
		}
		liberatedOrigin = pos;
		lastLiberatedOrigin = pos;
	}

	public override void Update(bool eu)
	{
		vel = Vector3.Slerp(vel, Custom.DegToVec(UnityEngine.Random.value * 360f), 0.2f);
		if (lizardGraphics != null && lizardGraphics.voiceVisualizationIntensity > 0f)
		{
			vel += Custom.DirVec(lizardGraphics.head.pos, pos) * Mathf.Lerp(-0.5f, 3f, lizardGraphics.voiceVisualization);
		}
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
			Vector2 vector = lizardGraphics.head.pos + Custom.RotateAroundOrigo(originPoint, lizardGraphics.HeadRotation(1f));
			liberatedOriginVel = vector - liberatedOrigin;
			liberatedOrigin = vector;
			lastLiberatedOrigin = vector;
			if (life < 0.5f || UnityEngine.Random.value < 1f / (10f + Stickiness * 80f) || !Custom.DistLess(pos, vector, 10f + 90f * Stickiness))
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
		sLeaser.sprites[0].color = Color.Lerp(lizardGraphics.HeadColor(timeStacker), lizardGraphics.palette.blackColor, 1f - Mathf.Clamp(Mathf.Lerp(lastLife, life, timeStacker) * 2f, 0f, 1f));
		int num2 = 0;
		if (hollow)
		{
			num2 = Custom.IntClamp((int)(Mathf.Pow(Mathf.InverseLerp(lifeTimeWhenFree, 0f, life), hollowNess) * 7f), 1, 7);
		}
		sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(_lizardBubbleName[num2]);
		if (stuckToOrigin || !freeFloating)
		{
			Vector2 vector = ((!stuckToOrigin) ? Vector2.Lerp(lastLiberatedOrigin, liberatedOrigin, timeStacker) : (Vector2.Lerp(lizardGraphics.head.lastPos, lizardGraphics.head.pos, timeStacker) + Custom.RotateAroundOrigo(originPoint, lizardGraphics.HeadRotation(timeStacker))));
			float num3 = Vector2.Distance(Vector2.Lerp(lastPos, pos, timeStacker), vector) / 16f;
			sLeaser.sprites[0].scaleX = Mathf.Min(num, num / Mathf.Lerp(num3, 1f, 0.35f));
			sLeaser.sprites[0].scaleY = Mathf.Max(num, num3 - 0.3125f);
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastPos, pos, timeStacker), vector);
			sLeaser.sprites[0].anchorY = Mathf.InverseLerp(1.25f, 0.3125f, num3) * 0.5f;
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
