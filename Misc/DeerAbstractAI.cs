using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class DeerAbstractAI : AbstractCreatureAI
{
	private WorldCoordinate? goToCoordinate;

	public WorldCoordinate? sporePos;

	private new int lastRoom;

	public int timeInRoom;

	public List<WorldCoordinate> allowedNodes;

	public bool damageGoHome;

	public static List<string> ALLOWEDROOMS;

	public static string[] UGLYHARDCODEDALLOWEDROOMS = new string[6] { "LF_E04", "LF_H01", "LF_J01", "LF_E05", "LF_H02", "LF_D07" };

	public static int[][] UGLYHARDCODEDALLOWEDNODES = new int[6][]
	{
		new int[2] { 6, 7 },
		new int[2] { 5, 6 },
		new int[1] { 2 },
		new int[1] { 10 },
		new int[1] { 5 },
		new int[2] { 8, 9 }
	};

	public DeerAbstractAI(World world, AbstractCreature parent)
		: base(world, parent)
	{
		lastRoom = parent.pos.room;
		allowedNodes = new List<WorldCoordinate>();
		if (ModManager.MMF)
		{
			string[] dehardcodedRooms = GetDehardcodedRooms();
			ALLOWEDROOMS = new List<string>(dehardcodedRooms);
			if (world.singleRoomWorld)
			{
				return;
			}
			for (int i = 0; i < dehardcodedRooms.Length; i++)
			{
				if (world.GetAbstractRoom(dehardcodedRooms[i]) == null)
				{
					continue;
				}
				int index = world.GetAbstractRoom(dehardcodedRooms[i]).index;
				for (int j = 0; j < world.GetAbstractRoom(index).nodes.Length; j++)
				{
					if (world.GetAbstractRoom(index).nodes[j].type == AbstractRoomNode.Type.SideExit && world.GetAbstractRoom(index).nodes[j].entranceWidth >= 3)
					{
						allowedNodes.Add(new WorldCoordinate(index, -1, -1, j));
					}
				}
			}
		}
		else
		{
			if (world.singleRoomWorld)
			{
				return;
			}
			for (int k = 0; k < UGLYHARDCODEDALLOWEDROOMS.Length; k++)
			{
				int index2 = world.GetAbstractRoom(UGLYHARDCODEDALLOWEDROOMS[k]).index;
				for (int l = 0; l < UGLYHARDCODEDALLOWEDNODES[k].Length; l++)
				{
					allowedNodes.Add(new WorldCoordinate(index2, -1, -1, UGLYHARDCODEDALLOWEDNODES[k][l]));
				}
			}
		}
	}

	public override void AbstractBehavior(int time)
	{
		if (path.Count > 0 && parent.realizedCreature == null)
		{
			FollowPath(time);
		}
		else if (world.rainCycle.TimeUntilRain < 800 && !parent.nightCreature && !parent.ignoreCycle)
		{
			if (!base.denPosition.HasValue || !parent.pos.CompareDisregardingTile(base.denPosition.Value))
			{
				GoToDen();
			}
		}
		else if (damageGoHome)
		{
			base.denPosition = new WorldCoordinate(world.offScreenDen.index, -1, -1, 0);
			if (parent.pos.room == world.offScreenDen.index)
			{
				damageGoHome = false;
			}
			GoToDen();
		}
		else if (sporePos.HasValue)
		{
			SetDestination(sporePos.Value);
			if (sporePos.Value.room == parent.pos.room && sporePos.Value.abstractNode == parent.pos.abstractNode && parent.realizedCreature == null)
			{
				sporePos = null;
			}
		}
		else
		{
			if (allowedNodes.Count == 0)
			{
				return;
			}
			if (!goToCoordinate.HasValue)
			{
				RoamToRandomRoom();
			}
			if (parent.pos.room != lastRoom)
			{
				lastRoom = parent.pos.room;
				timeInRoom = 0;
			}
			if (!ModManager.MMF || !MMF.cfgDeerBehavior.Value || timeInRoom <= 1200)
			{
				timeInRoom += time;
			}
			if (timeInRoom > 1200)
			{
				if (ModManager.MMF && MMF.cfgDeerBehavior.Value)
				{
					if (parent.realizedCreature != null)
					{
						if (parent.pos.x < 31 || parent.pos.x >= parent.Room.size.x - 30 || Random.value < 0.8f || (parent.realizedCreature as Deer).playersInAntlers.Count > 0)
						{
							timeInRoom += time;
							if (!(parent.realizedCreature as Deer).stayStill)
							{
								timeInRoom = 1201;
							}
							else if (timeInRoom > (((parent.realizedCreature as Deer).playersInAntlers.Count > 0) ? 2900 : 6200))
							{
								Custom.Log("Live deer triggered wander room");
								WorldCoordinate inRoomDestination = (parent.realizedCreature as Deer).AI.IdleRoomWanderGoal();
								(parent.realizedCreature as Deer).AI.inRoomDestination = inRoomDestination;
								(parent.realizedCreature as Deer).AI.SetDestination(inRoomDestination);
								timeInRoom = 1;
							}
						}
						else
						{
							timeInRoom += time;
							if (!(parent.realizedCreature as Deer).stayStill)
							{
								timeInRoom = 1201;
							}
							else if (timeInRoom > 2500)
							{
								Custom.Log("Live deer triggered leave room");
								goToCoordinate = null;
								timeInRoom = 1;
							}
						}
					}
					else
					{
						goToCoordinate = null;
						timeInRoom = 1;
					}
				}
				else
				{
					goToCoordinate = null;
					timeInRoom -= 1200;
				}
			}
			else if (goToCoordinate.HasValue && parent.pos.room != goToCoordinate.Value.room)
			{
				SetDestination(goToCoordinate.Value);
			}
		}
	}

	private void RoamToRandomRoom()
	{
		if (allowedNodes.Count == 0)
		{
			return;
		}
		WorldCoordinate worldCoordinate = allowedNodes[Random.Range(0, allowedNodes.Count)];
		AbstractRoom abstractRoom = world.GetAbstractRoom(worldCoordinate);
		for (int i = 0; i < abstractRoom.creatures.Count; i++)
		{
			if (abstractRoom.creatures[i].creatureTemplate.type == CreatureTemplate.Type.Deer)
			{
				return;
			}
		}
		goToCoordinate = worldCoordinate;
	}

	public void AttractToSporeCloud(WorldCoordinate cloudPos)
	{
		sporePos = cloudPos;
		goToCoordinate = sporePos;
	}

	public static List<int> GetAllowedNodesInRoom(World World, string roomName)
	{
		List<int> list = new List<int>();
		AbstractRoom abstractRoom = World.GetAbstractRoom(roomName);
		for (int i = 0; i < abstractRoom.nodes.Length; i++)
		{
			if (abstractRoom.nodes[i].type == AbstractRoomNode.Type.SideExit)
			{
				list.Add(i);
			}
		}
		return list;
	}

	private string[] GetDehardcodedRooms()
	{
		List<string> list = new List<string>();
		for (int i = world.firstRoomIndex; i < world.firstRoomIndex + (world.NumberOfRooms - 1); i++)
		{
			if (world.GetAbstractRoom(i).AttractionForCreature(parent.creatureTemplate.type) == AbstractRoom.CreatureRoomAttraction.Stay)
			{
				list.Add(world.GetAbstractRoom(i).name);
			}
		}
		return list.ToArray();
	}
}
