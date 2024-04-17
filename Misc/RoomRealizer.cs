using System;
using System.Collections.Generic;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

public class RoomRealizer
{
	private class RealizedRoomTracker
	{
		public AbstractRoom room;

		public int timeSinceVisited;

		public int age;

		public int roomChangesSinceVisited;

		public bool hasBeenVisited;

		public RealizedRoomTracker(AbstractRoom room, bool hasBeenVisited)
		{
			this.room = room;
			this.hasBeenVisited = hasBeenVisited;
		}

		public float DeletionFactor()
		{
			return (float)timeSinceVisited / 2400f + (float)roomChangesSinceVisited + (hasBeenVisited ? 0f : 24000f);
		}
	}

	private class RoomAndInt
	{
		public int counter;

		public AbstractRoom room;

		public RoomAndInt(AbstractRoom room)
		{
			this.room = room;
		}
	}

	public AbstractCreature followCreature;

	private World world;

	private List<RealizedRoomTracker> realizedRooms;

	private Room currentlyLoadingRoom;

	private float performanceBudget = 1500f;

	private int lastFrameFollowCreatureRoom;

	private AbstractRoom probableNextRoom;

	private AbstractRoom improbableNextRoom;

	private List<RoomAndInt> recentlyAbstractedRooms;

	private List<RoomAndInt> realizeNeighborCandidates;

	public RoomRealizer(AbstractCreature followCreature, World world)
	{
		this.followCreature = followCreature;
		this.world = world;
		realizedRooms = new List<RealizedRoomTracker>();
		recentlyAbstractedRooms = new List<RoomAndInt>();
		realizeNeighborCandidates = new List<RoomAndInt>();
	}

	public void Update()
	{
		followCreature = world.game.cameras[0].followAbstractCreature;
		if (followCreature == null)
		{
			return;
		}
		for (int num = realizedRooms.Count - 1; num >= 0; num--)
		{
			if (realizedRooms[num].room.index == followCreature.pos.room)
			{
				realizedRooms[num].timeSinceVisited = 0;
			}
			else
			{
				realizedRooms[num].timeSinceVisited++;
			}
			if (realizedRooms[num].room.realizedRoom != null && realizedRooms[num].room.realizedRoom.fullyLoaded)
			{
				realizedRooms[num].age++;
			}
			if (realizedRooms[num].room == followCreature.Room)
			{
				realizedRooms[num].hasBeenVisited = true;
			}
			if (realizedRooms[num].room.realizedRoom == null)
			{
				realizedRooms.RemoveAt(num);
			}
		}
		for (int num2 = recentlyAbstractedRooms.Count - 1; num2 >= 0; num2--)
		{
			recentlyAbstractedRooms[num2].counter++;
			if (recentlyAbstractedRooms[num2].counter > 80)
			{
				recentlyAbstractedRooms.RemoveAt(num2);
			}
		}
		if (lastFrameFollowCreatureRoom != followCreature.pos.room && !followCreature.Room.offScreenDen && !IsRoomRecentlyAbstracted(followCreature.Room))
		{
			if (ModManager.MSC && followCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
			{
				for (int num3 = realizedRooms.Count - 1; num3 >= 0; num3--)
				{
					if (realizedRooms[num3].room.index == lastFrameFollowCreatureRoom)
					{
						KillRoom(realizedRooms[num3].room);
						realizedRooms.RemoveAt(num3);
						break;
					}
				}
			}
			else
			{
				RemoveNotVisitedRooms();
			}
			RealizeAndTrackRoom(followCreature.Room, actuallyEntering: true);
			for (int i = 0; i < realizedRooms.Count; i++)
			{
				if (realizedRooms[i].room.index == followCreature.pos.room)
				{
					realizedRooms[i].roomChangesSinceVisited = 0;
				}
				else
				{
					realizedRooms[i].roomChangesSinceVisited++;
				}
			}
			realizeNeighborCandidates.Clear();
		}
		lastFrameFollowCreatureRoom = followCreature.pos.room;
		if (currentlyLoadingRoom != null)
		{
			if (currentlyLoadingRoom.fullyLoaded)
			{
				currentlyLoadingRoom = null;
			}
			return;
		}
		float num4 = CurrentPerformanceEstimation();
		if (num4 > performanceBudget / 4f)
		{
			CheckForAndDeleteDistantRooms();
		}
		if (num4 > performanceBudget)
		{
			ShaveDownPerformanceTo(num4, performanceBudget, ref realizedRooms);
			return;
		}
		if (followCreature.realizedCreature != null && followCreature.realizedCreature.room != null && followCreature.realizedCreature.room.readyForAI)
		{
			if (ModManager.MSC && followCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
			{
				if (probableNextRoom != null && probableNextRoom == followCreature.Room)
				{
					probableNextRoom = null;
				}
				if (improbableNextRoom != null && improbableNextRoom == followCreature.Room)
				{
					improbableNextRoom = null;
				}
				int num5 = 20;
				while (probableNextRoom == null || !(followCreature.abstractAI as OverseerAbstractAI).RoomAllowed(probableNextRoom.index) || (probableNextRoom.creatures.Count == 0 && num5 > 0))
				{
					probableNextRoom = followCreature.Room.world.GetAbstractRoom(UnityEngine.Random.Range(followCreature.Room.world.firstRoomIndex, followCreature.Room.world.firstRoomIndex + followCreature.Room.world.NumberOfRooms));
					num5--;
				}
				num5 = 20;
				while (improbableNextRoom == null || !(followCreature.abstractAI as OverseerAbstractAI).RoomAllowed(improbableNextRoom.index) || improbableNextRoom == probableNextRoom || (improbableNextRoom.creatures.Count == 0 && num5 > 0))
				{
					improbableNextRoom = followCreature.Room.world.GetAbstractRoom(UnityEngine.Random.Range(followCreature.Room.world.firstRoomIndex, followCreature.Room.world.firstRoomIndex + followCreature.Room.world.NumberOfRooms));
					num5--;
				}
			}
			else
			{
				int num6 = -1;
				int num7 = int.MaxValue;
				int num8 = -1;
				int num9 = 0;
				for (int j = 0; j < followCreature.Room.connections.Length; j++)
				{
					int num10 = followCreature.realizedCreature.room.aimap.ExitDistanceForCreatureAndCheckNeighbours(followCreature.pos.Tile, j, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
					num10 += (int)followCreature.pos.Tile.FloatDist(followCreature.realizedCreature.room.ShortcutLeadingToNode(j).StartTile);
					if (num10 < num7)
					{
						num6 = j;
						num7 = num10;
					}
					if (num10 > num9)
					{
						num8 = j;
						num9 = num10;
					}
				}
				if (num6 > -1 && followCreature.Room.connections[num6] > -1)
				{
					probableNextRoom = world.GetAbstractRoom(followCreature.Room.connections[num6]);
				}
				else
				{
					probableNextRoom = null;
				}
				if (num8 > -1 && num8 != num6 && followCreature.Room.connections[num8] > -1)
				{
					improbableNextRoom = world.GetAbstractRoom(followCreature.Room.connections[num8]);
				}
				else
				{
					improbableNextRoom = null;
				}
			}
			if (probableNextRoom != null && probableNextRoom.realizedRoom == null && !IsRoomRecentlyAbstracted(probableNextRoom))
			{
				if (num4 + RoomPerformanceEstimation(probableNextRoom) < performanceBudget)
				{
					RealizeAndTrackRoom(probableNextRoom, actuallyEntering: false);
				}
				else if (IfPossibleReleasePerformance(num4, performanceBudget - RoomPerformanceEstimation(probableNextRoom)))
				{
					RealizeAndTrackRoom(probableNextRoom, actuallyEntering: false);
				}
			}
		}
		if (currentlyLoadingRoom != null || world.rainCycle.RainGameOver || followCreature.Room.offScreenDen || !(num4 < performanceBudget / 2f))
		{
			return;
		}
		AbstractRoom abstractRoom = world.GetAbstractRoom(followCreature.Room.connections[UnityEngine.Random.Range(0, followCreature.Room.connections.Length)]);
		RoomAndInt roomAndInt = null;
		for (int k = 0; k < realizeNeighborCandidates.Count; k++)
		{
			if (roomAndInt != null)
			{
				break;
			}
			if (realizeNeighborCandidates[k].room == abstractRoom)
			{
				roomAndInt = realizeNeighborCandidates[k];
			}
		}
		if (roomAndInt == null)
		{
			roomAndInt = new RoomAndInt(abstractRoom);
			realizeNeighborCandidates.Add(roomAndInt);
		}
		if (abstractRoom != null && abstractRoom.realizedRoom == null && !IsRoomRecentlyAbstracted(abstractRoom) && num4 + RoomPerformanceEstimation(abstractRoom) < performanceBudget / 2f)
		{
			roomAndInt.counter++;
			if (roomAndInt.counter > 40)
			{
				RealizeAndTrackRoom(abstractRoom, actuallyEntering: false);
				Custom.Log("Activated random neighbor");
			}
		}
		else
		{
			roomAndInt.counter = 0;
		}
	}

	private void CheckForAndDeleteDistantRooms()
	{
		if (followCreature.Room.realizedRoom == null || !followCreature.Room.realizedRoom.fullyLoaded || realizedRooms.Count == 0)
		{
			return;
		}
		RealizedRoomTracker realizedRoomTracker = realizedRooms[UnityEngine.Random.Range(0, realizedRooms.Count)];
		if (realizedRoomTracker.room == followCreature.Room || realizedRoomTracker.room == probableNextRoom || !CanAbstractizeRoom(realizedRoomTracker))
		{
			return;
		}
		for (int i = 0; i < realizedRoomTracker.room.connections.Length; i++)
		{
			if (realizedRoomTracker.room.connections[i] <= -1)
			{
				continue;
			}
			AbstractRoom abstractRoom = world.GetAbstractRoom(realizedRoomTracker.room.connections[i]);
			if (abstractRoom == followCreature.Room || abstractRoom == probableNextRoom)
			{
				return;
			}
			if (!(RoomPerformanceEstimation(realizedRoomTracker.room) < performanceBudget / 4f))
			{
				continue;
			}
			for (int j = 0; j < abstractRoom.connections.Length; j++)
			{
				AbstractRoom abstractRoom2 = world.GetAbstractRoom(abstractRoom.connections[j]);
				if (abstractRoom2 == followCreature.Room || abstractRoom2 == probableNextRoom)
				{
					return;
				}
			}
		}
		Custom.Log("kill distant room:", realizedRoomTracker.room.name);
		KillRoom(realizedRoomTracker.room);
		realizedRooms.Remove(realizedRoomTracker);
	}

	public void ForceRealizeRoom(AbstractRoom room)
	{
		world.ActivateRoom(room);
	}

	private bool CanAbstractizeRoom(RealizedRoomTracker tracker)
	{
		if (ModManager.CoopAvailable)
		{
			foreach (AbstractCreature nonPermaDeadPlayer in tracker.room.world.game.NonPermaDeadPlayers)
			{
				if (nonPermaDeadPlayer.Room == tracker.room)
				{
					return false;
				}
			}
		}
		if (tracker.room != followCreature.Room && tracker.room.realizedRoom != null && tracker.room.realizedRoom.fullyLoaded && (!tracker.hasBeenVisited || tracker.timeSinceVisited > 200) && tracker.age > ((tracker.room == probableNextRoom) ? 400 : 40))
		{
			return tracker.room != tracker.room.world.game.cameras[0].room.abstractRoom;
		}
		return false;
	}

	private void RealizeAndTrackRoom(AbstractRoom room, bool actuallyEntering)
	{
		for (int i = 0; i < realizedRooms.Count; i++)
		{
			if (realizedRooms[i].room == room)
			{
				realizedRooms[i].hasBeenVisited = realizedRooms[i].hasBeenVisited || actuallyEntering;
				return;
			}
		}
		world.ActivateRoom(room);
		currentlyLoadingRoom = room.realizedRoom;
		realizedRooms.Add(new RealizedRoomTracker(room, actuallyEntering));
	}

	private void AddNewTrackedRoom(AbstractRoom room, bool actuallyEntering)
	{
		for (int i = 0; i < realizedRooms.Count; i++)
		{
			if (realizedRooms[i].room == room)
			{
				realizedRooms[i].hasBeenVisited = realizedRooms[i].hasBeenVisited || actuallyEntering;
				return;
			}
		}
		realizedRooms.Add(new RealizedRoomTracker(room, actuallyEntering));
	}

	private void ShaveDownPerformanceTo(float currentPerf, float goalPerformance, ref List<RealizedRoomTracker> candidates)
	{
		while (currentPerf > goalPerformance && candidates.Count > 0)
		{
			AbstractRoom abstractRoom = PutOutARoom(ref candidates);
			if (abstractRoom != null)
			{
				currentPerf -= RoomPerformanceEstimation(abstractRoom);
				continue;
			}
			break;
		}
	}

	private AbstractRoom PutOutARoom(ref List<RealizedRoomTracker> candidates)
	{
		float num = float.MinValue;
		RealizedRoomTracker realizedRoomTracker = null;
		for (int i = 0; i < candidates.Count; i++)
		{
			if (CanAbstractizeRoom(candidates[i]))
			{
				float num2 = candidates[i].DeletionFactor();
				if (followCreature.Room.connections.IndexfOf(candidates[i].room.index) == -1)
				{
					num2 += 1000f;
				}
				if (candidates[i].room == probableNextRoom)
				{
					num2 -= 1000f;
				}
				if (candidates[i].room == improbableNextRoom)
				{
					num2 += 1f;
				}
				if (num2 > num)
				{
					num = num2;
					realizedRoomTracker = candidates[i];
				}
			}
		}
		if (realizedRoomTracker != null)
		{
			Custom.Log("kill room:", realizedRoomTracker.room.name);
			KillRoom(realizedRoomTracker.room);
			realizedRooms.Remove(realizedRoomTracker);
			candidates.Remove(realizedRoomTracker);
			return realizedRoomTracker.room;
		}
		return null;
	}

	private bool IfPossibleReleasePerformance(float currentPeformance, float goalPerformance)
	{
		float num = 0f;
		List<RealizedRoomTracker> candidates = new List<RealizedRoomTracker>();
		for (int i = 0; i < realizedRooms.Count; i++)
		{
			if (CanAbstractizeRoom(realizedRooms[i]) && realizedRooms[i].room != probableNextRoom)
			{
				candidates.Add(realizedRooms[i]);
				num += RoomPerformanceEstimation(realizedRooms[i].room);
			}
		}
		if (currentPeformance - num <= goalPerformance)
		{
			ShaveDownPerformanceTo(currentPeformance, goalPerformance, ref candidates);
			return true;
		}
		return false;
	}

	private void RemoveNotVisitedRooms()
	{
		if (ModManager.MSC && world.game.rainWorld.safariMode)
		{
			return;
		}
		for (int num = realizedRooms.Count - 1; num >= 0; num--)
		{
			if (!realizedRooms[num].hasBeenVisited && Array.IndexOf(followCreature.Room.connections, realizedRooms[num].room.index) == -1 && (!ModManager.MMF || MMF.cfgVanillaExploits.Value || realizedRooms[num].room.realizedRoom == null || realizedRooms[num].room.realizedRoom.fullyLoaded))
			{
				KillRoom(realizedRooms[num].room);
				realizedRooms.RemoveAt(num);
			}
		}
	}

	public void KillRoom(AbstractRoom room)
	{
		room.Abstractize();
		for (int i = 0; i < recentlyAbstractedRooms.Count; i++)
		{
			if (recentlyAbstractedRooms[i].room == room)
			{
				recentlyAbstractedRooms[i].counter = 0;
				return;
			}
		}
		recentlyAbstractedRooms.Add(new RoomAndInt(room));
	}

	private bool IsRoomRecentlyAbstracted(AbstractRoom room)
	{
		for (int i = 0; i < recentlyAbstractedRooms.Count; i++)
		{
			if (recentlyAbstractedRooms[i].room == room)
			{
				return true;
			}
		}
		return false;
	}

	private float CurrentPerformanceEstimation()
	{
		float num = 0f;
		for (int i = 0; i < realizedRooms.Count; i++)
		{
			num += RoomPerformanceEstimation(realizedRooms[i].room);
		}
		return num;
	}

	public float RoomPerformanceEstimation(AbstractRoom testRoom)
	{
		float num = Mathf.Lerp(2080f, testRoom.size.x * testRoom.size.y, 0.25f) / 40f;
		for (int i = 0; i < testRoom.nodes.Length; i++)
		{
			if (testRoom.nodes[i].submerged)
			{
				num += Mathf.Lerp(50f, testRoom.size.x, 0.5f) * 0.2f;
				break;
			}
		}
		if (testRoom.singleRealizedRoom)
		{
			num += performanceBudget * 0.55f;
		}
		for (int j = 0; j < testRoom.creatures.Count; j++)
		{
			if (testRoom.creatures[j].state.alive)
			{
				num += GetCreaturePerformanceEstimation(testRoom.creatures[j]);
			}
		}
		return num;
	}

	private static float GetCreaturePerformanceEstimation(AbstractCreature crit)
	{
		CreatureTemplate.Type type = crit.creatureTemplate.type;
		if (ModManager.MSC)
		{
			if (type == MoreSlugcatsEnums.CreatureTemplateType.MirosVulture)
			{
				return 140f;
			}
			if (type == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
			{
				return 200f;
			}
			if (type == MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy)
			{
				return 200f;
			}
		}
		if (type == CreatureTemplate.Type.KingVulture)
		{
			return 140f;
		}
		if (type == CreatureTemplate.Type.Vulture)
		{
			return 100f;
		}
		if (type == CreatureTemplate.Type.Snail)
		{
			return 20f;
		}
		if (type == CreatureTemplate.Type.Leech)
		{
			return 10f;
		}
		if (type == CreatureTemplate.Type.GarbageWorm)
		{
			return 30f;
		}
		if (type == CreatureTemplate.Type.Fly)
		{
			return 10f;
		}
		if (type == CreatureTemplate.Type.BigEel)
		{
			return 300f;
		}
		if (type == CreatureTemplate.Type.Deer)
		{
			return 200f;
		}
		if (type == CreatureTemplate.Type.DaddyLongLegs)
		{
			return 200f;
		}
		if (type == CreatureTemplate.Type.BrotherLongLegs)
		{
			return 200f;
		}
		if (type == CreatureTemplate.Type.Scavenger)
		{
			return 300f;
		}
		if (type == CreatureTemplate.Type.JetFish)
		{
			return 25f;
		}
		if (crit.creatureTemplate.IsCicada)
		{
			return 25f;
		}
		if (type == CreatureTemplate.Type.BigSpider)
		{
			return 30f;
		}
		if (type == CreatureTemplate.Type.SpitterSpider)
		{
			return 50f;
		}
		if (type == CreatureTemplate.Type.DropBug)
		{
			return 20f;
		}
		if (crit.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.LizardTemplate)
		{
			return 50f;
		}
		return 10f;
	}

	public List<AbstractRoom> GetRealizedRooms()
	{
		List<AbstractRoom> list = new List<AbstractRoom>();
		for (int i = 0; i < realizedRooms.Count; i++)
		{
			list.Add(realizedRooms[i].room);
		}
		return list;
	}
}
