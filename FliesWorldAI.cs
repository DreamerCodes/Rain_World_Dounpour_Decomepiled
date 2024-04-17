using RWCustom;
using UnityEngine;

public class FliesWorldAI : World.WorldProcess
{
	public class Behavior : ExtEnum<Behavior>
	{
		public static readonly Behavior Inactive = new Behavior("Inactive", register: true);

		public static readonly Behavior Spread = new Behavior("Spread", register: true);

		public static readonly Behavior Gather = new Behavior("Gather", register: true);

		public static readonly Behavior Migrate = new Behavior("Migrate", register: true);

		public Behavior(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private float[][][] swarmRoomMap;

	public Behavior[] swarmRoomBehaviors;

	private int[] fliesToSpawn;

	private int fliesToRespawn;

	private bool allInitialFliesSpawned;

	private int[] migrationTargets;

	private Behavior SwarmRoomBehavior(int swarmRoom)
	{
		if (swarmRoom == -1 || !ActiveSwarmRoom(world.GetSwarmRoom(swarmRoom)))
		{
			return Behavior.Inactive;
		}
		if (world.rainCycle.RainApproaching < 0.75f)
		{
			return Behavior.Gather;
		}
		return swarmRoomBehaviors[swarmRoom];
	}

	public FliesWorldAI(World world)
		: base(world)
	{
		migrationTargets = new int[world.swarmRooms.Length];
		for (int i = 0; i < world.swarmRooms.Length; i++)
		{
			migrationTargets[i] = Random.Range(0, world.swarmRooms.Length);
		}
		swarmRoomBehaviors = new Behavior[world.swarmRooms.Length];
		for (int j = 0; j < world.swarmRooms.Length; j++)
		{
			swarmRoomBehaviors[j] = Behavior.Spread;
		}
		fliesToSpawn = new int[world.swarmRooms.Length];
		if (!world.game.setupValues.worldCreaturesSpawn || world.singleRoomWorld)
		{
			return;
		}
		for (int k = 0; k < world.swarmRooms.Length; k++)
		{
			if (world.regionState == null || world.regionState.SwarmRoomActive(k))
			{
				fliesToSpawn[k] = world.region.regionParams.batsPerActiveSwarmRoom;
			}
			else
			{
				fliesToRespawn += world.region.regionParams.batsPerInactiveSwarmRoom;
			}
		}
		if (world.region.name == "SU" && world.game.IsStorySession)
		{
			fliesToSpawn[2] = world.game.GetStorySession.characterStats.foodToHibernate + 1;
		}
		if (world.game.setupValues.cycleStartUp && (!(world.game.session is StoryGameSession) || (world.game.session as StoryGameSession).saveState.cycleNumber > 0))
		{
			return;
		}
		for (int l = 0; l < fliesToSpawn.Length; l++)
		{
			while (fliesToSpawn[l] > 0)
			{
				AddFlyToSwarmRoom(l);
				fliesToSpawn[l]--;
			}
		}
	}

	public void LoadDijkstraMaps(SwarmRoomMapper mapper)
	{
		swarmRoomMap = mapper.ReturnSwarmRoomMap();
	}

	public override void Update()
	{
		if (!world.singleRoomWorld && Random.value < 1f)
		{
			AbstractRoom abstractRoom = world.GetAbstractRoom(Random.Range(0, world.NumberOfRooms) + world.firstRoomIndex);
			if (abstractRoom.nodes.Length != 0)
			{
				int num = MigrationDirection(new WorldCoordinate(abstractRoom.index, -1, -1, Random.Range(0, abstractRoom.nodes.Length)));
				if (num > -1)
				{
					MoveFly(abstractRoom, world.GetAbstractRoom(num));
				}
			}
		}
		if (!allInitialFliesSpawned)
		{
			StartUpUpdate(Mathf.InverseLerp(600f, 2800f, world.rainCycle.timer));
		}
		if (fliesToRespawn > 0 && world.rainCycle.TimeUntilRain > 800)
		{
			RespawnFlyInWorld();
			fliesToRespawn--;
		}
		if (!(Random.value < 0.0025f))
		{
			return;
		}
		for (int i = 0; i < world.swarmRooms.Length; i++)
		{
			swarmRoomBehaviors[i] = Behavior.Gather;
		}
		int num2 = -1;
		for (int j = 0; j < 2; j++)
		{
			int num3 = 0;
			int num4 = -1;
			for (int k = 0; k < world.swarmRooms.Length; k++)
			{
				if (k != num2 && world.GetSwarmRoom(k).NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly) > num3)
				{
					num3 = world.GetSwarmRoom(k).NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly);
					num4 = k;
				}
			}
			num2 = num4;
			if (num4 == -1)
			{
				continue;
			}
			if (j == 0)
			{
				swarmRoomBehaviors[num4] = Behavior.Spread;
				continue;
			}
			swarmRoomBehaviors[num4] = Behavior.Migrate;
			migrationTargets[num4] = Random.Range(0, world.swarmRooms.Length);
			if (world.regionState == null)
			{
				continue;
			}
			for (int l = 0; l < 5; l++)
			{
				if (world.regionState.SwarmRoomActive(migrationTargets[num4]))
				{
					break;
				}
				migrationTargets[num4] = Random.Range(0, world.swarmRooms.Length);
			}
		}
	}

	private void MoveFly(AbstractRoom fromRoom, AbstractRoom toRoom)
	{
		if (fromRoom.realizedRoom == null)
		{
			world.MoveQuantifiedCreatureFromAbstractRoom(CreatureTemplate.Type.Fly, fromRoom, toRoom);
		}
		else if (fromRoom.realizedRoom.fliesRoomAi != null)
		{
			Fly randomFly = fromRoom.realizedRoom.fliesRoomAi.GetRandomFly();
			if (randomFly != null && randomFly.room != null)
			{
				randomFly.AI.LeaveRoom(new WorldCoordinate(toRoom.index, -1, -1, -1));
			}
		}
	}

	public float GetSwarmRoomDistance(WorldCoordinate coord, int swarmRoomIndex)
	{
		if (coord.abstractNode < 0)
		{
			return -1f;
		}
		if (coord.room == -1)
		{
			Custom.Log(coord.ToString());
			if (world.IsRoomInRegion(coord.room))
			{
				Custom.Log(world.GetAbstractRoom(coord).name);
			}
			Custom.Log(swarmRoomMap[coord.room - world.firstRoomIndex].Length.ToString(), swarmRoomMap[coord.room - world.firstRoomIndex][coord.abstractNode].Length.ToString(), swarmRoomIndex.ToString());
		}
		return swarmRoomMap[coord.room - world.firstRoomIndex][coord.abstractNode][swarmRoomIndex];
	}

	public int ClosestSwarmRoom(WorldCoordinate coord)
	{
		if (ActiveSwarmRoom(world.GetAbstractRoom(coord)))
		{
			return world.GetAbstractRoom(coord).swarmRoomIndex;
		}
		if (!coord.NodeDefined)
		{
			coord = QuickConnectivity.DefineNodeOfLocalCoordinate(coord, world, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
		}
		int result = -1;
		float num = float.MaxValue;
		for (int i = 0; i < world.swarmRooms.Length; i++)
		{
			if (SwarmRoomBehavior(i) != Behavior.Inactive && GetSwarmRoomDistance(coord, i) > -1f && GetSwarmRoomDistance(coord, i) < num)
			{
				num = GetSwarmRoomDistance(coord, i);
				result = i;
			}
		}
		return result;
	}

	public int MigrationDirection(WorldCoordinate coord)
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(coord);
		int result = -1;
		int num = ClosestSwarmRoom(coord);
		if (ActiveSwarmRoom(abstractRoom) && abstractRoom.NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly) < 6)
		{
			return -1;
		}
		float num2 = float.MaxValue;
		if (SwarmRoomBehavior(num) == Behavior.Gather)
		{
			if (abstractRoom.swarmRoomIndex != num && num != -1)
			{
				int[] connections = abstractRoom.connections;
				foreach (int num3 in connections)
				{
					if (num3 > -1)
					{
						float swarmRoomDistance = GetSwarmRoomDistance(world.NodeInALeadingToB(num3, abstractRoom.index), num);
						if (swarmRoomDistance < num2 && swarmRoomDistance > -1f)
						{
							num2 = swarmRoomDistance;
							result = num3;
						}
					}
				}
			}
		}
		else if (SwarmRoomBehavior(num) == Behavior.Spread)
		{
			int num4 = abstractRoom.NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly);
			int[] connections = abstractRoom.connections;
			foreach (int num5 in connections)
			{
				if (num5 > -1)
				{
					float num6 = (float)world.GetAbstractRoom(num5).NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly) * 20f + Random.value - GetSwarmRoomDistance(world.NodeInALeadingToB(num5, abstractRoom.index), num);
					if (num6 <= (float)num4 && num6 < num2)
					{
						num2 = num6;
						result = num5;
					}
				}
			}
		}
		else if (SwarmRoomBehavior(num) == Behavior.Migrate)
		{
			int num7 = migrationTargets[num];
			if (abstractRoom.swarmRoomIndex != num7)
			{
				for (int j = 0; j < abstractRoom.connections.Length; j++)
				{
					if (abstractRoom.connections[j] > -1)
					{
						float swarmRoomDistance2 = GetSwarmRoomDistance(world.NodeInALeadingToB(abstractRoom.connections[j], abstractRoom.index), num7);
						if (swarmRoomDistance2 != -1f && swarmRoomDistance2 < num2)
						{
							num2 = swarmRoomDistance2;
							result = abstractRoom.connections[j];
						}
					}
				}
			}
		}
		return result;
	}

	private void StartUpUpdate(float startUpfac)
	{
		if ((startUpfac < 1f && Random.value > 0.1f) || Random.value > startUpfac)
		{
			return;
		}
		float num = 0f;
		int num2 = -1;
		bool flag = true;
		for (int i = 0; i < world.swarmRooms.Length; i++)
		{
			float num3 = (float)fliesToSpawn[i] + Random.value;
			if (fliesToSpawn[i] > 0)
			{
				flag = false;
				if (num3 > num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		if (num2 > -1)
		{
			AddFlyToSwarmRoom(num2);
			fliesToSpawn[num2]--;
		}
		if (flag)
		{
			allInitialFliesSpawned = true;
		}
	}

	private void RespawnFlyInWorld()
	{
		float num = 0f;
		int num2 = -1;
		for (int i = 0; i < world.swarmRooms.Length; i++)
		{
			if (world.regionState == null || world.regionState.SwarmRoomActive(i))
			{
				float num3 = Random.value / (float)world.GetAbstractRoom(world.swarmRooms[i]).NumberOfQuantifiedCreatureInRoom(CreatureTemplate.Type.Fly);
				if (num3 > num)
				{
					num = num3;
					num2 = i;
				}
			}
		}
		if (num2 > -1)
		{
			AddFlyToSwarmRoom(num2);
		}
	}

	public void RespawnOneFly()
	{
		fliesToRespawn++;
	}

	private void AddFlyToSwarmRoom(int spawnRoom)
	{
		if (world.worldGhost != null && world.worldGhost.CreaturesSleepInRoom(world.GetSwarmRoom(spawnRoom)))
		{
			return;
		}
		if (world.GetSwarmRoom(spawnRoom).realizedRoom != null)
		{
			if (world.GetSwarmRoom(spawnRoom).realizedRoom.quantifiedCreaturesPlaced)
			{
				if (world.GetSwarmRoom(spawnRoom).realizedRoom.fliesRoomAi == null)
				{
					world.GetSwarmRoom(spawnRoom).realizedRoom.fliesRoomAi = new FliesRoomAI(world.GetSwarmRoom(spawnRoom).realizedRoom);
				}
				world.GetSwarmRoom(spawnRoom).realizedRoom.fliesRoomAi.CreateFlyInHive();
			}
		}
		else
		{
			int specific = Random.Range(0, world.GetSwarmRoom(spawnRoom).NodesRelevantToCreature(StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly)));
			specific = world.GetSwarmRoom(spawnRoom).CreatureSpecificToCommonNodeIndex(specific, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
			world.GetSwarmRoom(spawnRoom).AddQuantifiedCreature(specific, CreatureTemplate.Type.Fly);
		}
	}

	public bool ActiveSwarmRoom(AbstractRoom room)
	{
		if (!room.swarmRoom)
		{
			return false;
		}
		if (world.regionState != null)
		{
			return world.regionState.SwarmRoomActive(room.swarmRoomIndex);
		}
		return true;
	}
}
