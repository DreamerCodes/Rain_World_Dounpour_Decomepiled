using System;
using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class LongShoulderScales : LongBodyScales
{
	public LongShoulderScales(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		rigor = 0f;
		int num = 0;
		if (lGraphics.lizard.Template.type != CreatureTemplate.Type.PinkLizard || UnityEngine.Random.value < 1f / 3f)
		{
			num = UnityEngine.Random.Range(0, 3);
		}
		else if (lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard || UnityEngine.Random.value < 0.5f)
		{
			num = 2;
		}
		switch (num)
		{
		case 0:
			GeneratePatchPattern(0.05f, UnityEngine.Random.Range(4, 15), 0.9f, 2f);
			break;
		case 1:
			GenerateTwoLines(0.07f, 1f, 1.5f, 3f);
			break;
		case 2:
			GenerateSegments(0.1f, 0.8f, 5f);
			break;
		}
		MoveScalesTowardsHead();
		float num2 = Mathf.Lerp(1f, 1f / Mathf.Lerp(1f, scalesPositions.Length, Mathf.Pow(UnityEngine.Random.value, 2f)), 0.5f);
		float num3 = Mathf.Lerp(5f, 15f, UnityEngine.Random.value) * num2;
		float b = Mathf.Lerp(num3, 35f, Mathf.Pow(UnityEngine.Random.value, 0.5f)) * num2;
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard)
		{
			if (scalesPositions.Length > 8)
			{
				StretchDownOnBack((num == 0) ? 0.3f : 0.5f);
			}
			num2 = Mathf.Max(0.5f, num2);
			num3 = Mathf.Max(10f, num3) * 1.2f;
			b = Mathf.Max(25f, b) * 1.2f;
		}
		colored = lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard || lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard || UnityEngine.Random.value < 0.4f;
		if (UnityEngine.Random.value < 0.1f)
		{
			graphic = UnityEngine.Random.Range(0, 7);
		}
		else
		{
			graphic = UnityEngine.Random.Range(3, 6);
		}
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.PinkLizard && UnityEngine.Random.value < 0.25f)
		{
			graphic = 0;
		}
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard)
		{
			graphic = 0;
			if (UnityEngine.Random.value < 0.3f)
			{
				graphic = 3;
				scaleX = ((UnityEngine.Random.value < 0.5f) ? (-1f) : 1f);
			}
		}
		graphicHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + graphic).sourcePixelSize.y;
		scaleObjects = new LizardScale[scalesPositions.Length];
		backwardsFactors = new float[scalesPositions.Length];
		float num4 = 0f;
		float num5 = 1f;
		for (int i = 0; i < scalesPositions.Length; i++)
		{
			if (scalesPositions[i].y > num4)
			{
				num4 = scalesPositions[i].y;
			}
			if (scalesPositions[i].y < num5)
			{
				num5 = scalesPositions[i].y;
			}
		}
		float p = Mathf.Lerp(0.1f, 0.9f, UnityEngine.Random.value);
		for (int j = 0; j < scalesPositions.Length; j++)
		{
			scaleObjects[j] = new LizardScale(this);
			float num6 = Mathf.Pow(Mathf.InverseLerp(num5, num4, scalesPositions[j].y), p);
			scaleObjects[j].length = Mathf.Lerp(num3, b, Mathf.Lerp(Mathf.Sin(num6 * (float)Math.PI), 1f, (num6 < 0.5f) ? 0.5f : 0f));
			scaleObjects[j].width = Mathf.Lerp(0.8f, 1.2f, Mathf.Lerp(Mathf.Sin(num6 * (float)Math.PI), 1f, (num6 < 0.5f) ? 0.5f : 0f)) * num2;
			backwardsFactors[j] = scalesPositions[j].y * 0.7f;
		}
		numberOfSprites = (colored ? (scalesPositions.Length * 2) : scalesPositions.Length);
	}

	private void MoveScalesTowardsHead()
	{
		float num = 1f;
		for (int i = 0; i < scalesPositions.Length; i++)
		{
			if (scalesPositions[i].y < num)
			{
				num = scalesPositions[i].y;
			}
		}
		if (num > 0.07f)
		{
			num -= 0.07f;
			for (int j = 0; j < scalesPositions.Length; j++)
			{
				scalesPositions[j].y -= num;
			}
		}
	}

	private void StretchDownOnBack(float stretchTo)
	{
		float num = 1f;
		float num2 = 0f;
		for (int i = 0; i < scalesPositions.Length; i++)
		{
			num = Mathf.Min(num, scalesPositions[i].y);
			num2 = Mathf.Max(num2, scalesPositions[i].y);
		}
		if (!(num2 > stretchTo))
		{
			for (int j = 0; j < scalesPositions.Length; j++)
			{
				scalesPositions[j].y = Custom.LerpMap(scalesPositions[j].y, num, num2, num, stretchTo);
			}
		}
	}
}
