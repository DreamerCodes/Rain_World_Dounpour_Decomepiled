using UnityEngine;

public class DebugGraphicsModule : GraphicsModule
{
	public DebugGraphicsModule(PhysicalObject ow)
		: base(ow, internalContainers: false)
	{
	}

	public override void Update()
	{
		base.Update();
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[base.owner.bodyChunks.Length];
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("Circle20");
			sLeaser.sprites[i].scale = base.owner.bodyChunks[i].rad / 10f;
			sLeaser.sprites[i].color = new Color(1f, (i == 0) ? 0.5f : 0f, (i == 0) ? 0.5f : 0f);
		}
		AddToContainer(sLeaser, rCam, null);
		base.InitiateSprites(sLeaser, rCam);
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		for (int i = 0; i < base.owner.bodyChunks.Length; i++)
		{
			sLeaser.sprites[i].x = Mathf.Lerp(base.owner.bodyChunks[i].lastPos.x, base.owner.bodyChunks[i].pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[i].y = Mathf.Lerp(base.owner.bodyChunks[i].lastPos.y, base.owner.bodyChunks[i].pos.y, timeStacker) - camPos.y;
		}
	}
}
