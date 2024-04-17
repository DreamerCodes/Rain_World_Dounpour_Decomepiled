using RWCustom;
using UnityEngine;

public class SootMark : CosmeticSprite
{
	private float rad;

	private float flipX;

	private float flipY;

	private float rotat;

	private bool bigSprite;

	private float fade;

	public SootMark(Room room, Vector2 pos, float rad, bool bigSprite)
	{
		base.room = room;
		base.pos = pos;
		this.rad = rad;
		this.bigSprite = bigSprite;
		rotat = Random.value * 360f;
		flipX = ((Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(0.9f, 1.1f, Random.value);
		flipY = ((Random.value < 0.5f) ? (-1f) : 1f) * Mathf.Lerp(0.9f, 1.1f, Random.value);
		fade = 0.85f;
		for (int i = 0; i < 9; i++)
		{
			if (room.GetTile(pos + Custom.eightDirectionsAndZero[i].ToVector2() * Mathf.Min(30f, rad * 0.3f)).Solid)
			{
				fade = 1f;
				break;
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite(bigSprite ? "sootmark" : "sootmark2");
		sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["SootMark"];
		sLeaser.sprites[0].rotation = rotat;
		sLeaser.sprites[0].scaleX = flipX * (rad / (bigSprite ? 80f : 40f));
		sLeaser.sprites[0].scaleY = flipY * (rad / (bigSprite ? 80f : 40f));
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = pos.x - camPos.x;
		sLeaser.sprites[0].y = pos.y - camPos.y;
		sLeaser.sprites[0].alpha = fade;
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		sLeaser.sprites[0].color = palette.blackColor;
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		if (newContatiner == null)
		{
			newContatiner = rCam.ReturnFContainer("Foreground");
		}
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
