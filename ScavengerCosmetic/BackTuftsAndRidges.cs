using RWCustom;
using UnityEngine;

namespace ScavengerCosmetic;

public abstract class BackTuftsAndRidges : BackDecals
{
	public int scaleGraf;

	public float scaleGrafHeight;

	public float generalSize;

	public float xFlip;

	public float colored;

	public float[] colorAlphas;

	public bool useDetailColor;

	public bool Colored => colored > 0f;

	public BackTuftsAndRidges(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
		scaleGraf = Random.Range(0, 7);
		xFlip = -1f;
		if (scaleGraf == 3)
		{
			xFlip = 1f;
		}
		if (Random.value < 0.025f)
		{
			xFlip = 0f - xFlip;
		}
		if (Random.value < 0.5f)
		{
			xFlip *= 0.5f + 0.5f * Random.value;
		}
		if (Random.value > base.scavGrphs.iVars.generalMelanin)
		{
			colored = Mathf.Pow(Random.value, 0.5f);
		}
		useDetailColor = Random.value < 0.5f;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		scaleGrafHeight = Futile.atlasManager.GetElementWithName("LizardScaleA" + scaleGraf).sourcePixelSize.y;
		for (int i = 0; i < totalSprites; i++)
		{
			if (Colored)
			{
				sLeaser.sprites[firstSprite + i] = new FSprite("LizardScale" + ((i % 2 == 0) ? "A" : "B") + scaleGraf);
			}
			else
			{
				sLeaser.sprites[firstSprite + i] = new FSprite("LizardScaleA" + scaleGraf);
			}
			sLeaser.sprites[firstSprite + i].anchorY = 0.1f;
		}
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		Color blendedBodyColor = base.scavGrphs.BlendedBodyColor;
		Color col = (useDetailColor ? base.scavGrphs.BlendedDecorationColor : base.scavGrphs.BlendedHeadColor);
		for (int i = 0; i < totalSprites; i++)
		{
			if (Colored)
			{
				sLeaser.sprites[firstSprite + i].color = ((i % 2 == 0) ? blendedBodyColor : Custom.RGB2RGBA(col, colorAlphas[i / 2]));
			}
			else
			{
				sLeaser.sprites[firstSprite + i].color = blendedBodyColor;
			}
		}
	}
}
