using RWCustom;
using UnityEngine;

public class MouseSpark : CosmeticSprite
{
	private float lifeTime;

	private float life;

	private Color color;

	private bool graphic;

	private float dir;

	public MouseSpark(Vector2 pos, Vector2 vel, float maxLifeTime, Color color)
	{
		lastPos = pos;
		base.pos = pos;
		base.vel = vel;
		this.color = color;
		dir = Custom.AimFromOneVectorToAnother(-vel, vel);
		lifeTime = Mathf.Lerp(5f, maxLifeTime, Random.value);
		life = 1f;
		graphic = Random.value < 0.5f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		life -= 1f / lifeTime;
		if (life < 0f)
		{
			Destroy();
		}
		vel *= 0.7f;
		vel += Custom.DegToVec(dir) * Random.value * 2f;
		dir += Mathf.Lerp(-17f, 17f, Random.value);
		graphic = !graphic;
		if (room.GetTile(pos).Terrain == Room.Tile.TerrainType.Solid)
		{
			if (room.GetTile(lastPos).Terrain != Room.Tile.TerrainType.Solid)
			{
				vel *= 0f;
				pos = lastPos;
			}
			else
			{
				Destroy();
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite("pixel");
		sLeaser.sprites[0].color = color;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		if (Random.value < 0.01f)
		{
			sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName("mouseSparkB");
		}
		else
		{
			sLeaser.sprites[0].element = Futile.atlasManager.GetElementWithName(graphic ? "mouseSparkA" : "pixel");
		}
		if (Random.value < 0.125f)
		{
			sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
		}
		else
		{
			sLeaser.sprites[0].color = color;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		base.AddToContainer(sLeaser, rCam, newContatiner);
	}
}
