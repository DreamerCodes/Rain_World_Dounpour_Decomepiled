using UnityEngine;

namespace MoreSlugcats;

public class Snow : IDrawable
{
	private bool slatedForDeletetion;

	private Room room;

	private Color[] empty;

	private Texture2D lights;

	public int visibleSnow;

	public Snow(Room room)
	{
		this.room = room;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (visibleSnow == 0)
		{
			sLeaser.sprites[0].isVisible = false;
		}
		else
		{
			sLeaser.sprites[0].isVisible = true;
		}
		if (slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].x = room.game.rainWorld.options.ScreenSize.x * 0.5f;
		sLeaser.sprites[0].y = room.game.rainWorld.options.ScreenSize.y * 0.5f;
		sLeaser.sprites[0].scaleX = room.game.rainWorld.options.ScreenSize.x / 16f;
		sLeaser.sprites[0].scaleY = 48f;
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["DisplaySnowShader"];
		sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
		sLeaser.sprites[0].alpha = 1f;
		AddToContainer(sLeaser, rCam, null);
	}

	public void Update()
	{
	}

	public void Destroy()
	{
		slatedForDeletetion = true;
	}
}
