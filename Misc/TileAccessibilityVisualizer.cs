using RWCustom;
using UnityEngine;

public class TileAccessibilityVisualizer : UpdatableAndDeletable
{
	public DebugSprite lineSprite;

	public DebugSprite[,] sprites;

	public IntVector2 pos;

	public PathFinder pathFinder;

	private bool lastMouseButton;

	private static Color[] colorsList = new Color[5]
	{
		Color.red,
		Color.green,
		Color.magenta,
		Color.yellow,
		Color.cyan
	};

	public TileAccessibilityVisualizer(Room room)
	{
		base.room = room;
		sprites = new DebugSprite[30, 30];
		for (int i = 0; i < sprites.GetLength(0); i++)
		{
			for (int j = 0; j < sprites.GetLength(1); j++)
			{
				sprites[i, j] = new DebugSprite(default(Vector2), new FSprite("pixel"), room);
				room.AddObject(sprites[i, j]);
				sprites[i, j].sprite.scale = 18f;
				sprites[i, j].sprite.alpha = 0.5f;
				sprites[i, j].sprite.color = Color.black;
			}
		}
		lineSprite = new DebugSprite(default(Vector2), new FSprite("pixel"), room);
		room.AddObject(lineSprite);
		lineSprite.sprite.anchorY = 0f;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (!room.BeingViewed)
		{
			pathFinder = null;
			return;
		}
		if (Input.GetMouseButton(0) && !lastMouseButton)
		{
			LookForCreature((Vector2)Futile.mousePosition + room.game.cameras[0].pos);
		}
		lastMouseButton = Input.GetMouseButton(0);
		pos = room.GetTilePosition((Vector2)Futile.mousePosition + room.game.cameras[0].pos) - new IntVector2(sprites.GetLength(0) / 2, sprites.GetLength(1) / 2);
		for (int i = 0; i < sprites.GetLength(0); i++)
		{
			for (int j = 0; j < sprites.GetLength(1); j++)
			{
				sprites[i, j].sprite.color = ColorOfDBSprite(new WorldCoordinate(room.abstractRoom.index, pos.x + i, pos.y + j, -1));
				sprites[i, j].pos = room.MiddleOfTile(pos.x + i, pos.y + j);
			}
		}
		if (pathFinder != null && pathFinder.creature.realizedCreature != null)
		{
			Vector2 vector = (Vector2)Futile.mousePosition + room.game.cameras[0].pos;
			Vector2 vector2 = pathFinder.creature.realizedCreature.mainBodyChunk.pos;
			lineSprite.pos = vector;
			lineSprite.sprite.rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
			lineSprite.sprite.scaleY = Vector2.Distance(vector, vector2);
		}
	}

	private Color ColorOfDBSprite(WorldCoordinate coord)
	{
		if (pathFinder == null)
		{
			return Color.grey;
		}
		if (Input.GetMouseButton(1))
		{
			return colorsList[Mathf.Abs(pathFinder.PathingCellAtWorldCoordinate(coord).generation) % colorsList.Length];
		}
		if (pathFinder.CoordinateReachableAndGetbackable(coord))
		{
			return Color.green;
		}
		if (pathFinder.CoordinateReachable(coord))
		{
			return Color.yellow;
		}
		if (pathFinder.CoordinatePossibleToGetBackFrom(coord))
		{
			return Color.blue;
		}
		return Color.black;
	}

	public void LookForCreature(Vector2 fromPos)
	{
		float dst = float.MaxValue;
		pathFinder = null;
		for (int i = 0; i < room.abstractRoom.creatures.Count; i++)
		{
			if (room.abstractRoom.creatures[i].abstractAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI != null && room.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder != null && room.abstractRoom.creatures[i].realizedCreature != null && Custom.DistLess(fromPos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos, dst))
			{
				dst = Vector2.Distance(fromPos, room.abstractRoom.creatures[i].realizedCreature.mainBodyChunk.pos);
				pathFinder = room.abstractRoom.creatures[i].abstractAI.RealAI.pathFinder;
			}
		}
	}
}
