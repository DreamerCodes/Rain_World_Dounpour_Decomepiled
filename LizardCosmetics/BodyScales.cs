using System;
using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class BodyScales : Template
{
	public int graphic;

	public float scaleX;

	public bool colored;

	public Vector2[] scalesPositions;

	public BodyScales(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = ((this is LongHeadScales) ? SpritesOverlap.InFront : SpritesOverlap.BehindHead);
	}

	protected void GeneratePatchPattern(float startPoint, int numOfScales, float maxLength, float lengthExponent)
	{
		scalesPositions = new Vector2[numOfScales];
		float num = Mathf.Lerp(startPoint + 0.1f, Mathf.Max(startPoint + 0.2f, maxLength), Mathf.Pow(UnityEngine.Random.value, lengthExponent));
		for (int i = 0; i < scalesPositions.Length; i++)
		{
			Vector2 vector = Custom.DegToVec(UnityEngine.Random.value * 360f) * UnityEngine.Random.value;
			scalesPositions[i].y = Mathf.Lerp(startPoint * lGraphics.bodyLength / lGraphics.BodyAndTailLength, num * lGraphics.bodyLength / lGraphics.BodyAndTailLength, (vector.y + 1f) / 2f);
			scalesPositions[i].x = vector.x;
		}
	}

	protected void GenerateTwoLines(float startPoint, float maxLength, float lengthExponent, float spacingScale)
	{
		float num = Mathf.Lerp(startPoint + 0.1f, Mathf.Max(startPoint + 0.2f, maxLength), Mathf.Pow(UnityEngine.Random.value, lengthExponent));
		float num2 = num * lGraphics.BodyAndTailLength;
		float num3 = Mathf.Lerp(2f, 9f, UnityEngine.Random.value);
		if (lGraphics.lizard.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.BlueLizard)
		{
			num3 = 2f;
		}
		num3 *= spacingScale;
		int num4 = (int)(num2 / num3);
		if (num4 < 3)
		{
			num4 = 3;
		}
		scalesPositions = new Vector2[num4 * 2];
		for (int i = 0; i < num4; i++)
		{
			float y = Mathf.Lerp(0f, num, (float)i / (float)(num4 - 1));
			float num5 = 0.6f + 0.4f * Mathf.Sin((float)i / (float)(num4 - 1) * (float)Math.PI);
			scalesPositions[i * 2] = new Vector2(num5, y);
			scalesPositions[i * 2 + 1] = new Vector2(0f - num5, y);
		}
	}

	protected void GenerateSegments(float startPoint, float maxLength, float lengthExponent)
	{
		float num = Mathf.Lerp(startPoint + 0.1f, Mathf.Max(startPoint + 0.2f, maxLength), Mathf.Pow(UnityEngine.Random.value, lengthExponent));
		float num2 = num * lGraphics.BodyAndTailLength;
		float num3 = Mathf.Lerp(7f, 14f, UnityEngine.Random.value);
		if (lGraphics.lizard.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.RedLizard)
		{
			num3 = Mathf.Min(num3, 11f) * 0.75f;
		}
		int num4 = Math.Max(3, (int)(num2 / num3));
		int num5 = UnityEngine.Random.Range(1, 4) * 2;
		scalesPositions = new Vector2[num4 * num5];
		for (int i = 0; i < num4; i++)
		{
			float y = Mathf.Lerp(0f, num, (float)i / (float)(num4 - 1));
			for (int j = 0; j < num5; j++)
			{
				float num6 = 0.6f + 0.6f * Mathf.Sin((float)i / (float)(num4 - 1) * (float)Math.PI);
				num6 *= Mathf.Lerp(-1f, 1f, (float)j / (float)(num5 - 1));
				scalesPositions[i * num5 + j] = new Vector2(num6, y);
			}
		}
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
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		base.ApplyPalette(sLeaser, rCam, palette);
	}
}
