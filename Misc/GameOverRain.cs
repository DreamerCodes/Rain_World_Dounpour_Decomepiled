using RWCustom;
using UnityEngine;

internal class GameOverRain : CosmeticSprite
{
	private bool destroyNext;

	private float speed;

	private float floorLevel;

	public GameOverRain(Room room, Vector2 pos, float speed)
	{
		base.pos = pos;
		lastPos = pos;
		this.speed = speed;
		vel = new Vector2(0f, 0f - speed);
		for (int i = 0; room.GetTile(new IntVector2(0, i)).Terrain == Room.Tile.TerrainType.Solid; i++)
		{
			floorLevel = room.MiddleOfTile(new IntVector2(0, i)).y + 10f;
		}
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (destroyNext)
		{
			Destroy();
		}
		if (lastPos.y < floorLevel)
		{
			destroyNext = true;
		}
		for (int i = 0; i < room.physicalObjects.Length; i++)
		{
			for (int j = 0; j < room.physicalObjects[i].Count; j++)
			{
				PhysicalObject physicalObject = room.physicalObjects[i][j];
				for (int k = 0; k < physicalObject.bodyChunks.Length; k++)
				{
					if (physicalObject.bodyChunks[k].pos.x > pos.x - physicalObject.bodyChunks[k].rad && physicalObject.bodyChunks[k].pos.x < pos.x + physicalObject.bodyChunks[k].rad && pos.y < physicalObject.bodyChunks[k].pos.y && lastPos.y > physicalObject.bodyChunks[k].pos.y)
					{
						physicalObject.bodyChunks[k].vel.y -= speed * 0.01f / physicalObject.bodyChunks[k].mass;
					}
				}
			}
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[2]
		{
			new FSprite("pixel"),
			new FSprite("RainSplash")
		};
		sLeaser.sprites[0].anchorY = 0f;
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		sLeaser.sprites[0].scaleY = speed * 1.5f * (1f - (destroyNext ? timeStacker : 0f));
		sLeaser.sprites[1].isVisible = destroyNext;
		if (pos.y < floorLevel)
		{
			sLeaser.sprites[1].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
			sLeaser.sprites[1].y = floorLevel - camPos.y;
			sLeaser.sprites[1].rotation = Random.value * 360f;
			sLeaser.sprites[1].scale = Mathf.Lerp(speed * 0.005f, 0.04f, 0.75f);
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
