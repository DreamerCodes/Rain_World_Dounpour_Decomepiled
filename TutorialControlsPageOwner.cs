using Menu;
using UnityEngine;

public class TutorialControlsPageOwner : UpdatableAndDeletable, IDrawable
{
	public TutorialControlsPage controlsPage;

	public TutorialControlsPageOwner(RainWorldGame game)
	{
		controlsPage = new TutorialControlsPage(game.manager, game, this, game.cameras[0].hud.fContainers[1]);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		controlsPage.Update();
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[0];
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		controlsPage.GrafUpdate(timeStacker);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
	}
}
