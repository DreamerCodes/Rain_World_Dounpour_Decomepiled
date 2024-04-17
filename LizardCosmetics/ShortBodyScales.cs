using RWCustom;
using UnityEngine;

namespace LizardCosmetics;

public class ShortBodyScales : BodyScales
{
	public ShortBodyScales(LizardGraphics lGraphics, int startSprite)
		: base(lGraphics, startSprite)
	{
		int num = Random.Range(0, 3);
		if (lGraphics.lizard.Template.type == CreatureTemplate.Type.GreenLizard && Random.value < 0.7f)
		{
			num = 2;
		}
		else if (lGraphics.lizard.Template.type == CreatureTemplate.Type.BlueLizard && Random.value < 0.93f)
		{
			num = 1;
		}
		switch (num)
		{
		case 0:
			GeneratePatchPattern(0.1f, Random.Range(4, 15), 0.9f, 1.2f);
			break;
		case 1:
			GenerateTwoLines(0.1f, 1f, 1.5f, 1f);
			break;
		case 2:
			GenerateSegments(0.1f, 0.9f, (lGraphics.lizard.Template.type == CreatureTemplate.Type.PinkLizard) ? 1.5f : 0.6f);
			break;
		}
		numberOfSprites = scalesPositions.Length;
	}

	public override void Update()
	{
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
		{
			sLeaser.sprites[num] = new FSprite("pixel");
			sLeaser.sprites[num].scaleX = 2f;
			sLeaser.sprites[num].scaleY = 3f;
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int num = startSprite + scalesPositions.Length - 1; num >= startSprite; num--)
		{
			LizardGraphics.LizardSpineData backPos = GetBackPos(num - startSprite, timeStacker, changeDepthRotation: true);
			sLeaser.sprites[num].x = backPos.outerPos.x - camPos.x;
			sLeaser.sprites[num].y = backPos.outerPos.y - camPos.y;
			sLeaser.sprites[num].rotation = Custom.AimFromOneVectorToAnother(backPos.dir, -backPos.dir);
			sLeaser.sprites[num].color = lGraphics.HeadColor(timeStacker);
		}
	}
}
