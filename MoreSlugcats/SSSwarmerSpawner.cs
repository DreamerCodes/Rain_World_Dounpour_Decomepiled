using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class SSSwarmerSpawner : UpdatableAndDeletable
{
	private PlacedObject.ResizableObjectData data;

	private int counter;

	private int counterMax;

	private int maxSwamers;

	private Vector2 spawnPos;

	public SSSwarmerSpawner(Room room, PlacedObject.ResizableObjectData data)
	{
		base.room = room;
		this.data = data;
		counterMax = Random.Range(100, 600);
		maxSwamers = 25;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		spawnPos = data.owner.pos + data.handlePos;
		counter++;
		if (counter > counterMax)
		{
			SpawnSwarmer();
			counter = 0;
			counterMax = Random.Range(100, 600);
			counterMax = (int)((float)counterMax * Mathf.InverseLerp(-20f, maxSwamers, room.SwarmerCount));
		}
	}

	private void SpawnSwarmer()
	{
		if (room.SwarmerCount < maxSwamers)
		{
			Custom.Log("Spawn swarmer", room.SwarmerCount.ToString());
			new AbstractPhysicalObject(room.world, AbstractPhysicalObject.AbstractObjectType.SSOracleSwarmer, null, room.ToWorldCoordinate(spawnPos), room.game.GetNewID()).RealizeInRoom();
		}
	}
}
