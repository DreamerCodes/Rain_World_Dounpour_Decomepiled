using System;
using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class TailFin : Template
{
	public int bumps;

	public float spineLength;

	public float sizeSkewExponent;

	public float sizeRangeMin;

	public float sizeRangeMax;

	public float undersideSize;

	public int graphic;

	public float scaleX;

	public bool colored;

	public TailFin(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		spritesOverlap = SpritesOverlap.BehindHead;
		float num = Mathf.Lerp(4f, 7f, Mathf.Pow(UnityEngine.Random.value, 0.7f));
		spineLength = Custom.ClampedRandomVariation(0.5f, 0.17f, 0.5f) * lGraphics.BodyAndTailLength;
		undersideSize = Mathf.Lerp(0.3f, 0.9f, UnityEngine.Random.value);
		sizeRangeMin = Mathf.Lerp(0.1f, 0.3f, Mathf.Pow(UnityEngine.Random.value, 2f));
		sizeRangeMax = Mathf.Lerp(sizeRangeMin, 0.6f, UnityEngine.Random.value);
		sizeSkewExponent = Mathf.Lerp(0.5f, 1.5f, UnityEngine.Random.value);
		graphic = UnityEngine.Random.Range(0, 6);
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard)
		{
			graphic = 0;
			for (int i = 0; i < lGraphics.cosmetics.Count; i++)
			{
				if (lGraphics.cosmetics[i] is LongBodyScales)
				{
					graphic = (lGraphics.cosmetics[i] as LongBodyScales).graphic;
					break;
				}
			}
			sizeRangeMin *= 2f;
			sizeRangeMax *= 1.5f;
			spineLength = Custom.ClampedRandomVariation(0.3f, 0.17f, 0.5f) * lGraphics.BodyAndTailLength;
		}
		bumps = (int)(spineLength / num);
		scaleX = Mathf.Lerp(1f, 2f, UnityEngine.Random.value);
		if (graphic == 3 && UnityEngine.Random.value < 0.5f)
		{
			scaleX = 0f - scaleX;
		}
		else if (graphic != 0 && UnityEngine.Random.value < 1f / 15f)
		{
			scaleX = 0f - scaleX;
		}
		colored = UnityEngine.Random.value > 1f / 3f;
		numberOfSprites = (colored ? (bumps * 2) : bumps) * 2;
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < 2; i++)
		{
			int num = i * (colored ? (bumps * 2) : bumps);
			for (int num2 = startSprite + bumps - 1; num2 >= startSprite; num2--)
			{
				sLeaser.sprites[num2 + num] = new FSprite("LizardScaleA" + graphic);
				sLeaser.sprites[num2 + num].anchorY = 0.15f;
				if (colored)
				{
					sLeaser.sprites[num2 + bumps + num] = new FSprite("LizardScaleB" + graphic);
					sLeaser.sprites[num2 + bumps + num].anchorY = 0.15f;
				}
			}
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < 2; i++)
		{
			int num = i * (colored ? (bumps * 2) : bumps);
			for (int num2 = startSprite + bumps - 1; num2 >= startSprite; num2--)
			{
				float num3 = Mathf.InverseLerp(startSprite, startSprite + bumps - 1, num2);
				LizardGraphics.LizardSpineData lizardSpineData = lGraphics.SpinePosition(Mathf.Lerp(1f - spineLength / lGraphics.BodyAndTailLength, 1f, num3), timeStacker);
				switch (i)
				{
				case 0:
					sLeaser.sprites[num2 + num].x = lizardSpineData.outerPos.x - camPos.x;
					sLeaser.sprites[num2 + num].y = lizardSpineData.outerPos.y - camPos.y;
					break;
				case 1:
					sLeaser.sprites[num2 + num].x = lizardSpineData.pos.x + (lizardSpineData.pos.x - lizardSpineData.outerPos.x) * 0.85f - camPos.x;
					sLeaser.sprites[num2 + num].y = lizardSpineData.pos.y + (lizardSpineData.pos.y - lizardSpineData.outerPos.y) * 0.85f - camPos.y;
					break;
				}
				sLeaser.sprites[num2 + num].rotation = Custom.VecToDeg(Vector2.Lerp(lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.dir * ((i != 1) ? 1 : (-1)), num3));
				float num4 = Mathf.Lerp(sizeRangeMin, sizeRangeMax, Mathf.Sin(Mathf.Pow(num3, sizeSkewExponent) * (float)Math.PI));
				sLeaser.sprites[num2 + num].scaleX = Mathf.Sign(lGraphics.depthRotation) * scaleX * num4;
				sLeaser.sprites[num2 + num].scaleY = num4 * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lGraphics.depthRotation))) * ((i == 1) ? (0f - undersideSize) : 1f);
				if (colored)
				{
					switch (i)
					{
					case 0:
						sLeaser.sprites[num2 + bumps + num].x = lizardSpineData.outerPos.x - camPos.x;
						sLeaser.sprites[num2 + bumps + num].y = lizardSpineData.outerPos.y - camPos.y;
						break;
					case 1:
						sLeaser.sprites[num2 + bumps + num].x = lizardSpineData.pos.x + (lizardSpineData.pos.x - lizardSpineData.outerPos.x) * 0.85f - camPos.x;
						sLeaser.sprites[num2 + bumps + num].y = lizardSpineData.pos.y + (lizardSpineData.pos.y - lizardSpineData.outerPos.y) * 0.85f - camPos.y;
						break;
					}
					sLeaser.sprites[num2 + bumps + num].rotation = Custom.VecToDeg(Vector2.Lerp(lizardSpineData.perp * lizardSpineData.depthRotation, lizardSpineData.dir * ((i != 1) ? 1 : (-1)), num3));
					sLeaser.sprites[num2 + bumps + num].scaleX = Mathf.Sign(lGraphics.depthRotation) * scaleX * num4;
					sLeaser.sprites[num2 + bumps + num].scaleY = num4 * Mathf.Max(0.2f, Mathf.InverseLerp(0f, 0.5f, Mathf.Abs(lGraphics.depthRotation))) * ((i == 1) ? (0f - undersideSize) : 1f);
					if (i == 1)
					{
						sLeaser.sprites[num2 + bumps + num].alpha = Mathf.Pow(Mathf.InverseLerp(0.3f, 1f, Mathf.Abs(lGraphics.depthRotation)), 0.2f);
					}
				}
			}
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		for (int i = 0; i < 2; i++)
		{
			int num = i * (colored ? (bumps * 2) : bumps);
			for (int j = startSprite; j < startSprite + bumps; j++)
			{
				float f = Mathf.Lerp(0.05f, spineLength / lGraphics.BodyAndTailLength, Mathf.InverseLerp(startSprite, startSprite + bumps - 1, j));
				sLeaser.sprites[j + num].color = lGraphics.BodyColor(f);
				if (colored)
				{
					sLeaser.sprites[j + bumps + num].color = lGraphics.effectColor;
				}
			}
		}
	}
}
