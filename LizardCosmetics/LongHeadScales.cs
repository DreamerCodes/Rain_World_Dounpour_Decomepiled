using UnityEngine;

namespace LizardCosmetics;

public class LongHeadScales : LongBodyScales
{
	public LongHeadScales(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		rigor = Random.value;
		GenerateTwoHorns();
		float num = Mathf.Pow(Random.value, 0.7f) * lGraphics.lizard.lizardParams.headSize;
		colored = Random.value < 0.5f && lGraphics.lizard.Template.type != CreatureTemplate.Type.WhiteLizard;
		graphic = Random.Range(4, 6);
		if (num < 0.5f && Random.value < 0.5f)
		{
			graphic = 6;
		}
		else if (num > 0.8f)
		{
			graphic = 5;
		}
		if (num < 0.2f && lGraphics.lizard.Template.type != CreatureTemplate.Type.WhiteLizard)
		{
			colored = true;
		}
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.BlackLizard)
		{
			colored = false;
		}
		graphicHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + graphic).sourcePixelSize.y;
		scaleObjects = new LizardScale[scalesPositions.Length];
		backwardsFactors = new float[scalesPositions.Length];
		float value = Random.value;
		float num2 = Mathf.Pow(Random.value, 0.85f);
		for (int i = 0; i < scalesPositions.Length; i++)
		{
			scaleObjects[i] = new LizardScale(this);
			scaleObjects[i].length = Mathf.Lerp(5f, 35f, num);
			scaleObjects[i].width = Mathf.Lerp(0.65f, 1.2f, value * num);
			backwardsFactors[i] = num2;
		}
		numberOfSprites = (colored ? (scalesPositions.Length * 2) : scalesPositions.Length);
	}

	protected void GenerateTwoHorns()
	{
		scalesPositions = new Vector2[2];
		float y = Mathf.Lerp(0f, 0.07f, Random.value);
		float num = Mathf.Lerp(0.5f, 1.5f, Random.value);
		for (int i = 0; i < scalesPositions.Length; i++)
		{
			scalesPositions[i] = new Vector2((i == 0) ? (0f - num) : num, y);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
		{
			sLeaser.sprites[num].color = lGraphics.HeadColor(timeStacker);
			if (colored)
			{
				sLeaser.sprites[num + scalesPositions.Length].color = lGraphics.effectColor;
			}
		}
	}
}
