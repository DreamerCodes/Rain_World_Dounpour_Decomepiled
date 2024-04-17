using System;
using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class BodyStripes : Template
{
	public Vector2[] scalesPositions;

	public BodyStripes(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BehindHead;
		float num = 1.5f;
		float num2 = Mathf.Lerp(0.4f, 0.8f, UnityEngine.Random.value);
		float num3 = num2 * base.lGraphics.BodyAndTailLength;
		float num4 = Mathf.Lerp(5f, 12f, UnityEngine.Random.value);
		num4 *= num;
		int num5 = (int)(num3 / num4);
		if (num5 < 3)
		{
			num5 = 3;
		}
		scalesPositions = new Vector2[num5 * 2];
		for (int i = 0; i < num5; i++)
		{
			float y = Mathf.Lerp(0f, num2, (float)i / (float)(num5 - 1));
			float num6 = 0.6f + 0.4f * Mathf.Sin((float)i / (float)(num5 - 1) * (float)Math.PI);
			scalesPositions[i * 2] = new Vector2(num6, y);
			scalesPositions[i * 2 + 1] = new Vector2(0f - num6, y);
		}
		numberOfSprites = scalesPositions.Length;
	}

	protected LizardGraphics.LizardSpineData GetBackPos(int shoulderScale, float timeStacker, bool changeDepthRotation)
	{
		LizardGraphics.LizardSpineData result = lGraphics.SpinePosition(scalesPositions[shoulderScale].y, timeStacker);
		float num = Mathf.Clamp(scalesPositions[shoulderScale].x + result.depthRotation, -1f, 1f);
		result.outerPos = result.pos + result.perp * num * result.rad;
		if (changeDepthRotation)
		{
			result.depthRotation = num;
		}
		return result;
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
		{
			sLeaser.sprites[num] = TriangleMesh.MakeLongMesh(1, pointyTip: false, customColor: true);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
		{
			LizardGraphics.LizardSpineData backPos = GetBackPos(num - startSprite, timeStacker, changeDepthRotation: true);
			LizardGraphics.LizardSpineData backPos2 = GetBackPos(((num % 2 == 0) ? (num + 1) : (num - 1)) - startSprite, timeStacker, changeDepthRotation: true);
			float num2 = Custom.Dist(backPos.outerPos, backPos2.outerPos);
			float ang = Custom.AimFromOneVectorToAnother(backPos.outerPos, backPos2.outerPos);
			Vector2 vector = Custom.PerpendicularVector(Custom.DegToVec(ang));
			Vector2 vector2 = new Vector2(backPos.outerPos.x, backPos.outerPos.y);
			Vector2 vector3 = vector2 + Custom.DegToVec(ang) * (num2 * 0.5f);
			float num3 = 0.5f;
			float num4 = 3f;
			(sLeaser.sprites[num] as TriangleMesh).MoveVertice(0, vector2 - vector * num3 - camPos);
			(sLeaser.sprites[num] as TriangleMesh).MoveVertice(1, vector2 + vector * num3 - camPos);
			(sLeaser.sprites[num] as TriangleMesh).MoveVertice(2, vector3 - vector * num4 - camPos);
			(sLeaser.sprites[num] as TriangleMesh).MoveVertice(3, vector3 + vector * num4 - camPos);
			for (int i = 0; i < 4; i++)
			{
				if (i > 1)
				{
					(sLeaser.sprites[num] as TriangleMesh).verticeColors[i] = lGraphics.effectColor;
				}
				else
				{
					(sLeaser.sprites[num] as TriangleMesh).verticeColors[i] = lGraphics.BodyColor(timeStacker);
				}
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
	}
}
