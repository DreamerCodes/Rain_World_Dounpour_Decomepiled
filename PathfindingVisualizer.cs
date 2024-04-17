using RWCustom;
using UnityEngine;

public class PathfindingVisualizer
{
	public Room room;

	public PathFinder pather;

	public DebugSprite[,] sprites;

	public DebugSprite[][] sprites2;

	public Color[] colors;

	public IntVector2 offset;

	public PathfindingVisualizer(World world, Room room, PathFinder pather, int width, int height, IntVector2 offset)
	{
		this.room = room;
		this.pather = pather;
		this.offset = offset;
		sprites = new DebugSprite[width, height];
		sprites2 = new DebugSprite[world.NumberOfRooms][];
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			sprites2[i] = new DebugSprite[world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length];
			for (int j = 0; j < sprites2[i].Length; j++)
			{
				FSprite sp = new FSprite("pixel")
				{
					scale = 4f,
					color = new Color(Random.value * 0.15f, Random.value * 0.15f, Random.value * 0.15f)
				};
				sprites2[i][j] = new DebugSprite(room.MiddleOfTile(-3, i) + new Vector2((float)j * 5f, 0f), sp, room);
				room.AddObject(sprites2[i][j]);
			}
		}
		Vector2 vector = Custom.DegToVec(world.game.SeededRandom(pather.creature.ID.RandomSeed) * 360f) * 5f;
		for (int k = 0; k < width; k++)
		{
			for (int l = 0; l < height; l++)
			{
				FSprite sp2 = new FSprite("pixel")
				{
					scale = 6f,
					color = new Color(Random.value * 0.15f, Random.value * 0.15f, Random.value * 0.15f)
				};
				sprites[k, l] = new DebugSprite(room.MiddleOfTile(k + offset.x, l + offset.y) + vector, sp2, room);
				room.AddObject(sprites[k, l]);
			}
		}
		colors = new Color[6];
		colors[0] = new Color(1f, 0f, 0f);
		colors[1] = new Color(0f, 1f, 0f);
		colors[2] = new Color(0f, 0f, 1f);
		colors[3] = new Color(1f, 1f, 0f);
		colors[4] = new Color(0f, 1f, 1f);
		colors[5] = new Color(1f, 0f, 1f);
	}

	public void CleanseSprites()
	{
		for (int i = 0; i < sprites2.Length; i++)
		{
			for (int j = 0; j < sprites2[i].Length; j++)
			{
				sprites2[i][j].Destroy();
			}
		}
		for (int k = 0; k < sprites.GetLength(0); k++)
		{
			for (int l = 0; l < sprites.GetLength(1); l++)
			{
				sprites[k, l].Destroy();
			}
		}
	}

	public void CellAddedToCheckNext(WorldCoordinate coord)
	{
	}

	public void CellChecked(WorldCoordinate coord, int gen)
	{
		DebugSprite debugSprite = null;
		if (coord.room == room.abstractRoom.index && coord.TileDefined && Custom.InsideRect(coord.Tile, pather.coveredArea))
		{
			debugSprite = sprites[coord.x - offset.x, coord.y - offset.y];
		}
		else if (coord.NodeDefined)
		{
			debugSprite = sprites2[coord.room - room.world.firstRoomIndex][coord.abstractNode];
		}
		if ((coord.TileDefined && coord.room == room.abstractRoom.index) || coord.NodeDefined)
		{
			debugSprite.sprite.color = colors[gen % colors.Length];
		}
	}

	public void Blink(WorldCoordinate coord)
	{
		DebugSprite debugSprite = null;
		if (coord.room == room.abstractRoom.index && coord.TileDefined && Custom.InsideRect(coord.Tile, pather.coveredArea))
		{
			debugSprite = sprites[coord.x - offset.x, coord.y - offset.y];
		}
		else if (coord.NodeDefined)
		{
			debugSprite = sprites2[coord.room - room.world.firstRoomIndex][coord.abstractNode];
		}
		if ((coord.TileDefined && coord.room == room.abstractRoom.index) || coord.NodeDefined)
		{
			debugSprite.sprite.color = Custom.HSL2RGB(0f, 0f, Random.value);
		}
	}

	public void Reachable(WorldCoordinate coord)
	{
		DebugSprite debugSprite = null;
		if (coord.room == room.abstractRoom.index && coord.TileDefined && Custom.InsideRect(coord.Tile, pather.coveredArea))
		{
			debugSprite = sprites[coord.x - offset.x, coord.y - offset.y];
		}
		else if (coord.NodeDefined && coord.abstractNode < sprites2[coord.room - room.world.firstRoomIndex].Length)
		{
			debugSprite = sprites2[coord.room - room.world.firstRoomIndex][coord.abstractNode];
		}
		if ((coord.TileDefined && coord.room == room.abstractRoom.index) || (coord.NodeDefined && debugSprite != null))
		{
			debugSprite.sprite.color = new Color(0f, 0f, 0.5f);
		}
	}

	public void GetBackAble(WorldCoordinate coord, bool reachAbleAlso)
	{
		DebugSprite debugSprite = null;
		if (coord.room == room.abstractRoom.index && coord.TileDefined && Custom.InsideRect(coord.Tile, pather.coveredArea))
		{
			debugSprite = sprites[coord.x - offset.x, coord.y - offset.y];
		}
		else if (coord.NodeDefined && coord.abstractNode < sprites2[coord.room - room.world.firstRoomIndex].Length)
		{
			debugSprite = sprites2[coord.room - room.world.firstRoomIndex][coord.abstractNode];
		}
		if (((coord.TileDefined && coord.room == room.abstractRoom.index) || coord.NodeDefined) && debugSprite != null)
		{
			if (reachAbleAlso)
			{
				debugSprite.sprite.color = new Color(0.2f, 0.5f, 0.2f);
			}
			else
			{
				debugSprite.sprite.color = new Color(0.5f, 0f, 0f);
			}
		}
	}
}
