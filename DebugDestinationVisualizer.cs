using RWCustom;
using UnityEngine;

public class DebugDestinationVisualizer
{
	private AbstractSpaceVisualizer abstractSpaceVisualizer;

	private Room room;

	private PathFinder pathfinder;

	private DebugSprite sprite1;

	private DebugSprite sprite2;

	private DebugSprite sprite3;

	private DebugSprite sprite4;

	public DebugDestinationVisualizer(AbstractSpaceVisualizer abstractSpaceVisualizer, World world, PathFinder pathfinder, Color color)
	{
		this.abstractSpaceVisualizer = abstractSpaceVisualizer;
		this.pathfinder = pathfinder;
		room = world.GetAbstractRoom(pathfinder.creaturePos).realizedRoom;
		sprite1 = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
		sprite2 = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
		sprite3 = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
		sprite4 = new DebugSprite(new Vector2(0f, 0f), new FSprite("pixel"), room);
		sprite1.sprite.color = color;
		sprite2.sprite.color = color;
		sprite3.sprite.color = color;
		sprite4.sprite.color = color;
		sprite2.sprite.anchorY = 0f;
		sprite2.sprite.scaleX = 2f;
		sprite4.sprite.anchorY = 0f;
		sprite4.sprite.scaleX = 2f;
		room.AddObject(sprite1);
		room.AddObject(sprite2);
		room.AddObject(sprite3);
		room.AddObject(sprite4);
	}

	private void ChangeRooms(Room newRoom)
	{
		room.RemoveObject(sprite1);
		room.RemoveObject(sprite2);
		room.RemoveObject(sprite3);
		room.RemoveObject(sprite4);
		newRoom.AddObject(sprite1);
		newRoom.AddObject(sprite2);
		newRoom.AddObject(sprite3);
		newRoom.AddObject(sprite4);
		room = newRoom;
	}

	public void Update()
	{
		Vector2 vector = room.MiddleOfTile(pathfinder.creaturePos);
		if (room.abstractRoom.index == pathfinder.creaturePos.room && pathfinder.creature.realizedCreature != null)
		{
			vector = pathfinder.creature.realizedCreature.mainBodyChunk.pos;
		}
		else if (pathfinder.creaturePos.room != room.abstractRoom.index)
		{
			vector = abstractSpaceVisualizer.SpritePosition(pathfinder.creaturePos.room, pathfinder.creaturePos.abstractNode);
		}
		Vector2 vector2 = room.MiddleOfTile(pathfinder.GetEffectualDestination);
		if (pathfinder.GetEffectualDestination.room != room.abstractRoom.index)
		{
			vector2 = new Vector2(-100f, 1000f);
			sprite1.sprite.scale = 7f;
			sprite1.sprite.rotation = 0f;
		}
		else
		{
			sprite1.sprite.scale = 10f;
			sprite1.sprite.rotation = 45f;
			if (!pathfinder.GetEffectualDestination.TileDefined && pathfinder.GetEffectualDestination.NodeDefined)
			{
				vector2 = room.MiddleOfTile(room.LocalCoordinateOfNode(pathfinder.GetEffectualDestination.abstractNode));
			}
		}
		Vector2 vector3 = room.MiddleOfTile(pathfinder.GetDestination);
		if (pathfinder.GetDestination.room != room.abstractRoom.index)
		{
			vector3 = new Vector2(-100f, 1000f);
		}
		else if (!pathfinder.GetDestination.TileDefined && pathfinder.GetDestination.NodeDefined)
		{
			vector3 = room.MiddleOfTile(room.LocalCoordinateOfNode(pathfinder.GetDestination.abstractNode));
		}
		sprite3.sprite.scale = 5f;
		sprite3.sprite.rotation = 45f;
		sprite3.pos = vector2;
		sprite4.pos = vector2;
		sprite4.sprite.rotation = Custom.AimFromOneVectorToAnother(vector2, vector);
		sprite4.sprite.scaleY = Vector2.Distance(vector2, vector);
		sprite1.pos = vector3;
		sprite2.pos = vector3;
		sprite2.sprite.rotation = Custom.AimFromOneVectorToAnother(vector3, vector2);
		sprite2.sprite.scaleY = Vector2.Distance(vector2, vector3);
	}
}
