using System;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class SpineSpikes : Template
{
	public int bumps;

	public float spineLength;

	public float sizeSkewExponent;

	public float sizeRangeMin;

	public float sizeRangeMax;

	public int graphic;

	public float scaleX;

	public int colored;

	public SpineSpikes(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BehindHead;
		float num = Mathf.Lerp(5f, 8f, Mathf.Pow(UnityEngine.Random.value, 0.7f));
		spineLength = Mathf.Lerp(0.2f, 0.95f, UnityEngine.Random.value) * lGraphics.BodyAndTailLength;
		sizeRangeMin = Mathf.Lerp(0.1f, 0.5f, Mathf.Pow(UnityEngine.Random.value, 2f));
		sizeRangeMax = Mathf.Lerp(sizeRangeMin, 1.1f, UnityEngine.Random.value);
		if (UnityEngine.Random.value < 0.5f)
		{
			sizeRangeMax = 1f;
		}
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.BlueLizard)
		{
			sizeRangeMin = Mathf.Min(sizeRangeMin, 0.3f);
			sizeRangeMax = Mathf.Min(sizeRangeMax, 0.6f);
		}
		else if (lGraphics.lizard.Template.type != CreatureTemplate.Type.GreenLizard && UnityEngine.Random.value < 0.7f)
		{
			sizeRangeMin *= 0.7f;
			sizeRangeMax *= 0.7f;
		}
		else if (lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard && UnityEngine.Random.value < 0.7f)
		{
			sizeRangeMin = Mathf.Lerp(sizeRangeMin, 1.1f, 0.1f);
			sizeRangeMax = Mathf.Lerp(sizeRangeMax, 1.1f, 0.4f);
		}
		sizeSkewExponent = Mathf.Lerp(0.1f, 0.9f, UnityEngine.Random.value);
		bumps = (int)(spineLength / num);
		scaleX = 1f;
		graphic = UnityEngine.Random.Range(0, 5);
		if (graphic == 1)
		{
			graphic = 0;
		}
		if (graphic == 4)
		{
			graphic = 3;
		}
		else if (graphic == 3 && UnityEngine.Random.value < 0.5f)
		{
			scaleX = -1f;
		}
		else if (UnityEngine.Random.value < 1f / 15f)
		{
			scaleX = -1f;
		}
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.PinkLizard && UnityEngine.Random.value < 0.7f)
		{
			graphic = 0;
		}
		else if (lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard && UnityEngine.Random.value < 0.5f)
		{
			graphic = 3;
		}
		colored = UnityEngine.Random.Range(0, 3);
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.PinkLizard && UnityEngine.Random.value < 0.5f)
		{
			colored = 0;
		}
		else if (lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard && UnityEngine.Random.value < 0.5f)
		{
			colored = 2;
		}
		else if (lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard && UnityEngine.Random.value < 0.5f)
		{
			colored = 1;
		}
		if (ModManager.MSC && lGraphics.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.TrainLizard)
		{
			colored = 1;
			sizeRangeMin = 1f;
			sizeRangeMax = 3f;
		}
		numberOfSprites = ((colored > 0) ? (bumps * 2) : bumps);
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int num = startSprite + bumps - 1; num >= startSprite; num--)
		{
			sLeaser.sprites[num] = new FSprite("LizardScaleA" + graphic);
			sLeaser.sprites[num].anchorY = 0.15f;
			if (colored > 0)
			{
				sLeaser.sprites[num + bumps] = new FSprite("LizardScaleB" + graphic);
				sLeaser.sprites[num + bumps].anchorY = 0.15f;
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int num = startSprite + bumps - 1; num >= startSprite; num--)
		{
			float num2 = Mathf.InverseLerp(startSprite, startSprite + bumps - 1, num);
			LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(Mathf.Lerp(0.05f, spineLength / lGraphics.BodyAndTailLength, num2), timeStacker);
			sLeaser.sprites[num].x = lizardSpineData.outerPos.x - camPos.x;
			sLeaser.sprites[num].y = lizardSpineData.outerPos.y - camPos.y;
			sLeaser.sprites[num].rotation = Custom.AimFromOneVectorToAnother(-lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.perp * lizardSpineData.depthRotation);
			float num3 = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(Mathf.Pow(num2, sizeSkewExponent) * (float)Math.PI));
			sLeaser.sprites[num].scaleX = Mathf.Sign(lGraphics.depthRotation) * scaleX * num3;
			sLeaser.sprites[num].scaleY = num3 * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lGraphics.depthRotation)));
			if (colored > 0)
			{
				sLeaser.sprites[num + bumps].x = lizardSpineData.outerPos.x - camPos.x;
				sLeaser.sprites[num + bumps].y = lizardSpineData.outerPos.y - camPos.y;
				sLeaser.sprites[num + bumps].rotation = Custom.AimFromOneVectorToAnother(-lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.perp * lizardSpineData.depthRotation);
				sLeaser.sprites[num + bumps].scaleX = Mathf.Sign(lGraphics.depthRotation) * scaleX * num3;
				sLeaser.sprites[num + bumps].scaleY = num3 * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lGraphics.depthRotation)));
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = startSprite; i < startSprite + bumps; i++)
		{
			float f = Mathf.Lerp(0.05f, spineLength / lGraphics.BodyAndTailLength, Mathf.InverseLerp(startSprite, startSprite + bumps - 1, i));
			sLeaser.sprites[i].color = lGraphics.BodyColor(f);
			if (colored == 1)
			{
				sLeaser.sprites[i + bumps].color = lGraphics.effectColor;
			}
			else if (colored == 2)
			{
				float f2 = Mathf.InverseLerp(startSprite, startSprite + bumps - 1, i);
				sLeaser.sprites[i + bumps].color = Color.Lerp(lGraphics.effectColor, lGraphics.BodyColor(f), Mathf.Pow(f2, 0.5f));
			}
		}
	}
}
