using UnityEngine;

public class SunBlocker : UpdatableAndDeletable, IDrawable
{
	public override void Update(bool eu)
	{
		base.Update(eu);
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].color = new Color(0.003921569f, 0f, 0f);
		sLeaser.sprites[0].scaleX = 1500f;
		sLeaser.sprites[0].scaleY = 900f;
		sLeaser.sprites[0].anchorX = 0.5f;
		sLeaser.sprites[0].anchorY = 0.5f;
		sLeaser.sprites[0].x = 683f;
		sLeaser.sprites[0].y = 384f;
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = 683f;
		sLeaser.sprites[0].y = 384f;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		sLeaser.sprites[0].RemoveFromContainer();
		rCam.ReturnFContainer("Shadows").AddChild(sLeaser.sprites[0]);
	}
}
