using UnityEngine;

public class DebugSprite : CosmeticSprite
{
	public FSprite sprite;

	public DebugSprite(Vector2 ps, FSprite sp, Room rm)
	{
		pos = ps;
		sprite = sp;
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = sprite;
		rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = pos.x - camPos.x;
		sLeaser.sprites[0].y = pos.y - camPos.y;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
