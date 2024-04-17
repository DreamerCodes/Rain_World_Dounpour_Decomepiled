using System.Collections.Generic;
using UnityEngine;

public class FliesRoomAI : UpdatableAndDeletable
{
	public List<Fly> flies;

	public List<Fly> inHive;

	public FliesRoomAI(Room room)
	{
		base.room = room;
		flies = new List<Fly>();
		inHive = new List<Fly>();
	}

	public override void Update(bool eu)
	{
		for (int num = inHive.Count - 1; num >= 0; num--)
		{
			if (inHive[num].room == room)
			{
				inHive[num].RemoveFromRoom();
			}
			if (Random.value < 0.025f)
			{
				FlyEmergeFromHive(inHive[num]);
			}
		}
		for (int num2 = flies.Count - 1; num2 >= 0; num2--)
		{
			if (flies[num2].room != room)
			{
				flies.RemoveAt(num2);
			}
		}
		base.Update(eu);
	}

	public void Abstractize()
	{
	}

	public void CreateFlyInHive()
	{
		if (room.hives.Length != 0)
		{
			AbstractCreature abstractCreature = new AbstractCreature(room.game.world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly), null, RandomHiveNode(), room.game.GetNewID());
			room.abstractRoom.AddEntity(abstractCreature);
			abstractCreature.Realize();
			inHive.Add(abstractCreature.realizedCreature as Fly);
		}
	}

	public WorldCoordinate RandomHiveNode()
	{
		int abstractNode = Random.Range(room.exitAndDenIndex.Length + room.borderExits.Length, room.exitAndDenIndex.Length + room.borderExits.Length + room.hives.Length);
		return new WorldCoordinate(room.abstractRoom.index, -1, -1, abstractNode);
	}

	public void MoveFlyToHive(Fly fly)
	{
		flies.Remove(fly);
		inHive.Add(fly);
		fly.RemoveFromRoom();
	}

	private void FlyEmergeFromHive(Fly fly)
	{
		if (!FlyAI.RoomNotACycleHazard(room) && ((ModManager.MSC && room.world.rainCycle.preTimer > 0) || room.world.rainCycle.RainApproaching < 0.3f || room.world.rainCycle.RainGameOver))
		{
			return;
		}
		float num = 0f;
		int num2 = -1;
		for (int i = 0; i < room.hives.Length; i++)
		{
			float num3 = Random.value * 50f;
			foreach (AbstractCreature creature in room.abstractRoom.creatures)
			{
				if (StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly).CreatureRelationship(creature.creatureTemplate).type == CreatureTemplate.Relationship.Type.Afraid)
				{
					if (!ModManager.MMF)
					{
						num3 += (float)room.aimap.ExitDistanceForCreature(creature.pos.Tile, room.exitAndDenIndex.Length + i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
					}
					int num4 = room.aimap.ExitDistanceForCreature(creature.pos.Tile, room.exitAndDenIndex.Length + i, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
					if (num4 >= 1 && num4 < 20)
					{
						num3 -= 1000f;
					}
				}
			}
			if (num3 > num)
			{
				num = num3;
				num2 = i;
			}
		}
		if (num2 > -1)
		{
			int num5 = room.hives[num2].Length;
			if (num5 > 0)
			{
				fly.abstractCreature.pos.Tile = room.hives[num2][Random.Range(0, num5)];
				room.PlaySound(SoundID.Bat_Emerge_From_Grass, room.MiddleOfTile(fly.abstractCreature.pos));
				fly.PlaceInRoom(room);
				inHive.Remove(fly);
			}
		}
	}

	public Fly GetRandomFly()
	{
		if (flies.Count == 0)
		{
			return null;
		}
		return flies[Random.Range(0, flies.Count)];
	}

	public void AddFly(Fly fly)
	{
		if (!flies.Contains(fly))
		{
			flies.Add(fly);
		}
	}
}
