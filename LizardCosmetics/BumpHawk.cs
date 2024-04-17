using System;
using UnityEngine;

namespace LizardCosmetics;

public class BumpHawk : Template
{
	public int bumps;

	public float spineLength;

	public float sizeSkewExponent;

	public float sizeRangeMin;

	public float sizeRangeMax;

	public bool coloredHawk;

	public BumpHawk(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		coloredHawk = UnityEngine.Random.value < 0.5f;
		spritesOverlap = SpritesOverlap.BehindHead;
		float num;
		if (coloredHawk)
		{
			num = Mathf.Lerp(3f, 8f, Mathf.Pow(UnityEngine.Random.value, 0.7f));
			spineLength = Mathf.Lerp(0.3f, 0.7f, UnityEngine.Random.value) * lGraphics.BodyAndTailLength;
			sizeRangeMin = Mathf.Lerp(0.1f, 0.2f, UnityEngine.Random.value);
			sizeRangeMax = Mathf.Lerp(sizeRangeMin, 0.35f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
		}
		else
		{
			num = Mathf.Lerp(6f, 12f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
			spineLength = Mathf.Lerp(0.3f, 0.9f, UnityEngine.Random.value) * lGraphics.BodyAndTailLength;
			sizeRangeMin = Mathf.Lerp(0.2f, 0.3f, Mathf.Pow(UnityEngine.Random.value, 0.5f));
			sizeRangeMax = Mathf.Lerp(sizeRangeMin, 0.5f, UnityEngine.Random.value);
		}
		sizeSkewExponent = Mathf.Lerp(0.1f, 0.7f, UnityEngine.Random.value);
		bumps = (int)(spineLength / num);
		numberOfSprites = bumps;
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int num = startSprite + numberOfSprites - 1; num >= startSprite; num--)
		{
			float num2 = Mathf.InverseLerp(startSprite, startSprite + numberOfSprites - 1, num);
			sLeaser.sprites[num] = new FSprite("Circle20");
			sLeaser.sprites[num].scale = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Lerp(Mathf.Sin(Mathf.Pow(num2, sizeSkewExponent) * (float)Math.PI), 1f, (num2 < 0.5f) ? 0.5f : 0f));
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int num = startSprite + numberOfSprites - 1; num >= startSprite; num--)
		{
			float num2 = Mathf.InverseLerp(startSprite, startSprite + numberOfSprites - 1, num);
			float num3 = Mathf.Lerp(0.05f, spineLength / lGraphics.BodyAndTailLength, num2);
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(num3, timeStacker);
			sLeaser.sprites[num].x = lizardSpineData.outerPos.x - camPos.x;
			sLeaser.sprites[num].y = lizardSpineData.outerPos.y - camPos.y;
			if (coloredHawk || lGraphics.lizard.Template.type == CreatureTemplate.Type.WhiteLizard)
			{
				if (coloredHawk)
				{
					sLeaser.sprites[num].color = Color.Lerp(lGraphics.HeadColor(timeStacker), lGraphics.BodyColor(num3), num2);
				}
				else
				{
					sLeaser.sprites[num].color = lGraphics.DynamicBodyColor(num2);
				}
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		if (!coloredHawk)
		{
			for (int i = startSprite; i < startSprite + numberOfSprites; i++)
			{
				float f = Mathf.Lerp(0.05f, spineLength / lGraphics.BodyAndTailLength, Mathf.InverseLerp(startSprite, startSprite + numberOfSprites - 1, i));
				sLeaser.sprites[i].color = lGraphics.BodyColor(f);
			}
		}
	}
}
