using Menu;
using UnityEngine;

public class SandboxOverlayOwner : UpdatableAndDeletable, IDrawable
{
	public SandboxOverlay overlay;

	public SandboxEditorSelector selector;

	public SandboxGameSession gameSession;

	public bool fadeSprite;

	public SandboxOverlayOwner(Room room, SandboxGameSession gameSession, bool fadeSprite)
	{
		base.room = room;
		this.gameSession = gameSession;
		this.fadeSprite = fadeSprite;
		overlay = new SandboxOverlay(room.game.manager, room.game, this);
		gameSession.overlay = overlay;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		overlay.Update();
	}

	public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		if (!fadeSprite)
		{
			sLeaser.sprites = new FSprite[0];
			return;
		}
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		sLeaser.sprites[0].color = Color.black;
		sLeaser.sprites[0].isVisible = false;
		AddToContainer(sLeaser, rCam, null);
	}

	public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		overlay.GrafUpdate(timeStacker);
		if (fadeSprite && selector != null)
		{
			Vector2 vector = selector.DrawSize(timeStacker);
			float num = Mathf.Lerp(selector.lastVisFac, selector.visFac, timeStacker);
			if (num <= 0f)
			{
				sLeaser.sprites[0].isVisible = false;
			}
			else
			{
				sLeaser.sprites[0].isVisible = true;
				sLeaser.sprites[0].x = selector.DrawX(timeStacker) + vector.x / 2f;
				sLeaser.sprites[0].y = selector.DrawY(timeStacker) + vector.y / 2f;
				sLeaser.sprites[0].scaleX = (vector.x * 2f + 700f) / 16f;
				sLeaser.sprites[0].scaleY = (vector.y * 2f + 700f) / 16f;
				sLeaser.sprites[0].alpha = 0.6f * num;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}
	}

	public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("HUD2");
		}
		FSprite[] sprites = sLeaser.sprites;
		foreach (FSprite fSprite in sprites)
		{
			fSprite.RemoveFromContainer();
			newContatiner.AddChild(fSprite);
		}
	}
}
