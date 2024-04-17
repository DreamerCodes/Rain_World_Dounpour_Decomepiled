using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class JumpRings : Template
{
	public int RingSprite(int ring, int side, int part)
	{
		return startSprite + part * 4 + side * 2 + ring;
	}

	public JumpRings(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.InFront;
		numberOfSprites = 8;
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				for (int k = 0; k < 2; k++)
				{
					sLeaser.sprites[RingSprite(i, j, k)] = new FSprite("Circle20");
				}
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		float b = Mathf.Lerp(lGraphics.lastDepthRotation, lGraphics.depthRotation, timeStacker);
		float a = Mathf.Lerp(lGraphics.lastHeadDepthRotation, lGraphics.headDepthRotation, timeStacker);
		Color color = lGraphics.HeadColor(timeStacker);
		float num = 1f;
		if (lGraphics.lizard.animation == Lizard.Animation.PrepareToJump)
		{
			num = 0.5f + 0.5f * Mathf.InverseLerp(lGraphics.lizard.timeToRemainInAnimation, 0f, lGraphics.lizard.timeInAnimation);
			color = Color.Lerp(lGraphics.HeadColor(timeStacker), Color.Lerp(Color.white, lGraphics.effectColor, num), Random.value);
		}
		for (int i = 0; i < 2; i++)
		{
			float s = 0.06f + 0.12f * (float)i;
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(s, timeStacker);
			Vector2 vector = lizardSpineData.dir;
			Vector2 pos = lizardSpineData.pos;
			if (i == 0)
			{
				vector = (vector - Custom.DirVec(Vector2.Lerp(lGraphics.drawPositions[0, 1], lGraphics.drawPositions[0, 0], timeStacker), Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker))).normalized;
			}
			Vector2 vector2 = Custom.PerpendicularVector(vector);
			float num2 = 50f * Mathf.Lerp(a, b, (i == 0) ? 0.25f : 0.5f);
			for (int j = 0; j < 2; j++)
			{
				Vector2 vector3 = Custom.DegToVec(num2 + (((float)j == 0f) ? (-40f) : 40f));
				Vector2 p = pos + vector2 * lizardSpineData.rad * vector3.x;
				Vector2 vector4 = vector;
				vector4 = ((i != 0) ? (vector4 + 2f * Custom.DirVec(p, Vector2.Lerp(lGraphics.tail[0].lastPos, lGraphics.tail[0].pos, timeStacker)) * Mathf.Abs(vector3.y)).normalized : (vector4 - 2f * Custom.DirVec(p, Vector2.Lerp(lGraphics.head.lastPos, lGraphics.head.pos, timeStacker)) * Mathf.Abs(vector3.y)).normalized);
				sLeaser.sprites[RingSprite(i, j, 0)].x = p.x - camPos.x;
				sLeaser.sprites[RingSprite(i, j, 0)].y = p.y - camPos.y;
				sLeaser.sprites[RingSprite(i, j, 0)].rotation = Custom.VecToDeg(vector4);
				p = pos + vector2 * (lizardSpineData.rad + 2f * Mathf.Pow(Mathf.Clamp01(Mathf.Abs(vector3.x) * Mathf.Abs(vector3.y)), 0.5f)) * vector3.x;
				p -= vector4 * (1f - num) * 4f;
				sLeaser.sprites[RingSprite(i, j, 1)].x = p.x - camPos.x;
				sLeaser.sprites[RingSprite(i, j, 1)].y = p.y - camPos.y;
				sLeaser.sprites[RingSprite(i, j, 1)].rotation = Custom.VecToDeg(vector4);
				float t = Mathf.Pow(Mathf.Clamp01(Mathf.Abs(vector3.x)), 2f);
				sLeaser.sprites[RingSprite(i, j, 0)].scaleX = ((vector3.y > 0f) ? Mathf.Lerp(0.45f, 0f, t) : 0f);
				sLeaser.sprites[RingSprite(i, j, 0)].scaleY = 0.55f;
				sLeaser.sprites[RingSprite(i, j, 0)].color = new Color(1f, 0f, 0f);
				sLeaser.sprites[RingSprite(i, j, 0)].color = color;
				sLeaser.sprites[RingSprite(i, j, 1)].scaleX = ((vector3.y > 0f) ? (Mathf.Lerp(0.27f, 0f, t) * num) : 0f);
				sLeaser.sprites[RingSprite(i, j, 1)].scaleY = 0.33f * num;
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[RingSprite(i, j, 1)].color = palette.blackColor;
			}
		}
	}
}
