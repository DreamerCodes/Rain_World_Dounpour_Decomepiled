using System.Collections.Generic;
using UnityEngine;

public class BigEelAbstractAI : AbstractCreatureAI
{
	private WorldCoordinate? goToCoordinate;

	private new int lastRoom;

	public int timeInRoom;

	public BigEelAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		lastRoom = parent.pos.room;
	}

	public override void AbstractBehavior(int time)
	{
		if (path.Count > 0 && parent.realizedCreature == null)
		{
			FollowPath(time);
		}
		else if (world.seaAccessNodes.Length != 0)
		{
			if (!goToCoordinate.HasValue)
			{
				AddRandomCheckRoom();
			}
			if (parent.pos.room != lastRoom)
			{
				lastRoom = parent.pos.room;
				timeInRoom = 0;
			}
			timeInRoom += time;
			if (timeInRoom > 1200 || (world.singleRoomWorld && goToCoordinate.HasValue && parent.pos.room == goToCoordinate.Value.room && parent.pos.room == world.offScreenDen.index))
			{
				goToCoordinate = null;
				timeInRoom -= 1200;
			}
			else if (goToCoordinate.HasValue && parent.pos.room != goToCoordinate.Value.room)
			{
				SetDestination(goToCoordinate.Value);
			}
		}
	}

	private void AddRandomCheckRoom()
	{
		if (world.singleRoomWorld)
		{
			List<WorldCoordinate> list = new List<WorldCoordinate>();
			if (base.denPosition.HasValue)
			{
				list.Add(base.denPosition.Value);
			}
			for (int i = 0; i < world.GetAbstractRoom(world.firstRoomIndex).nodes.Length; i++)
			{
				if (world.GetAbstractRoom(world.firstRoomIndex).nodes[i].type == AbstractRoomNode.Type.SeaExit)
				{
					list.Add(new WorldCoordinate(world.firstRoomIndex, -1, -1, i));
				}
			}
			if (list.Count > 0)
			{
				goToCoordinate = list[Random.Range(0, list.Count)];
			}
		}
		else if (Random.value > 0.2f && parent.pos.room != world.offScreenDen.index)
		{
			AddRoomClusterToCheckList(world.GetAbstractRoom(parent.pos), includeOriginal: false);
		}
		else if (world.seaAccessNodes.Length != 0)
		{
			AddRoomClusterToCheckList(world.GetAbstractRoom(world.seaAccessNodes[Random.Range(0, world.seaAccessNodes.Length)]), includeOriginal: true);
		}
	}

	private void AddRoomClusterToCheckList(AbstractRoom originalRoom, bool includeOriginal)
	{
		List<WorldCoordinate> list = new List<WorldCoordinate>();
		for (int i = (includeOriginal ? (-1) : 0); i < originalRoom.connections.Length; i++)
		{
			AbstractRoom abstractRoom = ((i > -1 && originalRoom.connections[i] > -1) ? world.GetAbstractRoom(originalRoom.connections[i]) : originalRoom);
			if (!(abstractRoom.AttractionForCreature(parent.creatureTemplate.type) != AbstractRoom.CreatureRoomAttraction.Forbidden))
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < abstractRoom.creatures.Count; j++)
			{
				if (abstractRoom.creatures[j].creatureTemplate.type == CreatureTemplate.Type.BigEel)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				continue;
			}
			for (int k = 0; k < abstractRoom.nodes.Length; k++)
			{
				if (abstractRoom.nodes[k].type == AbstractRoomNode.Type.SeaExit && abstractRoom.nodes[k].entranceWidth > 20)
				{
					list.Add(new WorldCoordinate(abstractRoom.index, -1, -1, k));
				}
			}
		}
		if (list.Count > 0)
		{
			goToCoordinate = list[Random.Range(0, list.Count)];
		}
	}
}
