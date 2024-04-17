using UnityEngine;

namespace MoreSlugcats;

public class LocalBlizzard : UpdatableAndDeletable, IDrawable
{
	public Vector2 pos;

	public float rad;

	public float intensity;

	public float scale;

	public float angle;

	public LocalBlizzard(Vector2 initPos, float initRad, float initIntensity, float initScale)
	{
		pos = initPos;
		rad = initRad;
		intensity = initIntensity;
		scale = initScale;
		angle = 0f;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		sLeaser.sprites[0].MoveToBack();
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = new Vector2(pos.x - camPos.x + 0.5f, pos.y - camPos.y + 0.5f);
		float num = rad / 8f;
		sLeaser.sprites[0].x = vector.x;
		sLeaser.sprites[0].y = vector.y;
		sLeaser.sprites[0].scale = num;
		sLeaser.sprites[0].color = new Color(intensity, scale, 0f, 0f);
		sLeaser.sprites[0].rotation = angle * 360f;
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = room.game.rainWorld.Shaders[(ModManager.MMF && room.game.rainWorld.options.quality == Options.Quality.LOW) ? "FastLocalBlizzard" : "LocalBlizzard"];
		AddToContainer(sLeaser, rCam, null);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}
}
