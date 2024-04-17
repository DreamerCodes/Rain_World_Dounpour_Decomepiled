using UnityEngine;

namespace LizardCosmetics;

public class AxolotlGills : LongBodyScales
{
	public AxolotlGills(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		rigor = Random.value;
		float num = Mathf.Pow(Random.value, 0.7f) * lGraphics.lizard.lizardParams.headSize;
		colored = true;
		graphic = Random.Range(0, 6);
		if (graphic == 2)
		{
			graphic = Random.Range(0, 6);
		}
		graphicHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + graphic).sourcePixelSize.y;
		int num2 = Random.Range(2, 8);
		scalesPositions = new Vector2[num2 * 2];
		scaleObjects = new LizardScale[scalesPositions.Length];
		backwardsFactors = new float[scalesPositions.Length];
		float value = Random.value;
		float num3 = Mathf.Lerp(0.1f, 0.9f, Random.value);
		for (int i = 0; i < num2; i++)
		{
			float y = Mathf.Lerp(0f, 0.07f, Mathf.Pow(Random.value, 1.3f));
			float num4 = Mathf.Lerp(0.5f, 1.5f, Random.value);
			float num5 = Mathf.Lerp(0.2f, 1f, Mathf.Pow(Random.value, 0.5f));
			float num6 = Mathf.Pow(Random.value, 0.5f);
			for (int j = 0; j < 2; j++)
			{
				scalesPositions[i * 2 + j] = new Vector2((j == 0) ? (0f - num4) : num4, y);
				scaleObjects[i * 2 + j] = new LizardScale(this);
				scaleObjects[i * 2 + j].length = Mathf.Lerp(5f, 35f, num * num5);
				scaleObjects[i * 2 + j].width = Mathf.Lerp(0.65f, 1.2f, value * num);
				backwardsFactors[i * 2 + j] = num3 * num6;
			}
		}
		numberOfSprites = (colored ? (scalesPositions.Length * 2) : scalesPositions.Length);
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
