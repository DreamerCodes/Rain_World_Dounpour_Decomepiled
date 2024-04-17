using RWCustom;
using UnityEngine;

public class DebugPerTileVisualizer
{
	private DebugSprite[,] sprites;

	private FLabel lbl;

	public DebugPerTileVisualizer(Room room)
	{
		sprites = new DebugSprite[room.TileWidth, room.TileHeight];
		for (int i = 0; i < room.TileWidth; i++)
		{
			for (int j = 0; j < room.TileHeight; j++)
			{
				FSprite sp = new FSprite("pixel")
				{
					scale = 20f
				};
				sprites[i, j] = new DebugSprite(room.MiddleOfTile(i, j), sp, room);
				room.AddObject(sprites[i, j]);
			}
		}
		lbl = new FLabel(Custom.GetFont(), "");
		lbl.x = 100f;
		lbl.y = 100f;
		Futile.stage.AddChild(lbl);
	}

	public void Update(Room room)
	{
		for (int i = 0; i < sprites.GetLength(0); i++)
		{
			for (int j = 0; j < sprites.GetLength(1); j++)
			{
				UpdateTile(i, j, room);
			}
		}
	}

	private void UpdateTile(int x, int y, Room room)
	{
		FSprite sprite = sprites[x, y].sprite;
		sprite.alpha = 0.5f;
		float num = Random.value * 0.1f;
		if (!room.readyForAI)
		{
			return;
		}
		num = 0f;
		ArtificialIntelligence artificialIntelligence = null;
		float bonus = Custom.LerpMap(Futile.mousePosition.x, 0f, 1000f, -1f, 1f);
		float movementBasedVision = 0f;
		foreach (PhysicalObject item in room.physicalObjects[1])
		{
			if (item is Creature && (item as Creature).abstractCreature.abstractAI != null && (item as Creature).abstractCreature.abstractAI.RealAI != null)
			{
				artificialIntelligence = (item as Creature).abstractCreature.abstractAI.RealAI;
				movementBasedVision = (item as Creature).Template.movementBasedVision;
			}
		}
		foreach (PhysicalObject item2 in room.physicalObjects[1])
		{
			if (item2 is Player)
			{
				bonus = item2.firstChunk.VisibilityBonus(movementBasedVision);
			}
		}
		if (artificialIntelligence != null)
		{
			num = artificialIntelligence.VisualScore(room.MiddleOfTile(new IntVector2(x, y)), bonus);
			if (artificialIntelligence.VisualContact(room.MiddleOfTile(new IntVector2(x, y)), bonus))
			{
				sprite.color = Custom.HSL2RGB((1f - num) * 0.7f, 1f, 0.5f);
			}
			else
			{
				sprite.color = new Color(0f, 0f, 0f);
			}
		}
	}
}
