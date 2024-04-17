using UnityEngine;

namespace MoreSlugcats;

public class ProjectionCircle : CosmeticSprite
{
	public float radius;

	public float thickness;

	public ProjectionCircle(Vector2 position, float radius, float thickness)
	{
		pos = position;
		this.radius = radius;
		this.thickness = thickness;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2];
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = rCam.room.game.rainWorld.Shaders["VectorCircle"];
			sLeaser.sprites[i].scale = 0f;
		}
		sLeaser.sprites[1].color = new Color(0.003921569f, 0f, 0f);
		sLeaser.sprites[0].color = new Color(0f, 0f, 0f);
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("BackgroundShortcuts"));
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < 2; i++)
		{
			sLeaser.sprites[i].x = pos.x - camPos.x;
			sLeaser.sprites[i].y = pos.y - camPos.y;
			if (i == 0)
			{
				sLeaser.sprites[i].scale = radius / 4f;
			}
			else
			{
				sLeaser.sprites[i].scale = Mathf.Max(radius / 4f - thickness / 4f, 0f);
			}
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}
}
