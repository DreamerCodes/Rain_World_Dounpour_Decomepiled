using UnityEngine;

namespace MoreSlugcats;

public class OEsphere : UpdatableAndDeletable, IDrawable
{
	public Vector2 pos;

	public float rad;

	private Color color;

	public int depth;

	public float lIntensity;

	public OEsphere(Vector2 initPos, float initRad, int initDepth)
	{
		pos = initPos;
		rad = initRad;
		depth = initDepth;
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[0]);
		rCam.ReturnFContainer("GrabShaders").AddChild(sLeaser.sprites[1]);
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[2]);
		sLeaser.sprites[1].MoveToBack();
		sLeaser.sprites[0].MoveToBack();
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		Vector2 vector = new Vector2(pos.x - camPos.x + 0.5f, pos.y - camPos.y + 0.5f);
		float num = rad / 8f;
		float a = (float)depth / 30f;
		float[] array = new float[3] { 1f, 2.87f, 4f };
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i].x = vector.x;
			sLeaser.sprites[i].y = vector.y;
			sLeaser.sprites[i].scale = num * array[i];
		}
		sLeaser.sprites[0].color = new Color(1f, 1f, 1f, a);
		sLeaser.sprites[1].color = new Color(1f, 1f, 1f, a);
		sLeaser.sprites[2].color = new Color(lIntensity, 1f, 1f, a);
		if (base.slatedForDeletetion || room != rCam.room)
		{
			sLeaser.CleanSpritesAndRemove();
		}
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[3];
		sLeaser.sprites[0] = new FSprite("Futile_White");
		sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["OESphereBase"];
		sLeaser.sprites[1] = new FSprite("Futile_White");
		sLeaser.sprites[1].shader = rCam.room.game.rainWorld.Shaders["OESphereTop"];
		sLeaser.sprites[2] = new FSprite("Futile_White");
		sLeaser.sprites[2].shader = rCam.room.game.rainWorld.Shaders["OESphereLight"];
		AddToContainer(sLeaser, rCam, null);
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
	}
}
