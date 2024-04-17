using UnityEngine;

namespace MoreSlugcats;

public class FadeOut : UpdatableAndDeletable, IDrawable
{
	public float duration;

	public Color fadeColor;

	public bool fadeIn;

	public float fade;

	public float lastFade;

	public bool freezeFade;

	public FadeOut(Room room, Color color, float duration, bool fadeIn)
	{
		base.room = room;
		this.fadeIn = fadeIn;
		this.duration = duration;
		fadeColor = color;
		if (this.fadeIn)
		{
			fade = 1f;
			lastFade = 1f;
		}
		else
		{
			fade = 0f;
			lastFade = 0f;
		}
	}

	public bool IsDoneFading()
	{
		if (fade != 0f || !fadeIn)
		{
			if (fade == 1f)
			{
				return !fadeIn;
			}
			return false;
		}
		return true;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		lastFade = fade;
		if (!freezeFade)
		{
			if (fadeIn)
			{
				fade = Mathf.Max(0f, fade - 1f / duration);
			}
			else
			{
				fade = Mathf.Min(1f, fade + 1f / duration);
			}
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].scaleX = (rCam.room.RoomRect.right - rCam.room.RoomRect.left) / 8f;
		sLeaser.sprites[0].scaleY = (rCam.room.RoomRect.top - rCam.room.RoomRect.bottom) / 8f;
		sLeaser.sprites[0].x = rCam.room.RoomRect.Center.x;
		sLeaser.sprites[0].y = rCam.room.RoomRect.Center.y;
		sLeaser.sprites[0].alpha = 0f;
		AddToContainer(sLeaser, rCam, null);
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].alpha = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFade, fade, timeStacker)), 1.8f);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = fadeColor;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		newContatiner = rCam.ReturnFContainer("Bloom");
		sLeaser.sprites[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.sprites[0]);
	}
}
