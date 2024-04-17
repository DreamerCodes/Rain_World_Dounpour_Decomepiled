using MoreSlugcats;
using UnityEngine;

namespace LizardCosmetics;

public class TailTuft : LongBodyScales
{
	public TailTuft(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		rigor = 0f;
		if (Random.value < 1f / 7f || (Random.value < 0.9f && lGraphics.lizard.Template.type == CreatureTemplate.Type.BlueLizard) || lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && lGraphics.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			if (lGraphics.lizard.Template.type == CreatureTemplate.Type.BlueLizard || lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard)
			{
				GenerateTwoLines(0f, (lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard) ? 0.3f : 0.7f, 1f, 3f);
			}
			else
			{
				GenerateTwoLines(0f, 0.4f, 1.2f, 1.3f);
			}
		}
		else
		{
			GeneratePatchPattern(0f, Random.Range(3, 7), 1.6f, 1.5f);
		}
		MoveScalesTowardsTail();
		float num = Mathf.Lerp(1f, 1f / Mathf.Lerp(1f, scalesPositions.Length, Mathf.Pow(Random.value, 2f)), 0.5f);
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && lGraphics.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			num = Mathf.Max(num, 0.4f) * 1.1f;
		}
		float a = Mathf.Lerp(5f, 10f, Random.value) * num;
		float b = Mathf.Lerp(a, 25f, Mathf.Pow(Random.value, 0.5f)) * num;
		colored = Random.value < 0.8f;
		graphic = Random.Range(3, 7);
		if (graphic == 3)
		{
			graphic = 1;
		}
		if (Random.value < 1f / 30f)
		{
			graphic = Random.Range(0, 7);
		}
		if (Random.value < 0.8f || lGraphics.lizard.Template.type == CreatureTemplate.Type.RedLizard || (ModManager.MSC && lGraphics.lizard.Template.type == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard))
		{
			for (int i = 0; i < lGraphics.cosmetics.Count; i++)
			{
				if (lGraphics.cosmetics[i] is LongBodyScales)
				{
					graphic = (lGraphics.cosmetics[i] as LongBodyScales).graphic;
					break;
				}
				if (lGraphics.cosmetics[i] is SpineSpikes)
				{
					graphic = (lGraphics.cosmetics[i] as SpineSpikes).graphic;
					break;
				}
			}
		}
		graphicHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + graphic).sourcePixelSize.y;
		scaleObjects = new LizardScale[scalesPositions.Length];
		backwardsFactors = new float[scalesPositions.Length];
		float num2 = 0f;
		float num3 = 1f;
		for (int j = 0; j < scalesPositions.Length; j++)
		{
			if (scalesPositions[j].y > num2)
			{
				num2 = scalesPositions[j].y;
			}
			if (scalesPositions[j].y < num3)
			{
				num3 = scalesPositions[j].y;
			}
		}
		float num4 = Mathf.Lerp(1f, 1.5f, Random.value);
		for (int k = 0; k < scalesPositions.Length; k++)
		{
			scaleObjects[k] = new LizardScale(this);
			float t = Mathf.InverseLerp(num3, num2, scalesPositions[k].y);
			scaleObjects[k].length = Mathf.Lerp(a, b, t);
			scaleObjects[k].width = Mathf.Lerp(0.8f, 1.2f, t) * num;
			backwardsFactors[k] = 0.3f + 0.7f * Mathf.InverseLerp(0.75f, 1f, scalesPositions[k].y);
			scalesPositions[k].x *= Mathf.InverseLerp(1.05f, 0.85f, scalesPositions[k].y) * num4;
		}
		numberOfSprites = (colored ? (scalesPositions.Length * 2) : scalesPositions.Length);
	}

	private void MoveScalesTowardsTail()
	{
		float num = 0f;
		for (int i = 0; i < scalesPositions.Length; i++)
		{
			if (scalesPositions[i].y > num)
			{
				num = scalesPositions[i].y;
			}
		}
		for (int j = 0; j < scalesPositions.Length; j++)
		{
			scalesPositions[j].y += 0.9f - num;
		}
	}
}
