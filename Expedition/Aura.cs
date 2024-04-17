using UnityEngine;

namespace Expedition;

public class Aura : UpdatableAndDeletable, IDrawable
{
	public Player ply;

	public float counter;

	public Aura(Player ply)
	{
		counter = 0f;
		this.ply = ply;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		counter = ExpeditionGame.egg.counter;
	}

	public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			sLeaser.sprites[i] = new FSprite("Futile_White");
			sLeaser.sprites[i].shader = rCam.game.rainWorld.Shaders["LightSource"];
			sLeaser.sprites[i].scale = 35f;
			sLeaser.sprites[i].alpha = 1f;
		}
		AddToContainer(sLeaser, rCam, rCam.ReturnFContainer("ForegroundLights"));
	}

	public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		for (int i = 0; i < sLeaser.sprites.Length; i++)
		{
			if (ply != null)
			{
				sLeaser.sprites[i].x = Mathf.Lerp(ply.mainBodyChunk.lastPos.x, ply.mainBodyChunk.pos.x, timeStacker) - camPos.x;
				sLeaser.sprites[i].y = Mathf.Lerp(ply.mainBodyChunk.lastPos.y, ply.mainBodyChunk.pos.y, timeStacker) - camPos.y;
				sLeaser.sprites[i].color = new HSLColor(Mathf.Sin(counter / 20f), 1f, 0.75f).rgb;
				sLeaser.sprites[i].alpha = ((ply != null) ? 1f : 0f);
			}
		}
	}

	public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[0]);
	}
}
