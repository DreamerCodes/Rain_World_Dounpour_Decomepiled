using UnityEngine;

namespace MoreSlugcats;

public class PointVisualizer
{
	public DebugSprite[] sprites;

	public PointVisualizer(Room room, int numPoints)
	{
		sprites = new DebugSprite[numPoints];
		for (int i = 0; i < numPoints; i++)
		{
			FSprite sp = new FSprite("pixel")
			{
				scale = 6f
			};
			sprites[i] = new DebugSprite(Vector2.zero, sp, room);
			room.AddObject(sprites[i]);
		}
	}

	public void CleaseSprites()
	{
		for (int i = 0; i < sprites.Length; i++)
		{
			sprites[i].Destroy();
		}
	}

	public void PointPosition(int ind, Vector2 pos)
	{
		sprites[ind].pos = pos;
	}

	public void PointColor(int ind, Color col)
	{
		sprites[ind].sprite.color = col;
	}
}
