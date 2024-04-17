using RWCustom;
using Unity.Mathematics;
using UnityEngine;

namespace ScavengerCosmetic;

public class SpineViz : Template
{
	public SpineViz(ScavengerGraphics owner, int firstSprite)
		: base(owner, firstSprite)
	{
		totalSprites = 40;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		for (int i = 0; i < totalSprites / 2; i++)
		{
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[firstSprite + i * 2 + j] = new FSprite("pixel");
				sLeaser.sprites[firstSprite + i * 2 + j].anchorY = 0f;
			}
			sLeaser.sprites[firstSprite + i * 2].color = new Color(1f, 0f, 0f);
			sLeaser.sprites[firstSprite + i * 2 + 1].color = new Color(0f, 0f, 1f);
		}
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < totalSprites / 2; i++)
		{
			float num = (float)i / (float)(totalSprites / 2 - 1);
			Vector2 vector = base.scavGrphs.OnSpinePos(num, timeStacker);
			sLeaser.sprites[firstSprite + i * 2].rotation = Custom.Float2ToDeg(base.scavGrphs.OnSpineOutwardsDir(new float2(1f, num), timeStacker));
			sLeaser.sprites[firstSprite + i * 2 + 1].rotation = Custom.Float2ToDeg(base.scavGrphs.OnSpineOutwardsDir(new float2(-1f, num), timeStacker));
			for (int j = 0; j < 2; j++)
			{
				sLeaser.sprites[firstSprite + i * 2 + j].x = vector.x - camPos.x;
				sLeaser.sprites[firstSprite + i * 2 + j].y = vector.y - camPos.y;
				sLeaser.sprites[firstSprite + i * 2 + j].scaleY = base.scavGrphs.OnSpineWidth(num, timeStacker) * 4f / 2f;
				sLeaser.sprites[firstSprite + i * 2 + j].scaleX = 1f;
			}
		}
	}
}
