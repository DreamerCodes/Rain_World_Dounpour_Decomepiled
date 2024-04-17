using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class MissionTracker : AIModule
{
	public abstract class Mission
	{
		public MissionTracker mTracker;

		public float baseUtility;

		public float currentUtility;

		public bool deleteMeNextFrame;

		public int forgetCounter;

		public Mission(MissionTracker mTracker, float utility, int forgetCounter)
		{
			this.mTracker = mTracker;
			baseUtility = utility;
			currentUtility = utility;
			this.forgetCounter = forgetCounter;
			Custom.Log("Start mission!", ToString());
		}

		public virtual void UpdateUtility()
		{
			if (forgetCounter > -1)
			{
				forgetCounter--;
				if (forgetCounter < 1)
				{
					Abandon();
				}
			}
		}

		public virtual void Act()
		{
		}

		public void Abandon()
		{
			Custom.Log("Abandon mission!", ToString());
			deleteMeNextFrame = true;
		}
	}

	public class LeaveRoom : Mission
	{
		public WorldCoordinate? destination;

		public int destUnreachable;

		public LeaveRoom(MissionTracker mTracker, float utility, int forgetCounter)
			: base(mTracker, utility, forgetCounter)
		{
			if (mTracker.AI.creature.world.singleRoomWorld)
			{
				Abandon();
			}
		}

		public override void UpdateUtility()
		{
			base.UpdateUtility();
		}

		public override void Act()
		{
			WorldCoordinate coord = mTracker.AI.creature.pos;
			World world = mTracker.AI.creature.world;
			if (!coord.NodeDefined)
			{
				coord = QuickConnectivity.DefineNodeOfLocalCoordinate(coord, world, mTracker.AI.creature.creatureTemplate);
			}
			if (destination.HasValue)
			{
				if (coord.CompareDisregardingTile(destination.Value))
				{
					Abandon();
				}
				else
				{
					mTracker.AI.creature.abstractAI.SetDestination(destination.Value);
				}
				if (mTracker.AI.pathFinder != null && (!mTracker.AI.pathFinder.CoordinatePossibleToGetBackFrom(destination.Value) || !mTracker.AI.pathFinder.CoordinateReachable(destination.Value)))
				{
					destUnreachable++;
					if (destUnreachable > 1000)
					{
						Abandon();
					}
				}
				else
				{
					destUnreachable = 0;
				}
				return;
			}
			int num = int.MaxValue;
			int num2 = -1;
			if (world.GetAbstractRoom(coord).realizedRoom != null && world.GetAbstractRoom(coord).realizedRoom.readyForAI)
			{
				if (mTracker.AI.threatTracker != null && mTracker.AI.threatTracker.Utility() > 0f)
				{
					num2 = mTracker.AI.threatTracker.FindMostAttractiveExit();
					if (num2 != -1 && world.GetAbstractRoom(coord).nodes[num2].type != AbstractRoomNode.Type.Exit)
					{
						num2 = -1;
					}
				}
				if (num2 == -1)
				{
					for (int i = 0; i < world.GetAbstractRoom(coord).connections.Length; i++)
					{
						int num3 = world.GetAbstractRoom(coord).realizedRoom.aimap.ExitDistanceForCreatureAndCheckNeighbours(coord.Tile, world.GetAbstractRoom(coord).CommonToCreatureSpecificNodeIndex(i, mTracker.AI.creature.creatureTemplate), mTracker.AI.creature.creatureTemplate);
						if (num3 > -1 && num3 < num && world.GetAbstractRoom(coord).nodes[i].type == AbstractRoomNode.Type.Exit && world.GetAbstractRoom(coord).connections[i] > -1)
						{
							num = num3;
							num2 = i;
						}
					}
				}
			}
			if (num2 == -1)
			{
				if (world.GetAbstractRoom(coord).nodes[coord.abstractNode].type == AbstractRoomNode.Type.Exit && world.GetAbstractRoom(coord).connections[coord.abstractNode] > -1)
				{
					num2 = coord.abstractNode;
				}
				else
				{
					for (int j = 0; j < world.GetAbstractRoom(coord).connections.Length; j++)
					{
						if (world.GetAbstractRoom(coord).connections[j] > -1 && world.GetAbstractRoom(coord).ConnectionAndBackPossible(coord.abstractNode, j, mTracker.AI.creature.creatureTemplate))
						{
							num2 = j;
							break;
						}
					}
					if (num2 == -1 && world.GetAbstractRoom(coord).connections.Length != 0)
					{
						num2 = Random.Range(0, world.GetAbstractRoom(coord).connections.Length);
					}
				}
			}
			AbstractRoom abstractRoom = world.GetAbstractRoom(world.GetAbstractRoom(coord).connections[num2]);
			WorldCoordinate value = new WorldCoordinate(abstractRoom.index, -1, -1, abstractRoom.ExitIndex(world.GetAbstractRoom(coord).index));
			int num4 = -1;
			float num5 = float.MaxValue;
			for (int k = 0; k < abstractRoom.nodes.Length; k++)
			{
				if (value.abstractNode != k && abstractRoom.ConnectionAndBackPossible(value.abstractNode, k, mTracker.AI.creature.creatureTemplate))
				{
					float num6 = ((abstractRoom.nodes[k].type == AbstractRoomNode.Type.Exit) ? 1f : 10f);
					num6 *= (float)abstractRoom.ConnectivityCost(value.abstractNode, k, mTracker.AI.creature.creatureTemplate);
					if (num6 < num5)
					{
						num5 = num6;
						num4 = k;
					}
				}
			}
			if (num4 == -1)
			{
				destination = value;
			}
			else if (abstractRoom.nodes[num4].type == AbstractRoomNode.Type.Exit && abstractRoom.connections[num4] > -1)
			{
				AbstractRoom abstractRoom2 = world.GetAbstractRoom(abstractRoom.connections[num4]);
				destination = new WorldCoordinate(abstractRoom2.index, -1, -1, abstractRoom2.ExitIndex(abstractRoom.index));
			}
			else
			{
				destination = new WorldCoordinate(abstractRoom.index, -1, -1, num4);
			}
		}
	}

	public List<Mission> missions;

	private float utility;

	public Mission mostRelevantMission;

	public MissionTracker(ArtificialIntelligence AI)
		: base(AI)
	{
		missions = new List<Mission>();
	}

	public override void Update()
	{
		utility = 0f;
		mostRelevantMission = null;
		for (int num = missions.Count - 1; num >= 0; num--)
		{
			missions[num].UpdateUtility();
			if (missions[num].deleteMeNextFrame)
			{
				missions.RemoveAt(num);
			}
			else if (missions[num].currentUtility > utility)
			{
				utility = missions[num].currentUtility;
				mostRelevantMission = missions[num];
			}
		}
	}

	public void ActOnMostImportantMission()
	{
		if (mostRelevantMission != null)
		{
			mostRelevantMission.Act();
		}
	}

	public void AddMission(Mission mission, bool canCoexistWithMissionOfSameType)
	{
		if (!canCoexistWithMissionOfSameType)
		{
			bool flag = true;
			for (int i = 0; i < missions.Count; i++)
			{
				if (missions[i].GetType() == mission.GetType() && missions[i].currentUtility > utility)
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				return;
			}
			for (int num = missions.Count - 1; num >= 0; num--)
			{
				if (missions[num].GetType() == mission.GetType())
				{
					missions[num].Abandon();
					missions.RemoveAt(num);
				}
			}
		}
		missions.Add(mission);
	}

	public override float Utility()
	{
		return utility;
	}
}
