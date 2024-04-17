using System.Collections.Generic;
using RWCustom;

public class SwarmRoomMapper
{
	private World world;

	private int currentSwarmRoom;

	private int[,] migrationBlockages;

	private List<WorldCoordinate> checkNext;

	public bool done;

	private float[][][] map;

	public SwarmRoomMapper(World world, int[,] migrationBlockages)
	{
		this.world = world;
		this.migrationBlockages = migrationBlockages;
		map = new float[world.NumberOfRooms][][];
		for (int i = 0; i < world.NumberOfRooms; i++)
		{
			map[i] = new float[world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length][];
			for (int j = 0; j < world.GetAbstractRoom(i + world.firstRoomIndex).nodes.Length; j++)
			{
				map[i][j] = new float[world.swarmRooms.Length];
				for (int k = 0; k < world.swarmRooms.Length; k++)
				{
					map[i][j][k] = -1f;
				}
			}
		}
		currentSwarmRoom = -1;
		InitNewMapping();
	}

	public void Update()
	{
		if (done)
		{
			return;
		}
		if (checkNext.Count > 0)
		{
			WorldCoordinate worldCoordinate = checkNext[0];
			checkNext.RemoveAt(0);
			AbstractRoom abstractRoom = world.GetAbstractRoom(worldCoordinate);
			for (int i = 0; i < abstractRoom.nodes.Length; i++)
			{
				WorldCoordinate worldCoordinate2 = new WorldCoordinate(worldCoordinate.room, -1, -1, i);
				float num = abstractRoom.nodes[i].ConnectionCost(worldCoordinate.abstractNode, StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly));
				if (i == worldCoordinate.abstractNode && i < abstractRoom.connections.Length && abstractRoom.connections[i] > -1)
				{
					worldCoordinate2 = world.NodeInALeadingToB(abstractRoom.connections[i], abstractRoom.index);
					num = (float)world.TotalShortCutLengthBetweenTwoConnectedRooms(abstractRoom.connections[i], abstractRoom.index) * StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly).ConnectionResistance(MovementConnection.MovementType.ShortCut).resistance;
					num += StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Fly).ConnectionResistance(MovementConnection.MovementType.BetweenRooms).resistance;
					if (MigrationBlocked(worldCoordinate2, worldCoordinate))
					{
						num = -1f;
					}
				}
				if (worldCoordinate2.abstractNode < 0 || world.GetNode(worldCoordinate2).submerged)
				{
					num = -1f;
				}
				if (num > -1f && map[worldCoordinate2.room - world.firstRoomIndex][worldCoordinate2.abstractNode][currentSwarmRoom] == -1f)
				{
					map[worldCoordinate2.room - world.firstRoomIndex][worldCoordinate2.abstractNode][currentSwarmRoom] = map[worldCoordinate.room - world.firstRoomIndex][worldCoordinate.abstractNode][currentSwarmRoom] + num;
					checkNext.Add(worldCoordinate2);
				}
			}
		}
		else
		{
			InitNewMapping();
		}
	}

	private void InitNewMapping()
	{
		currentSwarmRoom++;
		if (currentSwarmRoom < world.swarmRooms.Length)
		{
			checkNext = new List<WorldCoordinate>();
			for (int i = 0; i < world.GetSwarmRoom(currentSwarmRoom).nodes.Length; i++)
			{
				if (!world.GetSwarmRoom(currentSwarmRoom).nodes[i].submerged)
				{
					checkNext.Add(new WorldCoordinate(world.GetSwarmRoom(currentSwarmRoom).index, -1, -1, i));
					map[world.GetSwarmRoom(currentSwarmRoom).index - world.firstRoomIndex][i][currentSwarmRoom] = 0f;
				}
			}
		}
		else
		{
			Custom.Log("Mapped all", currentSwarmRoom.ToString(), "swarm rooms");
			done = true;
		}
	}

	private bool MigrationBlocked(WorldCoordinate from, WorldCoordinate to)
	{
		if (world.GetAbstractRoom(to).shelter || world.GetAbstractRoom(to).gate)
		{
			return false;
		}
		for (int i = 0; i < migrationBlockages.GetLength(0); i++)
		{
			if (migrationBlockages[i, 1] == to.room && (migrationBlockages[i, 0] == from.room || migrationBlockages[i, 0] == -1))
			{
				return true;
			}
		}
		return false;
	}

	public float[][][] ReturnSwarmRoomMap()
	{
		return map;
	}
}
